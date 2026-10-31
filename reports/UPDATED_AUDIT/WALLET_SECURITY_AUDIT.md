# Sana3y Hub — Wallet Security Audit (Updated Audit)

**Audit date:** 2026-10-31 · Static analysis only. No real financial transaction, no production access, no real payment provider call was made or attempted.

**Files reviewed in full:** `Controllers/WalletController.cs`, `WalletTopUpsController.cs`, `WalletWithdrawalsController.cs`, `WalletPaymentsController.cs`, `PaymobWebhookController.cs`, `AdminWalletController.cs`, `AdminWalletWithdrawalsController.cs`, `AdminPlatformWithdrawalsController.cs`, `CustomerPaymentsController.cs`, `ArtisanCashPaymentsController.cs`; `Models/Wallet.cs`, `WalletTransaction.cs`, `WalletTopUp.cs`, `WalletWithdrawal.cs`, `PlatformWallet.cs`, `PlatformWalletTransaction.cs`, `PlatformWithdrawal.cs`, `PlatformCommission.cs`, `ServicePayment.cs`; wallet entities/edges in `Data/ApplicationDbContext.cs:948–1499`; migrations `20261029182731_AddWallets`, `20261029183832_ConfigureWalletTables`, `20261029185612_AddWalletTopUps`, `20261030012137_AddPlatformCommission`, `20261030021641_AddPlatformWalletAndWithdrawals`.

---

## 1. Wallet architecture and workflows

### 1.1 Data model (all monetary amounts `decimal(18,2)`, currency `EGP`)

| Table | Key integrity constraints (evidence) |
|---|---|
| `Wallets` | one per user, unique index (`ApplicationDbContext.cs:978–979`); `CK_Wallets_Balance [Balance] >= 0` (`:953–955`); FK to Users `Restrict` (`:981–984`) |
| `WalletTransactions` | immutable-ish ledger: `Amount > 0`, `BalanceAfter >= 0`, direction/type/status whitelists (`:992–1016`); unique `(Provider, ProviderTransactionId)` where not null (`:1076–1082`); FKs all `Restrict` (`:1055–1068`) |
| `WalletTopUps` | unique `SpecialReference`, unique `ProviderOrderId` and `ProviderTransactionId` (filtered, `:1152–1161`); state whitelist Pending/Paid/Failed/Cancelled/Expired (`:1096–1104`) |
| `WalletWithdrawals` | `Amount > 0`, status whitelist (`:1175–1187`), payout-method whitelist (`:1189–1191`) |
| `PlatformWallets` | unique `AccountCode`; `Balance >= 0` (`:1275–1305`) |
| `PlatformWalletTransactions` | type whitelist, `BalanceAfter >= 0`, unique `PlatformCommissionId` when not null (one ledger row per commission, `:1382–1390`) |
| `PlatformWithdrawals` | amount/status/payout whitelists (`:1400–1415`) |
| `PlatformCommissions` | per-`ServicePaymentId`; `Status` Collected/Due/Reversed |

### 1.2 Workflow map (as implemented)

| Flow | Endpoint(s) | Money moves | Gateway |
|---|---|---|---|
| Wallet creation | implicit — `GetOrCreateWalletAsync` on first access (`WalletController.cs:104–155`, `WalletPaymentsController.cs:521–577`) | `Balance = 0` | — |
| Balance retrieval | `GET /api/Wallet/me` (`WalletController.cs:29–54`), aggregate in CustomerDashboard | read-only | — |
| Deposits (top-ups) | `POST /api/WalletTopUps` creates a **Pending** `WalletTopUp` + Paymob Unified Checkout intention (`WalletTopUpsController.cs:46–380`); credit happens **only** in `POST /api/PaymobWebhook/transaction-processed` (`PaymobWebhookController.cs:377–445`) | wallet `+Amount` | Paymob |
| Payments | `POST /api/WalletPayments/requests/{id}/pay` debits customer total, credits artisan net, credits platform commission, writes a `ServicePayment` and three ledger rows (`WalletPaymentsController.cs:37–497`) | internal transfer | none (internal) |
| Withdrawals | `POST /api/WalletWithdrawals` reserves (debits) the amount and creates a Pending request (`WalletWithdrawalsController.cs:32–276`); Admin approve/complete/fail/reject (`AdminWalletWithdrawalsController.cs:120–421`) | reserved at request-time | manual payout by admin |
| Refunds (withdrawal) | reject/fail return the reserved amount via `RefundWithdrawal` (`AdminWalletWithdrawalsController.cs:484–525`) | internal | — |
| Refunds (service payments) | **none implemented** — `RefundCredit` ledger type is defined (`ApplicationDbContext.cs:1014`) but never written anywhere in the codebase | — | — |
| Transaction history | `GET /api/Wallet/my-transactions` (`WalletController.cs:58–101`), `GET /api/WalletWithdrawals/my-requests`, admin lists | read-only | — |
| Admin test credit | `POST /api/AdminWallet/users/{id}/test-credit` — **Development-only**, returns 404 otherwise (`AdminWalletController.cs:41–45`), writes a permanent `DevelopmentTest` top-up + ledger row | wallet `+Amount` | — |
| Platform wallet | commission accrual on wallet payments; admin earnings withdrawals with reserve/refund state machine (`AdminPlatformWithdrawalsController.cs:257–753`); MAIN wallet seeded/back-filled by migration (`20261030021641_AddPlatformWalletAndWithdrawals.cs:228–286`) | internal | — |

**External integrations found: Paymob only** (host `accept.paymob.com`, Unified Checkout `/unifiedcheckout/` + intention API + HMAC-verified webhook). Config keys: `Paymob:SecretKey`, `PublicKey`, `HmacSecret`, `NotificationUrl`, `RedirectionUrl`, `Paymob:IntegrationIds[]`. No other provider, no cash-payout automation.

### 1.3 Authorization model

| Surface | Role gate | Extra DB re-checks |
|---|---|---|
| User wallet endpoints | `[Authorize(Roles = "Customer,Artisan")]` (`WalletController.cs:17`, `WalletTopUpsController.cs:24`, `WalletWithdrawalsController.cs:19`) | active account (`WalletTopUpsController.cs:114`), artisan-approval gate for withdrawals (`WalletWithdrawalsController.cs:130–134`) |
| Wallet payments | `[Authorize(Roles = "Customer")]` (`WalletPaymentsController.cs:20`) | ownership in claim predicate (`:58–63`), completed-once state machine (`:438–461`) |
| Admin wallet endpoints | `[Authorize(Roles = "Admin")]` (all 3 controllers) | `GetActiveAdminIdAsync` — admin id, role **and** active status re-verified from DB on every action (`AdminWalletWithdrawalsController.cs:528–548`, `AdminPlatformWithdrawalsController.cs:861–881`) |
| Paymob webhook | `[AllowAnonymous]` (`:22`) | HMAC-SHA512 fixed-time verify before any parse/use (`:100–120, 461–484`) |

**Conclusion:** every wallet operation is scoped by the JWT user id or an admin DB re-verification; no wallet endpoint was found that acts on another user's wallet from user input.

---

## 2. Balance integrity

| Question | Verdict | Evidence |
|---|---|---|
| Can users set/manipulate their balance? | **No** — no endpoint accepts a `Balance` or amount-written-to-wallet input; all debits/credits are derived server-side (offer price from DB `:105–115`, top-up amount matched against Paymob notification `:329–343`, commission from server config `:500–518`). | mass-assignment scan |
| Floating-point used? | **No** — `decimal` everywhere; Paymob cents handled as `decimal`/`long` (`decimal.ToInt64(input.Amount * 100m)` `WalletTopUpsController.cs:155–156`) | all monetary columns |
| Rounding correct? | **Yes** — commission = `Math.Round(price * rate / 100m, 2, MidpointRounding.AwayFromZero)` (`WalletPaymentsController.cs:139–142`); 2-dp enforcement on all inputs (`WalletTopUpsController.cs:59–61`, `WalletWithdrawalsController.cs:46–53`) | — |
| Negative balance possible? | **No in the ledger** — DB CHECK `[Balance] >= 0` on both wallet tables; every debit is a conditional atomic update `WHERE Balance >= amount` (`WalletWithdrawalsController.cs:173–186`, `WalletPaymentsController.cs:217–229`, `AdminPlatformWithdrawalsController.cs:347–359`). If the race allowed a debit of a drained row, `ExecuteUpdateAsync` returns 0 rows → Conflict. The CHECK constraint is the backstop that would abort any inconsistent write with a DB error rather than persist a negative balance. | `ApplicationDbContext.cs:953–955, 1275–1277` |
| Precision drift across operations? | **No** — debit full price, credit `price − commission`, platform `+commission`; sum of credits always equals debit (`WalletPaymentsController.cs:216–265`) | — |

## 3. Transaction integrity & atomicity

- All money-mutating operations run inside an explicit `BeginTransactionAsync(IsolationLevel.Serializable)` (`WalletPaymentsController.cs:50–53`, `WalletWithdrawalsController.cs:138–141`, `AdminWalletWithdrawalsController.cs:131–134`, `AdminPlatformWithdrawalsController.cs:314–317`, `PaymobWebhookController.cs:198–201`, plus cash confirmations in `CustomerPaymentsController.cs:259–262` and `ArtisanCashPaymentsController.cs:43–46`).
- Debit → row insert → ledger insert → request-state transition are committed together; **every failure path rolls back explicitly**, including rare "credit writer returned 0 rows" mid-transaction failures (`WalletPaymentsController.cs:257–265, 350–358, 453–461`).
- No partial commit path was found: SaveChanges calls between steps happen inside the DB transaction; rollback restores balances.
- Ledger rows (`WalletTransactions`, `PlatformWalletTransactions`) are written in the same transaction as the balance mutation, so `BalanceAfter` chains stay connected to balance changes.

## 4. Double spending & race conditions

| Scenario | Protection | Evidence |
|---|---|---|
| Two simultaneous withdrawals draining the same balance | Each executes `UPDATE Wallets SET Balance -= @amt WHERE Id=@id AND UserId=@uid AND Balance >= @amt`; the second sees the post-commit balance (or rolls back) → cannot go negative or double-spend | `WalletWithdrawalsController.cs:173–186` |
| Concurrent wallet payment double-execution | First claim flips `AwaitingPayment → PaymentProcessing` via conditional update (`:58–71`); a duplicate pays only if it can re-claim — second request gets `409 conflict` with current status | `WalletPaymentsController.cs:57–102` |
| Concurrent credit + debit on the same wallet | Serializable isolation serializes writers; conditional updates make outcome deterministic; contention results in a deadlock victim (see WAL-04) rather than lost updates | isolation choices above |
| Replay of a payment webhook | idempotency guard: a `Paid` top-up short-circuits (`PaymobWebhookController.cs:239–252`) + unique filtered index on `ProviderTransactionId` at the ledger level | `ApplicationDbContext.cs:1076–1082, 1159–1161` |
| Double commission ledger row | one ledger row per commission enforced by unique index | `ApplicationDbContext.cs:1382–1390` |

No pessimistic row locks (`UPDLOCK`) or `rowversion` columns are used; correctness relies on **conditional atomic updates + Serializable isolation**. That combination is sound for these straight-line mutations; the residual risks are WAL-04 (deadlocks under contention) and WAL-03 (duplicate *requests*, not duplicate money).

## 5. Idempotency

| Flow | Idempotent? | Detail |
|---|---|---|
| Webhook credit | **Yes** | `Status == "Paid"` early return; re-runs are harmless (`PaymobWebhookController.cs:241–252`) |
| Top-up creation | No (by design) | Each request creates a distinct intention; a user retrying after a 502 is told *not to* retry before checking status (`WalletTopUpsController.cs:254–261`); over-crediting is impossible because each intention maps to one wallet credit |
| Wallet payment | **Yes** (state machine) | `AwaitingPayment → PaymentProcessing → Completed` one-way claim; retry of a completed payment → 409 (`WalletPaymentsController.cs:73–102, 438–461`) |
| Withdrawal creation | **No** | No idempotency key; a double-clicked withdrawal creates two independent Pending requests and two debits — each individually safe, but the user may accidentally reserve twice (WAL-03) |
| Admin approve/reject/fail | **Yes** | Strict status gates make every transition exactly-once (`AdminWalletWithdrawalsController.cs:151, 229, 305, 388`) |

## 6. Transaction history confidentiality & auditability

- Users can only list **their own** ledger (scoped `t.WalletId == wallet.Id` where wallet belongs to the claims id — `WalletController.cs:74–76`) and their own withdrawals (`w.Wallet.UserId == userId` — `WalletWithdrawalsController.cs:294`).
- Full destination account numbers are exposed to admins only; every user-facing list/detail response masks accounts to `****last4` (`WalletWithdrawalsController.cs:344–360`).
- Audit trail quality: every balance mutation has a matching ledger row (posted in the same transaction) with actor references (`ReviewedByAdminId`), correlated provider ids, and running `BalanceAfter`. Gaps: no who-changed-what log for wallet *record* edits (AdminNotes etc.), and platform-commission rows are written only for wallet payments — **cash payments produce no commission ledger** (business gap, BR-CASH below).

## 7. Deposits / payment confirmation authenticity

- Users **cannot self-credit**: the only credit paths are (a) HMAC-verified Paymob webhook, (b) dev-gated admin test credit, (c) internal transfer/payment flows. No "mark as paid" user endpoint exists.
- Webhook verification: HMAC secret required at startup (`:45–56`); unsigned/malformed → 401/400; integration-id allow-list (`:158–189`); fields read **only after** HMAC passes (`:122+`); exact amount & currency match required (`:312–343`); `pending`, `error_occured`, `is_voided`, `is_refunded` gate crediting (`:254–290`); late success against a `Cancelled/Expired` top-up requires manual reconciliation, never auto-credited (`:292–310`). Wallet/payment status or amount cannot be manipulated from webhook input beyond linking a verified Paymob order (`:206–237`).
- **Unable to verify (external):** field ordering/format conformance with the live Paymob HMAC spec, and end-to-end checkout behavior. Marked WAL-09 — requires a Paymob sandbox account; deliberately not performed.

## 8. Refunds & payment state transitions

- Withdrawal refunds are exactly-once: `Pending → Rejected` and `Approved → Failed` are the only refunding transitions, both terminal; `complete` requires `Approved` and is terminal (`Paid`). A refunded request can never be re-refunded (status gates + terminal states).
- Wallet-paid service payments: **no refund flow exists** (also true for cash and Paymob service payments) — `RefundCredit` is an orphaned ledger type (WAL-06). Impact: a business gap, not an exploit.
- Cash payments: `AwaitingCashConfirmation → Paid` only when **both** customer and artisan confirm; duplicate confirms are absorbed (early `Paid` return); method and request-state gates prevent confirming a non-cash or non-pending payment (`CustomerPaymentsController.cs:279–313`, `ArtisanCashPaymentsController.cs:98–131`).
- Wallet payment state machine: invalid transitions (e.g., paying a request not `AwaitingPayment`, or with an active electronic payment) are rejected with 409 and the request status claim is rolled back (`WalletPaymentsController.cs:73–102, 164–192`).

---

## 9. Test cases (safe, static/behavioral analysis) and results

*No live execution environment was available (no .NET SDK, no Docker daemon — see TEST_RESULTS.md). Each case was evaluated by full-code-path analysis of the exact controllers listed. "Analysis result" records the observed code behavior, not a runtime observation.*

| # | Test case | Method | Expected secure behavior | Code path result | Verdict |
|---|---|---|---|---|---|
| WT-01 | Withdraw entire balance, then repeat | Trace `WalletWithdrawalsController.cs:138–199` | Second withdrawal rejected `insufficient` | Conditional update returns 0 rows when `Balance < amount` → 409 | **Pass (by analysis)** |
| WT-02 | Two concurrent withdrawals of 100 against balance 150 | Trace idle/debit interleave under Serializable + conditional update | Exactly one succeeds, or serialization error; never −50 | Conditional `WHERE Balance >= amount` prevents negative; deadlock victim gets 500 rollback | **Pass (no negative possible); WAL-04 500-on-contention residual** |
| WT-03 | Retry identical webhook notification twice | Trace `PaymobWebhookController.cs:239–252` | Second call no-ops | `Status == "Paid"` → 200 "processed before", no second credit | **Pass (by analysis)** |
| WT-04 | Forge webhook with valid JSON, wrong/missing HMAC | Trace `:59–120` | 401, no state change | HMAC required; fixed-time compare; DB txn starts only after verification | **Pass (by analysis)** |
| WT-05 | Webhook amount mismatch (pay 90 EGP for a 100 EGP top-up) | Trace `:329–343` | Reject, no credit | `amountCents != Amount*100` → rollback, 409 | **Pass (by analysis)** |
| WT-06 | User B calls `POST /api/WalletWithdrawals` with user A's wallet id | API takes no wallet id from the body — only own wallet resolved from claims | Cannot act on A's wallet | Wallet resolved `WHERE w.UserId == userId` (`:143–147`); debit predicate includes ownership (`:174–176`) | **Pass (by analysis)** |
| WT-07 | Pay request twice with wallet | Trace `:57–71` first claim | Second pay → 409 with current status | Status claim via ExecuteUpdate `AwaitingPayment→PaymentProcessing`; second claim finds 0 rows → 409 | **Pass (by analysis)** |
| WT-08 | Pay request while a Paymob payment is pending | Trace `:164–192` | 409 | Active non-cash payment → rollback + 409 | **Pass (by analysis)** |
| WT-09 | Customer opens `/api/Wallet/me` having never created a wallet | Trace `:104–155` | Wallet auto-created with 0.00; concurrent creation race handled | Unique index + `DbUpdateException` → re-read | **Pass (by analysis); WAL-01 note (GET with side effect)** |
| WT-10 | Admin rejects a withdrawal, then repeats reject | Trace `AdminWalletWithdrawalsController.cs:229–252` | Second reject → 409 | `Status != "Pending"` → 409; refund applied once | **Pass (by analysis)** |
| WT-11 | Admin approves then fails → net effect zero | Trace `:348–419` + `RefundWithdrawal` | Balance restored once, ledger shows Debit + WithdrawalRefund | Refund `+= Amount` once per terminal transition; `Paid` is terminal (no refund after) | **Pass (by analysis)** |
| WT-12 | Non-admin calls admin withdrawal endpoints | `[Authorize(Roles="Admin")]` + DB role re-check `GetActiveAdminIdAsync` | 401/403 | Implemented on every admin action | **Pass (by analysis)** |
| WT-13 | Inactive (suspended) user calls wallet/pay endpoints | JWT `OnTokenValidated` fails non-Active accounts (`Program.cs:247–252`) + per-controller re-checks | 401 everywhere | Fail-closed implemented | **Pass (by analysis)** |
| WT-14 | Negative/zero/EUR-currency withdrawal or top-up | DTO Range + controller checks + CK constraints | 400/409 | `Amount <= 0` → 400; non-EGP currency → Conflict (`WalletWithdrawalsController.cs:159–167`); top-up Range 1–10000 | **Pass (by analysis); WAL-02 note (no max cap on withdrawals)** |
| WT-15 | Wallet pay with 0-commission config (`Rate=0`) | Trace `:299–397` | Payment completes with no platform credit/ledger row | `if (commissionAmount > 0)` branch skipped; `platformWalletBalanceAfter` null | **Pass (by analysis)** |
| WT-16 | Pay request with no accepted offer / artisan wallet missing | Trace `:105–125, 200–214` | 409; wallet auto-created as needed | `acceptedOffer == null || Price <= 0` → 409; wallets created inside tx | **Pass (by analysis)** |

---

## 10. Wallet findings summary

| ID | Sev | Conf | Location | Finding | Status |
|----|-----|------|----------|---------|--------|
| WAL-01 | Low | Confirmed (static) | `WalletController.cs:41–43, 70–72` | **GET endpoints mutate state**: `GET /api/Wallet/me` and `/my-transactions` create a wallet if absent (side effect on a read; also gives wallet creation to any authenticated GET). Harmless to integrity but harms cacheability/REST semantics and makes an "open wallet" step implicit. | New |
| WAL-02 | Low | Confirmed (static) | `CreateWalletWithdrawalDto.cs:5–12`, `WalletWithdrawalsController.cs:46–53` | **No maximum withdrawal amount** — Range max is 9,999,999,999,999,999.99; only positivity + 2-dp are enforced. While overdraw is impossible, there is no per-request/period cap; the admin review queue is the sole control. Top-ups are capped at 10,000 for contrast. | New |
| WAL-03 | Low | Confirmed (static) | `WalletWithdrawalsController.cs:32–276` (no idempotency key) | **No idempotency/duplicate protection on withdrawal creation** — concurrent/double submits create two Pending reservations. Money-safe, UX-safe for admins, but user-hostile (locked funds until admin resolves). | New |
| WAL-04 | Medium | Medium (static reasoning) | Serializable transactions in all wallet flows, e.g. `WalletPaymentsController.cs:50–53` | **Serializable-isolation deadlock exposure under wallet contention**: concurrent operations touching the same hot rows (e.g. artisan wallet receiving payments while withdrawals/admin actions run) elect a deadlock victim → unhandled 1205 surfaces as generic 500 via ProblemDetails. No retry policy. Integrity is not compromised; availability of money endpoints under load is. | New |
| WAL-05 | Info | Confirmed (static) | `WalletPaymentsController.cs:282` vs `ServicePayment.cs` comment, `CreateServicePaymentDto.cs:8–12` | **`PaymentMethod = "Wallet"` is outside the documented method set** (Cash/Card/MobileWallet/Kiosk/Installments). No DB CHECK exists, so it persists fine; inconsistent docs/validation only. | New |
| WAL-06 | Info (business gap) | Confirmed (static) | `ApplicationDbContext.cs:1014` (`RefundCredit`); grep: never written | **No refund flow exists for any paid service payment** (wallet, cash, or Paymob). `RefundCredit` ledger type is defined but unused. A customer/artisan dispute has no system path for returning money. | New |
| WAL-07 | Info | Confirmed (static) | `WalletTopUpsController.cs:254–261` | **Dangling pending top-ups possible**: if the Paymob HTTP call fails or the response can't be parsed after the `WalletTopUp` row is saved, the record stays `Pending`/`Failed` with no reconciliation job; users are only warned not to retry. | New |
| WAL-08 | Low | Confirmed (static) | `PaymobWebhookController.cs:41` | **No rate limit / body-size cap on the anonymous webhook** (Kestrel 30 MB default is the only bound). HMAC failures are cheap to reject; still a DoS-waste surface. | New |
| WAL-09 | Unable to verify | — | `PaymobWebhookController.cs:461–531`, `WalletTopUpsController.cs:226–238` | **Live Paymob API conformance unverifiable offline**: exact HMAC field-ordering conformance (implementation matches Paymob's documented 20-field SHA-512 pattern shape), checkout redirect behavior, notification URL reachability, and `IntegrationIds` correctness require a sandbox account. Intentionally not exercised (no production/provider contact). | Blocked (environment) |
| WAL-10 | Info | Confirmed (static) | `AdminWalletController.cs:35–45` | Dev-only test-credit endpoint properly returns 404 outside Development and always writes a `DevelopmentTest` ledger record. Residual risk: if a prod deployment is accidentally run with `ASPNETCORE_ENVIRONMENT=Development`, admins can mint wallet money — flagged so ops treats env-configuration as a control. | New |
| WAL-11 | Info | Confirmed (static) | `AdminWalletWithdrawalsController.cs:90–115` | Withdrawal **detail** endpoint purposely exposes unmasked bank account numbers to Admins; lists mask. Documented inline; acceptable admin behavior, listed for completeness. | New |
| BR-CASH | Info (business logic) | Confirmed (static) | `CustomerPaymentsController.cs:243–369`, `ArtisanCashPaymentsController.cs:30–187`, `WalletPaymentsController.cs` | **Cash payments generate no commission and no platform ledger**: the dual-confirm cash flow marks the request Completed without any `PlatformCommission`/ledger entry, while wallet payments always charge commission. Revenue consistency between payment channels is untracked. | New |

## 11. Verified-correct wallet security properties (with evidence)

1. **BOLA/BFLA:** no wallet endpoint acts on a client-supplied wallet/user id; admin surface re-verifies role+status in DB (see §1.3).
2. **No fake deposit path:** user-facing endpoints never credit balances; credits come from HMAC webhook, internal transfers, or dev-gated admin credit only.
3. **Amount source-of-truth:** payment amounts come from accepted offers in the DB, never from request bodies (`WalletPaymentsController.cs:105–115`).
4. **Atomicity:** explicit Serializable transactions with explicit rollback on every failure branch, including mid-transaction update anomalies (`WalletPaymentsController.cs:453–461`).
5. **Ledger completeness:** every balance delta is paired with a same-transaction ledger row carrying `BalanceAfter` and correlation ids (`WalletPaymentsController.cs:400–435`, `PaymobWebhookController.cs:411–425`).
6. **Uniqueness backstops:** unique filtered indexes on provider ids, `SpecialReference`, and commission-id prevent double-ledger rows even under code bugs (`ApplicationDbContext.cs:1076–1082, 1152–1161, 1382–1390`).
7. **DB guardrails:** CHECK constraints on non-negative balances and positive amounts/money enums on every financial table (`ApplicationDbContext.cs:948–1499`).
8. **Secret hygiene:** Paymob keys/OTP keys live only in configuration (not committed); source contains no embedded credentials (grep evidence).

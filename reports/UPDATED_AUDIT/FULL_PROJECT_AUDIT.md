# Sana3y Hub — Full Project Audit (Updated Audit)

**Report date:** 2026-10-31
**Repository:** `D:\vs code files\Sanay3e-` (branch `main`, commit `834bbbe`, working tree clean)
**Baseline for change discovery:** commit `59994ef` — the tip state covered by the previous audit report (`reports/PROJECT_AUDIT_REPORT.md`, dated 2026-10-24, later deleted from the repo in commit `5fc9ab9`; content recovered from git history for regression work and never modified).
**Method:** read-only static review of the complete current tree + git history. No source files modified, no dependencies installed, no real financial transaction, no production or payment-provider contact, no secrets exposed.

---

## 1. Executive summary

Since the previous audit the project grew from a 5-controller auth/OTP backend into a full marketplace + wallet API (**22 controllers, ~84 endpoints**), adding customer orders/addresses/notifications/reviews, artisan offers with state machines, cash payments, **a complete Paymob-backed Wallet module (top-ups via Unified Checkout, HMAC-verified webhook credit, withdrawals with admin review, internal payments with platform commission)** and a platform-earnings wallet for admins.

Overall assessment: the **Wallet module — the highest-impact surface — is implemented to a strong standard**: decimal accounting, DB CHECK constraints (`Balance >= 0`), conditional atomic debit updates (`UPDATE … WHERE Balance >= @amt`), Serializable transactions around every money mutation, an HMAC-verified idempotent webhook, DB-re-verified admin authorization, and account masking. No critical wallet vulnerability was identified in static review.

The dominant remaining risks are **operational rather than exploitative**: (1) OTP/account-verification still works only in Development (production onboarding is blocked by design, and the anonymous OTP endpoints remain the most fragile design in the codebase), (2) **zero automated tests** across ~31,000 newly added lines, (3) no build/CI verification available in the audit environment, and (4) missing business flows (no refunds anywhere; cash payments bypass commission accounting).

**Finding counts (this audit):** 0 Critical-exploit, 2 High (both the OTP/verification design family and the production-onboarding blocker), 6 Medium, 9 Low, 8 Informational/business-gap. 14 previous-audit findings re-verified: 8 Fixed, 5 Still Present, 1 Partially Fixed.

---

## 2. Change discovery since the previous audit

`git diff --name-status 59994ef..HEAD`: **85 files changed, +31,200 / −89.**

| Category | Change |
|---|---|
| New API controllers (16) | `CustomerController`, `CustomerDashboardController`, `CustomerNotificationsController`, `CustomerOffersController`, `CustomerPaymentsController`, `CustomerReviewsController`, `ArtisanOffersController`, `ArtisanCashPaymentsController`, `PublicController`, `AdminSpecialtiesController`, `AdminPlatformWithdrawalsController`, `AdminWalletController`, `AdminWalletWithdrawalsController`, `WalletController`, `WalletTopUpsController`, `WalletWithdrawalsController`, `WalletPaymentsController`, `PaymobWebhookController` |
| Modified (7) | `AccountController` (rewritten — DB-backed profile + phone-change), `AuthController` (artisan multipart registration + uniform handling), `Program.cs` (rate limiting, headers, HSTS, health, CORS, fail-closed JWT status checks), `ApplicationDbContext` (+18 entities' configuration), `OtpVerification`, `ArtisanServiceType`, snapshot |
| New models (18) | all wallet/payment/domain models listed in §1 of WALLET_SECURITY_AUDIT.md plus `ServiceRequest/Offer/Payment/Review/Attachment`, `Specialty`, `HomePageContent`, `CustomerAddress`, `Notification`, `Platform*` |
| New DTOs (14) | wallet top-up/withdrawal, admin withdrawal/platform-withdrawal DTOs, service request/offer/payment/review DTOs, address/profile/phone-change DTOs |
| New migrations (12) | `AddPublicHomepageAndSpecialties` → `AddCustomerServiceRequests` → `AddServiceOffers` → `AddServicePayments` → `AddServiceReviews` → `AddWallets` → `ConfigureWalletTables` → `AddWalletTopUps` → `AddPlatformCommission` → `AddWalletWithdrawals` → `AddPlatformWalletAndWithdrawals` (seeds/back-fills `MAIN` platform wallet) → `AddPhoneChangeOtp` → `AddCustomerAddresses` → `AddNotifications` |
| Deleted | `reports/*` (2 previous audit reports — recovered via `git show 59994ef:…`), `SQLQuery1.sql` |
| Added | `Sanaeye/` React frontend (out of API scope; CORS `AllowFrontend` = `http://localhost:5173` matches Vite default) |

External payment integration found: **Paymob only** (`WalletTopUpsController.cs:226–238` — intention API `https://accept.paymob.com/v1/intention/`; `PaymobWebhookController` — transaction callback; config keys `Paymob:SecretKey/PublicKey/HmacSecret/NotificationUrl/RedirectionUrl/IntegrationIds`). No other gateways; payouts to users are manual admin transfers, not provider calls.

Secrets: none committed (grep scan + gitignore + prior git-history audit unchanged). `appsettings*` still absent from disk/repo — every config (`Jwt:*`, `Otp:HashKey`, `Paymob:*`, `PlatformCommission:Rate`, connection string) must come from environment/user-secrets.

---

## 3. Architecture (current)

| Aspect | Value |
|---|---|
| Framework | ASP.NET Core, `net10.0`, C# nullable+implicit usings |
| Data | EF Core 10.0.13 / SQL Server; `decimal(18,2)` money; check constraints; `Restrict` FKs everywhere financial |
| Auth | JWT HS256, 1 h, ≥32-byte key, pinned algorithm, `ClockSkew` 30 s; **per-request DB re-validation of account status + role (fail-closed)** in `OnTokenValidated` (`Program.cs:191–283`) |
| Middleware order | ExceptionHandler(ProblemDetails) → security headers → HSTS (prod) → HTTPS redirect → Routing → RateLimiter → CORS → AuthN → AuthZ (`Program.cs:295–336`) |
| Rate limiting | 4 IP-partitioned named policies (auth surface only; no global default) |
| API docs | OpenAPI + Scalar, Development only (`Program.cs:323–327`) |
| Health | `/health` liveness + `/health/ready` DB connectivity (`Program.cs:342–379`) |
| Files | Private storage: artisan images (content-sniffed JPEG/PNG ≤5 MB) and customer request attachments (whitelisted ext/MIME/magic ≤60 MB batches) — GUID names, no public serving routes |
| Deployment | Root-context Dockerfile (fixed), root `.dockerignore` excluding secrets/bin/obj |
| Frontend | `Sanaeye/` React (Vite) SPA — registration/login/OTP/home features only; no wallet UI yet |
| Tests / CI | **None** |

Domain flow: register (customer/artisan+images) → OTP verify → Active/PendingApproval → admin approve → orders → offers accept → start/finish → payment (cash dual-confirm | wallet | Paymob — last two for service payments not yet integrated for Paymob) → reviews → wallet pay/withdraw flows (see WALLET_SECURITY_AUDIT.md §1.2).

---

## 4. Complete findings register

Severity = actual impact × exploitability given the current code. Every finding includes required fields; regression status column covers §5 table entries.

### 4.1 High

**SEC-02R — Production verification pipeline still non-functional (High · Confirmed · Regression: Still Present)**
- Files: `Controllers/OtpController.cs:41–47`; `Controllers/AccountController.cs:145–151`
- Both `POST /api/Otp/request` and the new phone-change request return **501** outside Development. Consequence: production customers/artisans can register but never activate wallets (top-ups require Active accounts, `WalletTopUpsController.cs:114`), phone changes, or complete flows. This is the single largest product/integrity blocker: the financial module inherits identification power from an email/SMS channel that does not exist yet.
- Business impact: no real user onboarding in a deployed environment; opportunistic attackers get no benefit, but operators must not launch without this.
- Remediation: provider integration (P0-1 in roadmap) + tie verification to server-issued tickets.

**SEC-07A — Anonymous OTP endpoints bind client-supplied identity; dev mode leaks live codes for any user id (High in Development / Medium overall · Confirmed · Regression: improved by rate limiting, otherwise Still Present)**
- Files: `OtpController.cs:36–50` (`request.UserId` from body, `:49–50`), `:144` (`developmentCode` in response), `:150–166`, `:358–365` (verify activates accounts); `AccountController.cs:256` (same pattern, phone change — but these two phone-change endpoints are `[Authorize]`-gated, unlike OtpController).
- In Development, any unauthenticated caller can request an OTP for **any user id** and receive it in the response, then verify it. In Production the 501 gate prevents minting codes, but `/api/Otp/verify` itself remains anonymous and consumes any valid unused code — the integrity of account activation rests entirely on one environment check.
- Business impact: account activation trust; combined with SEC-02's removal this becomes an account-takeover primitive if OTP delivery goes live without redesign.
- Remediation: ticket-based flow (P0-2), keep rate limiting (present), remove `developmentCode` (or only for seeded test users), add status checks (suspended users currently can re-verify — low-severity residue noted in OTA-01 area).

### 4.2 Medium

**WAL-04 — Serializable-isolation deadlock exposure on wallet hot rows (Medium · Medium confidence, static)**
- Files: `WalletPaymentsController.cs:50–53`, `WalletWithdrawalsController.cs:138–141`, and every admin/refund/cash-confirm transaction.
- Parallel operations touching the same wallet rows can deadlock (SQL Server 1205) → transaction aborts → generic 500 to clients. Integrity is preserved (no lost updates possible: debits are conditional `Balance >= @amt` updates), but money endpoints can fail under concurrent load, and there is no retry policy anywhere.
- Business impact: availability/UX of the financial core under load, not loss of funds.
- Remediation: deadlock-retry wrapper or replace Serializable with lock-based Finan patterns already implicit in the atomic conditional updates.

**API-01 — Anonymous review endpoint exposes other customers' full names, unlimited (Medium · Confirmed, static)**
- File: `CustomerReviewsController.cs:159–204` — `[AllowAnonymous]` route returns `CustomerName = customer.FullName` for arbitrary artisan ids plus no rate limiting. Enables bulk PII (real names) scraping of the customer base.
- Remediation: display-handle/initials or authenticate the consumer; add rate limiting.

**SEC-07B — OTP verify probing oracle (Medium → operational · Confirmed · Regression: Still Present)**
- Files: `OtpController.cs:251–258` vs `:314–318` — distinct "No active verification code was found" / "Invalid verification code" responses let an attacker learn whether a given user currently has a live code. Low cost to fix (single generic message).

**BR-CASH — Cash payments bypass commission accounting (Medium business-logic · Confirmed, static)**
- Files: `CustomerPaymentsController.cs:243–369`, `ArtisanCashPaymentsController.cs:30–187` — cash confirmations complete the order with **no** `PlatformCommission`/platform-ledger entry, while wallet payments always book commission. Platform revenue accounting is inconsistent by channel; platform earnings reports (`AdminPlatformWithdrawalsController.GetPlatformWallet`) will understate true revenuepicture.
- Remediation: either record a `Due` commission for cash payments and reconcile manually, or declare cash as commission-free in `PlatformCommission`/docs — pick one, make it explicit.

**WAL-03 — No idempotency on withdrawal creation (Medium · Confirmed, static)**
- File: `WalletWithdrawalsController.cs:32–276` — retry/double-click creates two Pending requests and two reservations (each individually safe). Funds lock-up until admins resolve; no client idempotency key exists.
- Remediation: optional client `IdempotencyKey` + unique filtered index, as done for provider ids.

**OTA-01 (meta) — Plaintext dev OTP now surfaces in three places (Medium in Development · Confirmed)**
- Files: `OtpController.cs:144`, `AccountController.cs:256` (phone change), plus `OtpService` always generates. All are dev-gated (`IsDevelopment()` checks at `OtpController.cs:41–47`, `AccountController.cs:145–151`). Production relies on env correctness; treat as configuration-controlled risk (previous SEC-10 → expanded surface).

### 4.3 Low

| ID | Finding | Files / lines | Notes |
|---|---|---|---|
| WAL-01 | `GET /api/Wallet/me` and `/my-transactions` have a create-wallet side effect | `WalletController.cs:41–43, 70–72, 104–155` | race-safe (unique index + `DbUpdateException` re-read); REST purity issue only |
| WAL-02 | No maximum withdrawal amount (Range cap ≈ 10^16; only > 0 + 2 dp) | `CreateWalletWithdrawalDto.cs:5–12`, `WalletWithdrawalsController.cs:46–53` | admin review is the only brake; top-ups capped 1–10,000 for contrast |
| WAL-05 | `PaymentMethod = "Wallet"` outside documented method set | `WalletPaymentsController.cs:282` vs `ServicePayment.cs`, `CreateServicePaymentDto.cs` | no DB CHECK → persists fine; docs/consistency |
| WAL-07 | Dangling `Pending` top-ups if Paymob call/response breaks after row save | `WalletTopUpsController.cs:254–261, 321–338` | no reconcile job; user warned not to retry |
| WAL-08 | Anonymous webhook: no rate limit, unbounded body (30 MB Kestrel default) | `PaymobWebhookController.cs:41` | cheap HMAC reject; resource-waste surface |
| OTA-03 | Private storage keys (`ProfileImagePath`) leak in API shapes | `CustomerDashboardController.cs:236, 261`, `CustomerOffersController.cs:74–94` | inert now; becomes exposure if a serving route is added |
| SEC-06A | Registration enumeration messages (email vs phone 409 wording) | `AuthController.cs:61–79, 140–160` | Regression: Still Present |
| SEC-08 | `LoginRequest.PhoneNumber` lacks `[Phone]`/length | `DTOs/LoginRequest.cs:11` | Regression: Still Present |
| BUG-06 | `.http` scratch file references dead `/weatherforecast/` route | `Sanay3y.API.http:3` | Regression: Still Present |
| BUG-07 | Dead `LoginResponse`/`RegisterResponse` DTOs unreferenced | `DTOs/LoginResponse.cs`, `DTOs/RegisterResponse.cs` | Regression: Still Present |

### 4.4 Informational / business gaps

| ID | Finding | Files / lines |
|---|---|---|
| WAL-06 | No refund flow anywhere; `RefundCredit` ledger type defined but never written | `ApplicationDbContext.cs:1014` + grep |
| WAL-09 | Paymob live-API conformance (HMAC field order, checkout UX, notification URL) **unverifiable offline** | blocked by environment by design |
| WAL-10 | Dev-only admin test-credit endpoint correctly 404s in prod; env misconfiguration would enable money minting | `AdminWalletController.cs:41–45` |
| WAL-11 | Admin withdrawal detail shows unmasked bank numbers (admin-only, documented inline) | `AdminWalletWithdrawalsController.cs:90–115` |
| BR-01 | Numeric DTO bounds gaps (`SortOrder`, offer Price up to 10^9 EGP) | `AdminSpecialtiesController.cs:167–181`, `CreateServiceOfferDto.cs:8` |
| CFG-01 | No `appsettings.example.json`; many new config keys (`Paymob:*`, `Otp:HashKey`, `PlatformCommission:Rate`) invite misconfiguration | repo level |
| SEC-09 | Password policy = MinLength 8 only (carried over) | `DTOs/RegisterRequest.cs` |
| N-01 | No default/global rate limiter for authenticated endpoints (money endpoints unlimited in frequency) | `Program.cs:77–155` |

### 4.5 Verified-secure properties (explicit negatives with evidence)

- **SQL injection: none** — zero `FromSqlRaw`/`SqlQuery` in application code; all data access typed LINQ with constant/parameterized predicates (grep + controller reads). Migration `migrationBuilder.Sql` blocks contain only DDL/seed, no user input.
- **XSS/CSRF: N/A** — pure JSON API, bearer-token-only auth, no cookies.
- **Mass assignment: none** — all entity mutations use field whitelists; no binder-to-entity mapping anywhere (subagent cross-check of all controllers).
- **File upload safety: strong** — content-signature sniffing (no extension trust), MIME whitelists, per-type/total size caps, private storage with GUID names, traversal-guarded delete (`PrivateFileStorageService.cs:171–187`), DB-failure cleanup (`AuthController.cs:421–437`), and the one serving route is ownership-scoped with `nosniff`+`no-store` (`CustomerController.cs:1106–1160`).
- **Wallet BOLA/BFLA: none found** — every user wallet action derives identity from claims; every admin action re-verifies an active Admin in DB (WALLET_SECURITY_AUDIT.md §1.3).
- **Fake deposits: none** — the only autonomous-user credit path is the HMAC-verified webhook; balances cannot be written directly.
- **Secrets: none committed or hardcoded** (scan T3); JWT/OTP keys length-validated at startup with fail-fast (`Program.cs:38–48`, `OtpService.cs:18–31`).

---

## 5. Regression results vs previous audit

| Previous ID | Previous finding | Verdict | Evidence |
|---|---|---|---|
| BUG-01 | `/api/Account/me` always returns null name/email | **Fixed** | Controller now reads DB: `AccountController.cs:42–87`; JWT also now carries Name/Email claims (`JwtService.cs:60–78`) |
| SEC-01 | No rate limiting | **Fixed** | `Program.cs:77–155` + `[EnableRateLimiting]` at `AuthController.cs:47,121,352`, `OtpController.cs:35,149`. Residual: no global limiter (N-01) |
| SEC-02 | OTP delivery 501 in production | **Still Present** | `OtpController.cs:41–47`; now also affects phone-change (`AccountController.cs:145–151`) |
| SEC-03 | Unsalted SHA-256 OTP hash | **Fixed** | `OtpService.cs` — HMAC-SHA256 with mandatory ≥32-byte server key, fail-fast at startup, `FixedTimeEquals` verify |
| SEC-04 | No security headers / HSTS | **Fixed** | `Program.cs:299–320` (4 headers every response; HSTS prod-only) |
| SEC-05 | Role/status frozen in token; no revocation | **Partially Fixed** | `Program.cs:191–283` re-validates status + role from DB per request (suspension takes effect immediately) — but tokens are still 1 h with no revocation store; 1h window is now harmless because checks fail closed. Treat remaining gap as accepted-risk unless key compromise is a concern |
| SEC-06 | Register account enumeration | **Still Present** | `AuthController.cs:61–79, 140–160` distinct 409 wordings |
| SEC-07 | OTP verify unauthenticated + response differentiation | **Still Present** (rate limiting added to both endpoints) | `OtpController.cs:150–161` vs `:251–258/314–318` |
| SEC-08 | Login phone unvalidated | **Still Present** | `LoginRequest.cs:11` |
| SEC-09 | Weak password policy | **Still Present** | `RegisterRequest` MinLength 8 only |
| SEC-10 | Dev-only plaintext OTP | **Still Present** (expanded to 3 surfaces, all dev-gated) | see OTA-01 |
| SEC-11 | No CORS policy | **Fixed (decision made)** | `Program.cs:27–36` — explicit allow-list `http://localhost:5173`, no credentials |
| BUG-02 | Dockerfile context-only builds | **Fixed** | Root-context COPY paths (`Dockerfile:11–14, 17`) + root `.dockerignore`; root-context invocation now expected to succeed (runtime-verified in the previous audit's approach wasn't possible here — Docker daemon down; mark build-output unverified) |
| BUG-03 | Empty-bodied 500s | **Fixed** | `Program.cs:70–71, 295–296`: `AddProblemDetails` + `UseExceptionHandler` |
| BUG-04 | Registration duplicate race → 500 | **Fixed** | `AuthController.cs:100–108, 268+` catch `DbUpdateException` → `IsUniqueConstraintViolation` → 409 (`:439–456`) |
| BUG-05 | OTP attempt-counter race | **Fixed** | `OtpController.cs:277–305` — atomic conditional `ExecuteUpdate` (`WHERE … Attempts < 5`) inside a transaction; wrong code still counts the attempt (`:307–318`); consume-once guard (`:320–339`) |
| BUG-06 | Dead `.http` route | **Still Present** | `Sanay3y.API.http:3` |
| BUG-07 | Dead response DTOs | **Still Present** | grep: definitions only |
| BUG-08 | No health endpoint | **Fixed** | `Program.cs:342–379` (`/health`, `/health/ready`) |
| (audit finding) | No admin can exist via API | **Still Present** | no Admin creation path in code; manual SQL still required |
| (audit finding) | Missing test coverage | **Still Present** (worse — 31k new untested lines) | no test project |
| (audit finding) | Vulnerability scan blocked (no SDK) | **Unable to Verify (same cause)** | TEST_RESULTS.md §1 |
| (audit finding) | No committed example config (CFG-01/DOC-01) | **Still Present** | no `appsettings.example.json` |
| (audit finding) | No `.dockerignore` (CFG-03) | **Fixed** | root `.dockerignore` present, excludes secrets/bin/obj |

Summary: **8 Fixed / 1 Partially Fixed / 10 Still Present / 1 Unable to Verify** of the re-checkable previous findings.

---

## 6. Wallet security assessment (summary)

Full detail in `WALLET_SECURITY_AUDIT.md`. Bottom line:

- **Balance integrity: sound** — decimal(18,2) everywhere, no floats, rounding `AwayFromZero`, server-side amount derivation, conditional atomic debits with `Balance >= @amt` guard, DB CHECK `[Balance] >= 0` as a final backstop. Negative balances are structurally impossible under the current code paths.
- **Authorization: sound** — claims-derived wallet resolution everywhere; admin re-verification in DB; Paymob webhook state-gated by HMAC. No IDOR/BOLA found.
- **Transaction integrity: sound** — Serializable transactions span debit→ledger→state transitions with explicit rollback on all failure branches, including mid-transaction anomalies.
- **Double spending: prevented** — conditional updates guarantee a debit applies iff funds exist at execution time; webhooks are state-machine idempotent plus unique-index-protected at the ledger level.
- **Idempotency: good where needed** (webhook, payments, admin transitions); missing only on withdrawal creation (WAL-03, UX not money loss).
- **History/PII: protected** — own-wallet scope enforced; account numbers masked everywhere except admin detail views.
- **Deposits: verified only via HMAC webhook** — pending/voided/refunded/mismatched amounts never credit; late-success-on-cancelled requires manual reconciliation (safe default).
- **Refunds: withdrawal refunds exactly-once**; **service-payment refunds do not exist** (business gap WAL-06), and **cash flow bypasses commission accounting** (BR-CASH).
- **Residual risks:** WAL-04 (deadlock availability), WAL-08 (webhook DoS waste), WAL-09 (externalprovider conformance unverifiable offline).

## 7. Functional bugs / business-logic issues (non-security)

1. **BR-CASH** — cash payments create zero commission/ledger while wallet payments always do (see 4.2).
2. **WAL-05** — `"Wallet"` payment method inconsistency between model docs, validation regex, and persisted value.
3. **WAL-06** — no refund path for any payment channel; customer disputes have no system mechanism.
4. **WAL-07** — unreconcilable dangling Pending top-ups after partial Paymob failures.
5. **BR-01** — DTO bound gaps (`SortOrder`, 10^9 EGP offer ceiling) — no exploitation path, hygiene.
6. **`.http` file** — instructions in the scratch file do not match any real route (BUG-06).
7. **OTA-03** — shape-only exposure of private storage keys (inert until a serving route exists).

## 8. Environment & execution limitations

1. No .NET SDK (runtime-only), no Docker daemon, no MSBuild → compile, dependency scan, and any runtime/socket testing were blocked. The complete reason trail is in `TEST_RESULTS.md` §1.
2. No SQL Server/DB → no runtime verification of CHECK constraints, deadlocks, or concurrent flows.
3. No Paymob sandbox → provider conformance left explicitly unverified (WAL-09).
4. All wallet test cases WT-01…WT-16 were evaluated by code-path tracing only — no runtime execution.

## 9. Report set

| File | Content |
|---|---|
| `FULL_PROJECT_AUDIT.md` | this document |
| `API_SECURITY_MATRIX.md` | 84-endpoint inventory with auth/roles/DTO/ownership/security observations |
| `WALLET_SECURITY_AUDIT.md` | wallet architecture, 16 analyzed test cases, findings WAL-01…WAL-11 |
| `TEST_RESULTS.md` | executed/blocked/not-run matrix with evidence |
| `REMEDIATION_ROADMAP.md` | P0–P3 order of work |

*No previous report was overwritten (the directory did not exist — previous reports remain at commit `59994ef` in git history, untouched).*

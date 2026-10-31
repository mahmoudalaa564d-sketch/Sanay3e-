# Sana3y Hub — Remediation Roadmap (Updated Audit)

**Audit date:** 2026-10-31 · Prioritized by real-world exploitability and business impact. Wallet/financial items come first where they carry actual money risk.

---

## P0 — Critical (do before any production traffic/money)

| # | Action | Findings | Where |
|---|--------|----------|-------|
| 1 | **Ship real OTP/e-mail/SMS delivery** so account verification, phone change, and (by extension) the whole onboarding and wallet-eligibility lifecycle work outside Development; until then `/api/Otp/request` must not be the only identity-proof control. | SEC-02 | `OtpController.cs:41–47`, `AccountController.cs:145–151` |
| 2 | **Rebuild the OTP flow around a signed server-side ticket** instead of client-supplied `UserId`: request/verify should be tied to the registration context (single-use, short-lived token) — removes the anonymous activate-any-user path that currently survives only behind the 501 gate. | SEC-07A | `OtpController.cs:49–50, 163–164` |
| 3 | **Add automated tests + CI** (xUnit + `WebApplicationFactory` + Testcontainers SQL Server) before money flows go live: wallet payment, withdrawal reserve/refund, webhook idempotency, dual-confirm cash flow. Nothing in the wallet is test-protected today. | T7, WAL-* | repo-level |
| 4 | **Obtain Paymob sandbox + integration verification** (HMAC ordering, notification URL, `IntegrationIds`) and a manual reconciliation runbook for `Pending`/`Failed` top-ups. | WAL-09, WAL-07 | external |

## P1 — High

| # | Action | Findings | Where |
|---|--------|----------|-------|
| 5 | Add a **global fallback rate limiter** + stricter limits on money endpoints (withdrawals/payments) and the Paymob webhook; add `[RequestSizeLimit]` there. Avoids resource-exhaustion via the anonymous webhook and unauthenticated review endpoint. | WAL-08, API-01 | `Program.cs:77–155`, `PaymobWebhookController.cs:41`, `CustomerReviewsController.cs:159` |
| 6 | **Introduce a refund flow** for wallet/cash/Paymob service payments using the existing `RefundCredit` ledger type with per-payment refund caps (never exceed original; state-machine-gated). | WAL-06 | new endpoint + `ServicePayment` state machine |
| 7 | **Deadlock resilience for wallet flows:** retry-on-1205 policy around Serializable transactions (or replace Serializable with row-versioned/`UPDLOCK` patterns already mathematically provided by the conditional updates), so transient failures return retriable errors instead of raw 500s. | WAL-04 | all wallet transaction blocks |
| 8 | **Withdrawal business rules:** per-request maximum (mirk topups run 1–10,000), optional daily cap, and an idempotency key (client-supplied reference or `WalletId+nonce` unique index) to absorb double-submits. | WAL-02, WAL-03 | `WalletWithdrawalsController.cs:46–100` |
| 9 | Unify the **payment-method taxonomy** (`"Wallet"` → add to model comment/enum or rename) and wire cash-channel commission policy explicitly (today cash produces no platform commission — verify this is intended). | WAL-05, BR-CASH | `ServicePayment.cs`, `WalletPaymentsController.cs:282` |

## P2 — Medium

| # | Action | Findings | Where |
|---|--------|----------|-------|
| 10 | **OTP response oracle hardening:** return one generic message for "no active code" vs "invalid code" on `/api/Otp/verify`; also check `AccountStatus` before consuming OTP (suspended accounts shouldn't re-verify contact). | SEC-07B, OTA-01 note | `OtpController.cs:251–258, 314–318` |
| 11 | Remove **anonymous customer-name PII surface** on artisan reviews (display handle or initials), or gate behind authenticated customer view; keep rate limiting anyway. | API-01 | `CustomerReviewsController.cs:159–204` |
| 12 | Replace pre-audit **account-enumeration messages** at registration with one neutral 409 (dedupe email/phone wording). | SEC-06A | `AuthController.cs:61–79, 140–160` |
| 13 | Add `[Phone]`/length validation parity on `LoginRequest.PhoneNumber`; document/provide `appsettings.example.json` (new Paymob + `Otp:HashKey` + `PlatformCommission:Rate` keys make configuration mistakes more likely). | SEC-08, CFG-01 | DTOs |
| 14 | Upper-bound numeric business DTOs (`SortOrder`, offer `Price` max ≫ realistic) and profile-image key exposure clean-up in dashboard/offer payloads. | BR-01, OTA-03 | DTOs, `CustomerDashboardController.cs:236, 261` |

## P3 — Low

| # | Action | Findings | Where |
|---|--------|----------|-------|
| 15 | GET `/api/Wallet/me` / `my-transactions` should read-only (create wallet explicitly via a POST or during onboarding). WAL-01. |
| 16 | Cleanup: dead `LoginResponse`/`RegisterResponse` DTOs, `Sanay3y.API.http` rewrite with real routes (incl. health + wallet examples). BUG-07, BUG-06. |
| 17 | Admin bootstrap path (documented seed or first-admin flow) + operator action audit log tables for admin review actions. |
| 18 | Wire a reconciliation sweeper for dangling Pending top-ups (WAL-07) and an ops alert on mismatched webhook amounts. |
| 19 | Password policy (complexity/breached-password check) + rehash-on-login path. SEC-09 (carried from previous audit). |
| 20 | Mining the CORS/deployment checklist: production frontend origins, HSTS verification behind proxy, switching `Scalar`/OpenAPI remains dev-only (already dev-gated). |

---

## Recommended implementation order

1. P0-1, P0-2 (identity-proof flow) — everything else depends on trustworthy account activation.
2. P0-3, P0-4 — protect the financial code before it runs.
3. P1-5 → P1-7 → P1-8 — abuse resistance + money-flow robustness.
4. P1-6, P1-9 — refund capability and channel consistency.
5. P2 items in listed order (cheap, incremental).
6. P3 hygiene.

**Deliberately excluded:** no real transaction, provider, or production-database testing is part of any item above; each is implementable and verifiable with stubs/local containers.

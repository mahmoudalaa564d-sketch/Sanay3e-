# Sana3y Hub — API Security Matrix (Updated Audit)

**Audit date:** 2026-10-31
**Baseline repository state:** branch `main`, commit `834bbbe`, working tree clean.
**Scope:** every endpoint actually implemented in `Sanay3y.API/Controllers/` (no endpoint is listed that was not found in code).
**Note on rows:** "Claims-id filter" means the handler restricts DB access by the user id taken from the JWT (`ClaimTypes.NameIdentifier` ?? `"sub"`), which is the primary BOLA/IDOR control.

---

## 1. Endpoint inventory (84 endpoints / 22 controllers)

Legend for **Auth**: `Anon+RL` = anonymous but rate-limited; `JWT` = any authenticated active user; `Role:X` = JWT + role; `HMAC` = anonymous but signature-verified.
Legend for **Ownership**: `ID` = claims-id filter present; `State` = status/state-machine gate; `—` = n/a.
Legend for **Findings**: see §3 (finding IDs) — `✓` means no new issue in this row.

### 1.1 Anonymous / public surface

| # | Endpoint | Method | Controller file | Auth | Role | Input DTO | Response | Ownership checks | Findings |
|---|----------|--------|-----------------|------|------|-----------|----------|------------------|----------|
| 1 | `/api/Auth/register` | POST | AuthController.cs:48 (`:47` RegisterPolicy) | Anon+RL | — | `RegisterRequest` | 201 `{userId, role, accountStatus}` | — | SEC-06A (enumeration messages), BUG-07 |
| 2 | `/api/Auth/register-artisan` | POST | AuthController.cs:122 (`:124` 16 MB limit) | Anon+RL | — | `ArtisanRegisterRequest` (multipart, 3 images) | 201 | — | `✓` (sniffed JPEG/PNG, private storage, cleanup on failure) |
| 3 | `/api/Auth/login` | POST | AuthController.cs:353 (`:352` LoginPolicy) | Anon+RL | — | `LoginRequest` | 200 `{token, role...}` | — | SEC-08 (no `[Phone]` on LoginRequest) |
| 4 | `/api/Otp/request` | POST | OtpController.cs:36 (`:35` OtpRequestPolicy) | Anon+RL | — | `RequestOtpRequest` (`UserId`, `Channel`) | 200 (dev: `developmentCode`) or **501 in prod** | — (client-supplied `UserId`) | SEC-07A (unauthenticated, client-chosen `UserId`), SEC-02 (501 blocks prod onboarding) |
| 5 | `/api/Otp/verify` | POST | OtpController.cs:150 (`:149` OtpVerifyPolicy) | Anon+RL | — | `VerifyOtpRequest` (`UserId`, `Channel`, 6-digit code) | 200 | — (client-supplied `UserId`; activates account :358–365) | SEC-07A, SEC-07B (distinguishable "no active code" vs "invalid code" responses) |
| 6 | `/api/Public/home` | GET | PublicController.cs:21 (`:22` AllowAnonymous) | Anon | — | — | 200 static content | — | `✓` |
| 7 | `/api/Public/specialties` | GET | PublicController.cs:106 (`:107` AllowAnonymous) | Anon | — | — | 200 active specialties | — | `✓` |
| 8 | `/api/PaymobWebhook/transaction-processed` | POST | PaymobWebhookController.cs:41 (`:22` AllowAnonymous) | HMAC (HMAC-SHA512, fixed-time compare :461–484) | — | raw JSON `obj` graph | 200/4xx/5xx JSON | Correlation by `ProviderOrderId`/`SpecialReference` only after HMAC | WAL-08 (no rate limit; Kestrel 30 MB default body cap); unable-to-verify WAL-09 |

### 1.2 Authenticated user surface (`[Authorize]`, all re-check `AccountStatus == "Active"` server-side, claims id fallback `NameIdentifier ?? sub`)

| # | Endpoint | Method | Controller file | Auth | Role | Input DTO | Response | Ownership | Findings |
|---|----------|--------|-----------------|------|------|-----------|----------|-----------|----------|
| 9 | `/api/Account/me` | GET | AccountController.cs:42 | JWT | any | — | own profile (email/phone own only) | ID (DB read by claims id) | `✓` (BUG-01 fixed - reads DB) |
| 10 | `/api/Account/me` | PUT | AccountController.cs:91 | JWT | Customer | `UpdateCustomerProfileDto` | 200 | ID + active gate | `✓` |
| 11 | `/api/Account/me/phone-change/request` | POST | AccountController.cs:136 | JWT | Customer | `RequestPhoneChangeOtpDto` | 200 (dev: `developmentCode` :256, prod: 501) | ID | OTA-01 (dev code, dev-gated) |
| 12 | `/api/Account/me/phone-change/verify` | POST | AccountController.cs:265 | JWT | Customer | `VerifyPhoneChangeOtpDto` | 200 | ID + atomic attempt/consume (`ExecuteUpdate` :362–398) | `✓` |
| 13 | `/api/Customer/addresses` | POST | CustomerController.cs:41 | JWT | Customer | `CreateCustomerAddressDto` | 201 | ID + active (`:53–57`), default-race handled (Serializable) | `✓` |
| 14 | `/api/Customer/addresses` | GET | CustomerController.cs:143 | JWT | Customer | — | own addresses | ID (`:156`) | `✓` |
| 15 | `/api/Customer/addresses/{id}` | PUT | CustomerController.cs:186 | JWT | Customer | `UpdateCustomerAddressDto` | 200 | ID (`a.Id == id && a.CustomerUserId == customerId` :207–210) | `✓` |
| 16 | `/api/Customer/addresses/{id}` | DELETE | CustomerController.cs:257 | JWT | Customer | — | 200 | ID (`:268–271`) | `✓` |
| 17 | `/api/Customer/addresses/{id}/default` | PUT | CustomerController.cs:292 | JWT | Customer | — | 200 | ID (`:307–310`) | `✓` |
| 18 | `/api/Customer/orders` | POST | CustomerController.cs:358 | JWT | Customer | `CreateServiceRequestDto` | 201 | ID (`CustomerUserId = customer.Id` :435); specialty validated | `✓` |
| 19 | `/api/Customer/orders` | GET | CustomerController.cs:506 | JWT | Customer | query `status`, `requestNumber` | 200 | ID (`:521`); status whitelist (`:533–566`) | `✓` |
| 20 | `/api/Customer/orders/{id}` | GET | CustomerController.cs:604 | JWT | Customer | — | 200 | ID (`:619–621`) | `✓` |
| 21 | `/api/Customer/orders/{id}/cancel` | PUT | CustomerController.cs:684 | JWT | Customer | — | 200 | ID twice (fetch `:702–705` + conditional `ExecuteUpdate` `:741–746`); cascades to offers | `✓` |
| 22 | `/api/Customer/orders/{id}/attachments` | POST | CustomerController.cs:851 | JWT | Customer | `UploadServiceRequestAttachmentsDto` (multipart) | 201 | ID (`:882–886`); `PendingOffers` state (`:896–903`); ≤5 files/60 MB/10/img-video sniff (`:905–983`) | `✓` (private storage; GUID names; sanitized names) |
| 23 | `/api/Customer/orders/{id}/attachments/{attachmentId}` | GET | CustomerController.cs:1106 | JWT | Customer | — | file stream | ID triple-scoped (`:1120–1123`); `nosniff` + `no-store` (`:1158–1159`) | `✓` |
| 24 | `/api/CustomerDashboard/summary` | GET | CustomerDashboardController.cs:28 | JWT | Customer | — | wallet/payment aggregates | ID (wallet sums scoped `:91–150`; payments `:156–158`) | `✓` |
| 25 | `/api/CustomerDashboard/specialties` | GET | CustomerDashboardController.cs:195 | JWT | Customer | — | 200 | — | `✓` |
| 26 | `/api/CustomerDashboard/recommended-artisans` | GET | CustomerDashboardController.cs:219 | JWT | Customer | — | 200 | — (public directory data) | OTA-03 (`ProfileImagePath` inert private key exposed in shape) |
| 27 | `/api/CustomerDashboard/search-services` | GET | CustomerDashboardController.cs:272 | JWT | Customer | query `query` | 200 | — (validated, parameterized) | `✓` |
| 28 | `/api/CustomerDashboard/profile` | GET | CustomerDashboardController.cs:336 | JWT | Customer | — | own profile | ID (`:352–369`) | `✓` |
| 29 | `/api/CustomerNotifications` | GET | CustomerNotificationsController.cs:39 | JWT | Customer | query `page`,`pageSize` (clamped 1–100 `:51–52`) | 200 | ID (`:54–56`) | `✓` |
| 30 | `/api/CustomerNotifications/unread-count` | GET | CustomerNotificationsController.cs:~90 | JWT | Customer | — | 200 | ID (`:98–101`) | `✓` |
| 31 | `/api/CustomerNotifications/{id}/read` | POST/PUT | CustomerNotificationsController.cs:111 | JWT | Customer | — | 200 | ID (`:121–124`) | `✓` |
| 32 | `/api/CustomerNotifications/read-all` | PUT/POST | CustomerNotificationsController.cs:151 | JWT | Customer | — | 200 | ID (`:160–164`) | `✓` |
| 33 | `/api/CustomerNotifications/{id}` | DELETE | CustomerNotificationsController.cs:185 | JWT | Customer | — | 200 | ID (`:194–198`) | `✓` |
| 34 | `/api/CustomerOffers/requests/{requestId}/offers` | GET | CustomerOffersController.cs:29 | JWT | Customer | — | offers on own request | ID (request first `:42–47`) | `✓` |
| 35 | `/api/CustomerOffers/requests/{requestId}/offers/{offerId}/accept` | POST | CustomerOffersController.cs:111 | JWT | Customer | — | 200 | ID (`:131–135`) + offer-belongs-to-request (`:153–158`) + Serializable state transition | `✓` |
| 36 | `/api/CustomerReviews/requests/{requestId}` | POST | CustomerReviewsController.cs:30 | JWT | Customer | `CreateServiceReviewDto` (`Rating` 1–5) | 201 | ID (`:46–51`), own paid payment (`:71–76`), single review enforced (`:102–140`) | `✓` |
| 37 | `/api/CustomerReviews/artisans/{artisanUserId}` | GET | CustomerReviewsController.cs:159 (`:160` AllowAnonymous) | **Anon** | — | — | reviews incl. `CustomerName` | — | API-01 (anonymous cross-user full-name PII, no rate limit) |
| 38 | `/api/CustomerPayments/requests/{requestId}/payments` | POST | CustomerPaymentsController.cs:32 | JWT | Customer | `CreateServicePaymentDto` (whitelist of 5 methods) | 201 cash payment | ID (`:89–101`), `AwaitingPayment` state, no-active-payment guard (`:141–157`) | WAL-06 note (card/mobile/kiosk → 501 not integrated) |
| 39 | `/api/CustomerPayments/my-payments` | GET | CustomerPaymentsController.cs:207 | JWT | Customer | — | own payments | ID (`:222`) | `✓` |
| 40 | `/api/CustomerPayments/{paymentId}/confirm-cash-paid` | POST | CustomerPaymentsController.cs:245 | JWT | Customer | — | 200 | ID (`:264–269`), Cash-only, dual-confirmation state machine (Serializable) | `✓` |
| 41 | `/api/Wallet/me` | GET | WalletController.cs:29 | JWT | Customer,Artisan | — | own wallet balance | ID (`:41–43`) | WAL-01 (GET has a create-wallet side effect) |
| 42 | `/api/Wallet/my-transactions` | GET | WalletController.cs:58 | JWT | Customer,Artisan | — | own ledger | ID (`:70–76`) | WAL-01 |
| 43 | `/api/WalletTopUps` | POST | WalletTopUpsController.cs:46 | JWT | Customer,Artisan | `CreateWalletTopUpDto` (Range 1–10000) | 201 `{checkoutUrl}` | ID + active + email/phone required; Paymob intention server-side | WAL-05 note, WAL-13 |
| 44 | `/api/WalletWithdrawals` | POST | WalletWithdrawalsController.cs:32 | JWT | Customer,Artisan | `CreateWalletWithdrawalDto` | 200 | ID + active (+artisan Approved `:130–134`); ownership in debit predicate (`:173–177`); Serializable | WAL-02 (no max cap), WAL-03 (no idempotency key) |
| 45 | `/api/WalletWithdrawals/my-requests` | GET | WalletWithdrawalsController.cs:280 | JWT | Customer,Artisan | — | own withdrawals (masked account numbers) | ID (`w.Wallet.UserId == userId` :292–294) | `✓` |
| 46 | `/api/WalletPayments/requests/{requestId}/pay` | POST | WalletPaymentsController.cs:37 | JWT | Customer | — | 200 payment summary | ID (ownership in claim predicate `:58–63`); offer price from DB (`:105–115`); Serializable | WAL-04 (Serializable deadlock exposure), WAL-05 |
| 47 | `/api/ArtisanCashPayments/{paymentId}/confirm-received` | POST | ArtisanCashPaymentsController.cs:30 | JWT | Artisan | — | 200 | artisan Active+Approved (`:49–68`); owns accepted offer (`:85–96`); Cash + dual-confirm state machine | `✓` |
| 48 | `/api/ArtisanOffers/requests/{requestId}/offers` | POST | ArtisanOffersController.cs:31 | JWT | Artisan | `CreateServiceOfferDto` (`Price` 0.01–1e9) | 201 | Active+Approved (`:45–64`); open `PendingOffers` request (`:84–90`); one-offer-per-artisan (`:92–104`) | BR-02 (no per-artisan max exposure; price DTO max very high) |
| 49 | `/api/ArtisanOffers/my-offers` | GET | ArtisanOffersController.cs:180 | JWT | Artisan | — | own offers | ID (`:194`) | `✓` |
| 50 | `/api/ArtisanOffers/requests/{requestId}/start` | POST | ArtisanOffersController.cs:220 | JWT | Artisan | — | 200 | owns accepted offer (`:260–270`) + conditional state update (`:282–294`) | `✓` |
| 51 | `/api/ArtisanOffers/requests/{requestId}/finish` | POST | ArtisanOffersController.cs:341 | JWT | Artisan | — | 200 | same (`:381–415`) | `✓` |

### 1.3 Admin surface (all class-level `[Authorize(Roles = "Admin")]`; all money endpoints additionally re-verify an active Admin in DB — `GetActiveAdminIdAsync`)

| # | Endpoint | Method | Controller file | Role | Input DTO | Ownership/DB re-check | Findings |
|---|----------|--------|-----------------|------|-----------|------------------------|----------|
| 52 | `/api/Admin/pending-artisans` | GET | AdminController.cs:25 | Admin | — | admin DB re-verify (`:74–81`, `:91–95`) | `✓` (PII of artisans is admin-appropriate) |
| 53 | `/api/Admin/artisans/{userId}/approve` | PUT | AdminController.cs:50 | Admin | `ApproveArtisanRequest`-shape | profile state gate (`:65–66`) + admin re-verify (`:91–95`) | `✓` (whitelist writes) |
| 54 | `/api/Admin/artisans/{userId}/reject` | PUT | AdminController.cs:120 | Admin | `RejectArtisanRequest` | same (`:163–171`) | `✓` |
| 55 | `/api/Admin/specialties` | GET | AdminSpecialtiesController.cs:25 | Admin | — | — | `✓` |
| 56 | `/api/Admin/specialties` | POST | AdminSpecialtiesController.cs:38 | Admin | `CreateSpecialtyRequest` (in-file DTO) | duplicate check (`:45–54`) | `✓` |
| 57 | `/api/Admin/specialties/{id}` | PUT | AdminSpecialtiesController.cs:75 | Admin | same | duplicate-excluding-self (`:94–104`) | `✓` |
| 58 | `/api/Admin/specialties/{id}/status` | PATCH | AdminSpecialtiesController.cs:117 | Admin | `UpdateSpecialtyStatusRequest` | single whitelisted field (`:133`) | `✓` |
| 59 | `/api/Admin/specialties/{id}` | DELETE | AdminSpecialtiesController.cs:146 | Admin | — | soft-disable (`:160`) | `✓` |
| 60 | `/api/AdminWallet/users/{userId}/test-credit` | POST | AdminWalletController.cs:35 | Admin (**dev-only** `:42–45` returns 404 otherwise) | `AdminTestWalletCreditDto` (Range 1–10000) | target must be active Customer/Artisan (`:71–77`); Serializable | WAL-10 (dev-only; permanent ledger record kept) |
| 61 | `/api/AdminWalletWithdrawals/pending` | GET | AdminWalletWithdrawalsController.cs:31 | Admin | — | admin re-verify (`:35–39`) | `✓` (masked accounts in list) |
| 62 | `/api/AdminWalletWithdrawals/approved` | GET | AdminWalletWithdrawalsController.cs:47 | Admin | — | same (`:51–54`) | `✓` |
| 63 | `/api/AdminWalletWithdrawals/{id}` | GET | AdminWalletWithdrawalsController.cs:63 | Admin | — | admin re-verify (`:68–71`) | `✓` (full payout details, admin-only) |
| 64 | `/api/AdminWalletWithdrawals/{id}/approve` | POST | AdminWalletWithdrawalsController.cs:120 | Admin | `ApproveWalletWithdrawalDto` | `Pending` gate (`:151–160`); no double deduction (`:170–171`) | `✓` |
| 65 | `/api/AdminWalletWithdrawals/{id}/reject` | POST | AdminWalletWithdrawalsController.cs:189 | Admin | `RejectWalletWithdrawalDto` | `Pending` gate (`:229–238`); refund + ledger (`:242–252`) | `✓` (terminal state prevents double refund) |
| 66 | `/api/AdminWalletWithdrawals/{id}/complete` | POST | AdminWalletWithdrawalsController.cs:266 | Admin | `CompleteWalletWithdrawalDto` | `Approved` gate (`:305–314`) | `✓` |
| 67 | `/api/AdminWalletWithdrawals/{id}/fail` | POST | AdminWalletWithdrawalsController.cs:348 | Admin | `FailWalletWithdrawalDto` | `Approved` gate (`:388–397`); refund + ledger (`:401–409`) | `✓` |
| 68 | `/api/AdminPlatformWithdrawals/wallet` | GET | AdminPlatformWithdrawalsController.cs:32 | Admin | — | admin re-verify (`:36–39`) | `✓` |
| 69 | `/api/AdminPlatformWithdrawals/transactions` | GET | AdminPlatformWithdrawalsController.cs:98 | Admin | — | same (`:102–105`) | `✓` |
| 70 | `/api/AdminPlatformWithdrawals/requests` | GET | AdminPlatformWithdrawalsController.cs:149 | Admin | — | same (`:153–156`) | `✓` |
| 71 | `/api/AdminPlatformWithdrawals/pending` | GET | AdminPlatformWithdrawalsController.cs:165 | Admin | — | same (`:169–172`) | `✓` |
| 72 | `/api/AdminPlatformWithdrawals/approved` | GET | AdminPlatformWithdrawalsController.cs:181 | Admin | — | same (`:185–188`) | `✓` |
| 73 | `/api/AdminPlatformWithdrawals/{id}` | GET | AdminPlatformWithdrawalsController.cs:197 | Admin | — | same (`:202–205`) | `✓` (unmasked destination by design, admin-only) |
| 74 | `/api/AdminPlatformWithdrawals` | POST | AdminPlatformWithdrawalsController.cs:257 | Admin | `CreatePlatformWithdrawalDto` | conditional negative-safe debit (`:347–359`) under Serializable | `✓` |
| 75 | `/api/AdminPlatformWithdrawals/{id}/approve` | POST | AdminPlatformWithdrawalsController.cs:453 | Admin | `ApprovePlatformWithdrawalDto` | `Pending` gate (`:484–493`) | `✓` |
| 76 | `/api/AdminPlatformWithdrawals/{id}/reject` | POST | AdminPlatformWithdrawalsController.cs:521 | Admin | `RejectPlatformWithdrawalDto` | `Pending` gate + refund/ledger (`:561–580`) | `✓` |
| 77 | `/api/AdminPlatformWithdrawals/{id}/complete` | POST | AdminPlatformWithdrawalsController.cs:598 | Admin | `CompletePlatformWithdrawalDto` | `Approved` gate (`:637–646`) | `✓` |
| 78 | `/api/AdminPlatformWithdrawals/{id}/fail` | POST | AdminPlatformWithdrawalsController.cs:680 | Admin | `FailPlatformWithdrawalDto` | `Approved` gate + refund/ledger (`:720–739`) | `✓` |
| 79–84 | `/health` (GET, liveness), `/health/ready` (GET, DB connectivity), OpenAPI + Scalar (dev-only, `Program.cs:323–327`) | GET | Program.cs:342–379 | Anon (liveness) / Anon (readiness) | — | — | `✓` (readiness reveals only 200/503, no details) |

Route-counting notes: `/weatherforecast` does **not** exist (the `.http` file still references it — BUG-06). No endpoint uses `FromForm`/`FromQuery` without validation or rate limiting as noted. No `IIS`-style `OPTIONS` overrides; CORS limited to `http://localhost:5173` (`Program.cs:27–36`).

---

## 2. Cross-cutting security controls (as implemented)

| Control | Status | Evidence |
|---|---|---|
| Authentication | JWT Bearer HS256, 1 h, `ClockSkew` 30 s, allowed algorithm pinned | `Program.cs:157–284`, `JwtService.cs:41–92` |
| Per-request account status + role re-validation (fail-closed) | Implemented | `Program.cs:191–283` (`OnTokenValidated`) |
| Rate limiting | 4 named policies (login 5/min, register 5/10 min, OTP request 3/5 min, OTP verify 5/5 min), IP-partitioned; **no default global limiter** — all other endpoints unlimited | `Program.cs:77–155` |
| CORS | Explicit allow-list `http://localhost:5173`, any header/method, no credentials | `Program.cs:27–36, 334` |
| Security headers | `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy` on every response; HSTS production-only | `Program.cs:299–320` |
| Error handling | `UseExceptionHandler` + `AddProblemDetails` → structured ProblemDetails, no stack traces in body | `Program.cs:70–71, 295–296` |
| SQL injection | **No raw SQL anywhere in Controllers**; all data access typed LINQ/EF; raw SQL exists only in migrations (DDL/seed, no user input) | grep evidence across `Controllers/` |
| Mass assignment | No DTO→entity auto-mapping; every entity write is an explicit field whitelist (verified in all controllers) | e.g., `AdminController.cs:102–106`, `AdminWalletWithdrawalsController.cs:164–168` |
| File uploads | Content-sniffed (magic bytes), extension/MIME whitelists, size caps, private storage, GUID names, traversal-guarded deletes, DB-failure cleanup | `PrivateFileStorageService.cs:108–187`, `CustomerController.cs:905–1087`, `AuthController.cs:180–201` |
| Sensitive data exposure | Password hashes never returned; destination accounts masked in all list endpoints; full account numbers admin-detail-only; PII limited to admin endpoints + dev-gated OTP responses | `AdminWalletWithdrawalsController.cs:551–563`, `PaymobWebhookController` (dev-gate absent, HMAC-gated) |
| Webhook authenticity | HMAC-SHA512 over exact Paymob field ordering, fixed-time comparison, integration-id allow-list, amount/currency/state validation, idempotency | `PaymobWebhookController.cs:100–353, 461–484` |

---

## 3. Endpoint-specific findings

| ID | Severity | Confidence | Affected | Finding | Status vs previous audit |
|----|----------|-----------|----------|---------|--------------------------|
| SEC-07A | High in Development / Medium overall | Confirmed (static) | `OtpController.cs:36–50, 144, 150–166, 358–365` | `/api/Otp/request` and `/api/Otp/verify` are fully anonymous and bind a **client-supplied `UserId`**; in Development `/request` returns the plaintext OTP for **any** user id, and `/verify` activates accounts. In Production the request endpoint returns **501** (`:41–47`), so no OTP can be minted — the deployment-level mitigation is a single environment check. | **Still Present** (rate limiting added = improvement over SEC-07) |
| SEC-07B | Low | Confirmed (static) | `OtpController.cs:251–258` vs `:314–318` | Verify responses distinguish "no active code was found" from "Invalid verification code" — probing oracle for whether a user has a live OTP. | **Still Present** |
| SEC-02 | High (functional/product) | Confirmed (static) | `OtpController.cs:40–47`, `AccountController.cs:145–151` | OTP delivery not implemented outside Development → production cannot onboard or verify accounts; new phone-change flow has the same 501 gate. | **Still Present** |
| SEC-06A | Low | Confirmed (static) | `AuthController.cs:61–79` (register), `:140–160` (artisan) | Distinct "email already registered" vs "phone already registered" 409 messages → account-enumeration oracle. Login uses a uniform 401 (correct). | **Still Present** |
| SEC-08 | Low | Confirmed (static) | `DTOs/LoginRequest.cs:11` | `PhoneNumber` on login has no `[Phone]`/length validation (register has both). | **Still Present** |
| API-01 | Medium | Confirmed (static) | `CustomerReviewsController.cs:160, 192–204` | Anonymous artisan-review endpoint returns full display names of other users with **no rate limiting** — an unauthenticated cross-user PII scraping surface. | **New** |
| OTA-01 | Info | Confirmed (static) | `AccountController.cs:145–151, 256`; `OtpController.cs:139–145` | Plaintext `developmentCode` returned on three OTP endpoints — dev-gated (same risk as previous SEC-10; now three surfaces instead of one). | **Still Present / expanded** |
| OTA-03 | Info | Confirmed (static) | `CustomerDashboardController.cs:236, 261`, `CustomerOffersController.cs:74–94` | Private storage keys (`ProfileImagePath`) are exposed in API payloads; inert today (no public serving route) but becomes a functional/ACL issue if a file-serving route is added later. | **New (informational)** |
| WAL-08 | Low | Confirmed (static) | `PaymobWebhookController.cs:41` | Anonymous webhook has no rate limit and parses an unbounded JSON body (Kestrel 30 MB default cap is the only guard). HMAC failure is cheap, but repeated large POSTs can waste CPU/IO. | **New** |
| BR-01 | Low | Confirmed (static) | `AdminSpecialtiesController.cs:167–181` (`SortOrder` int, no Range); `CreateServiceOfferDto.cs:8` (Price max 1e9 EGP) | Several numeric DTOs lack sane upper bounds within the new business modules (no monetary impact; wallet amounts themselves are properly bounded). | **New** |
| BUG-07 | Low | Confirmed (static) | `DTOs/LoginResponse.cs`, `DTOs/RegisterResponse.cs` | Both DTOs still referenced nowhere (grep: definition files only). | **Still Present** |
| BUG-06 | Low | Confirmed (static) | `Sanay3y.API.http:3` | Scratch HTTP file still references removed `/weatherforecast/` route; offers no useful examples for the 84 real endpoints. | **Still Present** |

---

## 4. Missing security controls (module-level)

1. **No global/default rate limiter** — only the 4 named auth policies are limited. All 60+ authenticated endpoints (wallet payments, withdrawals, reviews, notifications, dashboard) accept unlimited request frequency. Recommend a conservative global fallback limiter plus stricter policy on money endpoints (`Program.cs:77–155`).
2. **No API abuse protection on the webhook** (see WAL-08) — consider IP allow-list / Cloudflare-Paymob network restriction + a size cap (`[RequestSizeLimit]`).
3. **No request-logging / audit middleware for money movements** — ledger tables provide a data audit trail (good), but there is no admin/operator action log correlating who did what when beyond `ReviewedByAdminId` columns.
4. **No automated tests** — 0 test files in the repo; nothing guards these 84 endpoints against regressions (see TEST_RESULTS.md).
5. **No CI/CD** — no pipeline runs build, `dotnet list package --vulnerable`, or tests.
6. **No production OTP delivery** — the single largest deployment blocker (SEC-02).
7. **No admin bootstrap path** — `Role == "Admin"` can only be seeded by manual SQL (unchanged from previous audit).
8. **HSTS + HTTPS redirect exist but Kestrel is configured for port 8080 HTTP-only in the container** (`Dockerfile:4`); TLS termination relies on the external proxy — unchanged, needs manual deployment verification.

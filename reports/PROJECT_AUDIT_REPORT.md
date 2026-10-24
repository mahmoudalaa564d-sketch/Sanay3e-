# Sanay3y (Sanay3e-) — Audit & Testing Assessment Report

**Report date:** 2026-10-24
**Auditor scope:** Read-only static review, local Docker build verification, local runtime smoke test. No source files modified, no dependencies changed, no git state changed, no secrets exposed.

---

## 1. Executive Summary

The repository contains a backend-only ASP.NET Core **.NET 10** REST API ("Sanay3y") implementing user registration (customer/artisan), OTP-based contact verification, artisan approval by admin, and JWT-bearer authentication backed by EF Core / SQL Server. No frontend, no test project, and no CI/CD exist yet.

Overall assessment: **well-structured early-stage backend with a solid security foundation** (PBKDF2 password hashing, hashed + fixed-time-compared OTP codes, dev-only plaintext OTP code exposure, confirmed 401 gating on protected endpoints, no committed secrets), but with **two functional contract bugs, missing production-grade abuse protection (rate limiting), an unsalted OTP hash scheme, and an incomplete production verification path** (OTP delivery returns 501 outside Development, so no account can complete activation in production).

- **0** Critical, **2** High, **6** Medium, **8** Low/Informational confirmed-findings-equivalent issues identified in detail below.
- **No known-vulnerable dependency could be confirmed** — the vulnerability scan was blocked (no .NET SDK installed). Package versions pinned are current at the `10.0.13` patch level.
- Build **passed** inside the official `sdk:10.0` container (restore + Release publish, zero compile/type errors).
- Runtime smoke test (local container, throwaway config, no database): authentication gating, dev-only OpenAPI, model validation, and generic production error responses all behaved correctly; an unhandled-exception path returned an **empty-bodied 500**.

---

## 2. Project Overview and Architecture

| Aspect | Value |
|---|---|
| Solution | `Sanay3y.slnx` → single project `Sanay3y.API/Sanay3y.API.csproj` |
| Framework / TFM | ASP.NET Core, `net10.0` (`Sanay3y.API.csproj:4`) |
| Language | C# 13 (Nullable + ImplicitUsings enabled) |
| Data | EF Core 10 preview-stable `10.0.13`, SQL Server (`UseSqlServer`, `Program.cs:41-42`) |
| Auth | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.13), HS256, 1 h tokens |
| API docs | OpenAPI + Scalar, dev-only (`Program.cs:85-89`) |
| Deployment | `Sanay3y.API/Dockerfile` (aspnet:10.0 runtime on port 8080), comment references Render (`Program.cs:33-34`) |
| Testing | **None** (no test project, no frameworks installed) |
| CI/CD | **None** |
| Frontend | **None** |

Monolithic layered layout: `Controllers/` (4), `Services/` (3: `PasswordService`, `JwtService`, `OtpService`), `Data/ApplicationDbContext`, `Models/` (3), `DTOs/` (7), `Migrations/` (2 + snapshot).

Domain flow: `register` / `register-artisan` → account `PendingVerification` → OTP verify (customer → `Active`; artisan → `PendingApproval`) → admin approve (`Active`) or reject (`Suspended`). Login forbids anything ≠ `Active`.

---

## 3. Repository and Git Status

- Branch: `main`, up to date with `origin/main`, working tree **clean**. Unchanged during the audit.
- Commits: `80c8258` (project upload), `3cc7d2f` (initial). The initial commit excludes all `Sanay3y.API` files — `appsettings.json` was added via `.gitignore:9` at commit 1.
- Secret hygiene: `git log --all --name-only` confirms **`appsettings.json`, `appsettings.Development.json`, `.env*`, `*.pfx/p12` were never committed**. Searching the repo for hardcoded connection strings/keys found none. User secrets (`ca68b48b-…`, `Sanay3y.API.csproj:7`) are **not configured locally**.
- `appsettings.json` does **not exist on disk**. `appsettings.example.json` / `.env.example` also absent → no committed template despite the `.gitignore:11` exception line expecting one (**DOC-01**).

---

## 4. Files and Components Reviewed

All 30 non-generated files read in full:

| File | Notes |
|---|---|
| `Program.cs` | config, auth, middleware, OpenAPI gating |
| `Controllers/AuthController.cs` | register / register-artisan / login |
| `Controllers/AccountController.cs` | `/me` claims readout |
| `Controllers/AdminController.cs` | pending-artisans, approve, reject |
| `Controllers/OtpController.cs` | OTP request / verify |
| `Services/JwtService.cs`, `PasswordService.cs`, `OtpService.cs` | token, hashing, OTP |
| `Data/ApplicationDbContext.cs` | constraints, indexes, relationships |
| `Models/*.cs` (3), `DTOs/*.cs` (7) | entities + request models |
| `Migrations/20261016…`, `20261017…_AddArtisanProfilesAndOtp.cs`, snapshot | schema match verified |
| `Dockerfile`, `Properties/launchSettings.json`, `Sanay3y.API.http`, `SQLQuery1.sql`, `.gitignore`, `README.md` | config/docs |

TODO/FIXME/HACK/placeholder scan: **0 hits** in code (the code itself is complete; gaps are at infrastructure level).

---

## 5. Tests Executed and Their Results

| # | Test | Result |
|---|---|---|
| T1 | Docker build `docker build -t sanay3y-api-audit Sanay3y.API` (restore + Release `dotnet publish`, official `sdk:10.0`) | **PASS** (no errors, zero compiler warnings/errors) |
| T2 | Docker build from the repo root, the "natural" invocation `docker build -f Sanay3y.API/Dockerfile .` | **FAIL** — `"/Sanay3y.API.csproj": not found` (see BUG-02) |
| T3 | Container boot in Production env with throwaway config (no DB) | **PASS** — app starts |
| T4 | Unauth GET `/api/Account/me`, `/api/Admin/pending-artisans` | **PASS** — both `401` |
| T5 | GET `/openapi/v1.json` in Production | **PASS** — `404` (dev-gated as intended) |
| T6 | GET unknown route / GET on POST-only routes | **PASS** — `404` / `405`, routing healthy |
| T7 | POST with invalid models (login no password; register short password; register email+phone; OTP channel "Fax") | **PASS** — `400` with per-field messages |
| T8 | POST register, valid shape, unreachable DB | Observed: `500` with **empty body** (see BUG-03); no stack trace/string leakage |
| T9 | Container log review (7 requests) | **PASS** — no secrets/connection strings logged; console exception traces logged for logs only |
| T10 | POST login with valid body | **NOT RUN** — would require a real DB |

Container `sanay3y-audit-smoke` was stopped; image `sanay3y-api-audit` deleted. See `reports/TEST_RESULTS.md` for the same table.

## 6. Build, Lint, and Type-Check Results

- `dotnet build` locally: **BLOCKED** — only the .NET *runtime* 10.0.13 is installed, no SDK (`dotnet --list-sdks` empty).
- Compile/type-check: **PASSED via Docker** (T1 above — `dotnet restore` + `dotnet publish -c Release` include full Roslyn type-checking).
- Lint/formatter: **NOT RUN** — no analyzer/style packages configured in the csproj.
- Dependency vulnerability scan (`dotnet list-package --vulnerable`): **BLOCKED** (needs SDK); see §9.

## 7. Functional Issues and Bugs

### BUG-01 — `/api/Account/me` can never return fullName or email (High, **confirmed by code**)
- `Services/JwtService.cs:52-63` — the token carries only `sub` (`NameIdentifier` after inbound claim mapping) and `role`. There is **no `name`/`unique_name` claim and no `email` claim**, and the email may not even exist (phone-only registers).
- `Controllers/AccountController.cs:21,23` — `fullName = User.Identity?.Name` (needs a `name` claim → always `null`) and `email = User.FindFirstValue("email")` (claim absent → always `null`).
- Impact: a core userinfo endpoint returns `{"id":1,"fullName":null,"email":null,"role":"Customer"}` for every authenticated user — any frontend will display empty user data.
- Remediation: include `ClaimTypes.Name` (or `name`), and the email claim conditionally, in `GenerateToken`, and/or re-read the user from the DB in `/me`.

### BUG-02 — Dockerfile only builds when invoked from inside `Sanay3y.API/` (Medium, **confirmed**, T2)
- `Dockerfile:10,12` — `COPY ["Sanay3y.API.csproj", "./"]` / `COPY . .` assume the build context is the project folder. `docker build -f Sanay3y.API/Dockerfile .` from repo root (and therefore typical CI) fails with `not found` (T2).
- Remediation: either document/pin the context to `Sanay3y.API/` or rewrite paths as `Sanay3y.API/…` with a root `.dockerignore`.

### BUG-03 — Unhandled DB errors produce empty-bodied `500`s (Medium, **confirmed**, T8)
- A valid-shaped duplicate/unreachable-DB registration returns `500` with **no content** — no `ProblemDetails`, no usable message for clients (stack trace goes to the server log only). Root cause: no `UseExceptionHandler` / `AddProblemDetails`, and the register check-then-insert race (below) is never caught.
- Remediation: `builder.Services.AddProblemDetails()` + `app.UseExceptionHandler()`; wrap inserts to translate SQL unique-error 2601/2627 into `409`.

### BUG-04 — Registration duplicate race → 500 instead of 409 (Medium, code-evidenced, not runtime-confirmed)
- `Controllers/AuthController.cs:40-57, 98-115` — `AnyAsync` existence check then `SaveChangesAsync`; two concurrent requests with the same email pass the check and one blows up on the unique filtered indexes (`ApplicationDbContext.cs:66-72`) as BUG-03's empty 500.

### BUG-05 — OTP brute-force guard is racy under concurrent requests (Medium, code-evidenced)
- `Controllers/OtpController.cs:197-206` — `otp.Attempts` is read, checked (`>= 5`), and incremented with a plain `SaveChangesAsync`; no transaction/rowversion. Two parallel verify calls on the fifth attempt can both see `Attempts == 4` and both succeed at guessing, eroding the 5-attempt cap. (`Attempts` also never increments on already-expired codes — minor.)

### BUG-06 — `Sanay3y.API.http` references a removed route (Low, confirmed)
- `Sanay3y.API.http:3` — `GET …/weatherforecast/` returns 404; the file is a leftover template and provides no useful request examples.

### BUG-07 — No naming collision with unused DTOs / dead code (Low)
- `DTOs/LoginResponse.cs`, `DTOs/RegisterResponse.cs` are referenced nowhere; both responses are anonymous objects inline. Confirmed via repo-wide grep.

### BUG-08 — No operational health endpoint (Low)
- No `/health` or similar; Docker/Render health checks (`EXPOSE 8080`, no `HEALTHCHECK` in `Dockerfile`) have nothing to poll for liveness that doesn't touch DB-dependent endpoints.

### Production registration is non-functional by design (High, confirmed)
- `Controllers/OtpController.cs:37-45` — `POST /api/Otp/request` returns **501 "OTP delivery is not configured yet."** outside Development. In a real deployment no account can pass `PendingVerification` (customers) or reach `PendingApproval` (artisans): the entire user lifecycle stops at registration. Email/SMS integration is the missing piece.

### No admin can ever exist via the API (Medium, confirmed)
- `Role` is hard-set to `"Customer"` / `"Artisan"` at registration; **nothing in the codebase sets `Role = "Admin"`**. `AdminController` (`[Authorize(Roles = "Admin")]`) is unreachable until someone runs ad-hoc SQL — an undocumented manual seeding step.

---

## 8. Security Findings Ranked by Severity

| ID | Sev. | Confidence | Location | Finding |
|---|---|---|---|---|
| SEC-01 | **High** | Confirmed (static) | `Program.cs:79-97` | **No rate limiting or abuse prevention anywhere.** `builder.Services.AddRateLimiter` is absent; no middleware or per-endpoint throttle. `POST /api/Auth/login` (`AuthController.cs:154`) permits unlimited password guesses; token issuance `3600 s` and generic `401` messages are fine, but the OWASP brute-force protection layer is missing entirely. |
| SEC-02 | **High** | Confirmed (code) | `OtpController.cs:39-45,145-295` | **OTP delivery unimplemented in production (501)** → while technically a "feature flag", combined with `/verify` remaining open it means the deployed API accepts verification calls for a flow that never worked. Houses of a critical lifecycle half exist in prod with none reachable. |
| SEC-03 | Medium | Confirmed (code) | `Services/OtpService.cs:19-26` | **Unsalted SHA-256 over a 6-digit code.** A stolen `OtpVerifications.CodeHash` row is trivially reversed from ~10⁶ precomputed candidates; no server-side secret/HMAC. Mitigations present: 5-min expiry, hashed not plaintext, `CryptographicOperations.FixedTimeEquals` (good), 5 attempts. Remediation: hash as `HMAC-SHA256(code, serverKey)` or salt per-OTP; never store bare SHA of low-entropy values. |
| SEC-04 | Medium | Confirmed (static) | `Program.cs:91-97` | **No security headers and no HSTS.** `UseHttpsRedirection` only (and with `ASPNETCORE_HTTP_PORTS=8080`/no HTTPS port in the container it silently no-ops; `aspnet:10.0` base has no cert). Missing: `UseHsts`, `X-Content-Type-Options`, CSP, etc. — no `AddHeaderTransfer`/custom middleware. Needs manual verification behind Render's TLS proxy. |
| SEC-05 | Medium | Confirmed (code) | `JwtService.cs:69`; no revocation store | **JWTRole/identity frozen for token lifetime (1 h) with no revocation.** Setting `AccountStatus = "Suspended"` (e.g. `AdminController.cs:180`) does not invalidate an outstanding token — a just-suspended admin/artisan retains full role access up to ~1 h. `ClockSkew` 30 s is tight (good), but consider short tokens + refresh + status check on requests, or a session blocklist. |
| SEC-06 | Low | Confirmed (code) | `AuthController.cs:43-46,52-55` | **Account enumeration at register** via distinct "already registered" 409 messages for email vs phone (100 % deterministic, unlike the login 401 which correctly uses a uniform message — `AuthController.cs:179-196` — so enumeration there is good). Low severity; acceptable if accepted risk. |
| SEC-07 | Low | Confirmed (code) | `OtpController.cs:179-216` | **/api/Otp/verify is completely unauthenticated** and response differs between "no active code" (`179-185`) and "Invalid verification code" (`208-215`) — an attacker can probe whether a user has a live OTP. Requires auth'less design matching registration flow; consider linking verify to a registration-created temp ticket instead of any `UserId`. |
| SEC-08 | Low | Confirmed (code) | `DTOs/LoginRequest.cs:8-11` | Login `PhoneNumber` has no `[Phone]` / `StringLength(20)` unlike register — inconsistent input validation; DB does the normalization for email only (`Trim().ToLowerInvariant()`), phone is trimmed only. |
| SEC-09 | Low | Confirmed (code) | `DTOs/RegisterRequest.cs:22` | **Password policy = MinLength 8 only** — no complexity, no compromised-password check. (PBKDF2 hashing via ASP.NET Identity `PasswordHasher` is solid — `PasswordService.cs:8-13` — and `SuccessRehashNeeded` is accepted as success without a rehash path.) |
| SEC-10 | Info | Confirmed (code) | `OtpController.cs:136-141` | **Plaintext development-only OTP returned in response** (`developmentCode`), properly gated behind `IsDevelopment()`. Risk only if a prod deployment mis-configures the environment var. Keep but log loudly. |
| SEC-11 | Info | Confirmed (static) | `Program.cs` | **No CORS policy configured.** Secure (default = no cross-origin allowed) but when the SPA arrives it will fail — decide policy explicitly. |

**Confirmed NOT vulnerable (with evidence):**
- SQL injection — all queries are typed LINQ/EF; no `FromSqlRaw` anywhere; `SQLQuery1.sql` is a scratch SELECT.
- XSS / output encoding — API returns JSON only; no HTML rendering is possible (any future frontend must encode output itself).
- CSRF — bearer-token auth only, no cookies (`Secure`/`HttpOnly` flags N/A).
- Broken object-level authorization — examined `Admin` approve/reject (admin existence double-check, `AdminController.cs:91-99,163-171` — good defense-in-depth) and `Otp` ask/verify; OTP endpoints are intentionally anonymous-by-design (SEC-07).
- Secrets — none committed (see §2); `Jwt:Key` freshness validated at startup (`Program.cs:20-22`), min 32 bytes enforced (`JwtService.cs:39-43`).
- Sensitive data in logs — production container logs contained no connection strings or password material (`EnableSensitiveDataLogging` never enabled; verified run T9).
- File uploads — none exist.

## 9. Dependency and Configuration Findings

| ID | Sev. | Finding |
|---|---|---|
| DEP-01 | Info | - Vulnerability scan **could not run** (no SDK). Pinned versions reviewed manually: `Microsoft.AspNetCore.*`/`Microsoft.EntityFrameworkCore.*` 10.0.13, `Scalar.AspNetCore` 2.18.0, `System.Security.Cryptography.Pkcs` 10.0.13 — no confirmed present advisories known at time of writing; **not machine-verified** (`dotnet list-package --vulnerable` blocked). |
| DEP-02 | Low | No `packages.lock.json` committed; but every PackageReference is an exact version → reasonable restore reproducibility. |
| CFG-01 | Low | `appsettings.json` missing from both disk and repo with **no committed example** (`CFG-02`/DOC-01) → fresh checkout cannot run (Program intentionally throws, `Program.cs:20-22,38-39`). |
| CFG-03 | Low | No `.dockerignore` (file effectively empty per T2/T1 transfer of `2B`) → local builds ship `bin/`, `obj/` into the context once they exist. |
| CFG-04 | Low | No CI/CD, no DATABASE-migration step bundled, no documented auto-migration — deployment relies on manual `dotnet ef database update`. |

## 10. Missing Tests and Coverage Gaps

- **No test project exists at all** (`dotnet --list-sdks` = none either). Zero coverage of: password/JWT/OTP services, controllers/middleware/auth (401s only verified by manual smoke), registrations/login/OTP lifecycle, admin approve/reject bounds, model-convalidation edge cases, Claims/claimType behavior in BUG-01 (a single integration test would have caught it).
- Recommended (not performed here): xUnit + `WebApplicationFactory` with ESqlite/EF-inmemory unit slice + `Testcontainers` for SQL Server integration.

## 11. Environment and Execution Limitations

1. **.NET SDK missing on host** — all `dotnet` commands blocked (only runtime 10.0.13 installed). Worked around via Docker.
2. **No SQL Server instance / connection string available** — endpoint behavior beyond auth/validation (register success, login, OTP flow, admin approve) could not be exercised; BUG-04's exact duplicate duplicate handling remains runtime-unverified.
3. **No real secrets** (per rules) — JWT key/connection string were throwaway dummy values used only in a local container; no real user data accessed at any point.
4. **Dependency scanner blocked** (DEP-01).
5. Tests deliberately **not started**: no production services/database touched; no destructive migration even if credentials had been available.

## 12. Recommended Fixes Ranked by Priority

| Priority | Action | Linked finding |
|---|---|---|
| P0 | Fix `/api/Account/me` claim bundle (`name` claim, email) | BUG-01 |
| P0 | Add global rate limiting (`AddRateLimiter`/sliding window) — hardest on `login`, `request`, `verify`, `register*(*)` | SEC-01 |
| P0 | Decide + implement real OTP delivery (IEmail/SMS provider + optional dev stub) — otherwise production cannot onboard users | SEC-02 |
| P1 | HMAC/qualify OTP hash with a server secret; fix threadsafety on `Attempts` via `WHERE Attempts < 5` conditional update | SEC-03, BUG-05 |
| P1 | `AddProblemDetails()` + `UseExceptionHandler` + translate unique-index race to `409` | BUG-03, BUG-04 |
| P1 | Dockerfile context (project-dir assumption) + add a real `.dockerignore` | BUG-02, CFG-03 |
| P1 | Headers: `UseHsts` (prod), `X-Content-Type-Options`, CSP; verify HTTPS port/redirects behind Render proxy | SEC-04 |
| P2 | Token revocation/short life + rehash-on-login; roles/claims status re-check | SEC-05, SEC-09 |
| P2 | Admin seeding plan + README quickstart + `appsettings.example.json` + `Sanay3y.API.http` rewrite + health endpoint | BUG-06/08, DOC-01, admin seeding |
| P3 | Consolidate naming (`Role`/`AccountStatus` magic strings → enums/constants), remove dead DTOs, CORS decision, `[Phone]`-style login validation, `CreatedAtAction` location-target fix | BUG-07, SEC-06/08, SEC-11 |

## 13. Remediation Roadmap

### P0 — Critical (immediate)
1. BUG-01 `/me` claims (client-blocking).
2. SEC-01 Authentication and OTP endpoint rate limiting (abuse-prevention backbone).
3. SEC-02 Real OTP channel (blocks all deployments).

### P1 — High
4. SEC-03 OTP hashing to keyed HMAC.
5. BUG-03/BUG-04 global exception handling + 409 translation.
6. BUG-05 attempts race fix.
7. BUG-02/CFG-03 Dockerfile context + `.dockerignore`.
8. SEC-04 HTTPS/HSTS/headers.

### P2 — Medium
9. SEC-05 token lifecycle/revocation.
10. SEC-09 password policy upgrade.
11. Admin bootstrap + docs + template config + BUG-06/08 + DEP-02 baseline.
12. CI pipeline running `dotnet build` + `dotnet list-package --vulnerable` + (new) tests.

### P3 — Low
13. Naming/constant consolidation, dead code, http-file rewrite, login input normalization cleanup, removal of duplicated JWT-config reads (`Program.cs:20-22` vs `JwtService.cs:27-37`).

## 14. Final Checklist

| Check | Status |
|---|---|
| Git state unchanged, no project file modified | **Passed** |
| No secrets committed / leaked (checked) | **Passed** |
| Password hashing (PBKDF2, base `PasswordHasher`) | **Passed** (manual review) |
| JWT ≥ 32-byte key + issuer/audience/lifetime validation (`Program.cs:55-72`) | **Passed** (manual review) |
| Unauth access → 401 on `/me`, Admin endpoints | **Passed** (runtime T4) |
| OpenAPI/Scalar dev-only | **Passed** (runtime T5) |
| Model validation with clean 400 details | **Passed** (runtime T7) |
| Production errors don't leak internals in body | **Passed** (T8/T9 — trace-id only) |
| Docker build (proper context): compile/typecheck | **Passed** (T1) |
| Docker build from repo root | **Failed** — BUG-02 |
| Unhandled production errors return usable error responses | **Failed** — empty 500 (BUG-03) |
| `/api/Account/me` returns caller identity info | **Failed** — always null halves (BUG-01) |
| Rate limiting present | **Failed** — absent (SEC-01) |
| OTP flow end-to-end (incl. in production) | **Blocked** — needs SDK + DB + real email/SMS (SEC-02 fundamental) |
| `dotnet build` / lint / `--vulnerable` scan | **Blocked** — no SDK |
| Unit/integration/regression tests | **Not Run** — none exist |
| Admin ability to exist via normal admin workflow | **Blocked** — requires manual SQL seed (undocumented) |
| Real email/phone-SMS (since 501) and CORS with the future SPA | **Needs Manual Verification** |

## 15. Limitations & Unverified Zone

- Runtime domain behavior (register success, login, OTP verifysuccesses, transition conditions to `PendingApproval`/`Suspended`/`Active`), synchronous concurrency/races, and actual DB constraint effets are **suspected, not runtime-confirmed**, for BUG-04/BUG-05 and any findings rating below confirmed there.
- Dependency CVEs unverified by tooling (DEP-01); security-header activation behind Render TLS unverified (SEC-04).

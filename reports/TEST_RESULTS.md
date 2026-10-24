# Sanay3y Audit — Test Results

Run: 2026-10-24 · Windows host · Docker 29.9.2 · .NET runtime 10.0.13 only (**no SDK**)

| # | Test / Check | Command | Result | Evidence |
|---|---|---|---|---|
| T1 | Compile & Restore (Release) | `docker build -t sanay3y-api-audit Sanay3y.API` | **PASS** | `dotnet restore` + `dotnet publish -c Release` completed, 0 errors/warnings |
| T2 | Docker build from repo root | `docker build -f Sanay3y.API/Dockerfile .` | **FAIL** | `"/Sanay3y.API.csproj": not found` — Dockerfile assumes project dir as context (BUG-02) |
| T3 | App boots in Production env (no DB) | `docker run … ASPNETCORE_ENVIRONMENT=Production` + dummy throwaway config | **PASS** | Container started, listening on 8080 |
| T4 | Unauth GET `/api/Account/me` | GET → | **PASS** | `401` |
| T4 | Unauth GET `/api/Admin/pending-artisans` | GET → | **PASS** | `401` |
| T5 | OpenAPI disabled in Production | GET `/openapi/v1.json` | **PASS** | `404` (dev-gated) |
| T6 | Routing sanity | GET unknown route `404`; GET on POST-only endpoints | **PASS** | `404` / `405` |
| T7 | Model validation | POST login (no password) / register (short pwd) / register (email+phone both) / otp (channel "Fax") | **PASS** | All `400` with per-field messages |
| T8 | Unhandled exception response shape | POST register valid-shaped, DB unreachable | **PASS (no info leak)** / **FAIL (UX)** | `500` with **empty body** — no ProblemDetails (BUG-03) |
| T9 | No secrets logged in Production | `docker logs` after 7 requests | **PASS** | no connection string/password in output |
| T10 | Login with valid credentials | — | **NOT RUN** | requires real SQL Server + data |
| T11 | Registration → OTP → admin approve lifecycle | — | **NOT RUN** | requires SDK + DB + real email/SMS |
| T12 | `dotnet build` / lint local | `dotnet build` | **BLOCKED** | no .NET SDK installed (`dotnet --list-sdks` empty) |
| T13 | Dependency vulnerability scan | `dotnet list-package --vulnerable` | **BLOCKED** | requires SDK |
| T14 | Unit / integration / regression tests | — | **NOT RUN** | none exist in repository |
| T15 | Secrets in git history | `git log --all --name-only` | **PASS** | appsettings/env/keys never committed |
| T16 | TODO/FIXME/placeholder scan | repo grep | **PASS** | 0 hits in code |
| T17 | Container cleanup | `docker stop` + `docker rmi sanay3y-api-audit` | **PASS** | nothing left running behind |

Notes:
- No production or shared systems were touched; the smoke-test container used throwaway credentials and no database connection was ever established.
- All runtime assertions were executed for real; nothing above is assumed.

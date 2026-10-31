# Sana3y Hub — Development & Testing Environment Readiness Assessment

**Assessment date:** 2026-10-31
**Method:** read-only probes only. Nothing was installed, no project file modified, no Git history/working-tree change made (beyond this report file itself), no migrations run, no external services contacted. The `reports/` directory was found to be already committed by the user (commit `ead36b6 reprots`) — the existing audit reports were **not modified**.

---

## 1. Current environment inventory

| # | Component | Probe (executed) | Result |
|---|-----------|------------------|--------|
| 1.1 | .NET host/runtime | `dotnet --info` | **Host 10.0.14** (x64, RID `win-x64`); runtimes: `Microsoft.NETCore.App 10.0.14`, `Microsoft.WindowsDesktop.App 10.0.14`. **"No SDKs were found."** `global.json`: not found. EF tools cannot run (no SDK). |
| 1.2 | TargetFramework | `Sanay3y.API.csproj:3` | `net10.0`; packages all pinned `10.0.13` (EF Core, JwtBearer, OpenApi, Design/Tools, Pkcs) + `Scalar.AspNetCore 2.18.0`. Package ↔ TFM are mutually consistent; **build blocked by missing SDK, not by package mismatch**. |
| 1.3 | Docker Desktop | exe probe + `docker version` + service/pipe check | **Installed** at `C:\Users\ma7moud\AppData\Local\Programs\DockerDesktop\Docker Desktop.exe`. **Daemon unavailable**: `docker version` → *failed to connect* to `dockerDesktopLinuxEngine` pipe (pipe absent); `docker-desktop` WSL distro exists but **State: Stopped (WSL2)**. Previous audit used Docker successfully, so the desktop engine is expected to work once started. |
| 1.4 | SQL Server | service scan (`Get-Service *SQL*` → none), `where.exe sqlcmd` → not found, LocalDB instance dir → absent, TCP `127.0.0.1:1433` → **closed/timeout** | **No SQL Server of any kind available locally.** |
| 1.5 | Existing database configuration | file existence checks (contents **not read**) | `Sanay3y.API/appsettings.json` **missing** (not committed — gitignored; `.dockerignore` also excludes it); **no user-secrets directory** for the project's `UserSecretsId` (`ca68b48b-…`) → `Program.cs:38–55` will **throw at startup**: missing `Jwt:Key/Issuer/Audience` and `ConnectionStrings:DefaultConnection`. |
| 1.6 | Git | `git branch --show-current`, `git log`, `git status --porcelain` | Branch **`main`**, HEAD `ead36b6 reprots`, working tree **clean** (0 modified files). History intact. |
| 1.7 | Test infrastructure | repo-wide glob `*Test*`/`*test*` | **None** — no test project, no framework, no CI. (Matches updated audit finding T7.) |
| 1.8 | NuGet feed reachability | `Invoke-WebRequest https://api.nuget.org/v3/index.json -Method Head` | **HTTP 200** — network path to nuget.org works. **Actual restore untested** (requires SDK). No custom `nuget.config` → default single feed `nuget.org`. |
| 1.9 | Compile ability | (derived from 1.1–1.3) | **BLOCKED today**: no SDK for `dotnet build`; Docker is the only previously-proven compile path but the daemon is down. **No claim of buildability is made.** |
| 1.10 | Node.js (frontend `Sanaeye/`) | `node --version` | v24.22.0 + npm present — SPA tooling ready (`npm ci` would run `eslint`/`vite`; not verified) |
| 1.11 | Machines resources | `Get-PSDrive C`, `Win32_OperatingSystem` | C: **29.8 GB free** (enough for SDK ~1 GB + SQL image ~1.6 GB + build artifacts); RAM 15.7 GB total, **3.8 GB free at probe time** (tight if running Docker + SQL + IDE simultaneously) |
| 1.12 | Ports | TCP listener probe | 5134, 7136, 8080, 5173 all **free** — no conflicts with `launchSettings.json`, SPA dev server, or Docker port mappings |

## 2. Missing prerequisites (ordered)

| Pri | Missing item | Impact | Options to fill (choose one; **requires approval**) |
|---|---|---|---|
| P1 | **.NET SDK 10.0.1xx** | Cannot restore, build, test, or run `dotnet ef` | (A) `winget install Microsoft.DotNet.SDK.10` — or (B) official script `dotnet-install.ps1 -Channel 10.0 -InstallDir …`. The installed runtime 10.0.14 will be reused; a matching-latest SDK (10.0.1xx ≥ 13) is needed for the `10.0.13` packages. |
| P2 | **SQL Server local instance** | App startup + migrations + all integration/wallet testing impossible | (A) **Docker container** `mcr.microsoft.com/mssql/server:2022-latest` (preferred once Docker runs — disposable, matches Testcontainers plans), or (B) SQL Server Express/LocalDB install. EF `UseSqlServer` requires real SQL Server semantics (CHECK constraints, `Serializable`, `ExecuteUpdate`) — **EF InMemory is NOT an acceptable wallet-test substitute**. |
| P3 | **Docker Desktop engine** | Compile-in-container fallback + SQL container + future integration harness | Start Docker Desktop (`"C:\Users\ma7moud\AppData\Local\Programs\DockerDesktop\Docker Desktop.exe"`), verify `wsl -l -v` shows `docker-desktop Running` and `docker info` succeeds. |
| P4 | **Application configuration** | App cannot boot even with SDK+DB | User secrets (already has `UserSecretsId`) or `appsettings.Development.json` (gitignored) or env vars — see §4 key list. |
| P5 | **Test project + CI** | Nothing to run even after P1 | New `tests/` project (xUnit + `WebApplicationFactory` + Testcontainers) — repo modification, needs approval. |

## 3. Compatibility issues

1. **Package/TFM vs SDK:** everything targets `net10.0` with `10.0.13` packages — consistent; any 10.0.1xx SDK (≥ 10.0.13) builds it. The runtime-only install (10.0.14) suggests the SDK was never installed on this machine (previous audit's Docker workaround explains buildability then).
2. **Dockerfile vs engine platform:** `mcr.microsoft.com/dotnet/aspnet|sdk:10.0` are Linux images → WSL2 backend is required (already present, `docker-desktop` distro exists, just stopped). No Windows-container concern.
3. **SQL Server feature dependency:** the wallet module's safety net (DB CHECK constraints, `Balance >= 0`) and its `IsolationLevel.Serializable` / `ExecuteUpdate` concurrency design are **SQL Server behaviors**; `dotnet ef database update` and all wallet tests must run against real SQL Server (container OK).
4. **RAM headroom:** 3.8 GB free may be insufficient to run SDK build + Docker + SQL container + IDE concurrently; close applications or plan a modest WSL memory cap (`%UserProfile%\.wslconfig` → `memory=4GB`) before starting the stack.
5. **CORS/launch ports:** `AllowFrontend` is pinned to `http://localhost:5173` (`Program.cs:32`) — the SPA dev server must use that port (Vite default; port 5173 is free ✓). Backend launch ports 5134/7136 free ✓. Container port 8080 free ✓.
6. **No `global.json`:** after SDK install, pinning one (e.g. `"sdk": { "version": "10.0.1xx", "rollForward": "latestFeature" }`) is recommended for reproducible builds — repo modification, needs approval.

## 4. Required configuration keys (names only — values never displayed, never set, never committed)

All resolved from config by `Program.cs`, `JwtService.cs`, `OtpService.cs`, `WalletTopUpsController.cs`, `PaymobWebhookController.cs`, `WalletPaymentsController.cs`:

| Key | Constraint visible in code | Failure mode if missing |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | startup throw (`Program.cs:51–55`) |
| `Jwt:Key` · `Jwt:Issuer` · `Jwt:Audience` | Key must be **≥ 32 bytes** validated at startup (`Program.cs:38–48`, `JwtService.cs:41–45`) | startup throw |
| `Otp:HashKey` | **≥ 32 bytes**, validated in ctor (`OtpService.cs:18–31`) | startup throw / endpoint failures |
| `Paymob:SecretKey` · `Paymob:PublicKey` · `Paymob:HmacSecret` | non-empty (`WalletTopUpsController.cs:91–99`, `PaymobWebhookController.cs:45–56`) | top-up creation → 503, webhook → 503 (fail-safe) |
| `Paymob:NotificationUrl` | absolute **HTTPS** URI (`:94–99`) | top-up creation → 503 |
| `Paymob:RedirectionUrl` | optional; if present must be HTTPS (`:209–224`) | top-up creation → 503 |
| `Paymob:IntegrationIds[]` | ≥ 1 positive int | top-up creation / webhook → 503 |
| `PlatformCommission:Rate` | optional; default `10`; must parse ≥0 and <100 (`WalletPaymentsController.cs:500–518`) | wallet payments → 500 |

Local env-var equivalents use double underscore (`Jwt__Key`, `Paymob__IntegrationIds__0`, …). Secrets must go into **user secrets** (`dotnet user-secrets set "Jwt:Key" "<value>"` — placeholder shown; never paste real values into the repo) or `appsettings.Development.json` (verify it stays in `.gitignore` — it does).

## 5. Exact commands to prepare the environment (pending your approval — nothing executed)

```powershell
# ---- P1: .NET SDK (choose A or B) ----
# A) winget
winget install Microsoft.DotNet.SDK.10
# B) install script (no admin needed, per-user)
Invoke-WebRequest https://dot.net/v1/dotnet-install.ps1 -OutFile "$env:TEMP\dotnet-install.ps1"
& "$env:TEMP\dotnet-install.ps1" -Channel 10.0 -InstallDir "$env:USERPROFILE\dotnet-sdk"
# (B) requires adding to PATH before use

# Verify
dotnet --list-sdks          # expect a 10.0.1xx SDK
dotnet --info

# ---- P3: Docker engine ----
Start-Process "C:\Users\ma7moud\AppData\Local\Programs\DockerDesktop\Docker Desktop.exe"
# wait for tray → then verify:
docker version              # expect server+client
wsl -l -v                   # docker-desktop → Running, version 2

# ---- P2: SQL Server (option A: container, after Docker runs) ----
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<STRONG-PASSWORD>" `
  -p 1433:1433 --name sanay3y-sql -d mcr.microsoft.com/mssql/server:2022-latest
docker ps                   # status Up

# ---- P2 (option B: LocalDB, if container route not wanted) ----
winget install --id Microsoft.SqlServer.2019.LocalDB   # via winget/VS-installer; verify:
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION"

# ---- P4: configuration secrets (enter values interactively; never echoed/committed) ----
cd "D:\vs code files\Sanay3e-\Sanay3y.API"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<local-connstring>"
dotnet user-secrets set "Jwt:Key" "<>=32 chars>"; dotnet user-secrets set "Jwt:Issuer" "<issuer>"
dotnet user-secrets set "Jwt:Audience" "<audience>"
dotnet user-secrets set "Otp:HashKey" "<>=32 chars>"
dotnet user-secrets set "PlatformCommission:Rate" "10"
# Paymob keys only needed for top-up/webhook tests:
dotnet user-secrets set "Paymob:SecretKey" "<...>" ; dotnet user-secrets set "Paymob:PublicKey" "<...>"
dotnet user-secrets set "Paymob:HmacSecret" "<...>"; dotnet user-secrets set "Paymob:NotificationUrl" "https://<local-tunnel-or-host>/api/PaymobWebhook/transaction-processed"
dotnet user-secrets set "Paymob:RedirectionUrl" "https://<frontend-host>"

# ---- 9/10: restore + compile verification (first real proofs) ----
dotnet restore Sanay3y.API/Sanay3y.API.csproj
dotnet build Sanay3y.API/Sanay3y.API.csproj -c Debug --no-restore   # must succeed before claiming buildability

# ---- schema (creates/updates the local database ONLY) ----
dotnet ef database update --project Sanay3y.API -o Migrations   # after `dotnet tool install --global dotnet-ef` if absent

# ---- run locally ----
dotnet run --project Sanay3y.API    # then GET /health and /health/ready
docker build -f Sanay3y.API/Dockerfile .   # repo-root context build (was fixed in audit)
```

## 6. Recommended testing architecture

| Layer | Tooling | Why |
|---|---|---|
| Unit (pure logic) | xUnit 3 + NSubstitute; real `OtpService` (keyless-config injected test secret), JwtService, wallet math/commission computations | No DB; fast; covers rounding, HMAC, config validation |
| Persistence/concurrency | **Testcontainers: `mcr.microsoft.com/mssql/server:2022`** with real migrations applied per test-class | The wallet's guarantees (CHECK `Balance >= 0`, conditional `ExecuteUpdate`, `Serializable`, deadlock behavior) are **SQL Server semantics** — InMemory fakeres would give false confidence |
| API integration | `WebApplicationFactory<Program>` + Testcontainers SQL; bind user-secrets/test config via `IWebHostEnvironment` overrides | End-to-end auth/roles/ownership + wallet flows (WT-01…WT-16 from the audit as xUnit cases) |
| External provider | **No real Paymob**: fake webhook posts with locally-computed HMACs using a test `HmacSecret` (the controller reads secret from config → fully testable offline) | Verifies WAL-09-adjacent code without contacting Paymob |
| E2E smoke | `docker compose` (api + sqlserver) + scripted curl/`Invoke-WebRequest` of `/health`, register→login; optional Playwright for SPA | Deployment-shaped verification |
| CI (later) | GitHub Actions: `dotnet build`, `dotnet test`, `dotnet list package --vulnerable`, Docker build | Replaces the manual probes that keep getting blocked |

## 7. Step-by-step setup plan (for approval)

1. **Approve P1 → install .NET SDK 10.0.1xx**; verify `dotnet --list-sdks` non-empty.
2. **Start Docker Desktop**; verify daemon (`docker version`) and `docker-desktop` WSL distro Running.
3. **Create local SQL Server** (container preferred); record connection string locally only.
4. **Configure secrets** via `dotnet user-secrets` (interactive values; nothing displayed/committed).
5. **`dotnet restore`** — first live proof of NuGet feasibility (feed already reachable: HTTP 200).
6. **`dotnet build`** — only after this succeeds do we legally state "project is buildable" (until then: *unverified*).
7. **`dotnet ef database update`** against the local DB only.
8. **`dotnet run`** → probe `/health`, `/health/ready`, register → app works end-to-end locally.
9. **`docker build -f Sanay3y.API/Dockerfile .`** from repo root → container parity proof.
10. **(Second approval) scaffold `tests/`** with xUnit + WebApplicationFactory + Testcontainers; port audit test cases WT-01…WT-16 into automated tests; wire CI.

## 8. Honest status statement

- **Buildable: UNVERIFIED.** No SDK/Docker/MSBuild available today; no build has been executed on the current tree. The last verified compile predates ~31,000 newly added lines.
- **Database: NONE available.** No service, no LocalDB, no 1433 listener; connection string absent (no config exists to read — no secrets were displayed or probed beyond existence checks).
- **What IS proven:** runtime 10.0.14 present; TFM/packages consistent; nuget.org reachable; Docker Desktop installed with WSL2 backend present (stopped); git clean on `main` with audited reports committed as `ead36b6`; Node 24 ready for the SPA; required configuration keys enumerated with their in-code constraints.

**Nothing was installed or changed during this assessment.** Awaiting your approval for §5 step 1 onward before any installation or repository modification.

# Sana3y Hub — Test Results (Updated Audit)

**Audit date:** 2026-10-31 · No source files modified; no dependencies installed; no git history changed; no production systems or payment providers contacted.

---

## 1. Environment probe (executed during this audit)

| Probe | Command | Result |
|---|---|---|
| .NET SDK present? | `dotnet --list-sdks` | **Empty output** → no SDK installed; only runtime `10.0.14` and `Microsoft.WindowsDesktop.App 10.0.14` (`dotnet --list-runtimes`) |
| Docker available? | `docker version --format ...` | **FAIL** — `failed to connect to the docker API at npipe:////./pipe/dockerDesktopLinuxEngine` (daemon not running; Docker Desktop installed but engine pipe absent). `Start-Service com.docker.service` → service not loadable / pipe still absent. |
| MSBuild (VS Build Tools)? | vswhere + `C:\Program Files\dotnet\sdk` probe | **Not present** (vswhere not found; no `dotnet\sdk` directory) |
| Git state | `git status --porcelain` | Clean before audit; **remains clean** — audit created only `reports/UPDATED_AUDIT/*` (new files) |

**Consequence:** compile, package-restore-dependent scans, and runtime/socket-level tests were **not runnable in this environment**. Prior audit reached the same conclusion (its build worked via Docker; Docker is now unavailable).

---

## 2. Executed checks and outcomes

| # | Check | Type | Result | Evidence |
|---|---|---|---|---|
| T1 | Full-source read of all 22 controllers, 18 new/changed models, wallet DTOs, `Program.cs`, `ApplicationDbContext.cs`, `JwtService.cs`, `OtpService.cs`, `Dockerfile`, `.dockerignore`, `csproj` | Static review | **PASS** (analysis performed) | File-by-file citations throughout the audit reports |
| T2 | Endpoint inventory reconstruction (84 endpoints) | Static review | **PASS** — every listed route traced to a `[Http*]` attribute | API_SECURITY_MATRIX.md §1 |
| T3 | Secret-hygiene scan (`secret|password|apikey|api_key|token\s*=\s*"..."`) across source | Static scan | **PASS — no hardcoded secrets** | grep returned no matches; `appsettings.json` never committed (gitignore + git history check in previous audit, unchanged) |
| T4 | Docker build / `dotnet build` compile + type-check | Build | **BLOCKED** — no SDK, no Docker daemon | Environment probe §1 (T1 above) |
| T5 | `dotnet list package --vulnerable` | Dependency scan | **BLOCKED** — requires restore/SDK | Same |
| T6 | Lint / analyzer run | Static analysis tooling | **NOT RUN** — no analyzer packages configured in `Sanay3y.API.csproj`; no external tool invocation without approval | csproj review |
| T7 | Unit / integration / API tests | Test execution | **NOT POSSIBLE — no test project exists** (glob for `*Test*`/`*test*` found no test assemblies) | Repo-wide glob |
| T8 | Pinned package versions sanity check | Manual review | **PASS (manual)** — all `Microsoft.*` fixed at `10.0.13`, `Scalar.AspNetCore 2.18.0` (unchanged from previous audit; manual review only, not CVE-scanner-verified) | csproj |
| T9 | Regression re-verification of previous-audit findings | Static review vs `reports/PROJECT_AUDIT_REPORT.md` recovered from commit `59994ef` | **PASS** — 16 findings re-checked with file:line evidence | FULL_PROJECT_AUDIT.md §5 |
| T10 | Wallet business-logic safe test cases WT-01 … WT-16 | Static/behavioral trace | **PASS by code-path analysis** (no runtime execution available) | WALLET_SECURITY_AUDIT.md §9 |
| T11 | Git-history change discovery since baseline `59994ef` | `git diff --name-status`, `git log --diff-filter=A -- "*Wallet*"` | **PASS** — 85 files changed (+31,200 / −89): 16 new API controllers, 18 new models, 12 new migrations, frontend dir added, old reports removed | FULL_PROJECT_AUDIT.md §2 |

## 3. Summary table

| Category | Passed | Failed | Blocked | Not run / Not possible |
|---|---|---|---|---|
| Static review & scans | 5 (T1, T2, T3, T8, T9) | 0 | 0 | 0 |
| Build & compile | 0 | 0 | 2 (T4, T5) | 0 |
| Tooling | 0 | 0 | 0 | 1 (T6) |
| Automated tests | 0 | 0 | 0 | 1 (T7 — none exist) |
| Wallet logic cases | 16 (analysis-mode) | 0 | 1 (WAL-09 external) | 0 |

## 4. Evidence constraints & honesty notes

- Every "PASS by analysis" verdict in WALLET_SECURITY_AUDIT.md §9 reflects **code-path tracing, not runtime execution**. The wallet module has **zero automated test coverage**; these behaviors are unverified at runtime.
- The **Paymob integration is unverifiable offline** (WAL-09): HMAC field-ordering conformance, checkout URL behavior, and webhook delivery were deliberately not exercised. Nothing may be claimed about them beyond implementation reading.
- Compilability of the current tree (including all wallet code) is **unconfirmed in this environment** — the last verified compile evidence available is the previous audit's Docker build (commit `59994ef` era) and the project has since added ~31,000 lines.
- No test was skipped for convenience: each Blocked/Not-run row cites a concrete environmental cause.

## 5. Testing limitations

1. No .NET SDK / Docker daemon / MSBuild → build, unit tests, integration tests, Fuzz-style endpoints probing all impossible without user approval to install/start tooling.
2. No SQL Server instance / connection string → no DB-level concurrency, deadlock, or CHECK-constraint verification.
3. No Paymob sandbox credentials → external provider behaviors unverifiable.
4. No test framework/config in repo → even with an SDK, nothing to run.

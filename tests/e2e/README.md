# End-to-end tests

Playwright/TypeScript smoke tests for this repository's generated DancingGoat sample.
`live-site` checks the public home page; `admin-setup` signs in once and saves a local
session; `admin-tests` checks the authenticated administration shell. Admin selectors
are isolated in `admin/auth.ts` and `admin/adminShellPage.ts`.

## Local run

Requires Node 24, a populated DancingGoat database, a license for `localhost`, and a
test administrator. See [setup](../../docs/Contributing-Setup.md). Start `aspire run`
first so Development Proxy mode has the admin webpack server. Alternatively start
the client and sample separately. These tests do not create a database or license.

```powershell
Set-Location tests\e2e
npm ci
npm run install:browsers
npm run typecheck
npx playwright test --list
$env:XPERIENCE_ADMIN_PASSWORD = '<local test administrator password>'
npm test
npm run test:live-site
npm run test:admin
npm run report
```

The local runner reuses an existing site or starts DancingGoat with `dotnet run`
(Debug). CI starts its own Release app with the admin assets embedded.
`/admin` is the readiness URL: a live-site license failure must fail the actual
home-page test, not be mistaken for a server-start timeout.

## Configuration

| Variable | Default |
| --- | --- |
| `DANCING_GOAT_BASE_URL` | `DancingGoat` launch profile URL (`http://localhost:18319`) |
| `ADMIN_BASE_URL` | `<base URL>/admin` |
| `XPERIENCE_ADMIN_USERNAME` | `administrator` |
| `XPERIENCE_ADMIN_PASSWORD` | Required for admin tests; no assumed local password |
| `E2E_WEB_SERVER_COMMAND` | `dotnet run --project "<absolute DancingGoat path>"` |
| `E2E_WEB_SERVER_TIMEOUT_MS` | `180000` |

## CI and artifacts

`.github/workflows/ci.yml` always runs `npm ci`, type checking, and test discovery.
The browser job uses disposable SQL Server/Mailpit services and the existing
dbmanager tool, not repository-template setup scripts. It requires the optional
`XPERIENCE_LICENSE_KEY` repository secret (also a Dependabot secret for its PRs).
Use a license valid for `localhost`; forks/missing secrets explicitly skip browsers.
Never use `pull_request_target` to expose the license to untrusted PR code.

CI does not upload authenticated browser artifacts in this public repository.
Local HTML reports, traces, screenshots, and `playwright/.auth/admin.json` are
ignored; they can contain credentials or signed-in content. Do not commit or attach
them without review. Test state is regenerated on each run.

These are baseline smoke tests, not Component Registry feature coverage. Add
registry-specific tests when implementing or changing registry behavior.

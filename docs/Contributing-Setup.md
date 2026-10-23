# Contributing Setup

## Required Software

The requirements to setup, develop, and build this project are listed below.

### .NET Runtime

.NET SDK 10.0 or newer

- <https://dotnet.microsoft.com/en-us/download/dotnet/10.0>
- See `global.json` file for specific SDK requirements

### Node.js Runtime

- [Node.js](https://nodejs.org/en/download) LTS or newer
- [NVM for Windows](https://github.com/coreybutler/nvm-windows) or [NVM for macOS](https://github.com/nvm-sh/nvm) to manage multiple installed versions of Node.js
- See `engines` in the solution `package.json` for specific version requirements

### Local services

- SQL Server 2019 or newer (SQL Server 2022 for the optional container).
- [Aspire CLI](https://aspire.dev/get-started/install-cli/) 13.6 or newer. The AppHost
  SDK and integration versions are pinned in source; the CLI alone is not enough.
- Docker Desktop with Linux containers for managed SQL/Mailpit containers.
- [Mailpit](https://mailpit.axllent.org/) for local system email capture.
- Chromium installed by Playwright for E2E tests.

## Sample Project

### Database Setup

The checked-in sample is generated from Xperience 31.9.2. It already references the
Component Registry libraries and enables Management API/MCP only in Development.
No template generation or repo-template setup scripts are needed.

If you already have a populated database, keep its connection string and matching
`CMSHashStringSalt`. Do not recreate it or replace its salt. Set machine-specific
values with `dotnet user-secrets set --project examples\DancingGoat <key> <value>`.
Never commit a real password or license; checked-in local development settings are
not deployment credentials.

For a **new** database, use the sample's own dbmanager tool, following the official
[database installation guidance](https://docs.kentico.com/documentation/developers-and-admins/installation#create-the-project-database).
Run this in a PowerShell session after setting `XPERIENCE_SQL_SERVER` (for example
`localhost,1433`), `XPERIENCE_SQL_PASSWORD`, `XPERIENCE_ADMIN_PASSWORD`, and
`XPERIENCE_LICENSE_FILE` to your chosen server, SQL `sa` password, new test admin
password, and license file path. Use a license valid for `localhost`.

```powershell
Push-Location examples\DancingGoat
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'Tool restore failed.' }
$settingsPath = Join-Path $PWD 'appsettings.json'
$original = [IO.File]::ReadAllText($settingsPath)
try {
    dotnet kentico-xperience-dbmanager `
        --server-name $env:XPERIENCE_SQL_SERVER `
        --database-name 'xperience-by-kentico-component-registry' `
        --username sa --password $env:XPERIENCE_SQL_PASSWORD `
        --admin-password $env:XPERIENCE_ADMIN_PASSWORD `
        --sample-website-domain 'localhost:18319' `
        --sample-email-service-domain 'localhost:18319' `
        --license-file $env:XPERIENCE_LICENSE_FILE
    if ($LASTEXITCODE -ne 0) { throw 'Database creation failed.' }
    $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
    dotnet user-secrets set 'ConnectionStrings:CMSConnectionString' $settings.ConnectionStrings.CMSConnectionString
    if ($LASTEXITCODE -ne 0) { throw 'Saving connection string failed.' }
    dotnet user-secrets set 'CMSHashStringSalt' $settings.CMSHashStringSalt
    if ($LASTEXITCODE -ne 0) { throw 'Saving salt failed; retain the generated salt securely before closing this session.' }
}
finally {
    [IO.File]::WriteAllText($settingsPath, $original)
    Pop-Location
}
```

Dbmanager generates the salt; the commands move both values into user secrets and
restore the tracked settings. If saving secrets fails, `$settings` retains the
generated values in that session for recovery. Never pass
`--recreate-existing-database` without explicitly choosing to destroy that database.
For Windows authentication or a non-`sa` login, use the dbmanager options documented
by `dotnet kentico-xperience-dbmanager --help` from the sample directory.

Without a license, database installation is possible but live-site E2E tests and
custom administration applications cannot be verified. Add the license through
administration before running the full suite. The command sets the sample channel
domains. For other channels on a new database, set website
channel domains and email channel service domains to `localhost:18319` (no scheme)
in administration or using the configured Management MCP; do not change shared
database domains without agreement.

### Run with Aspire

From the root:

```powershell
dotnet restore --locked-mode
aspire run
```

The dashboard links DancingGoat (`http://localhost:18319`), administration (`/admin`),
and the Mailpit inbox. Aspire also installs npm dependencies and starts the admin
client dev server in `src\Kentico.Xperience.Labs.ComponentRegistry.Admin\Client`
(port 3045), matching the sample's existing Development Proxy settings. No shared
services are stopped or recreated.

By default SQL uses DancingGoat's existing connection string/user secrets. Mailpit
is an **external**, machine-wide service on SMTP 1025 and UI 8025. If those ports
are unused, start your own Mailpit once:

```powershell
docker run -d --name mailpit --restart unless-stopped `
    -p 127.0.0.1:1025:1025 -p 127.0.0.1:8025:8025 axllent/mailpit:v1.31.6
```

Do not replace a container or listener owned by another project. To use a per-run
Mailpit instead, set `Mail:UseExistingServer` to `false` in **AppHost** user secrets;
Aspire injects its dynamic SMTP endpoint into the sample's existing
`SystemSmtpOptions`. This captures system email only: channel email configuration
and sample application logic remain unchanged.

To opt into an Aspire-managed SQL Server:

```powershell
dotnet user-secrets set --project examples\AppHost 'Sql:UseExistingServer' 'false'
dotnet user-secrets set --project examples\AppHost 'Parameters:sql-password' '<strong local SQL password>'
```

Start `aspire run`; SQL listens on `127.0.0.1,14333` with a persistent data volume.
For first-time setup, stop DancingGoat in the dashboard if it is reporting a missing
database, run the database creation commands above with
`XPERIENCE_SQL_SERVER=127.0.0.1,14333` and the same SQL password, then restart
DancingGoat. Aspire supplies the connection string, but the sample still needs its
matching generated salt in user secrets. There is deliberately no automatic
schema creation, update, or destructive recreation. Keep the SQL password stable
for the persistent volume; Aspire shutdown leaves this persistent container intact.
Set `Sql:UseExistingServer` back to `true` to return to your existing SQL setup.

Plain `aspire run` does not watch C# edits; use
`aspire resource dancinggoat rebuild`. For a user testing alongside you:

```powershell
$env:features__defaultWatchEnabled = 'true'
aspire run
```

The watcher owns builds/output; inspect resource logs rather than launching a
second build. Keep watch preferences out of committed Aspire settings.
`aspire agent init` is optional and can change global agent settings/telemetry
hooks; it is not part of repository setup.

Without Aspire, run `npm ci` (when needed) and `npm start` in the Admin Client
directory, alongside `dotnet run --project examples\DancingGoat` from the root.
The local Management API configuration and MCP port/secret are unchanged.

### Tests and CI

```powershell
dotnet build examples\AppHost\AppHost.csproj --configuration Debug --no-restore
dotnet test tests\Kentico.Xperience.Labs.ComponentRegistry.Tests --configuration Debug --no-restore
dotnet format --no-restore --verify-no-changes
```

Do not run local Release builds: the Admin project invokes `npm ci` and a production
client build. Existing CI continues to validate Release builds and unit tests.
See [E2E instructions](../tests/e2e/README.md) for Playwright installation,
credentials, type checking, discovery, and browser tests.

CI always checks Playwright types/discovery. Browser tests use disposable SQL and
Mailpit services and initialize a new database directly with dbmanager. They need
the optional `XPERIENCE_LICENSE_KEY` repository secret (also a Dependabot secret
for its PRs); forks and runs without it explicitly skip browser execution. No
existing release secrets/pipelines are changed. Authenticated reports/traces are
not uploaded automatically from this public repository.

The Claude `setup-local` and `update-xperience` maintenance skills document setup
and coordinated Kentico updates without importing template automation.

## Development Workflow

1. Create a new branch with one of the following prefixes
   - `feat/` - for new functionality
   - `refactor/` - for restructuring of existing features
   - `fix/` - for bugfixes

1. Run `dotnet format` against the `Kentico.Xperience.Labs.ComponentRegistry.slnx` solution

   > use `dotnet: format` VS Code task.

1. Commit changes, with a commit message preferably following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary) convention.

1. Once ready, create a PR on GitHub. The PR will need to have all comments resolved and all tests passing before it will be merged.
   - The PR should have a helpful description of the scope of changes being contributed.
   - Include screenshots or video to reflect UX or UI updates
   - Indicate if new settings need to be applied when the changes are merged - locally or in other environments
   - Use the `populate-pr-template` agent skill (`.claude/skills/populate-pr-template/SKILL.md`, works with
     both Copilot and Claude) to fill in `.github/PULL_REQUEST_TEMPLATE.md` from git diff and CI evidence
     instead of writing the description by hand.

1. This repository uses `lf` line endings for text files. EditorConfig and Git enforce this on all platforms; no Git line-ending override is needed.

---
name: setup-local
description: Set up this repository's local DancingGoat development environment with SQL Server, Mailpit, Aspire, user secrets, and Playwright.
---

Run from the repository root. Follow `docs/Contributing-Setup.md`; do not assume
repo-template scripts exist here.

1. Check `global.json`, Node 24, SQL Server access, Docker (for containers), and the
   Aspire CLI. Check for existing services before creating containers; never stop
   unrelated services or overwrite an existing database.
2. Choose existing SQL (default) or the optional persistent Aspire SQL container.
   Use a user-supplied password in user secrets. Ask before database creation,
   recreation, or changing credentials; never silently use `--recreate-existing-database`.
3. Ask for a license file path or use `XPERIENCE_LICENSE_FILE`. Treat its contents as
   credentials: do not read, print, or commit them. Without a license, explain that
   live-site tests/custom administration applications cannot be verified.
4. Initialize a new database with the sample's own `kentico-xperience-dbmanager`
   tool, following the documented commands. Let dbmanager create the hash salt;
   keep the salt paired with the database. Move generated settings to DancingGoat
   user secrets and restore tracked appsettings as documented. Do not invent a salt
   for an existing database.
5. Install dependencies only when needed: restore .NET/tools, `npm ci` in the Admin
   Client and `tests\e2e`, then `npm run install:browsers` in `tests\e2e`.
6. `aspire run` starts the site and admin client. Default Mailpit is machine-wide
   on SMTP 1025 / UI 8025; opt into a per-run container via AppHost user secrets.
   Never run `aspire agent init` unless requested: it may modify global settings.
7. Verify the site at `http://localhost:18319`, administration at `/admin`, and
   Mailpit inbox. For a new database, set website channel domains/email service
   domains to `localhost:18319` (no scheme) through administration or configured
   Management MCP, only with the user's permission.
8. Validate with Debug builds, targeted unit tests, E2E typecheck/discovery, then
   browser tests with `XPERIENCE_ADMIN_PASSWORD`. No local Release builds.

Report commands/results, license/database readiness, URLs, and any omitted checks.
Never claim a successful browser run based only on discovery.

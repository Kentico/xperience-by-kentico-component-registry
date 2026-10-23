# Agent instructions

## Development

- Before a commit, format with: `dotnet format --no-restore`
- `examples/` is excluded from formatting by `examples/.editorconfig` (`generated_code = true`)
- The Dancing Goat connection string and management API secret can be committed to source control because they are for local development only.
- Package versions belong in `Directory.Packages.props`; after changing dependencies, regenerate locks with `dotnet restore --force-evaluate`, then verify `dotnet restore --locked-mode`.
- Do not run Release builds locally: the Admin project runs `npm ci`. Use Debug builds and targeted tests. CI retains the existing Release build.

## Local environment and E2E

- `aspire run` from the root starts `examples/AppHost`: DancingGoat and the Component Registry admin webpack server. It reuses configured SQL Server and machine-wide Mailpit by default. See `docs/Contributing-Setup.md` for optional containers and explicit database creation.
- Plain `aspire run` needs `aspire resource dancinggoat rebuild` after C# edits. When a user needs hot reload, set `$env:features__defaultWatchEnabled = 'true'; aspire run`. Do not run competing builds while the watcher owns output; read Aspire resource logs instead.
- Keep `features.defaultWatchEnabled` out of shared Aspire settings. Keep `Aspire.AppHost.Sdk` and Microsoft Aspire package versions aligned when updating.
- The Aspire MCP server in `.mcp.json` can inspect resources, logs, and restarts. Do not run `aspire agent init` unless requested; it can change global agent settings.
- Playwright lives in `tests/e2e`: `npm run typecheck`, `npx playwright test --list`, then `npm test` with a licensed database and `XPERIENCE_ADMIN_PASSWORD`. Never commit `playwright/.auth`, reports, traces, screenshots, or licenses.
- CI always checks E2E types/discovery; browser tests require the optional `XPERIENCE_LICENSE_KEY` repository/Dependabot secret. Missing secrets produce an explicit skip, not browser-test evidence.

## Repository maintenance skills

- Use `.claude/skills/setup-local/SKILL.md` for workstation setup and `.claude/skills/update-xperience/SKILL.md` for coordinated Kentico upgrades.
- `examples/DancingGoat` is generated from the matching Kentico template; preserve its target-specific project references, Management API, Component Registry/MCP startup, ports, and local/CI configuration when refreshing it.
- Do not copy repo-template setup/update scripts or overlay files into this repository. Generated sample instructions remain in `examples/DancingGoat/AGENTS.md`.

## Planning docs (`.agent-resources/`)

- `STATUS.md` – single source for current state (components, open checks, pending decisions). Update after each slice or when an item closes.
- `PLAN.md` – research, design rules, component catalog, decisions, append-only slice log. Don't track status there.
- `implementation-spec-*.md` – scope of each slice.

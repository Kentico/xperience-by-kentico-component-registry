---
name: update-xperience
description: Coordinate Xperience by Kentico NuGet/npm/tool updates and safely refresh this repository's generated DancingGoat sample.
---

This repository is not repo-template: there is no Update-Xperience.ps1 or template
overlay automation. Do not copy those scripts or invent commands invoking them.

1. Check `git status --short`. Do not overwrite existing changes; agree on the
   target Xperience version and review the official changelog for breaking changes.
2. Update all Kentico NuGet versions together in `Directory.Packages.props`, the
   sample dbmanager in `examples\DancingGoat\.config\dotnet-tools.json`, and
   `@kentico/*` dependencies in the Admin Client. Preserve unrelated version pins.
3. If regenerating DancingGoat, generate pristine current/target versions in
   separate repository-local work folders using isolated template hives. Compare
   upstream changes before merging; never replace the generated app blindly.
   Preserve this repository's project references, Component Registry registration,
   Development-only MCP endpoint, Management API setup, central package management,
   port 18319, SMTP/Proxy settings, CI settings, and existing assets.
4. Review generated sample AGENTS/skills and MCP configuration for relevant updates.
   Keep repository-specific instructions and configured ports/secrets; never import
   repo-template overlays, rename logic, or unrelated application customizations.
5. Regenerate npm locks with `npm install --no-audit --no-fund` in changed package
   folders. Restore sample tools, run `dotnet restore --force-evaluate`, then
   `dotnet restore --locked-mode`. Review lock diffs and advisory warnings.
6. Validate Debug builds/targeted unit tests, `dotnet format --no-restore
   --verify-no-changes`, Admin Client/E2E typechecks, and Playwright discovery.
   With a licensed local database, run the E2E suite. Do not run local Release
   builds; CI builds embedded admin assets and runs licensed browser tests.
7. Review README/version guidance and setup docs. Report changed versions,
   generated-file conflicts, preserved customizations, exact validation results,
   and the required database update procedure from the official Kentico docs.

Aspire upgrades are separate from Kentico upgrades: align `Aspire.AppHost.Sdk` and
Microsoft Aspire packages, check MailPit compatibility, review MessagePack/PKCS
pins, regenerate locks, and build the AppHost before changing documented defaults.

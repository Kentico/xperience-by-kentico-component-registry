# Contributing Setup

## Required Software

The requirements to setup, develop, and build this project are listed below.

### .NET Runtime

.NET SDK 10.0 or newer

- <https://dotnet.microsoft.com/en-us/download/dotnet/10.0>
- See `global.json` file for specific SDK requirements

### Node.js Runtime

- [Node.js](https://nodejs.org/en/download) v24 or newer
- [NVM for Windows](https://github.com/coreybutler/nvm-windows) to manage multiple installed versions of Node.js
- See `engines` in `src/Kentico.Xperience.ComponentRegistry.Admin/Client/package.json` for specific version requirements

### C# Editor

- VS Code/VS
- Cursor
- Rider

### Database

SQL Server 2019 or newer compatible database

- [SQL Server Linux](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-setup?view=sql-server-ver15)

### SQL Editor

- VS Code
- MS SQL Server Management Studio

## Sample Project

### Database Setup

The sample uses Xperience by Kentico `31.9.2`. Running it requires creating a new database using the included template; do not reuse the previous sample's database without upgrading it.

The library's NuGet dependencies and administration client's `@kentico` packages also use `31.9.2`. Keep the client package manifest and lockfile aligned with the Xperience version in `Directory.Packages.props`.

Change directory in your console to `./examples/DancingGoat` and follow the instructions in the Xperience
documentation on [creating a new database](https://docs.kentico.com/documentation/developers-and-admins/installation#create-the-project-database).

Run `dotnet tool restore` in the sample directory first to install its matching database manager. Store the connection string and hash salt in user secrets rather than tracked configuration files.

### MCP Setup

The repository root `.mcp.json` configures the documentation, Component Registry, Management API, and Chrome DevTools MCP servers. The sample runs at `http://localhost:18319`; its Component Registry endpoint is `/mcp`.

The Management API is enabled only in development. Set a secret of at least 32 characters in the sample's user secrets and export the same value before starting Copilot:

```powershell
$env:MANAGEMENT_API_SECRET = [System.Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "ManagementApi:Secret" $env:MANAGEMENT_API_SECRET --project .\examples\DancingGoat
copilot
```

Run these commands from the repository root. Keep the secret outside source control and export the same value in later Copilot sessions. See [Configure the Management MCP](https://docs.kentico.com/documentation/developers-and-admins/api/management-api/configure-management-mcp-server).

The sample inherits the library build settings through `examples/Directory.Build.props`, with upstream nullable settings and build analyzers disabled. `examples/.editorconfig` excludes sample C# files from `dotnet format` rewrites.

### Admin Customization

To run the Sample app Admin customization in development mode, add the following to your [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0&tabs=windows#secret-manager) for the application.

```json
"CMSAdminClientModuleSettings": {
   "kentico-xperience-integrations-component-registry-web-admin": {
      "Mode": "Proxy",
      "Port": 3045
   }
}
```

## Development Workflow

1. Create a new branch with one of the following prefixes
   - `feat/` - for new functionality
   - `refactor/` - for restructuring of existing features
   - `fix/` - for bugfixes

1. Run `dotnet format` against the `Kentico.Xperience.ComponentRegistry.slnx` solution

   > use `dotnet: format` VS Code task.

1. Commit changes, with a commit message preferably following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary) convention.

1. Once ready, create a PR on GitHub. The PR will need to have all comments resolved and all tests passing before it will be merged.
   - The PR should have a helpful description of the scope of changes being contributed.
   - Include screenshots or video to reflect UX or UI updates
   - Indicate if new settings need to be applied when the changes are merged - locally or in other environments

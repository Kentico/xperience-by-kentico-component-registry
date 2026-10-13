# Xperience by Kentico Labs: Component Registry

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support) [![CI: Build and Test](https://github.com/Kentico/xperience-by-kentico-component-registry/actions/workflows/ci.yml/badge.svg)](https://github.com/Kentico/xperience-by-kentico-component-registry/actions/workflows/ci.yml)

| Package                                        | NuGet                                                                                                                                                                                                                                 |
| ---------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Kentico.Xperience.Labs.ComponentRegistry       | [![Kentico.Xperience.Labs.ComponentRegistry - NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.Labs.ComponentRegistry.svg)](https://www.nuget.org/packages/Kentico.Xperience.Labs.ComponentRegistry)                   |
| Kentico.Xperience.Labs.ComponentRegistry.Admin | [![Kentico.Xperience.Labs.ComponentRegistry.Admin - NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.Labs.ComponentRegistry.Admin.svg)](https://www.nuget.org/packages/Kentico.Xperience.Labs.ComponentRegistry.Admin) |
| Kentico.Xperience.Labs.ComponentRegistry.MCP   | [![Kentico.Xperience.Labs.ComponentRegistry.MCP - NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.Labs.ComponentRegistry.MCP.svg)](https://www.nuget.org/packages/Kentico.Xperience.Labs.ComponentRegistry.MCP)       |

> [!NOTE]
> Migrating from `Kentico.Xperience.ComponentRegistry*`? These packages were renamed to `Kentico.Xperience.Labs.ComponentRegistry*` (final release under the old IDs: 2.0.0). Version 3.0.0 adds the Automation Builder registry (custom automation triggers, actions, and conditions) and includes breaking API changes. Swap your package references and update `using` statements to `Kentico.Xperience.Labs.ComponentRegistry`.

## Description

This project enables administrators to view all registered custom components in an Xperience by Kentico application, like Page Builder widgets or custom automation steps, and explore which channels, web pages, forms, emails, and automation processes use those components all through a friendly user interface in the Xperience administration.

A **Builder structure** tab on web pages and Email Builder emails visualizes the Page Builder or Email Builder configuration (template, editable areas, sections, zones, widgets, and personalized variants) and flags components that are no longer registered.

It optionally exposes the entire component registry over an in-application hosted MCP server, giving agents the ability to identify where components are used across channels, and even visit Page Builder pages using components to visually validate their configuration and design.

Interested in how and why this library was created? Read the blog post [Dream and Experiment: Building a Component Registry Dashboard with AI](https://community.kentico.com/blog/dream-and-experiment-building-a-component-registry-dashboard-with-ai).

### Screenshots

![Component registry for Page Builder in Xperience administration](https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/refs/heads/main/docs/images/component-registry-admin-page-builder.jpg)

![Component registry for Email Builder in Xperience administration](https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/refs/heads/main/docs/images/component-registry-admin-email-builder.jpg)

![Component registry for Form Builder in Xperience administration](https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/refs/heads/main/docs/images/component-registry-admin-form-builder.jpg)

![Component registry for Automation Builder in Xperience administration](https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/refs/heads/main/docs/images/component-registry-admin-automation-builder.jpg)

![Builder structure tab of a web page in Xperience administration](https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/refs/heads/main/docs/images/component-registry-admin-web-page-builder-structure.jpg)

![Builder structure tab of an email in Xperience administration](https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/refs/heads/main/docs/images/component-registry-admin-email-builder-structure.jpg)

## Requirements

### Library Version Matrix

| Xperience Version | Library Version | Package ID                                  |
| ----------------- | --------------- | ------------------------------------------- |
| >= 31.9.2         | >= 3.0.0        | `Kentico.Xperience.Labs.ComponentRegistry*` |
| >= 31.9.2         | 2.0.0 (final)   | `Kentico.Xperience.ComponentRegistry*`      |
| >= 31.2.1         | 1.0.0 - 1.0.1   | `Kentico.Xperience.ComponentRegistry*`      |

### Dependencies

- [ASP.NET Core 10.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.kentico.com)

## Package Installation

Add these package to your application using the .NET CLI.

The core package adds the integration's registry services.

```powershell
dotnet add package Kentico.Xperience.Labs.ComponentRegistry
```

The admin package adds the component registry administration UI application to the project.

```powershell
dotnet add package Kentico.Xperience.Labs.ComponentRegistry.Admin
```

The MCP project adds Component Registry MCP tools that you can register on your application's MCP server.

```powershell
dotnet add package Kentico.Xperience.Labs.ComponentRegistry.MCP
```

## Quick Start

1. Add the Admin and MCP NuGet packages to your Xperience by Kentico ASP.NET Core application.
1. Register the integration's services, MCP support services, an MCP server and the integration's MCP tools:

   ```csharp
   // ...
   builder.Services.AddComponentRegistry(); // Adds this library's services
   if (env.IsDevelopment())
   {
      builder.Services
        .AddComponentRegistryMcpServices() // Adds MCP support services for URL generation
        .AddMcpServer() // Host application is responsible for adding the McpServer
        .WithHttpTransport()
        .WithComponentRegistryTools(); // Adds this library's MCP tools
   }
   // ...
   ```

1. Enable the MCP server in the middleware pipeline:

   ```csharp
   // ...
   app.Kentico().MapRoutes();

   if (env.IsDevelopment())
   {
      app.MapMcp("/mcp"); // Host application is responsible for adding the endpoint
   }
   // ...
   ```

1. Configure your MCP server for your AI enabled development tool

   ```json
   {
     "servers": {
       "kentico.docs.mcp": {
         "type": "http",
         "url": "https://docs.kentico.com/mcp"
       },
       "your-app": {
         "type": "http",
         "url": "http://localhost:18319/mcp"
       }
     }
   }
   ```

> [!WARNING]
> If used in a production environment, the MCP server exposes draft content data without any authentication. The MCP server feature is **intended for development-environments only**.
>
> Use [environment identification extensions](https://docs.kentico.com/documentation/developers-and-admins/configuration/saas-configuration#environment-identification-extension-methods) or [environment specific settings](https://docs.kentico.com/guides/development/deployment/deploy-to-private-cloud#separate-the-app-settings) to disable the MCP server for non-local deployments.

## Full Instructions

View the [Usage Guide](./docs/Usage-Guide.md) for more detailed instructions on permission management and custom scenarios.

## Contributing

To see the guidelines for Contributing to Kentico open source software, please see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

Instructions and technical details for contributing to **this** project can be found in [Contributing Setup](./docs/Contributing-Setup.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.

## Support

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support)

This project has **Kentico Labs limited support**.

See [`SUPPORT.md`](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support) for more information.

For any security issues see [`SECURITY.md`](https://github.com/Kentico/.github/blob/main/SECURITY.md).

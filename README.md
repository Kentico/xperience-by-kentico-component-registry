# Xperience by Kentico Component Registry

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support) [![CI: Build and Test](https://github.com/Kentico/xperience-by-kentico-component-registry/actions/workflows/ci.yml/badge.svg)](https://github.com/Kentico/xperience-by-kentico-component-registry/actions/workflows/ci.yml)

| Package | NuGet |
| --- | --- |
| Kentico.Xperience.ComponentRegistry | [![Kentico.Xperience.ComponentRegistry - NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.ComponentRegistry.svg)](https://www.nuget.org/packages/Kentico.Xperience.ComponentRegistry) |
| Kentico.Xperience.ComponentRegistry.Admin | [![Kentico.Xperience.ComponentRegistry.Admin - NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.ComponentRegistry.Admin.svg)](https://www.nuget.org/packages/Kentico.Xperience.ComponentRegistry.Admin) |
| Kentico.Xperience.ComponentRegistry.MCP | [![Kentico.Xperience.ComponentRegistry.MCP - NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.ComponentRegistry.MCP.svg)](https://www.nuget.org/packages/Kentico.Xperience.ComponentRegistry.MCP) |
| Future packages (`Kentico.Xperience.ComponentRegistry.*`) | [View all matching packages](https://www.nuget.org/packages?q=Kentico.Xperience.ComponentRegistry) |

## Description

This project enables administrators to view all registered custom components in an Xperience by Kentico application, like Page Builder widgets, and explore which channels and web pages use those components all through a friendly user interface in the Xperience administration.

It optionally exposes the entire component registry over an in-application hosted MCP server, giving agents the ability to identify where components are used across channels, and even visit Page Builder pages using components to visually validate their configuration and design.

Interested in how and why this library was created? Read the blog post [Dream and Experiment: Building a Component Registry Dashboard with AI](https://community.kentico.com/blog/dream-and-experiment-building-a-component-registry-dashboard-with-ai).

### Screenshots

<div style="display: flex; gap: 1rem; flex-wrap: wrap">
  <a href="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/main/images/component-registry-admin-page-builder.jpg">
    <img src="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/main/images/component-registry-admin-page-builder.jpg"
    width="600" alt="Component registry for Page Builder in Xperience administration">
  </a>

  <a href="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/main/images/component-registry-admin-email-builder.jpg">
    <img src="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/main/images/component-registry-admin-email-builder.jpg"
    width="600" alt="Component registry for Email Builder in Xperience administration">
  </a>

  <a href="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/main/images/component-registry-admin-form-builder.jpg">
    <img src="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-component-registry/main/images/component-registry-admin-form-builder.jpg"
    width="600" alt="Component registry for Form Builder in Xperience administration">
  </a>

  </div>

## Requirements

### Library Version Matrix

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 31.2.1         | 1.0.0           |

### Dependencies

- [ASP.NET Core 10.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.kentico.com)

## Package Installation

Add these package to your application using the .NET CLI.

The core package adds the integration's registry services.

```powershell
dotnet add package Kentico.Xperience.ComponentRegistry
```

The admin package adds the component registry administration UI application to the project.

```powershell
dotnet add package Kentico.Xperience.ComponentRegistry.Admin
```

The MCP project adds Component Registry MCP tools that you can register on your application's MCP server.

```powershell
dotnet add package Kentico.Xperience.ComponentRegistry.MCP
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

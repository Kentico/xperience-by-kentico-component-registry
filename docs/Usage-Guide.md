# Usage Guide

## Setup

Add the "Admin" package to your ASP.NET Core application using the .NET CLI. This includes the custom admin UI application and all required services.

```powershell
dotnet add package Kentico.Xperience.ComponentRegistry.Admin
```

If you wish to separately install just the registry services, you can use the following:

```powershell
dotnet add package Kentico.Xperience.ComponentRegistry
```

## Quick Start

Register the library's services in your ASP.NET Core application:

```csharp
// Program.cs

// ...

builder.Services.AddComponentRegistry();

if (builder.Environment.IsDevelopment())
{
  builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithComponentRegistryTools();
}
```

Run the application and navigate to the "Component Registry" application in the Xperience administration under the "Development" category.

You can control access to Component Registry application and each of the 3 registry pages through Xperience's role and permission management.

### MCP endpoint (optional)

MCP support is disabled by default.

Add configuration:

```json
{
  "Kentico": {
    "Xperience": {
      "ComponentRegistry": {
        "Mcp": {
          "AgentAdminUserName": "mcpAgent"
        }
      }
    }
  }
}
```

### MCP agent administration user (required for preview URLs)

The `component_registry_get_web_page_preview_url` tool generates shareable preview links using a real Xperience administration user account.

1. In Xperience administration, create a dedicated administration user for MCP automation (for example `mcpAgent`).
1. Give the user enough permissions to generate preview URLs for all web pages across all channels.
1. Set the same username in configuration under `Kentico:Xperience:ComponentRegistry:Mcp:AgentAdminUserName`.
1. Keep this user dedicated to MCP operations (recommended: do not use a personal account).
1. Do not deploy this account to a non-local environment. It is for development-only workflows.

If this user is missing (or the username is not configured), preview URL generation returns `Success: false` with an actionable message telling you to create or configure the user.

Map endpoint in request pipeline:

```csharp
if (app.Environment.IsDevelopment())
{
  app.MapMcp("/mcp");
}
```

Configure your project's MCP servers using your AI development tool of choice.

Example: VS Code and GitHub Copilot `.vscode/mcp.json`

```json
{
  "servers": {
    "xperience-community.component-registry": {
      "type": "http",
      "url": "http://localhost:53856/mcp"
    }
  }
}
```

> [!WARNING]
> The MCP server exposes component definitions and usage details (including page and content details) without any authentication. The MCP server feature is **intended for development-environments only**.
>
> Use [environment identification extensions](https://docs.kentico.com/documentation/developers-and-admins/configuration/saas-configuration#environment-identification-extension-methods) or [environment specific settings](https://docs.kentico.com/guides/development/deployment/deploy-to-private-cloud#separate-the-app-settings) to disable the MCP server for non-local deployments.

### Permissions

- `View`: Required to see the application tile on the administration dashboard
- `View Page Builder components`: Enables viewing the Page Builder component registry page.
- `View Page Builder component usages`: Gives expanded permissions on the registry page, enabling viewing the usages of individual Page Builder components across all website channels. This permission does not evaluate any [page permission management](https://docs.kentico.com/x/permissions_pagelevel_xp) and could expose content to administration users they normally would not have access to.
- `View Form Builder components`: Enables viewing the Form Builder component registry page.
- `View Form Builder component usages`: Gives expanded permissions on the registry page, enabling viewing the usages of individual Form Builder components across all forms. This permission does not evaluate other roles defined through [role management](https://docs.kentico.com/x/7IVwCg) and could expose a list of forms to administration users they normally would not have access to.
- `View Email Builder components`: Enables viewing the Email Builder component registry page.
- `View Email Builder component usages`: Gives expanded permissions on the registry page, enabling viewing the usages of individual Email Builder components across all email channels. This permission does not evaluate other roles defined through [role management](https://docs.kentico.com/x/7IVwCg) and could expose content to administration users they normally would not have access to.

## MCP Tools Reference

When MCP support is enabled (via `appsettings.json`), the following tools are exposed through an HTTP endpoint to enable AI agents and other clients to discover and interact with component registrations.

### component_registry_list_definitions

Lists registered component definitions for a specific builder and component type.

**Parameters:**

- `builder` (string, required): Builder type - `page`, `email`, or `form`
- `componentType` (string, optional): Component type filter - `widget`, `section`, `template`, `page-template`, `component`, or `all` (default: `all`)

**Returns:** `ComponentDefinitionListResponse`

```json
{
  "Builder": "page",
  "ComponentType": "widget",
  "Items": [
    {
      "Builder": "page",
      "ComponentType": "widget",
      "Identifier": "MyCompany.HeroWidget",
      "Name": "Hero Banner",
      "Description": "Full-width hero banner with image and text",
      "IconClass": "icon-picture",
      "MarkedTypeName": "MyCompany.Components.HeroWidgetViewComponent",
      "PropertiesTypeName": "MyCompany.Components.HeroWidgetProperties",
      "ContentTypeNames": ["MyCompany.Article", "MyCompany.LandingPage"]
    }
  ]
}
```

**Example use case:** Discover all widgets available for Page Builder, or list all email templates registered in the system.

### component_registry_get_usage

Gets detailed usage information for a specific component identifier across all pages, emails, or forms.

**Parameters:**

- `builder` (string, required): Builder type - `page`, `email`, or `form`
- `componentType` (string, required): Component type - `widget`, `template`, `page-template`, `component`, or `section`
- `componentIdentifier` (string, required): The unique identifier of the component

**Returns:** Varies by builder type:

- **Page Builder**: `ComponentUsageDetailDto` - Contains `TotalPagesUsing`, `TotalVariants`, `LastModified`, and list of `Pages` with their `Variants`
- **Email Builder**: `EmailConfigurationUsageDetailDto` - Contains `TotalEmailConfigurationsUsing`, `TotalVariants`, and list of `EmailConfigurations` with their `Variants`
- **Form Builder**: `FormComponentUsageDetailDto` - Contains `TotalFormClassesUsing`, `TotalFormBuilderFormsUsing`, and lists of `FormClasses` and `FormBuilderForms`

Each variant includes:

- `LanguageName`: Display name of the language (e.g., "English (United States)")
- `IsPublished`: Whether the variant is published
- `LastModified`: Last modification date
- `ConfigurationJson`: The component's JSON configuration

**Example use case:** Determine which pages use a specific widget and in which languages, including whether changes are published or in draft.

### component_registry_get_page_batch_usage

Gets page builder usage details for multiple widget or page template identifiers in a single request (batch operation).

**Parameters:**

- `componentIdentifiers` (array of strings, required): List of component identifiers to query
- `componentType` (string, required): Component type - `widget` or `page-template`

**Returns:** `List<ComponentUsageDetailDto>` - Array of usage details, one per identifier

**Example use case:** Retrieve usage statistics for multiple widgets at once to build a dashboard or report showing which components are most frequently used.

### component_registry_get_web_page_url

Gets the absolute published URL for a web page by its ID and language, enabling agents to visit and validate published rendering.

**Parameters:**

- `webPageItemId` (int, required): The web page item ID from usage data (e.g., from `PageUsageDto.WebPageItemId`)
- `languageName` (string, required): Language name (e.g., "en-US")

**Returns:** `WebPageUrlResponse`

```json
{
  "WebPageItemId": 123,
  "LanguageName": "en-US",
  "IsPublished": true,
  "UrlType": "Published",
  "Url": "https://example.com/en-US/page-123",
  "Success": true,
  "ErrorMessage": null
}
```

### component_registry_get_web_page_preview_url

Gets a shareable preview URL for unpublished page changes by web page item ID and language.

**Parameters:**

- `webPageItemId` (int, required): The web page item ID from usage data (e.g., from `PageUsageDto.WebPageItemId`)
- `languageName` (string, required): Language name (e.g., "en-US")

**Returns:** `WebPageUrlResponse`

```json
{
  "WebPageItemId": 123,
  "LanguageName": "en-US",
  "IsPublished": false,
  "UrlType": "ShareablePreview",
  "Url": "https://example.com/preview/abc123def456",
  "Success": true,
  "ErrorMessage": null,
  "PreviewUrlState": "Existing"
}
```

Possible `PreviewUrlState` values:

- `Existing`: A shareable preview URL already existed for the language variant.
- `Generated`: A new shareable preview URL was generated by this call.
- `Removed`: A shareable preview URL was removed by a cleanup call.
- `NotFound`: No shareable preview URL existed to remove.

Example when the configured MCP agent user is missing:

```json
{
  "WebPageItemId": 123,
  "LanguageName": "en-US",
  "IsPublished": false,
  "UrlType": null,
  "Url": null,
  "Success": false,
  "ErrorMessage": "Preview URL requires administration user 'mcpAgent'. Create this user in Xperience administration (or update Kentico:Xperience:ComponentRegistry:Mcp:AgentAdminUserName) to enable preview URL generation.",
  "PreviewUrlState": null
}
```

**URL Types:**

- `Published`: Standard live page URL for published content
- `ShareablePreview`: Temporary preview link for unpublished draft changes

**Example use case:** After discovering a widget is used on specific pages, call `component_registry_get_web_page_url` for published variants and `component_registry_get_web_page_preview_url` for draft variants based on `PageVariantDto.IsPublished`.

### component_registry_remove_web_page_preview_url

Removes the shareable preview URL for a page language variant to clean up preview links generated by prior MCP calls.

**Parameters:**

- `webPageItemId` (int, required): The web page item ID from usage data (e.g., from `PageUsageDto.WebPageItemId`)
- `languageName` (string, required): Language name (e.g., "en-US")

**Returns:** `WebPageUrlResponse`

Example when a preview URL is removed:

```json
{
  "WebPageItemId": 123,
  "LanguageName": "en-US",
  "IsPublished": false,
  "UrlType": "ShareablePreview",
  "Url": null,
  "Success": true,
  "ErrorMessage": null,
  "PreviewUrlState": "Removed"
}
```

Example when there was nothing to remove:

```json
{
  "WebPageItemId": 123,
  "LanguageName": "en-US",
  "IsPublished": false,
  "UrlType": "ShareablePreview",
  "Url": null,
  "Success": true,
  "ErrorMessage": null,
  "PreviewUrlState": "NotFound"
}
```

Example when the configured MCP agent user is missing:

```json
{
  "WebPageItemId": 123,
  "LanguageName": "en-US",
  "IsPublished": false,
  "UrlType": null,
  "Url": null,
  "Success": false,
  "ErrorMessage": "Preview URL cleanup requires administration user 'mcpAgent'. Create this user in Xperience administration (or update Kentico:Xperience:ComponentRegistry:Mcp:AgentAdminUserName) to enable preview URL cleanup.",
  "PreviewUrlState": null
}
```

**Example use case:** After validating draft rendering with `component_registry_get_web_page_preview_url`, call `component_registry_remove_web_page_preview_url` to clean up temporary shareable preview links.

### Complete Workflow Example

1. **Discover components**: Use `component_registry_list_definitions` to find all page widgets
2. **Find usage**: Use `component_registry_get_usage` to see which pages use a specific widget
3. **Get URLs**: Use `component_registry_get_web_page_url` for published variants and `component_registry_get_web_page_preview_url` for unpublished variants
4. **Visit & validate**: Navigate to the returned URLs to inspect component rendering in published or preview mode
5. **Cleanup preview links** (optional): Use `component_registry_remove_web_page_preview_url` for preview URLs you no longer need

This enables AI agents to autonomously discover, analyze, and validate component implementations across your Xperience application.

## Custom use

You can inject `IComponentDefinitionStore<TDefinition>` into your own code to access all the component registrations where `TDefinition` is one of the following types:

- `Kentico.Xperience.ComponentRegistry.PageBuilderWidgetDefinition`
- `Kentico.Xperience.ComponentRegistry.PageBuilderSectionDefinition`
- `Kentico.Xperience.ComponentRegistry.PageBuilderPageTemplateDefinition`
- `Kentico.Xperience.ComponentRegistry.EmailBuilderWidgetDefinition`
- `Kentico.Xperience.ComponentRegistry.EmailBuilderSectionDefinition`
- `Kentico.Xperience.ComponentRegistry.EmailBuilderTemplateDefinition`
- `Kentico.Xperience.ComponentRegistry.FormBuilderComponentDefinition`
- `Kentico.Xperience.ComponentRegistry.FormBuilderSectionDefinition`

Each type has its own Store service.

By default the registry uses assembly scanning through Xperience's `[assembly: AssemblyDiscoverable]` marker attribute for fast identification. The registry will also automatically include the "host" ASP.NET Core assembly's components even if this assembly does not have the attribute.

You can also supply your own list of assemblies to scan for components using the `IServiceCollection` overload:

```csharp
IEnumerable<Assembly> assemblies = [...];

builder.Services.AddComponentRegistry(assemblies);
```

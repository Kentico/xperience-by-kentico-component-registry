using System.ComponentModel;

using ModelContextProtocol.Server;

namespace Kentico.Xperience.ComponentRegistry.MCP;

/// <summary>
/// MCP tools exposing component usage details.
/// </summary>
[McpServerToolType]
public class ComponentRegistryUsageMcpTools(IComponentUsageService componentUsageService)
{
    /// <summary>
    /// Gets usage detail for a specific registered component identifier.
    /// </summary>
    [McpServerTool(Name = "component_registry_get_usage")]
    [Description("Get usage detail for one component by builder/type/identifier. builder: page|email|form. type: widget|template|page-template|component|section")]
    public async Task<object> GetComponentUsage(
        string builder,
        string componentType,
        string componentIdentifier,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string normalizedBuilder = ComponentRegistryMcpToolsValidation.NormalizeBuilder(builder);
        string normalizedType = ComponentRegistryMcpToolsValidation.NormalizeComponentType(componentType);

        if (string.IsNullOrWhiteSpace(componentIdentifier))
        {
            throw new ArgumentException("Component identifier is required.", nameof(componentIdentifier));
        }

        return (normalizedBuilder, normalizedType) switch
        {
            ("page", "widget") => await componentUsageService.GetPageBuilderWidgetUsageAsync(componentIdentifier),
            ("page", "page-template") => await componentUsageService.GetPageBuilderPageTemplateUsageAsync(componentIdentifier),
            ("email", "widget") => await componentUsageService.GetEmailBuilderWidgetUsageAsync(componentIdentifier),
            ("email", "template") => await componentUsageService.GetEmailBuilderTemplateUsageAsync(componentIdentifier),
            ("form", "component") => await componentUsageService.GetFormBuilderComponentUsageAsync(componentIdentifier),
            ("form", "section") => await componentUsageService.GetFormBuilderSectionUsageAsync(componentIdentifier),
            _ => throw new ArgumentException($"Unsupported usage query for builder '{builder}' and componentType '{componentType}'.")
        };
    }

    /// <summary>
    /// Gets page builder usage details for many widget/template identifiers.
    /// </summary>
    [McpServerTool(Name = "component_registry_get_page_batch_usage")]
    [Description("Get page builder batch usage. componentType: widget|page-template. identifiers: list of component IDs.")]
    public async Task<List<ComponentUsageDetailDto>> GetPageBuilderBatchUsage(
        List<string> componentIdentifiers,
        string componentType,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (componentIdentifiers is null || componentIdentifiers.Count == 0)
        {
            throw new ArgumentException("At least one component identifier is required.", nameof(componentIdentifiers));
        }

        string normalizedType = ComponentRegistryMcpToolsValidation.NormalizeComponentType(componentType);
        string usageType = normalizedType switch
        {
            "widget" => "Widget",
            "page-template" => "PageTemplate",
            _ => throw new ArgumentException("componentType must be widget or page-template.", nameof(componentType))
        };

        return await componentUsageService.GetBatchUsageAsync(componentIdentifiers, usageType);
    }
}

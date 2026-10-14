using System.ComponentModel;

using ModelContextProtocol.Server;

namespace Kentico.Xperience.Labs.ComponentRegistry.MCP;

/// <summary>
/// MCP tools exposing component definition listings.
/// </summary>
[McpServerToolType]
public class ComponentRegistryDefinitionMcpTools(IComponentRegistryReadService readService)
{
    /// <summary>
    /// Lists registered component definitions for a builder and component type.
    /// </summary>
    [McpServerTool(Name = "component_registry_list_definitions")]
    [Description("List registered component definitions for page/email/form builders, Admin UI form components, and custom automation steps. builder: page|email|form|admin-form|automation. componentType: widget|section|template|page-template|component|trigger|action|condition|all")]
    public async Task<ComponentDefinitionListResponse> ListComponentDefinitions(
        string builder,
        string componentType = "all",
        CancellationToken cancellationToken = default)
    {
        string normalizedBuilder = ComponentRegistryMcpToolsValidation.NormalizeBuilder(builder);
        string normalizedType = ComponentRegistryMcpToolsValidation.NormalizeComponentType(componentType);

        var items = normalizedBuilder switch
        {
            "page" => await ListPageBuilderDefinitions(normalizedType, cancellationToken),
            "email" => await ListEmailBuilderDefinitions(normalizedType, cancellationToken),
            "form" => await ListFormBuilderDefinitions(normalizedType, cancellationToken),
            "admin-form" => await ListAdminFormDefinitions(normalizedType, cancellationToken),
            "automation" => await ListAutomationDefinitions(normalizedType, cancellationToken),
            _ => throw new ArgumentException($"Unknown builder '{builder}'. Use page|email|form|admin-form|automation.", nameof(builder))
        };

        return new ComponentDefinitionListResponse(normalizedBuilder, normalizedType, items);
    }

    private async Task<List<ComponentDefinitionItem>> ListPageBuilderDefinitions(string componentType, CancellationToken cancellationToken)
    {
        var model = await readService.GetPageBuilderRegistryAsync(cancellationToken);
        List<ComponentDefinitionItem> items = [];

        if (componentType is "all" or "widget")
        {
            items.AddRange(model.Widgets.Select(w => new ComponentDefinitionItem(
                Builder: "page",
                ComponentType: "widget",
                Identifier: w.Identifier,
                Name: w.Name,
                Description: w.Description,
                IconClass: w.IconClass,
                MarkedTypeName: w.MarkedTypeName,
                PropertiesTypeName: null,
                ContentTypeNames: null)));
        }

        if (componentType is "all" or "section")
        {
            items.AddRange(model.Sections.Select(s => new ComponentDefinitionItem(
                Builder: "page",
                ComponentType: "section",
                Identifier: s.Identifier,
                Name: s.Name,
                Description: s.Description,
                IconClass: s.IconClass,
                MarkedTypeName: s.MarkedTypeName,
                PropertiesTypeName: null,
                ContentTypeNames: null)));
        }

        if (componentType is "all" or "page-template")
        {
            items.AddRange(model.PageTemplates.Select(t => new ComponentDefinitionItem(
                Builder: "page",
                ComponentType: "page-template",
                Identifier: t.Identifier,
                Name: t.Name,
                Description: t.Description,
                IconClass: t.IconClass,
                MarkedTypeName: t.MarkedTypeName,
                PropertiesTypeName: null,
                ContentTypeNames: t.ContentTypeNames)));
        }

        return items;
    }

    private async Task<List<ComponentDefinitionItem>> ListEmailBuilderDefinitions(string componentType, CancellationToken cancellationToken)
    {
        var model = await readService.GetEmailBuilderRegistryAsync(cancellationToken);
        List<ComponentDefinitionItem> items = [];

        if (componentType is "all" or "widget")
        {
            items.AddRange(model.Widgets.Select(w => new ComponentDefinitionItem(
                Builder: "email",
                ComponentType: "widget",
                Identifier: w.Identifier,
                Name: w.Name,
                Description: w.Description,
                IconClass: w.IconClass,
                MarkedTypeName: w.MarkedTypeName,
                PropertiesTypeName: w.PropertiesTypeName,
                ContentTypeNames: null)));
        }

        if (componentType is "all" or "section")
        {
            items.AddRange(model.Sections.Select(s => new ComponentDefinitionItem(
                Builder: "email",
                ComponentType: "section",
                Identifier: s.Identifier,
                Name: s.Name,
                Description: s.Description,
                IconClass: s.IconClass,
                MarkedTypeName: s.MarkedTypeName,
                PropertiesTypeName: s.PropertiesTypeName,
                ContentTypeNames: null)));
        }

        if (componentType is "all" or "template")
        {
            items.AddRange(model.EmailTemplates.Select(t => new ComponentDefinitionItem(
                Builder: "email",
                ComponentType: "template",
                Identifier: t.Identifier,
                Name: t.Name,
                Description: t.Description,
                IconClass: t.IconClass,
                MarkedTypeName: t.MarkedTypeName,
                PropertiesTypeName: null,
                ContentTypeNames: t.ContentTypeNames)));
        }

        return items;
    }

    private async Task<List<ComponentDefinitionItem>> ListFormBuilderDefinitions(string componentType, CancellationToken cancellationToken)
    {
        var model = await readService.GetFormBuilderRegistryAsync(cancellationToken);
        List<ComponentDefinitionItem> items = [];

        if (componentType is "all" or "component")
        {
            items.AddRange(model.FormComponents.Select(c => new ComponentDefinitionItem(
                Builder: "form",
                ComponentType: "component",
                Identifier: c.Identifier,
                Name: c.Name,
                Description: c.Description,
                IconClass: c.IconClass,
                MarkedTypeName: c.MarkedTypeName,
                PropertiesTypeName: null,
                ContentTypeNames: null)));
        }

        if (componentType is "all" or "section")
        {
            items.AddRange(model.FormSections.Select(s => new ComponentDefinitionItem(
                Builder: "form",
                ComponentType: "section",
                Identifier: s.Identifier,
                Name: s.Name,
                Description: s.Description,
                IconClass: s.IconClass,
                MarkedTypeName: s.MarkedTypeName,
                PropertiesTypeName: null,
                ContentTypeNames: null)));
        }

        return items;
    }

    private async Task<List<ComponentDefinitionItem>> ListAdminFormDefinitions(string componentType, CancellationToken cancellationToken)
    {
        var model = await readService.GetAdminFormComponentRegistryAsync(cancellationToken);

        if (componentType is not ("all" or "component"))
        {
            return [];
        }

        return model.FormComponents.Where(c => c.IsAvailable).Select(c => new ComponentDefinitionItem(
            Builder: "admin-form",
            ComponentType: "component",
            Identifier: c.Identifier,
            Name: c.Name,
            Description: null,
            IconClass: null,
            MarkedTypeName: c.MarkedTypeName,
            PropertiesTypeName: null,
            ContentTypeNames: null)).ToList();
    }

    private async Task<List<ComponentDefinitionItem>> ListAutomationDefinitions(string componentType, CancellationToken cancellationToken)
    {
        var model = await readService.GetAutomationRegistryAsync(cancellationToken);
        List<ComponentDefinitionItem> items = [];

        if (componentType is "all" or "trigger")
        {
            items.AddRange(model.Triggers.Select(t => new ComponentDefinitionItem(
                Builder: "automation",
                ComponentType: "trigger",
                Identifier: t.Identifier,
                Name: t.Name,
                Description: t.Description,
                IconClass: t.IconName,
                MarkedTypeName: t.ImplementationTypeName,
                PropertiesTypeName: t.PropertiesTypeName,
                ContentTypeNames: null,
                TriggerDataTypeName: t.TriggerDataTypeName,
                ObjectType: t.ObjectType)));
        }

        if (componentType is "all" or "action")
        {
            items.AddRange(model.Actions.Select(a => ToAutomationStepItem(a, "action")));
        }

        if (componentType is "all" or "condition")
        {
            items.AddRange(model.Conditions.Select(c => ToAutomationStepItem(c, "condition")));
        }

        return items;
    }

    private static ComponentDefinitionItem ToAutomationStepItem(AutomationStepDto step, string componentType) =>
        new(
            Builder: "automation",
            ComponentType: componentType,
            Identifier: step.Identifier,
            Name: step.Name,
            Description: step.Description,
            IconClass: step.IconName,
            MarkedTypeName: step.ImplementationTypeName,
            PropertiesTypeName: step.PropertiesTypeName,
            ContentTypeNames: null);
}

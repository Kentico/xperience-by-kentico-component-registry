using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using RegisterEmailSectionAttribute = Kentico.EmailBuilder.Web.Mvc.RegisterEmailSectionAttribute;
using RegisterEmailTemplateAttribute = Kentico.EmailBuilder.Web.Mvc.RegisterEmailTemplateAttribute;
using RegisterEmailWidgetAttribute = Kentico.EmailBuilder.Web.Mvc.RegisterEmailWidgetAttribute;
using RegisterFormComponentAttribute = Kentico.Forms.Web.Mvc.RegisterFormComponentAttribute;
using RegisterFormSectionAttribute = Kentico.Forms.Web.Mvc.RegisterFormSectionAttribute;
using RegisterPageTemplateAttribute = Kentico.PageBuilder.Web.Mvc.PageTemplates.RegisterPageTemplateAttribute;
using RegisterSectionAttribute = Kentico.PageBuilder.Web.Mvc.RegisterSectionAttribute;
using RegisterWidgetAttribute = Kentico.PageBuilder.Web.Mvc.RegisterWidgetAttribute;

namespace Kentico.Xperience.ComponentRegistry;

public static class ServiceCollectionComponentRegistryExtensions
{
    /// <summary>
    /// Registers component registry services by scanning assemblies marked with <see cref="CMS.AssemblyDiscoverableAttribute"/> and the entry assembly
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddComponentRegistry(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => a.GetCustomAttributes(typeof(CMS.AssemblyDiscoverableAttribute), inherit: false).Length > 0)
            .Concat(Assembly.GetEntryAssembly() is { } entry ? [entry] : [])
            .ToList();

        return AddComponentRegistry(services, assemblies);
    }

    /// <summary>
    /// Registers component registry services by scanning specified assemblies for Page Builder, Email Builder, and Form Builder components
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assembliesToScan">Assemblies to scan for component registrations</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddComponentRegistry(this IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
    {
        var assemblies = assembliesToScan.ToList();

        services.AddSingleton<IComponentDefinitionStore<PageBuilderWidgetDefinition>>(_ =>
        {
            var store = new ComponentDefinitionStore<PageBuilderWidgetDefinition>();
            var attrs = assemblies
                .SelectMany(a => a.GetCustomAttributes(typeof(RegisterWidgetAttribute), inherit: false))
                .Cast<RegisterWidgetAttribute>()
                .DistinctBy(a => a.Identifier);

            foreach (var attr in attrs)
            {
                store.Add(new PageBuilderWidgetDefinition(
                    identifier: attr.Identifier,
                    name: attr.Name,
                    markedType: attr.MarkedType,
                    description: attr.Description,
                    iconClass: attr.IconClass,
                    allowCache: attr.AllowCache));
            }
            return store;
        });

        services.AddSingleton<IComponentDefinitionStore<PageBuilderSectionDefinition>>(_ =>
        {
            var store = new ComponentDefinitionStore<PageBuilderSectionDefinition>();
            var attrs = assemblies
                .SelectMany(a => a.GetCustomAttributes(typeof(RegisterSectionAttribute), inherit: false))
                .Cast<RegisterSectionAttribute>()
                .DistinctBy(a => a.Identifier);

            foreach (var attr in attrs)
            {
                store.Add(new PageBuilderSectionDefinition(
                    identifier: attr.Identifier,
                    name: attr.Name,
                    markedType: attr.MarkedType,
                    description: attr.Description,
                    iconClass: attr.IconClass));
            }
            return store;
        });

        services.AddSingleton<IComponentDefinitionStore<PageBuilderPageTemplateDefinition>>(_ =>
        {
            var store = new ComponentDefinitionStore<PageBuilderPageTemplateDefinition>();
            var attrs = assemblies
                .SelectMany(a => a.GetCustomAttributes(typeof(RegisterPageTemplateAttribute), inherit: false))
                .Cast<RegisterPageTemplateAttribute>()
                .DistinctBy(a => a.Identifier);

            foreach (var attr in attrs)
            {
                store.Add(new PageBuilderPageTemplateDefinition(
                    identifier: attr.Identifier,
                    name: attr.Name,
                    markedType: attr.MarkedType,
                    description: attr.Description,
                    iconClass: attr.IconClass,
                    contentTypeNames: attr.ContentTypeNames));
            }
            return store;
        });

        services.AddSingleton<IComponentDefinitionStore<EmailBuilderWidgetDefinition>>(_ =>
        {
            var store = new ComponentDefinitionStore<EmailBuilderWidgetDefinition>();
            var attrs = assemblies
                .SelectMany(a => a.GetCustomAttributes(typeof(RegisterEmailWidgetAttribute), inherit: false))
                .Cast<RegisterEmailWidgetAttribute>()
                .DistinctBy(a => a.Identifier);

            foreach (var attr in attrs)
            {
                store.Add(new EmailBuilderWidgetDefinition(
                    identifier: attr.Identifier,
                    name: attr.Name,
                    markedType: attr.MarkedType,
                    description: attr.Description,
                    iconClass: attr.IconClass,
                    propertiesType: attr.PropertiesType));
            }
            return store;
        });

        services.AddSingleton<IComponentDefinitionStore<EmailBuilderSectionDefinition>>(_ =>
        {
            var store = new ComponentDefinitionStore<EmailBuilderSectionDefinition>();
            var attrs = assemblies
                .SelectMany(a => a.GetCustomAttributes(typeof(RegisterEmailSectionAttribute), inherit: false))
                .Cast<RegisterEmailSectionAttribute>()
                .DistinctBy(a => a.Identifier);

            foreach (var attr in attrs)
            {
                store.Add(new EmailBuilderSectionDefinition(
                    identifier: attr.Identifier,
                    name: attr.Name,
                    markedType: attr.MarkedType,
                    description: attr.Description,
                    iconClass: attr.IconClass));
            }
            return store;
        });

        services.AddSingleton<IComponentDefinitionStore<EmailBuilderTemplateDefinition>>(_ =>
        {
            var store = new ComponentDefinitionStore<EmailBuilderTemplateDefinition>();
            var attrs = assemblies
                .SelectMany(a => a.GetCustomAttributes(typeof(RegisterEmailTemplateAttribute), inherit: false))
                .Cast<RegisterEmailTemplateAttribute>()
                .DistinctBy(a => a.Identifier);

            foreach (var attr in attrs)
            {
                store.Add(new EmailBuilderTemplateDefinition(
                    identifier: attr.Identifier,
                    name: attr.Name,
                    markedType: attr.MarkedType,
                    description: attr.Description,
                    iconClass: attr.IconClass,
                    contentTypeNames: attr.ContentTypeNames));
            }
            return store;
        });

        services.AddSingleton<IComponentDefinitionStore<FormBuilderComponentDefinition>>(_ =>
        {
            var store = new ComponentDefinitionStore<FormBuilderComponentDefinition>();
            var attrs = assemblies
                .SelectMany(a => a.GetCustomAttributes(typeof(RegisterFormComponentAttribute), inherit: false))
                .Cast<RegisterFormComponentAttribute>()
                .Where(attr => attr.IsAvailableInFormBuilderEditor)
                .DistinctBy(a => a.Identifier);

            foreach (var attr in attrs)
            {
                store.Add(new FormBuilderComponentDefinition(
                    identifier: attr.Identifier,
                    name: attr.Name,
                    markedType: attr.MarkedType,
                    description: attr.Description,
                    iconClass: attr.IconClass));
            }
            return store;
        });

        services.AddSingleton<IComponentDefinitionStore<FormBuilderSectionDefinition>>(_ =>
        {
            var store = new ComponentDefinitionStore<FormBuilderSectionDefinition>();
            var attrs = assemblies
                .SelectMany(a => a.GetCustomAttributes(typeof(RegisterFormSectionAttribute), inherit: false))
                .Cast<RegisterFormSectionAttribute>()
                .DistinctBy(a => a.Identifier);

            foreach (var attr in attrs)
            {
                store.Add(new FormBuilderSectionDefinition(
                    identifier: attr.Identifier,
                    name: attr.Name,
                    markedType: attr.MarkedType,
                    description: attr.Description,
                    iconClass: attr.IconClass));
            }
            return store;
        });

        services.AddScoped<IComponentUsageService, ComponentUsageService>();
        services.AddScoped<IComponentRegistryLocalizationService, ComponentRegistryLocalizationService>();
        services.AddScoped<IComponentRegistryReadService, ComponentRegistryReadService>();

        return services;
    }
}

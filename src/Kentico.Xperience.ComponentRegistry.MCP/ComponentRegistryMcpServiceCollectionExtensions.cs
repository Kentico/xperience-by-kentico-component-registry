using Kentico.Xperience.ComponentRegistry.MCP;

using ModelContextProtocol.Server;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for registering Component Registry MCP tools on an existing MCP server builder.
/// </summary>
public static class ComponentRegistryMcpServiceCollectionExtensions
{
    /// <summary>
    /// Adds Component Registry MCP tools to an existing MCP server builder.
    /// </summary>
    /// <remarks>
    /// This extension only registers tools from the <c>Kentico.Xperience.ComponentRegistry.MCP</c> assembly.
    /// The host application is responsible for MCP transport configuration and endpoint mapping.
    /// </remarks>
    /// <param name="builder">The MCP server builder.</param>
    /// <returns>The MCP server builder for method chaining.</returns>
    public static IMcpServerBuilder WithComponentRegistryTools(this IMcpServerBuilder builder) =>
        builder.WithToolsFromAssembly(typeof(ComponentRegistryDefinitionMcpTools).Assembly);
}

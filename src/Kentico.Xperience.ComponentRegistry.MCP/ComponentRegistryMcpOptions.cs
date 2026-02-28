namespace Kentico.Xperience.ComponentRegistry.MCP;

/// <summary>
/// Configuration options for the component registry MCP endpoint.
/// </summary>
public class ComponentRegistryMcpOptions
{
    /// <summary>
    /// Configuration section path.
    /// </summary>
    public const string SectionPath = "Kentico:Xperience:ComponentRegistry:Mcp";

    /// <summary>
    /// Username of administration user to represent agent-driven actions
    /// </summary>
    public string AgentAdminUserName { get; set; } = "mcpAgent";
}

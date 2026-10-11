namespace Kentico.Xperience.ComponentRegistry;

internal static class DeprecationMessages
{
    public const string PackageRenamed = "Kentico.Xperience.ComponentRegistry packages are deprecated and 2.0.0 is the final release. Use the Kentico.Xperience.Labs.ComponentRegistry packages (namespace Kentico.Xperience.Labs.ComponentRegistry).";
    public const string DiagnosticId = "KXCR0001";
#pragma warning disable S1075 // Obsolete UrlFormat requires a constant
    public const string Url = "https://github.com/Kentico/xperience-by-kentico-component-registry";
#pragma warning restore S1075
}

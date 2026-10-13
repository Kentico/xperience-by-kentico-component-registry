using CMS.Membership.Internal;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin;

/// <summary>
/// Evaluates <see cref="ComponentRegistryPermissions"/> for pages outside the Component Registry application (for example web page tabs).
/// <see cref="IUIPermissionEvaluator"/> evaluates against the application of the current page, so it cannot be used there.
/// </summary>
internal interface IComponentRegistryPermissionEvaluator
{
    /// <summary>
    /// Whether the current user has <paramref name="permission"/> in the Component Registry application. Administrators always do.
    /// </summary>
    public Task<bool> IsGranted(string permission);
}

internal static class ComponentRegistryPermissionEvaluatorExtensions
{
    /// <summary>
    /// Throws <see cref="ForbiddenAccessException"/> when the current user lacks <paramref name="permission"/>.
    /// </summary>
    public static async Task EnsureGranted(this IComponentRegistryPermissionEvaluator evaluator, string permission)
    {
        if (!await evaluator.IsGranted(permission))
        {
            throw new ForbiddenAccessException();
        }
    }
}

/// <remarks>
/// Uses <c>CMS.Membership.Internal</c>: there is no public API that evaluates a permission of another application.
/// </remarks>
internal sealed class ComponentRegistryPermissionEvaluator(
    IAuthenticatedUserAccessor authenticatedUserAccessor,
    IApplicationPermissionEvaluator applicationPermissionEvaluator) : IComponentRegistryPermissionEvaluator
{
    private readonly IAuthenticatedUserAccessor authenticatedUserAccessor = authenticatedUserAccessor;
    private readonly IApplicationPermissionEvaluator applicationPermissionEvaluator = applicationPermissionEvaluator;

    public async Task<bool> IsGranted(string permission)
    {
        var user = await authenticatedUserAccessor.Get();
        if (user is null)
        {
            return false;
        }

        return user.IsAdministrator()
            || applicationPermissionEvaluator.Evaluate(new ApplicationPermissionEvaluationContext
            {
                User = user,
                ApplicationName = ComponentRegistryApplicationPage.IDENTIFIER,
                PermissionName = permission,
            });
    }
}

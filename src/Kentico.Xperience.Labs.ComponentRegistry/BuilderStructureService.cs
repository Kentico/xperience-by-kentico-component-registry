using System.Data;

using CMS.ContentEngine;
using CMS.DataEngine;

namespace Kentico.Xperience.Labs.ComponentRegistry;

/// <summary>
/// Reads the Page Builder structure of web pages and the Email Builder structure of emails.
/// </summary>
public interface IBuilderStructureService
{
    /// <summary>
    /// Gets the Page Builder structure of the latest version of a web page language variant,
    /// or <c>null</c> when the page has no variant in <paramref name="languageName"/>.
    /// </summary>
    /// <param name="webPageItemId">Web page item ID.</param>
    /// <param name="languageName">Content language code name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<BuilderStructureDto?> GetWebPageStructureAsync(int webPageItemId, string languageName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Email Builder structure of the latest version of an email language variant,
    /// or <c>null</c> when the email has no variant in <paramref name="languageName"/>.
    /// </summary>
    /// <param name="emailConfigurationId">Email configuration ID.</param>
    /// <param name="languageName">Content language code name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<BuilderStructureDto?> GetEmailStructureAsync(int emailConfigurationId, string languageName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of <see cref="IBuilderStructureService"/>. Reads <c>CMS_ContentItemCommonData</c> like <see cref="ComponentUsageService"/>.
/// </summary>
public class BuilderStructureService(IComponentRegistryReadService componentRegistryReadService) : IBuilderStructureService
{
    // The latest version, plus the published version when a newer draft exists.
    private const string QUERY = """
        SELECT
            CL.ContentLanguageDisplayName,
            CICD.ContentItemCommonDataVersionStatus,
            CICD.ContentItemCommonDataIsLatest,
            CICD.ContentItemCommonDataVisualBuilderWidgets,
            CICD.ContentItemCommonDataVisualBuilderTemplateConfiguration
        FROM {owner}
        INNER JOIN CMS_ContentItemCommonData CICD ON {ownerContentItemId} = CICD.ContentItemCommonDataContentItemID
        INNER JOIN CMS_ContentLanguage CL ON CICD.ContentItemCommonDataContentLanguageID = CL.ContentLanguageID
        WHERE {ownerId} = @OwnerID
            AND CL.ContentLanguageName = @LanguageName
            AND (CICD.ContentItemCommonDataIsLatest = 1 OR CICD.ContentItemCommonDataVersionStatus = @Published)
        """;

    private static readonly string webPageQuery = QUERY
        .Replace("{owner}", "CMS_WebPageItem WPI")
        .Replace("{ownerContentItemId}", "WPI.WebPageItemContentItemID")
        .Replace("{ownerId}", "WPI.WebPageItemID");

    private static readonly string emailQuery = QUERY
        .Replace("{owner}", "EmailLibrary_EmailConfiguration EC")
        .Replace("{ownerContentItemId}", "EC.EmailConfigurationContentItemID")
        .Replace("{ownerId}", "EC.EmailConfigurationID");

    private readonly IComponentRegistryReadService componentRegistryReadService = componentRegistryReadService;

    /// <inheritdoc/>
    public Task<BuilderStructureDto?> GetWebPageStructureAsync(int webPageItemId, string languageName, CancellationToken cancellationToken = default) =>
        GetStructureAsync(webPageQuery, webPageItemId, languageName,
            async () => BuilderComponentLookup.From(await componentRegistryReadService.GetPageBuilderRegistryAsync(cancellationToken)),
            cancellationToken);

    /// <inheritdoc/>
    public Task<BuilderStructureDto?> GetEmailStructureAsync(int emailConfigurationId, string languageName, CancellationToken cancellationToken = default) =>
        GetStructureAsync(emailQuery, emailConfigurationId, languageName,
            async () => BuilderComponentLookup.From(await componentRegistryReadService.GetEmailBuilderRegistryAsync(cancellationToken)),
            cancellationToken);

    private static async Task<BuilderStructureDto?> GetStructureAsync(
        string query,
        int ownerId,
        string languageName,
        Func<Task<BuilderComponentLookup>> getLookup,
        CancellationToken cancellationToken)
    {
        var parameters = new QueryParameters(query, new QueryDataParameters
        {
            { "@OwnerID", ownerId },
            { "@LanguageName", languageName },
            { "@Published", (int)VersionStatus.Published },
        }, QueryTypeEnum.SQLQuery);

        string? languageDisplayName = null;
        BuilderDataVersion? latest = null;
        BuilderDataVersion? published = null;

        using (var connection = ConnectionHelper.GetConnection())
        {
            using var reader = await connection.ExecuteReaderAsync(parameters, CommandBehavior.Default, cancellationToken);

            while (reader is not null && await reader.ReadAsync(cancellationToken))
            {
                languageDisplayName = reader.GetString(0);
                var version = new BuilderDataVersion(
                    reader.GetInt32(1),
                    reader.IsDBNull(3) ? null : reader.GetString(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4));

                if (reader.GetBoolean(2))
                {
                    latest = version;
                }
                else
                {
                    published = version;
                }
            }
        }

        if (latest is null)
        {
            return null;
        }

        return BuilderStructureParser.Create(languageName, languageDisplayName ?? languageName, latest, published, await getLookup());
    }
}

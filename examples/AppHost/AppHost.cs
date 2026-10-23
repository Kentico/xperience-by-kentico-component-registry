using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var repositoryRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));

var dancingGoat = builder.AddProject<Projects.DancingGoat>("dancinggoat")
    .WithUrls(context =>
    {
        if (context.Urls.FirstOrDefault(url => url.Endpoint?.EndpointName == "http") is { } site)
        {
            site.DisplayText = "Live site";
            context.Urls.Add(new() { Url = $"{site.Url.TrimEnd('/')}/admin", DisplayText = "Administration", Endpoint = site.Endpoint });
        }
    });

if (builder.Configuration.GetValue("Mail:UseExistingServer", true))
{
    builder.AddExternalService("mailpit", "http://localhost:8025")
        .WithHttpHealthCheck("/");
}
else
{
    var mailpit = builder.AddMailPit("mailpit");
    dancingGoat
        .WithEnvironment("SystemSmtpOptions__Server__Host", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Host))
        .WithEnvironment("SystemSmtpOptions__Server__Port", mailpit.GetEndpoint("smtp").Property(EndpointProperty.Port))
        .WaitFor(mailpit);
}

if (!builder.Configuration.GetValue("Sql:UseExistingServer", true))
{
    // Keep the password stable across restarts of the persistent data volume.
    var password = builder.AddParameter("sql-password", secret: true);
    var sql = builder.AddSqlServer("sql", password, port: 14333)
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent);
    var server = sql.Resource.ConnectionStringExpression;

    // Xperience schema/data initialization is explicit; see Contributing-Setup.md.
    dancingGoat
        .WithEnvironment("ConnectionStrings__CMSConnectionString", ReferenceExpression.Create(
            $"{server};Initial Catalog=xperience-by-kentico-component-registry"))
        .WaitFor(sql);
}

builder.AddJavaScriptApp("admin-client", Path.Combine(repositoryRoot, "src", "Kentico.Xperience.Labs.ComponentRegistry.Admin", "Client"), "start")
    .WithNpm(install: true);

builder.Build().Run();

using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Vitrio.Api.Documentation;

internal sealed class SwaggerDocuments(IApiVersionDescriptionProvider versions) : IConfigureOptions<SwaggerGenOptions>
{
    public const string BearerScheme = "Bearer";

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var version in versions.ApiVersionDescriptions)
        {
            options.SwaggerDoc(version.GroupName, new OpenApiInfo
            {
                Title = "Vitrio API",
                Version = version.ApiVersion.ToString(),
                Description = version.IsDeprecated ? "Esta versão está obsoleta." : null
            });
        }

        options.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Access token da sessão."
        });
        options.OperationFilter<BearerRequirementFilter>();
    }
}

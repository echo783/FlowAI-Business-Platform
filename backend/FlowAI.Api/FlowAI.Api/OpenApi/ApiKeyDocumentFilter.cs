using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FlowAI.Api.OpenApi;

public class ApiKeyDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var path in swaggerDoc.Paths)
        {
            if (!path.Key.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (path.Value?.Operations is null)
            {
                continue;
            }

            foreach (var operation in path.Value.Operations.Values)
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new()
                    {
                        {
                            new OpenApiSecuritySchemeReference("ApiKey", swaggerDoc, null),
                            new List<string>()
                        }
                    }
                };
            }
        }
    }
}

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Vocabularity.Api.Configuration;

public sealed class PaginationDocumentation : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath;
        if (context.ApiDescription.HttpMethod != "GET" || path is not
            ("dictionaries" or "languages" or "users" or "user/activities" or "dictionary/{id}/words" or "user/{id}/dictionaries")) return;
        operation.RequestBody = null;
        foreach (var (name, defaultValue) in new[] { ("pageNumber", 1), ("pageSize", 12) })
            operation.Parameters.Add(new OpenApiParameter { Name = name, In = ParameterLocation.Query,
                Required = false, Schema = new OpenApiSchema { Type = "integer", Minimum = 1,
                    Maximum = name == "pageSize" ? 100 : null,
                    Default = new Microsoft.OpenApi.Any.OpenApiInteger(defaultValue) } });
    }
}

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Vocabularity.Api.Configuration;

public sealed class OperationResponseDocumentation : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<OperationResponseMetadata>().Any()) return;
        var schema = context.SchemaGenerator.GenerateSchema(typeof(OperationResponse), context.SchemaRepository);
        foreach (var status in new[] { "400", "401", "403", "404", "409", "429", "500" })
        {
            operation.Responses[status] = new OpenApiResponse
            {
                Description = ErrorResponse.ForStatus(int.Parse(status)),
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new() { Schema = schema }
                }
            };
        }
    }
}

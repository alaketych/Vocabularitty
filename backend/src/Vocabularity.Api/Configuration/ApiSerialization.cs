using FastEndpoints;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Vocabularity.Core;

namespace Vocabularity.Api.Configuration;

public static class ApiSerialization
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        MissingMemberHandling = MissingMemberHandling.Error,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public static void Configure(Config config)
    {
        // Keep the existing Newtonsoft.Json contract, including explicit snake_case names.
        config.Serializer.RequestDeserializer = async (request, requestType, _, cancellationToken) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync(cancellationToken);

            try
            {
                return JsonConvert.DeserializeObject(body, requestType, Settings)
                    ?? throw new ApiException(400, "A JSON request body is required.");
            }
            catch (JsonException)
            {
                throw new ApiException(400, "Invalid JSON or an unsupported request property.");
            }
        };

        config.Serializer.ResponseSerializer = (response, body, contentType, _, cancellationToken) =>
        {
            response.ContentType = contentType;
            return response.WriteAsync(JsonConvert.SerializeObject(body, Settings), cancellationToken);
        };

        config.Errors.UseProblemDetails();
    }
}


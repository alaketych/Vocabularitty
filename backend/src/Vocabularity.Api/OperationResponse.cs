using Newtonsoft.Json;

namespace Vocabularity.Api;

public sealed record OperationResponse(
    [property: JsonProperty("isSuccessfull")] bool IsSuccessfull,
    [property: JsonProperty("message")] string Message)
{
    public static object Failure(HttpContext context, string message) =>
        context.Items.ContainsKey(typeof(OperationResponseMetadata)) ||
        context.GetEndpoint()?.Metadata.GetMetadata<OperationResponseMetadata>() is not null
            ? new OperationResponse(false, message)
            : new ErrorResponse(message);
}

// Marks resource create/update endpoints, including errors before their handlers run.
public sealed class OperationResponseMetadata;

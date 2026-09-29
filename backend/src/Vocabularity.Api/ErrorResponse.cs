using Newtonsoft.Json;

namespace Vocabularity.Api;

public sealed record ErrorResponse([property: JsonProperty("ErrorMessage")] string ErrorMessage)
{
    public static string ForStatus(int status) => status switch
    {
        401 => "Authentication is required to access this resource.",
        403 => "You do not have permission to access this resource.",
        404 => "The resource was not found or you do not have permission to access it.",
        405 => "This HTTP method is not supported.",
        429 => "Too many requests. Please try again later.",
        _ => "The request could not be completed."
    };
}

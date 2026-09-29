using Newtonsoft.Json;

namespace Vocabularity.Service.Auth.Models;

public sealed record UserResponse(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("email")] string Email,
    [property: JsonProperty("icon")] string? Icon,
    [property: JsonProperty("role")] string Role);


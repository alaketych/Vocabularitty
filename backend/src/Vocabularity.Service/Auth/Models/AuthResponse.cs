using Newtonsoft.Json;

namespace Vocabularity.Service.Auth.Models;

public sealed record AuthResponse(
    [property: JsonProperty("access_token")] string AccessToken,
    [property: JsonProperty("expires_at")] DateTime ExpiresAt,
    [property: JsonProperty("user")] UserResponse User);


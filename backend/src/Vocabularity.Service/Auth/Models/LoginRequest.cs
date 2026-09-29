using Newtonsoft.Json;

namespace Vocabularity.Service.Auth.Models;

public sealed class LoginRequest
{
    [JsonProperty("username")]
    public string? Username { get; set; }
    [JsonProperty("email")]
    public string Email { get; set; } = "";

    [JsonProperty("password")]
    public string Password { get; set; } = "";
}


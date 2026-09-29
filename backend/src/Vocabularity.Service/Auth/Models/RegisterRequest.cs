using Newtonsoft.Json;

namespace Vocabularity.Service.Auth.Models;

public sealed class RegisterRequest
{
    [JsonProperty("email")]
    public string Email { get; set; } = "";

    [JsonProperty("password")]
    public string Password { get; set; } = "";

    [JsonProperty("icon")]
    public string? Icon { get; set; }
}


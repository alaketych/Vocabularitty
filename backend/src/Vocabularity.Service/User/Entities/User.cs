using Newtonsoft.Json;
using Vocabularity.Core;

namespace Vocabularity.Service.User.Entities;

public class User : Entity
{
    [JsonProperty("email")]
    public required string Email { get; set; }

    [JsonIgnore]
    public required string NormalizedEmail { get; set; }

    [JsonIgnore]
    public required string PasswordHash { get; set; }

    [JsonProperty("icon")]
    public string? Icon { get; set; }

    [JsonProperty("role")]
    public string Role { get; set; } = UserRoles.User;

    public User() : base(true) { }
}


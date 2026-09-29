using Newtonsoft.Json;

namespace Vocabularity.Core;

public abstract class Entity
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonProperty("is_active")]
    public bool IsActive { get; set; }

    protected Entity(bool isActive) => IsActive = isActive;
}


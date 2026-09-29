using Newtonsoft.Json;
using Vocabularity.Core;

namespace Vocabularity.Service.Activity.Entities;

public sealed class UserActivity() : Entity(true)
{
    [JsonProperty("user_id")]
    public string UserId { get; set; } = "";
    [JsonProperty("function")]
    public string Function { get; set; } = "";
    [JsonProperty("entity_id")]
    public string EntityId { get; set; } = "";
    [JsonProperty("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;
}

using Newtonsoft.Json;
using Vocabularity.Core;

namespace Vocabularity.Service.Dictionary.Entities;

public class Dictionary : Entity
{
    [JsonProperty("dictionary_name")]
    public required string Name { get; set; }
    [JsonProperty("user_id")]
    public required string UserId { get; set; }
    [JsonProperty("language_id")]
    public required string LanguageId { get; set; }

    [JsonProperty("position")]
    public int Position { get; set; }

    public Dictionary() : base(true) { }
}


using Newtonsoft.Json;
using Vocabularity.Core;

namespace Vocabularity.Service.Language.Entities;

public class Language : Entity
{
    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("original_name")]
    public required string OriginalName { get; set; }

    [JsonProperty("icon")]
    public string? Icon { get; set; }

    public Language() : base(true) { }
}



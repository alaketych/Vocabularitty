using Newtonsoft.Json;

namespace Vocabularity.Service.Language.Models;

public sealed class LanguageRequest
{
    /// <summary>The language's name in English, for example Ukrainian.</summary>
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    /// <summary>The native name, for example Українська.</summary>
    [JsonProperty("original_name")]
    public string OriginalName { get; set; } = "";

    /// <summary>An optional flag emoji or image URL.</summary>
    [JsonProperty("icon")]
    public string? Icon { get; set; }
}



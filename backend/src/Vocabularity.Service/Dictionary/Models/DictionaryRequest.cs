using Newtonsoft.Json;

namespace Vocabularity.Service.Dictionary.Models;

public sealed class DictionaryRequest
{
    [JsonProperty("dictionary_name")]
    public string Name { get; set; } = "";

    [JsonProperty("language_id")]
    public string LanguageId { get; set; } = "";
}


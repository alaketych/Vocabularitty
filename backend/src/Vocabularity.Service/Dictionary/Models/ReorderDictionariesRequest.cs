using Newtonsoft.Json;

namespace Vocabularity.Service.Dictionary.Models;

public sealed class ReorderDictionariesRequest
{
    // The complete ordered list of the authenticated user's dictionary IDs.
    [JsonProperty("dictionary_ids", Required = Required.Always)]
    public List<string> DictionaryIds { get; set; } = [];
}


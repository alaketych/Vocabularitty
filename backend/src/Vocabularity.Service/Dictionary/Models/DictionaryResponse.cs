using Newtonsoft.Json;

namespace Vocabularity.Service.Dictionary.Models;

public sealed record DictionaryResponse(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("dictionary_name")] string Name,
    [property: JsonProperty("user_id")] string UserId,
    [property: JsonProperty("language_id")] string LanguageId,
    [property: JsonProperty("position")] int Position);


using Newtonsoft.Json;

namespace Vocabularity.Service.Language.Models;

public sealed record LanguageResponse(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("original_name")] string OriginalName,
    [property: JsonProperty("icon")] string? Icon);



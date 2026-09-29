using Newtonsoft.Json;

namespace Vocabularity.Service.Dictionary.Models;

public sealed record WordResponse(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("dictionary_id")] string DictionaryId,
    [property: JsonProperty("original_word")] string OriginalWord,
    [property: JsonProperty("original_transcriptioned_word")] string? OriginalTranscriptionedWord,
    [property: JsonProperty("translated_word")] string TranslatedWord);


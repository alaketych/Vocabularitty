using Newtonsoft.Json;

namespace Vocabularity.Service.Dictionary.Models;

public sealed class WordRequest
{
    [JsonProperty("original_word")]
    public string OriginalWord { get; set; } = "";

    [JsonProperty("original_transcriptioned_word")]
    public string? OriginalTranscriptionedWord { get; set; }

    [JsonProperty("translated_word")]
    public string TranslatedWord { get; set; } = "";
}


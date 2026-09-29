using Newtonsoft.Json;
using Vocabularity.Core;

namespace Vocabularity.Service.Dictionary.Entities;

public class Word : Entity
{
    [JsonProperty("dictionary_id")]
    public required string DictionaryId { get; set; }
    [JsonProperty("original_word")]
    public required string OriginalWord { get; set; }
    [JsonProperty("original_transcriptioned_word")]
    public string? OriginalTranscriptionedWord { get; set; }
    [JsonProperty("translated_word")]
    public required string TranslatedWord { get; set; }
    public Word() : base(true) { }
}


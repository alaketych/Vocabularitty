using FastEndpoints;
using FluentValidation;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Words;

public sealed class WordValidator : Validator<WordRequest>
{
    public WordValidator()
    {
        RuleFor(request => request.OriginalWord).NotEmpty().MaximumLength(500).OverridePropertyName("original_word");
        RuleFor(request => request.OriginalTranscriptionedWord).MaximumLength(500).OverridePropertyName("original_transcriptioned_word");
        RuleFor(request => request.TranslatedWord).NotEmpty().MaximumLength(2000).OverridePropertyName("translated_word");
    }
}


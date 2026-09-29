using FastEndpoints;
using FluentValidation;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class ReorderValidator : Validator<ReorderDictionariesRequest>
{
    public ReorderValidator()
    {
        RuleFor(request => request.DictionaryIds)
            .NotNull()
            .Must(ids => ids is null || ids.Distinct(StringComparer.Ordinal).Count() == ids.Count)
            .WithMessage("Each dictionary ID must appear exactly once.")
            .OverridePropertyName("dictionary_ids");

        RuleForEach(request => request.DictionaryIds)
            .NotEmpty()
            .MaximumLength(36)
            .OverridePropertyName("dictionary_ids");
    }
}


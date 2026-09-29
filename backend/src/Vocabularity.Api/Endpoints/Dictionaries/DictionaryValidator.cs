using FastEndpoints;
using FluentValidation;
using Vocabularity.Service.Dictionary.Models;

namespace Vocabularity.Api.Endpoints.Dictionaries;

public sealed class DictionaryValidator : Validator<DictionaryRequest>
{
    public DictionaryValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200).OverridePropertyName("dictionary_name");
        RuleFor(request => request.LanguageId)
            .NotEmpty()
            .MaximumLength(35)
            .Matches("^[a-zA-Z]{2,8}(-[a-zA-Z0-9]{1,8})*$")
            .OverridePropertyName("language_id");
    }
}


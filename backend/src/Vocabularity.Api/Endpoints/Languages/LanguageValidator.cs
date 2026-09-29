using FastEndpoints;
using FluentValidation;
using Vocabularity.Service.Language.Models;

namespace Vocabularity.Api.Endpoints.Languages;

public sealed class LanguageValidator : Validator<LanguageRequest>
{
    public LanguageValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(100)
            .OverridePropertyName("name");

        RuleFor(request => request.OriginalName)
            .NotEmpty()
            .MaximumLength(100)
            .OverridePropertyName("original_name");

        RuleFor(request => request.Icon)
            .MaximumLength(2048)
            .OverridePropertyName("icon");
    }
}



using FastEndpoints;
using FluentValidation;
using Vocabularity.Service.Auth.Models;

namespace Vocabularity.Api.Endpoints.Auth;

public sealed class RegisterValidator : Validator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(254).OverridePropertyName("email");
        RuleFor(request => request.Password).NotEmpty().Length(12, 128).OverridePropertyName("password");
        RuleFor(request => request.Icon)
            .MaximumLength(2048)
            .Must(icon => icon is null || Uri.TryCreate(icon, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
            .WithMessage("Icon must be an absolute HTTP or HTTPS URL.")
            .OverridePropertyName("icon");
    }
}


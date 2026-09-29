using FastEndpoints;
using FluentValidation;
using Vocabularity.Service.Auth.Models;

namespace Vocabularity.Api.Endpoints.Auth;

public sealed class LoginValidator : Validator<LoginRequest>
{
    public LoginValidator(IWebHostEnvironment environment, IConfiguration configuration)
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(254)
            .When(request => !(Configuration.DemoMode.Enabled(environment, configuration) && request.Username == "admin" && string.IsNullOrEmpty(request.Email)))
            .OverridePropertyName("email");
        RuleFor(request => request.Username).Must(username => username is null ||
            (Configuration.DemoMode.Enabled(environment, configuration) && username == "admin"))
            .OverridePropertyName("username");
        RuleFor(request => request.Password).NotEmpty().MaximumLength(128).OverridePropertyName("password");
    }
}


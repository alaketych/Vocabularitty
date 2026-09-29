namespace Vocabularity.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "Vocabularity";
    public string Audience { get; set; } = "Vocabularity.Client";
    public string Key { get; set; } = "";
    public int LifetimeMinutes { get; set; } = 60;
}


namespace Vocabularity.Api.Configuration;

public static class DemoMode
{
    public static bool Enabled(IWebHostEnvironment environment, IConfiguration configuration) =>
        environment.IsDevelopment() && configuration.GetValue<bool>("Demo:Enabled");
}

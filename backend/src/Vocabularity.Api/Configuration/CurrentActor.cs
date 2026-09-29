using Vocabularity.Service.Activity;

namespace Vocabularity.Api.Configuration;

public sealed class CurrentActor(IHttpContextAccessor accessor) : ICurrentActor
{
    public string? UserId => accessor.HttpContext?.User.Identity?.IsAuthenticated == true
        ? accessor.HttpContext.User.FindFirst("sub")?.Value : null;
}

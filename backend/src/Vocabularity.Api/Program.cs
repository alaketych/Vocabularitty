using FastEndpoints;
using Vocabularity.Api.Configuration;
using Microsoft.EntityFrameworkCore;
using Vocabularity.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiDocumentation();
builder.Services.AddFastEndpoints();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:3000", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (DemoMode.Enabled(app.Environment, app.Configuration) && app.Configuration.GetValue("Demo:InitializeDatabase", true))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<VocabularityDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;
    response.ContentType = "application/json";
    await response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(
        Vocabularity.Api.OperationResponse.Failure(statusContext.HttpContext, Vocabularity.Api.ErrorResponse.ForStatus(response.StatusCode))));
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vocabularity API v1");
        options.DocumentTitle = "Vocabularity API";
    });
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.Use(async (context, next) =>
{
    // Exception handling clears endpoint metadata; retain the response contract for failures.
    if (context.GetEndpoint()?.Metadata.GetMetadata<Vocabularity.Api.OperationResponseMetadata>() is not null)
        context.Items[typeof(Vocabularity.Api.OperationResponseMetadata)] = true;
    await next(context);
});
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(ApiSerialization.Configure);

app.Run();

public partial class Program;


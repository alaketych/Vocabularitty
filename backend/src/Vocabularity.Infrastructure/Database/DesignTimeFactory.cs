using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vocabularity.Infrastructure.Database;

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<VocabularityDbContext>
{
    public VocabularityDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<VocabularityDbContext>()
        .UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Database=Vocabularity;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;")
        .UseDemoData((Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")) == "Development" &&
            Environment.GetEnvironmentVariable("Demo__Enabled") != "false").Options);
}


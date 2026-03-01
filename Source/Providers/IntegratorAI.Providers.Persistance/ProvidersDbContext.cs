using IntegratorAI.BuildingBlocks.Persistence;
using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Providers.Persistence;

public class ProvidersDbContext : EfContext
{
    public ProvidersDbContext(DbContextOptions<ProvidersDbContext> options) : base(options)
    { }

    internal DbSet<Provider> Providers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProviderConfiguration).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.BuildingBlocks.Persistence.Conventions;

namespace IntegratorAI.BuildingBlocks.Persistence;

public abstract class EfContext : DbContext, IUnitOfWork
{
    protected abstract string DefaultSchema { get; }

    protected EfContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
        configurationBuilder.Conventions.Add(_ => new TableNameConvention());
    }
}

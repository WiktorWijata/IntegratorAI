using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.BuildingBlocks.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.BuildingBlocks.Persistence;

public abstract class EfContext : DbContext, IUnitOfWork
{
    protected EfContext(DbContextOptions options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
        configurationBuilder.Conventions.Add(_ => new TableNameConvention());
    }
}

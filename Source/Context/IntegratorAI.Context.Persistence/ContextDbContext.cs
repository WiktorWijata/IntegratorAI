using Microsoft.EntityFrameworkCore;
using IntegratorAI.BuildingBlocks.Persistence;
using IntegratorAI.Context.Persistence.Configuration;

namespace IntegratorAI.Context.Persistence;

public class ContextDbContext : EfContext
{
    public ContextDbContext(DbContextOptions<ContextDbContext> options) : base(options) { }

    internal DbSet<Domain.Context> Contexts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContextConfiguration).Assembly);
    }
}

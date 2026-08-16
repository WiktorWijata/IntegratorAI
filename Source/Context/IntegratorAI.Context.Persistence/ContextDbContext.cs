using Microsoft.EntityFrameworkCore;
using IntegratorAI.Context.Persistence.Configuration;

namespace IntegratorAI.Context.Persistence;

public class ContextDbContext : EfContext
{
    protected override string DefaultSchema => "context";

    public ContextDbContext(DbContextOptions<ContextDbContext> options) : base(options) { }

    internal DbSet<Domain.Context> Contexts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContextConfiguration).Assembly);
    }
}

﻿using IntegratorAI.BuildingBlocks.Persistence;
using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Persistence.Configuration;

using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Providers.Persistence;

public class ProvidersDbContext : EfContext
{
    protected override string DefaultSchema => "providers";

    public ProvidersDbContext(DbContextOptions<ProvidersDbContext> options) : base(options)
    { }

    public DbSet<Provider> Providers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProviderConfiguration).Assembly);
    }
}

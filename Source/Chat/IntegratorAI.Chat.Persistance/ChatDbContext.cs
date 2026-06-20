using IntegratorAI.BuildingBlocks.Persistence;
using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Chat.Persistence;

public class ChatDbContext : EfContext, IChatUnitOfWork
{
    protected override string DefaultSchema => "chat";

    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    internal DbSet<Completion> Completions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompletionConfiguration).Assembly);
    }
}

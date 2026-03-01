using IntegratorAI.BuildingBlocks.Persistence;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Chat.Persistence;

public class ChatDbContext : EfContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    internal DbSet<Completion> Completions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompletionConfiguration).Assembly);
    }
}

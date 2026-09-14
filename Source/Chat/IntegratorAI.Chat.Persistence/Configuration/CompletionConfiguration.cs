using IntegratorAI.Chat.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntegratorAI.Chat.Persistence.Configuration;

public class CompletionConfiguration : IEntityTypeConfiguration<Completion>
{
    public void Configure(EntityTypeBuilder<Completion> builder)
    {
        builder.Ignore(c => c.UnsummarizedMessages);
        builder.Ignore(c => c.ContextMessages);

        builder.HasMany(c => c.Messages)
               .WithOne()
               .HasForeignKey(m => m.CompletionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Summary)
               .WithOne()
               .HasForeignKey<CompletionSummary>(s => s.CompletionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

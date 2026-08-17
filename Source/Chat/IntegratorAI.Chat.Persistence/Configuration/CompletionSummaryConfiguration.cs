using IntegratorAI.Chat.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntegratorAI.Chat.Persistence.Configuration;

public class CompletionSummaryConfiguration : IEntityTypeConfiguration<CompletionSummary>
{
    public void Configure(EntityTypeBuilder<CompletionSummary> builder)
    {
        builder.HasKey(s => s.CompletionId);
        builder.Property(s => s.Content).IsRequired();
    }
}

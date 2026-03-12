using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntegratorAI.Context.Persistence.Configuration;

public class ContextConfiguration : IEntityTypeConfiguration<Domain.Context>
{
    public void Configure(EntityTypeBuilder<Domain.Context> builder)
    {
        builder.HasMany(c => c.Tools)
               .WithOne()
               .HasForeignKey(t => t.ContextId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

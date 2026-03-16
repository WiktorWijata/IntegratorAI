using IntegratorAI.Context.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntegratorAI.Context.Persistence.Configuration;

public class ToolConfiguration : IEntityTypeConfiguration<Tool>
{
    public void Configure(EntityTypeBuilder<Tool> builder)
    {
        builder.HasMany(t => t.Parameters)
               .WithOne()
               .HasForeignKey(p => p.ToolId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Guardrails)
               .WithOne()
               .HasForeignKey(g => g.ToolId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

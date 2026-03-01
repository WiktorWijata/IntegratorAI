using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Chat.Persistence.Repositories;

public class CompletionRepository : ICompletionRepository
{
    private readonly ChatDbContext _context;

    public CompletionRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<Completion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Completions
            .Include(c => c.Messages)
            .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Completion completion, CancellationToken cancellationToken = default)
    {
        await _context.Completions.AddAsync(completion, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

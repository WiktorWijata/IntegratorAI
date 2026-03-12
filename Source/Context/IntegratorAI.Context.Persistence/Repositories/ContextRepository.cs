using Microsoft.EntityFrameworkCore;
using IntegratorAI.Context.Domain.Repositories;

namespace IntegratorAI.Context.Persistence.Repositories;

public class ContextRepository : IContextRepository
{
    private readonly ContextDbContext _context;

    public ContextRepository(ContextDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Context?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contexts.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Domain.Context context, CancellationToken cancellationToken = default)
    {
        await _context.Contexts.AddAsync(context, cancellationToken);
    }
}

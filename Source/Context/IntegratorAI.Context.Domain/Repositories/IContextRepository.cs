namespace IntegratorAI.Context.Domain.Repositories;

public interface IContextRepository
{
    Task<Context?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Context context, CancellationToken cancellationToken = default);
}

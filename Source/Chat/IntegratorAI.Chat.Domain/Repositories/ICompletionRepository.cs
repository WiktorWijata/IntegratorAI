namespace IntegratorAI.Chat.Domain.Repositories;

public interface ICompletionRepository
{
    Task<Completion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Completion completion, CancellationToken cancellationToken = default);
}

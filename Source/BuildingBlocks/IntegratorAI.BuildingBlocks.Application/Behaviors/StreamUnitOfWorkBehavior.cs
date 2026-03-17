using System.Runtime.CompilerServices;
using MediatR;

namespace IntegratorAI.BuildingBlocks.Application.Behaviors;

public class StreamUnitOfWorkBehavior<TRequest, TResponse, TUnitOfWork> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TUnitOfWork : IUnitOfWork
{
    private readonly TUnitOfWork _unitOfWork;

    public StreamUnitOfWorkBehavior(TUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in next().WithCancellation(cancellationToken))
        {
            yield return item;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using MediatR;

namespace IntegratorAI.BuildingBlocks.Application.Behaviors;

public class UnitOfWorkBehavior<TRequest, TResponse, TUnitOfWork> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TUnitOfWork : IUnitOfWork
{
    private readonly TUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(TUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}

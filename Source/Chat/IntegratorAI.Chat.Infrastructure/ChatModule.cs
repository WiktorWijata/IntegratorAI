using IntegratorAI.Chat.Application.Commands;
using IntegratorAI.Chat.Application.Queries;
using IntegratorAI.Chat.Contracts;
using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Infrastructure;

public class ChatModule : IChatModule
{
    private readonly IMediator _mediator;

    public ChatModule(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<CompletionDto> CreateCompletion(string prompt, Guid? contextId, CancellationToken cancellationToken = default)
        => _mediator.Send(new CreateCompletionCommand(prompt, contextId), cancellationToken);

    public Task<CompletionDto> ContinueCompletion(Guid completionId, string prompt, CancellationToken cancellationToken = default)
        => _mediator.Send(new ContinueCompletionCommand(completionId, prompt), cancellationToken);

    public IAsyncEnumerable<string> StreamCreateCompletion(string prompt, Guid? contextId, CancellationToken cancellationToken = default)
        => _mediator.CreateStream(new StreamCreateCompletionCommand(prompt, contextId), cancellationToken);

    public IAsyncEnumerable<string> StreamContinueCompletion(Guid completionId, string prompt, CancellationToken cancellationToken = default)
        => _mediator.CreateStream(new StreamContinueCompletionCommand(completionId, prompt), cancellationToken);

    public Task<CompletionDto> GetCompletion(Guid completionId, CancellationToken cancellationToken = default)
        => _mediator.Send(new GetCompletionQuery(completionId), cancellationToken);
}

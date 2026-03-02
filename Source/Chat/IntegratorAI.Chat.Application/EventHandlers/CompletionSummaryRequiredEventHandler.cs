using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.BuildingBlocks.Domain.Event;
using IntegratorAI.Chat.Domain.Events;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;

namespace IntegratorAI.Chat.Application.EventHandlers;

public class CompletionSummaryRequiredEventHandler : IHandleEvent<CompletionSummaryRequiredEvent>
{
    private readonly IProviderModule _providerModule;
    private readonly IUnitOfWork _unitOfWork;

    public CompletionSummaryRequiredEventHandler(IProviderModule providerModule, IUnitOfWork unitOfWork)
    {
        _providerModule = providerModule;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompletionSummaryRequiredEvent @event, CancellationToken cancellationToken = default)
    {
        var completion = @event.Completion;
        var unsummarized = completion.UnsummarizedMessages.ToList();

        MessageDto[] summaryMessage = completion.Summary != null
            ? [new MessageDto { Role = "system", Content = $"Summary: {completion.Summary.Content}" }]
            : [];

        var completionDto = new CompletionDto
        {
            Messages =
            [
                .. summaryMessage,
                .. unsummarized.Select(m => new MessageDto
                {
                    Role = m.Role.ToString(),
                    Content = m.Content
                }),
            ]
        };

        var summary = await _providerModule.SummaryCompletionAsync(completionDto, cancellationToken);

        completion.SetSummary(summary.Content, unsummarized.Max(m => m.Index));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

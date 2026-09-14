using IntegratorAI.Chat.Domain.Events;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using RescuePC.Software.Domain.Event;

namespace IntegratorAI.Chat.Application.EventHandlers;

public class CompletionSummaryRequiredEventHandler : IHandleEvent<CompletionSummaryRequiredEvent>
{
    private readonly IProviderModule _providerModule;
    private readonly IChatUnitOfWork _unitOfWork;

    public CompletionSummaryRequiredEventHandler(
        IProviderModule providerModule, 
        IChatUnitOfWork unitOfWork)
    {
        _providerModule = providerModule;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompletionSummaryRequiredEvent @event, CancellationToken cancellationToken = default)
    {
        var completion = @event.Completion;
        var unsummarized = completion.UnsummarizedMessages.ToList();

        ProviderMessageDto[] summaryMessage = completion.Summary != null
            ? [new ProviderMessageDto { Role = "system", Content = $"Summary: {completion.Summary.Content}" }]
            : [];

        var completionDto = new ProviderCompletionDto
        {
            Messages =
            [
                .. summaryMessage,
                .. unsummarized.Select(m => new ProviderMessageDto
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

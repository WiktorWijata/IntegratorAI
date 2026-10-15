using IntegratorAI.Chat.Contracts.Models;
using MediatR;

namespace IntegratorAI.Chat.Application.Commands;

public class StreamToolResultsCommand : IStreamRequest<CompletionStreamEvent>
{
    public StreamToolResultsCommand(Guid completionId, IReadOnlyList<ToolResultDto> results)
    {
        CompletionId = completionId;
        Results = results;
    }

    public Guid CompletionId { get; }
    public IReadOnlyList<ToolResultDto> Results { get; }
}

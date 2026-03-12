using IntegratorAI.Api.Contracts.Chat;
using IntegratorAI.Api.Contracts.Chat.Models;
using IntegratorAI.Chat.Contracts.Models;

namespace IntegratorAI.Api.Mapping;

public static class CompletionMapping
{
    public static CompletionResponse ToResponse(this CompletionResponseDto dto)
    {
        return new CompletionResponse
        {
            Id = dto.CompletionId,
            Messages = dto.Messages?.Select(m => new CompletionMessage
            {
                Role = m.Role.ToString().ToLowerInvariant(),
                Content = m.Content
            }).ToArray()
        };
    }
}

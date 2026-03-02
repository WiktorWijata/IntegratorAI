using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Mapping;

public static class MessageMapping
{
    public static MessageRequest ToRequest(this CompletionDto dto, string model)
    {
        return new MessageRequest
        {
            Model = model,
            Messages = dto.Messages?.Select(m => new Message
            {
                Role = m.Role?.ToLowerInvariant(),
                Content = m.Content
            }).ToArray()
        };
    }

    public static MessageDto ToMessageDto(this Choice choice)
    {
        return new MessageDto {
            Role = choice.Message?.Role,
            Content = choice.Message?.Content
        };
    }
}

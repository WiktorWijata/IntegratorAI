using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Mapping;

public static class MessageMapping
{
    public static MessageRequest ToRequest(this ProviderCompletionDto dto, string model, bool stream = false)
    {
        return new MessageRequest
        {
            Model = model,
            Stream = stream,
            Messages = dto.Messages?.Select(m => new Message
            {
                Role = m.Role?.ToLowerInvariant(),
                Content = m.Content
            }).ToArray()
        };
    }

    public static ProviderMessageDto ToMessageDto(this Choice choice)
    {
        return new ProviderMessageDto {
            Role = choice.Message?.Role,
            Content = choice.Message?.Content
        };
    }
}

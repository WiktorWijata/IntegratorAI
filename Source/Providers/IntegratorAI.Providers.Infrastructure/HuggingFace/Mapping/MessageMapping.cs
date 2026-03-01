using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;

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
                Role = m.Role,
                Content = m.Content
            }).ToArray()
        };
    }

    public static CompletionDto ToDto(this MessageResponse response)
    {
        return new CompletionDto
        {
            Messages = response.Choices?.Select(c => new MessageDto
            {
                Role = c.Message?.Role,
                Content = c.Message?.Content
            }).ToArray()
        };
    }
}

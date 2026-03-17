using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Mapping;

public static class MessageMapping
{
    extension(ProviderCompletionDto dto)
    {
        public MessageRequest ToRequest(string model, bool stream = false)
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
    }

    extension(Choice choice)
    {
        public ProviderMessageDto ToMessageDto()
        {
            return new ProviderMessageDto
            {
                Role = choice.Message?.Role,
                Content = choice.Message?.Content
            };
        }
    }
}

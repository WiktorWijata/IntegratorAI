using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Mapping;

namespace IntegratorAI.Providers.UnitTests.HuggingFace;

public class MessageMappingTests
{

    [Fact]
    public void ToRequest_SetsModelCorrectly()
    {
        var dto = new CompletionDto { Messages = [] };

        var result = dto.ToRequest("test-model");

        Assert.Equal("test-model", result.Model);
    }

    [Fact]
    public void ToRequest_ConvertsRoleToLowercase()
    {
        var dto = new CompletionDto
        {
            Messages = [new MessageDto { Role = "USER", Content = "hello" }]
        };

        var result = dto.ToRequest("model");

        Assert.Equal("user", result.Messages[0].Role);
    }

    [Fact]
    public void ToRequest_MapsContentCorrectly()
    {
        var dto = new CompletionDto
        {
            Messages = [new MessageDto { Role = "user", Content = "test content" }]
        };

        var result = dto.ToRequest("model");

        Assert.Equal("test content", result.Messages[0].Content);
    }

    [Fact]
    public void ToRequest_WhenMessagesIsNull_ReturnsNullMessages()
    {
        var dto = new CompletionDto { Messages = null! };

        var result = dto.ToRequest("model");

        Assert.Null(result.Messages);
    }

    [Fact]
    public void ToRequest_MapsMultipleMessages()
    {
        var dto = new CompletionDto
        {
            Messages =
            [
                new MessageDto { Role = "System", Content = "sys" },
                new MessageDto { Role = "User", Content = "usr" }
            ]
        };

        var result = dto.ToRequest("model");

        Assert.Equal(2, result.Messages.Length);
        Assert.Equal("system", result.Messages[0].Role);
        Assert.Equal("user", result.Messages[1].Role);
    }

    [Fact]
    public void ToRequest_StreamIsFalse_ByDefault()
    {
        var dto = new CompletionDto { Messages = [] };

        var result = dto.ToRequest("model");

        Assert.False(result.Stream);
    }


    [Fact]
    public void ToMessageDto_MapsRoleAndContent()
    {
        var choice = new Choice
        {
            Message = new Message { Role = "assistant", Content = "answer" }
        };

        var result = choice.ToMessageDto();

        Assert.Equal("assistant", result.Role);
        Assert.Equal("answer", result.Content);
    }

    [Fact]
    public void ToMessageDto_WhenMessageIsNull_ReturnsNullRoleAndContent()
    {
        var choice = new Choice { Message = null };

        var result = choice.ToMessageDto();

        Assert.Null(result.Role);
        Assert.Null(result.Content);
    }

    [Fact]
    public void ToRequest_WhenMessagesIsEmpty_ReturnsEmptyMessagesArray()
    {
        var dto = new CompletionDto { Messages = [] };

        var result = dto.ToRequest("model");

        Assert.NotNull(result.Messages);
        Assert.Empty(result.Messages);
    }

    [Fact]
    public void ToRequest_PreservesMessageOrder()
    {
        var dto = new CompletionDto
        {
            Messages =
            [
                new MessageDto { Role = "system", Content = "first" },
                new MessageDto { Role = "user", Content = "second" },
                new MessageDto { Role = "assistant", Content = "third" }
            ]
        };

        var result = dto.ToRequest("model");

        Assert.Equal("first", result.Messages[0].Content);
        Assert.Equal("second", result.Messages[1].Content);
        Assert.Equal("third", result.Messages[2].Content);
    }

    [Fact]
    public void ToRequest_HandlesNullRoleInMessage()
    {
        var dto = new CompletionDto
        {
            Messages = [new MessageDto { Role = null, Content = "hello" }]
        };

        var result = dto.ToRequest("model");

        Assert.Null(result.Messages[0].Role);
        Assert.Equal("hello", result.Messages[0].Content);
    }
}

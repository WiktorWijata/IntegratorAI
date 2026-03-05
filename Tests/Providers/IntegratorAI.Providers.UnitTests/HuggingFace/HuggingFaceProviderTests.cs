using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.Consts;
using IntegratorAI.Providers.Infrastructure.HuggingFace;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using Moq;

namespace IntegratorAI.Providers.UnitTests.HuggingFace;

public class HuggingFaceProviderTests
{
    private readonly Mock<IHuggingFaceApi> _apiMock = new();
    private readonly HuggingFaceProvider _provider;

    public HuggingFaceProviderTests()
    {
        _provider = new HuggingFaceProvider(_apiMock.Object)
        {
            PrimaryModel = "primary-model",
            SummarizationModel = string.Empty
        };
    }

    private static CompletionDto BuildCompletion(params (string Role, string Content)[] messages)
        => new() { Messages = messages.Select(m => new MessageDto { Role = m.Role, Content = m.Content }).ToArray() };

    private static MessageResponse BuildChatResponse(string role, string content)
        => new() { Choices = [new Choice { Message = new Message { Role = role, Content = content } }] };


    [Fact]
    public async Task CompletionAsync_ReturnsFirstChoice_WhenResponseHasChoices()
    {
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .ReturnsAsync(BuildChatResponse("assistant", "reply"));

        var result = await _provider.CompletionAsync(BuildCompletion(("user", "hi")));

        Assert.Equal("assistant", result.Role);
        Assert.Equal("reply", result.Content);
    }

    [Fact]
    public async Task CompletionAsync_UsesPrimaryModel_InRequest()
    {
        _provider.PrimaryModel = "my-model";
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .ReturnsAsync(BuildChatResponse("assistant", "ok"));

        await _provider.CompletionAsync(BuildCompletion(("user", "hi")));

        _apiMock.Verify(a => a.ChatAsync(It.Is<MessageRequest>(r => r.Model == "my-model")), Times.Once);
    }

    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenChoicesIsNull()
    {
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .ReturnsAsync(new MessageResponse { Choices = null });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.CompletionAsync(BuildCompletion(("user", "hi"))));
    }

    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenChoicesIsEmpty()
    {
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .ReturnsAsync(new MessageResponse { Choices = [] });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.CompletionAsync(BuildCompletion(("user", "hi"))));
    }


    [Fact]
    public async Task SummaryCompletionAsync_UsesPipelineApi_WhenSummarizationModelIsSet()
    {
        _provider.SummarizationModel = "summary-model";
        _apiMock
            .Setup(a => a.PipelineAsync<SummarizationResponse[]>(It.IsAny<string>(), It.IsAny<PipelineRequest>()))
            .ReturnsAsync([new SummarizationResponse { SummaryText = "short" }]);

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "long text")));

        _apiMock.Verify(
            a => a.PipelineAsync<SummarizationResponse[]>("summary-model", It.IsAny<PipelineRequest>()),
            Times.Once);
    }

    [Fact]
    public async Task SummaryCompletionAsync_ReturnsSystemRoleMessage_WhenPipelineSucceeds()
    {
        _provider.SummarizationModel = "summary-model";
        _apiMock
            .Setup(a => a.PipelineAsync<SummarizationResponse[]>(It.IsAny<string>(), It.IsAny<PipelineRequest>()))
            .ReturnsAsync([new SummarizationResponse { SummaryText = "summary here" }]);

        var result = await _provider.SummaryCompletionAsync(BuildCompletion(("user", "text")));

        Assert.Equal(PromptRoles.System, result.Role);
        Assert.Equal("summary here", result.Content);
    }

    [Fact]
    public async Task SummaryCompletionAsync_JoinsMessagesAsInput_WhenPipelineCalled()
    {
        _provider.SummarizationModel = "summary-model";
        PipelineRequest? capturedRequest = null;
        _apiMock
            .Setup(a => a.PipelineAsync<SummarizationResponse[]>(It.IsAny<string>(), It.IsAny<PipelineRequest>()))
            .Callback<string, PipelineRequest>((_, r) => capturedRequest = r)
            .ReturnsAsync([new SummarizationResponse { SummaryText = "x" }]);

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "hello"), ("assistant", "world")));

        Assert.NotNull(capturedRequest);
        Assert.Contains("user: hello", capturedRequest!.Inputs);
        Assert.Contains("assistant: world", capturedRequest.Inputs);
    }

    [Fact]
    public async Task SummaryCompletionAsync_ThrowsInvalidOperationException_WhenPipelineReturnsEmpty()
    {
        _provider.SummarizationModel = "summary-model";
        _apiMock
            .Setup(a => a.PipelineAsync<SummarizationResponse[]>(It.IsAny<string>(), It.IsAny<PipelineRequest>()))
            .ReturnsAsync([]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.SummaryCompletionAsync(BuildCompletion(("user", "text"))));
    }


    [Fact]
    public async Task SummaryCompletionAsync_FallsBackToChat_WhenSummarizationModelIsEmpty()
    {
        _provider.SummarizationModel = string.Empty;
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .ReturnsAsync(BuildChatResponse("assistant", "summary"));

        var result = await _provider.SummaryCompletionAsync(BuildCompletion(("user", "text")));

        _apiMock.Verify(a => a.ChatAsync(It.IsAny<MessageRequest>()), Times.Once);
        Assert.Equal("summary", result.Content);
    }

    [Fact]
    public async Task SummaryCompletionAsync_AddsSummarizationSystemPrompt_WhenNoSummarizationModel()
    {
        _provider.SummarizationModel = string.Empty;
        MessageRequest? capturedRequest = null;
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .Callback<MessageRequest>(r => capturedRequest = r)
            .ReturnsAsync(BuildChatResponse("assistant", "ok"));

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "some text")));

        Assert.NotNull(capturedRequest);
        Assert.Equal(PromptRoles.System, capturedRequest!.Messages[0].Role);
        Assert.Equal(SystemPrompts.SummarizationPrompt, capturedRequest.Messages[0].Content);
    }

    [Fact]
    public async Task SummaryCompletionAsync_PreservesOriginalMessages_WhenNoSummarizationModel()
    {
        _provider.SummarizationModel = string.Empty;
        MessageRequest? capturedRequest = null;
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .Callback<MessageRequest>(r => capturedRequest = r)
            .ReturnsAsync(BuildChatResponse("assistant", "ok"));

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "msg1"), ("assistant", "msg2")));

        Assert.NotNull(capturedRequest);
        Assert.Equal(3, capturedRequest!.Messages.Length);
        Assert.Equal("msg1", capturedRequest.Messages[1].Content);
        Assert.Equal("msg2", capturedRequest.Messages[2].Content);
    }

    [Fact]
    public async Task SummaryCompletionAsync_ThrowsInvalidOperationException_WhenChatFallbackReturnsNoChoices()
    {
        _provider.SummarizationModel = string.Empty;
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .ReturnsAsync(new MessageResponse { Choices = [] });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.SummaryCompletionAsync(BuildCompletion(("user", "text"))));
    }

    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenChoiceMessageIsNull()
    {
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .ReturnsAsync(new MessageResponse { Choices = [new Choice { Message = null }] });

        var result = await _provider.CompletionAsync(BuildCompletion(("user", "hi")));

        Assert.Null(result.Role);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task CompletionAsync_MapsAllMessagesToRequest()
    {
        MessageRequest? capturedRequest = null;
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .Callback<MessageRequest>(r => capturedRequest = r)
            .ReturnsAsync(BuildChatResponse("assistant", "ok"));

        await _provider.CompletionAsync(BuildCompletion(("system", "sys"), ("user", "hi"), ("assistant", "hey")));

        Assert.NotNull(capturedRequest);
        Assert.Equal(3, capturedRequest!.Messages.Length);
    }

    [Fact]
    public async Task CompletionAsync_ConvertsRolesToLowercase_InRequest()
    {
        MessageRequest? capturedRequest = null;
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .Callback<MessageRequest>(r => capturedRequest = r)
            .ReturnsAsync(BuildChatResponse("assistant", "ok"));

        await _provider.CompletionAsync(BuildCompletion(("USER", "hi"), ("ASSISTANT", "hey")));

        Assert.NotNull(capturedRequest);
        Assert.Equal("user", capturedRequest!.Messages[0].Role);
        Assert.Equal("assistant", capturedRequest.Messages[1].Role);
    }

    [Fact]
    public async Task SummaryCompletionAsync_ThrowsInvalidOperationException_WhenPipelineReturnsNull()
    {
        _provider.SummarizationModel = "summary-model";
        _apiMock
            .Setup(a => a.PipelineAsync<SummarizationResponse[]>(It.IsAny<string>(), It.IsAny<PipelineRequest>()))
            .ReturnsAsync((SummarizationResponse[]?)null!);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.SummaryCompletionAsync(BuildCompletion(("user", "text"))));
    }

    [Fact]
    public async Task SummaryCompletionAsync_UsesPrimaryModel_WhenNoSummarizationModel()
    {
        _provider.PrimaryModel = "primary-model";
        _provider.SummarizationModel = string.Empty;
        MessageRequest? capturedRequest = null;
        _apiMock
            .Setup(a => a.ChatAsync(It.IsAny<MessageRequest>()))
            .Callback<MessageRequest>(r => capturedRequest = r)
            .ReturnsAsync(BuildChatResponse("assistant", "ok"));

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "text")));

        Assert.NotNull(capturedRequest);
        Assert.Equal("primary-model", capturedRequest!.Model);
    }

    [Fact]
    public async Task SummaryCompletionAsync_JoinsMessagesWithNewline_InPipelineInput()
    {
        _provider.SummarizationModel = "summary-model";
        PipelineRequest? capturedRequest = null;
        _apiMock
            .Setup(a => a.PipelineAsync<SummarizationResponse[]>(It.IsAny<string>(), It.IsAny<PipelineRequest>()))
            .Callback<string, PipelineRequest>((_, r) => capturedRequest = r)
            .ReturnsAsync([new SummarizationResponse { SummaryText = "x" }]);

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "hello"), ("assistant", "world")));

        Assert.NotNull(capturedRequest);
        Assert.Equal("user: hello\nassistant: world", capturedRequest!.Inputs);
    }
}

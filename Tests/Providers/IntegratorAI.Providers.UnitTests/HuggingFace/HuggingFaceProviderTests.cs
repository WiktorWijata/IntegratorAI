using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Infrastructure.Consts;
using IntegratorAI.Providers.Infrastructure.HuggingFace;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Models;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;
using IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;
using NSubstitute;

namespace IntegratorAI.Providers.UnitTests.HuggingFace;

public class HuggingFaceProviderTests
{
    private readonly IHuggingFaceApi _api = Substitute.For<IHuggingFaceApi>();
    private readonly HuggingFaceProvider _provider;

    public HuggingFaceProviderTests()
    {
        _provider = new HuggingFaceProvider(_api)
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
        _api
            .ChatAsync(Arg.Any<MessageRequest>())
            .Returns(BuildChatResponse("assistant", "reply"));

        var result = await _provider.CompletionAsync(BuildCompletion(("user", "hi")));

        Assert.Equal("assistant", result.Role);
        Assert.Equal("reply", result.Content);
    }

    [Fact]
    public async Task CompletionAsync_UsesPrimaryModel_InRequest()
    {
        _provider.PrimaryModel = "my-model";
        _api
            .ChatAsync(Arg.Any<MessageRequest>())
            .Returns(BuildChatResponse("assistant", "ok"));

        await _provider.CompletionAsync(BuildCompletion(("user", "hi")));

        await _api.Received(1).ChatAsync(Arg.Is<MessageRequest>(r => r.Model == "my-model"));
    }

    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenChoicesIsNull()
    {
        _api
            .ChatAsync(Arg.Any<MessageRequest>())
            .Returns(new MessageResponse { Choices = null });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.CompletionAsync(BuildCompletion(("user", "hi"))));
    }

    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenChoicesIsEmpty()
    {
        _api
            .ChatAsync(Arg.Any<MessageRequest>())
            .Returns(new MessageResponse { Choices = [] });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.CompletionAsync(BuildCompletion(("user", "hi"))));
    }


    [Fact]
    public async Task SummaryCompletionAsync_UsesPipelineApi_WhenSummarizationModelIsSet()
    {
        _provider.SummarizationModel = "summary-model";
        _api
            .PipelineAsync<SummarizationResponse[]>(Arg.Any<string>(), Arg.Any<PipelineRequest>())
            .Returns([new SummarizationResponse { SummaryText = "short" }]);

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "long text")));

        await _api.Received(1).PipelineAsync<SummarizationResponse[]>("summary-model", Arg.Any<PipelineRequest>());
    }

    [Fact]
    public async Task SummaryCompletionAsync_ReturnsSystemRoleMessage_WhenPipelineSucceeds()
    {
        _provider.SummarizationModel = "summary-model";
        _api
            .PipelineAsync<SummarizationResponse[]>(Arg.Any<string>(), Arg.Any<PipelineRequest>())
            .Returns([new SummarizationResponse { SummaryText = "summary here" }]);

        var result = await _provider.SummaryCompletionAsync(BuildCompletion(("user", "text")));

        Assert.Equal(PromptRoles.System, result.Role);
        Assert.Equal("summary here", result.Content);
    }

    [Fact]
    public async Task SummaryCompletionAsync_JoinsMessagesAsInput_WhenPipelineCalled()
    {
        _provider.SummarizationModel = "summary-model";
        PipelineRequest? capturedRequest = null;
        _api
            .PipelineAsync<SummarizationResponse[]>(Arg.Any<string>(), Arg.Do<PipelineRequest>(r => capturedRequest = r))
            .Returns([new SummarizationResponse { SummaryText = "x" }]);

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "hello"), ("assistant", "world")));

        Assert.NotNull(capturedRequest);
        Assert.Contains("user: hello", capturedRequest!.Inputs);
        Assert.Contains("assistant: world", capturedRequest.Inputs);
    }

    [Fact]
    public async Task SummaryCompletionAsync_ThrowsInvalidOperationException_WhenPipelineReturnsEmpty()
    {
        _provider.SummarizationModel = "summary-model";
        _api
            .PipelineAsync<SummarizationResponse[]>(Arg.Any<string>(), Arg.Any<PipelineRequest>())
            .Returns([]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.SummaryCompletionAsync(BuildCompletion(("user", "text"))));
    }


    [Fact]
    public async Task SummaryCompletionAsync_FallsBackToChat_WhenSummarizationModelIsEmpty()
    {
        _provider.SummarizationModel = string.Empty;
        _api
            .ChatAsync(Arg.Any<MessageRequest>())
            .Returns(BuildChatResponse("assistant", "summary"));

        var result = await _provider.SummaryCompletionAsync(BuildCompletion(("user", "text")));

        await _api.Received(1).ChatAsync(Arg.Any<MessageRequest>());
        Assert.Equal("summary", result.Content);
    }

    [Fact]
    public async Task SummaryCompletionAsync_AddsSummarizationSystemPrompt_WhenNoSummarizationModel()
    {
        _provider.SummarizationModel = string.Empty;
        MessageRequest? capturedRequest = null;
        _api
            .ChatAsync(Arg.Do<MessageRequest>(r => capturedRequest = r))
            .Returns(BuildChatResponse("assistant", "ok"));

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
        _api
            .ChatAsync(Arg.Do<MessageRequest>(r => capturedRequest = r))
            .Returns(BuildChatResponse("assistant", "ok"));

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
        _api
            .ChatAsync(Arg.Any<MessageRequest>())
            .Returns(new MessageResponse { Choices = [] });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _provider.SummaryCompletionAsync(BuildCompletion(("user", "text"))));
    }

    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenChoiceMessageIsNull()
    {
        _api
            .ChatAsync(Arg.Any<MessageRequest>())
            .Returns(new MessageResponse { Choices = [new Choice { Message = null }] });

        var result = await _provider.CompletionAsync(BuildCompletion(("user", "hi")));

        Assert.Null(result.Role);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task CompletionAsync_MapsAllMessagesToRequest()
    {
        MessageRequest? capturedRequest = null;
        _api
            .ChatAsync(Arg.Do<MessageRequest>(r => capturedRequest = r))
            .Returns(BuildChatResponse("assistant", "ok"));

        await _provider.CompletionAsync(BuildCompletion(("system", "sys"), ("user", "hi"), ("assistant", "hey")));

        Assert.NotNull(capturedRequest);
        Assert.Equal(3, capturedRequest!.Messages.Length);
    }

    [Fact]
    public async Task CompletionAsync_ConvertsRolesToLowercase_InRequest()
    {
        MessageRequest? capturedRequest = null;
        _api
            .ChatAsync(Arg.Do<MessageRequest>(r => capturedRequest = r))
            .Returns(BuildChatResponse("assistant", "ok"));

        await _provider.CompletionAsync(BuildCompletion(("USER", "hi"), ("ASSISTANT", "hey")));

        Assert.NotNull(capturedRequest);
        Assert.Equal("user", capturedRequest!.Messages[0].Role);
        Assert.Equal("assistant", capturedRequest.Messages[1].Role);
    }


    [Fact]
    public async Task SummaryCompletionAsync_UsesPrimaryModel_WhenNoSummarizationModel()
    {
        _provider.PrimaryModel = "primary-model";
        _provider.SummarizationModel = string.Empty;
        MessageRequest? capturedRequest = null;
        _api
            .ChatAsync(Arg.Do<MessageRequest>(r => capturedRequest = r))
            .Returns(BuildChatResponse("assistant", "ok"));

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "text")));

        Assert.NotNull(capturedRequest);
        Assert.Equal("primary-model", capturedRequest!.Model);
    }

    [Fact]
    public async Task SummaryCompletionAsync_JoinsMessagesWithNewline_InPipelineInput()
    {
        _provider.SummarizationModel = "summary-model";
        PipelineRequest? capturedRequest = null;
        _api
            .PipelineAsync<SummarizationResponse[]>(Arg.Any<string>(), Arg.Do<PipelineRequest>(r => capturedRequest = r))
            .Returns([new SummarizationResponse { SummaryText = "x" }]);

        await _provider.SummaryCompletionAsync(BuildCompletion(("user", "hello"), ("assistant", "world")));

        Assert.NotNull(capturedRequest);
        Assert.Equal("user: hello\nassistant: world", capturedRequest!.Inputs);
    }
}

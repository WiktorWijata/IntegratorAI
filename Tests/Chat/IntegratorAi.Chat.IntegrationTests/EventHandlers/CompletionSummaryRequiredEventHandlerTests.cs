using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Application.EventHandlers;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Events;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using NSubstitute;

namespace IntegratorAI.Chat.IntegrationTests.EventHandlers;

public class CompletionSummaryRequiredEventHandlerTests
{
    private readonly IProviderModule _providerModule;
    private readonly IChatUnitOfWork _unitOfWork;
    private readonly CompletionSummaryRequiredEventHandler _handler;

    public CompletionSummaryRequiredEventHandlerTests()
    {
        _providerModule = Substitute.For<IProviderModule>();
        _unitOfWork = Substitute.For<IChatUnitOfWork>();
        _handler = new CompletionSummaryRequiredEventHandler(_providerModule, _unitOfWork);
    }

    private static Completion BuildCompletion(int messageCount)
    {
        var completion = new Completion(new Message(MessageRole.User, "msg 1"));
        for (var i = 2; i <= messageCount; i++)
        {
            var role = i % 2 == 0 ? MessageRole.Assistant : MessageRole.User;
            completion.AddMessage(new Message(role, $"msg {i}"));
        }
        return completion;
    }

    [Fact]
    public async Task Handle_CallsSummaryCompletionAsync()
    {
        var completion = BuildCompletion(4);
        _providerModule
            .SummaryCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "summary text" });

        await _handler.Handle(new CompletionSummaryRequiredEvent(completion));

        await _providerModule
            .Received(1)
            .SummaryCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SetsSummaryOnCompletion()
    {
        var completion = BuildCompletion(4);
        _providerModule
            .SummaryCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "summary text" });

        await _handler.Handle(new CompletionSummaryRequiredEvent(completion));

        Assert.NotNull(completion.Summary);
        Assert.Equal("summary text", completion.Summary!.Content);
    }

    [Fact]
    public async Task Handle_SetsSummarizedUpToIndex_AsMaxUnsummarizedIndex()
    {
        var completion = BuildCompletion(4);
        _providerModule
            .SummaryCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "summary" });

        await _handler.Handle(new CompletionSummaryRequiredEvent(completion));

        Assert.Equal(4, completion.Summary!.SummarizedUpToIndex);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        var completion = BuildCompletion(4);
        _providerModule
            .SummaryCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "summary" });

        await _handler.Handle(new CompletionSummaryRequiredEvent(completion));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IncludesExistingSummaryAsSystemMessage()
    {
        var completion = BuildCompletion(6);
        completion.SetSummary("previous summary", summarizedUpToIndex: 2);

        _providerModule
            .SummaryCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "new summary" });

        await _handler.Handle(new CompletionSummaryRequiredEvent(completion));

        await _providerModule
            .Received(1)
            .SummaryCompletionAsync(
                Arg.Is<ProviderCompletionDto>(dto =>
                    dto.Messages[0].Role == "system" &&
                    dto.Messages[0].Content.Contains("previous summary")),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotIncludeSystemMessage_WhenNoExistingSummary()
    {
        var completion = BuildCompletion(4);
        _providerModule
            .SummaryCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "summary" });

        await _handler.Handle(new CompletionSummaryRequiredEvent(completion));

        await _providerModule
            .Received(1)
            .SummaryCompletionAsync(
                Arg.Is<ProviderCompletionDto>(dto =>
                    dto.Messages.All(m => m.Role != "system")),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UpdatesExistingSummary_WhenCalledTwice()
    {
        var completion = BuildCompletion(4);
        completion.SetSummary("old summary", summarizedUpToIndex: 2);

        _providerModule
            .SummaryCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "updated summary" });

        await _handler.Handle(new CompletionSummaryRequiredEvent(completion));

        Assert.Equal("updated summary", completion.Summary!.Content);
        Assert.NotNull(completion.Summary.ModifiedAt);
    }
}


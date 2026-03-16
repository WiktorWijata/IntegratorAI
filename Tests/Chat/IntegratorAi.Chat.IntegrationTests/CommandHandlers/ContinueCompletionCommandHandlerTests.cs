using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Application.CommandHandlers;
using IntegratorAI.Chat.Contracts.Commands;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Persistence;
using IntegratorAI.Chat.Persistence.Repositories;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace IntegratorAI.Chat.IntegrationTests.CommandHandlers;

public class ContinueCompletionCommandHandlerTests : IDisposable
{
    private readonly ChatDbContext _context;
    private readonly CompletionRepository _repository;
    private readonly IProviderModule _providerModule;
    private readonly ContinueCompletionCommandHandler _handler;

    public ContinueCompletionCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ChatDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new CompletionRepository(_context);
        _providerModule = Substitute.For<IProviderModule>();

        _handler = new ContinueCompletionCommandHandler(
            _providerModule,
            _repository,
            new ChatSettings { SummarizationThreshold = 10 });
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    private async Task<Completion> SeedCompletionAsync(params Message[] additionalMessages)
    {
        var completion = new Completion(new Message(MessageRole.User, "initial message"));
        foreach (var msg in additionalMessages)
            completion.AddMessage(msg);

        await _repository.AddAsync(completion);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        return completion;
    }

    [Fact]
    public async Task Handle_ThrowsInvalidOperationException_WhenCompletionNotFound()
    {
        var command = new ContinueCompletionCommand(Guid.NewGuid(), "follow up");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsResponseWithAllMessages()
    {
        var completion = await SeedCompletionAsync(new Message(MessageRole.Assistant, "first reply"));
        _providerModule
            .CompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "continued reply" });

        var result = await _handler.Handle(
            new ContinueCompletionCommand(completion.Id, "follow up"),
            CancellationToken.None);

        Assert.Equal(4, result.Messages.Length);
        Assert.Equal("continued reply", result.Messages[3].Content);
    }

    [Fact]
    public async Task Handle_SendsUnsummarizedMessagesToProvider()
    {
        var completion = await SeedCompletionAsync(new Message(MessageRole.Assistant, "reply 1"));
        _providerModule
            .CompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "reply" });

        await _handler.Handle(
            new ContinueCompletionCommand(completion.Id, "follow up"),
            CancellationToken.None);

        await _providerModule
            .Received(1)
            .CompletionAsync(
                Arg.Is<ProviderCompletionDto>(dto => dto.Messages.Length == 3),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IncludesSummaryAsSystemMessage_WhenSummaryExists()
    {
        var completion = await SeedCompletionAsync();
        _context.ChangeTracker.Clear();

        var loaded = await _repository.GetByIdAsync(completion.Id);
        loaded!.SetSummary("conversation summary", summarizedUpToIndex: 1);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        _providerModule
            .CompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "reply" });

        await _handler.Handle(
            new ContinueCompletionCommand(completion.Id, "next question"),
            CancellationToken.None);

        await _providerModule
            .Received(1)
            .CompletionAsync(
                Arg.Is<ProviderCompletionDto>(dto =>
                    dto.Messages[0].Role == "System" &&
                    dto.Messages[0].Content.Contains("conversation summary")),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotIncludeSystemMessage_WhenNoSummary()
    {
        var completion = await SeedCompletionAsync();
        _providerModule
            .CompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "reply" });

        await _handler.Handle(
            new ContinueCompletionCommand(completion.Id, "follow up"),
            CancellationToken.None);

        await _providerModule
            .Received(1)
            .CompletionAsync(
                Arg.Is<ProviderCompletionDto>(dto =>
                    dto.Messages.All(m => m.Role != "System")),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TriggersSummarization_WhenThresholdExceeded()
    {
        var handler = new ContinueCompletionCommandHandler(
            _providerModule,
            _repository,
            new ChatSettings { SummarizationThreshold = 2 });

        var completion = await SeedCompletionAsync(
            new Message(MessageRole.Assistant, "reply 1"),
            new Message(MessageRole.User, "msg 3"));

        _providerModule
            .CompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new ProviderMessageDto { Role = "Assistant", Content = "reply" });

        var result = await handler.Handle(
            new ContinueCompletionCommand(completion.Id, "msg 5"),
            CancellationToken.None);

        Assert.Equal(5, result.Messages.Length);
    }
}


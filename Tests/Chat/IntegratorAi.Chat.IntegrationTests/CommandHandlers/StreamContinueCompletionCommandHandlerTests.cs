using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Application.CommandHandlers;
using IntegratorAI.Chat.Application.Commands;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Events;
using IntegratorAI.Chat.Persistence;
using IntegratorAI.Chat.Persistence.Repositories;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace IntegratorAI.Chat.IntegrationTests.CommandHandlers;

public class StreamContinueCompletionCommandHandlerTests : IDisposable
{
    private readonly ChatDbContext _context;
    private readonly CompletionRepository _repository;
    private readonly IProviderModule _providerModule;
    private readonly StreamContinueCompletionCommandHandler _handler;

    public StreamContinueCompletionCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ChatDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new CompletionRepository(_context);
        _providerModule = Substitute.For<IProviderModule>();

        _handler = new StreamContinueCompletionCommandHandler(
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

    private static async IAsyncEnumerable<string> StreamTokens(params string[] tokens)
    {
        foreach (var token in tokens)
            yield return token;
    }

    [Fact]
    public async Task Handle_ThrowsInvalidOperationException_WhenCompletionNotFound()
    {
        var command = new StreamContinueCompletionCommand(Guid.NewGuid(), "follow up");

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in _handler.Handle(command, CancellationToken.None)) { }
        });
    }

    [Fact]
    public async Task Handle_YieldsAllTokens_FromProvider()
    {
        var completion = await SeedCompletionAsync(new Message(MessageRole.Assistant, "first reply"));
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("tok1", "tok2"));

        var tokens = new List<string>();
        await foreach (var token in _handler.Handle(new StreamContinueCompletionCommand(completion.Id, "follow up"), CancellationToken.None))
            tokens.Add(token);

        Assert.Equal(["tok1", "tok2"], tokens);
    }

    [Fact]
    public async Task Handle_AddsUserAndAssistantMessages_InDatabase()
    {
        var completion = await SeedCompletionAsync(new Message(MessageRole.Assistant, "first reply"));
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("second reply"));

        await foreach (var _ in _handler.Handle(new StreamContinueCompletionCommand(completion.Id, "follow up"), CancellationToken.None)) { }

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var saved = await _repository.GetByIdAsync(completion.Id);
        Assert.Equal(4, saved!.Messages.Count);
        Assert.Equal("second reply", saved.Messages.Last().Content);
        Assert.Equal(MessageRole.Assistant, saved.Messages.Last().Role);
    }

    [Fact]
    public async Task Handle_TriggersSummarization_WhenThresholdExceeded()
    {
        var handler = new StreamContinueCompletionCommandHandler(
            _providerModule,
            _repository,
            new ChatSettings { SummarizationThreshold = 2 });

        var completion = await SeedCompletionAsync(
            new Message(MessageRole.Assistant, "reply 1"),
            new Message(MessageRole.User, "msg 3"));

        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("reply 2"));

        await foreach (var _ in handler.Handle(new StreamContinueCompletionCommand(completion.Id, "msg 4"), CancellationToken.None)) { }

        var tracked = _context.ChangeTracker.Entries<Completion>().Single().Entity;
        Assert.Contains(tracked.GetDomainEvents(), e => e is CompletionSummaryRequiredEvent);
    }

    [Fact]
    public async Task Handle_DoesNotTriggerSummarization_WhenBelowThreshold()
    {
        var completion = await SeedCompletionAsync();
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("reply"));

        await foreach (var _ in _handler.Handle(new StreamContinueCompletionCommand(completion.Id, "follow up"), CancellationToken.None)) { }

        var tracked = _context.ChangeTracker.Entries<Completion>().Single().Entity;
        Assert.DoesNotContain(tracked.GetDomainEvents(), e => e is CompletionSummaryRequiredEvent);
    }
}

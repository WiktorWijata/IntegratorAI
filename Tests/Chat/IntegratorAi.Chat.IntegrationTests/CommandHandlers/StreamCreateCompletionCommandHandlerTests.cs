using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Application.CommandHandlers;
using IntegratorAI.Chat.Application.Commands;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Persistence;
using IntegratorAI.Chat.Persistence.Repositories;
using IntegratorAI.Context.Contracts;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace IntegratorAI.Chat.IntegrationTests.CommandHandlers;

public class StreamCreateCompletionCommandHandlerTests : IDisposable
{
    private readonly ChatDbContext _context;
    private readonly CompletionRepository _repository;
    private readonly IProviderModule _providerModule;
    private readonly IContextModule _contextModule;
    private readonly StreamCreateCompletionCommandHandler _handler;

    public StreamCreateCompletionCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ChatDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new CompletionRepository(_context);
        _providerModule = Substitute.For<IProviderModule>();
        _contextModule = Substitute.For<IContextModule>();

        _handler = new StreamCreateCompletionCommandHandler(_providerModule, _contextModule, _repository);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    private static async IAsyncEnumerable<string> StreamTokens(params string[] tokens)
    {
        foreach (var token in tokens)
            yield return token;
    }

    [Fact]
    public async Task Handle_YieldsAllTokens_FromProvider()
    {
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("Hello", " world", "!"));

        var tokens = new List<string>();
        await foreach (var token in _handler.Handle(new StreamCreateCompletionCommand("Hi"), CancellationToken.None))
            tokens.Add(token);

        Assert.Equal(["Hello", " world", "!"], tokens);
    }

    [Fact]
    public async Task Handle_PersistsCompletion_InDatabase()
    {
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("response"));

        await foreach (var _ in _handler.Handle(new StreamCreateCompletionCommand("Hello"), CancellationToken.None)) { }

        var completionId = _context.ChangeTracker.Entries<Completion>().Single().Entity.Id;
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var saved = await _repository.GetByIdAsync(completionId);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Handle_AddsAssistantMessage_AfterStreaming()
    {
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("tok1", "tok2"));

        await foreach (var _ in _handler.Handle(new StreamCreateCompletionCommand("Hello"), CancellationToken.None)) { }

        var completion = _context.ChangeTracker.Entries<Completion>().Single().Entity;
        Assert.Equal(2, completion.Messages.Count);
        Assert.Equal("tok1tok2", completion.Messages.Last().Content);
        Assert.Equal(MessageRole.Assistant, completion.Messages.Last().Role);
    }

    [Fact]
    public async Task Handle_FetchesContextPrompt_WhenContextIdProvided()
    {
        var contextId = Guid.NewGuid();
        _contextModule
            .GetContextPrompt(contextId, Arg.Any<CancellationToken>())
            .Returns("You are a helpful assistant");
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("response"));

        await foreach (var _ in _handler.Handle(new StreamCreateCompletionCommand("Hello", contextId), CancellationToken.None)) { }

        await _contextModule.Received(1).GetContextPrompt(contextId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotCallContextModule_WhenContextIdIsNull()
    {
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("response"));

        await foreach (var _ in _handler.Handle(new StreamCreateCompletionCommand("Hello", null), CancellationToken.None)) { }

        await _contextModule.DidNotReceive().GetContextPrompt(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SystemMessage_ContainsContextPrompt()
    {
        var contextId = Guid.NewGuid();
        _contextModule
            .GetContextPrompt(contextId, Arg.Any<CancellationToken>())
            .Returns("You are a customer support agent");
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(StreamTokens("response"));

        await foreach (var _ in _handler.Handle(new StreamCreateCompletionCommand("Hello", contextId), CancellationToken.None)) { }

        _providerModule
            .Received(1)
            .StreamCompletionAsync(
                Arg.Is<ProviderCompletionDto>(dto =>
                    dto.Messages.Any(m => m.Role == "System" && m.Content.Contains("You are a customer support agent"))),
                Arg.Any<CancellationToken>());
    }
}

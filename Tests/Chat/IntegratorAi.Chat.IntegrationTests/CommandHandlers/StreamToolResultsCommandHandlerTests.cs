using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.Chat.Application;
using IntegratorAI.Chat.Application.CommandHandlers;
using IntegratorAI.Chat.Application.Commands;
using IntegratorAI.Chat.Contracts.Models;
using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Persistence;
using IntegratorAI.Chat.Persistence.Repositories;
using IntegratorAI.Context.Contracts;
using IntegratorAI.Context.Contracts.Models;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using RescuePC.Software.Domain.Exceptions;

namespace IntegratorAI.Chat.IntegrationTests.CommandHandlers;

public class StreamToolResultsCommandHandlerTests : IDisposable
{
    private readonly ChatDbContext _context;
    private readonly CompletionRepository _repository;
    private readonly IProviderModule _providerModule = Substitute.For<IProviderModule>();
    private readonly IContextModule _contextModule = Substitute.For<IContextModule>();
    private readonly ChatSettings _settings = new() { SummarizationThreshold = 100 };
    private readonly StreamToolResultsCommandHandler _handler;
    private readonly Guid _contextId = Guid.NewGuid();

    public StreamToolResultsCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ChatDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new CompletionRepository(_context);

        _contextModule.GetContextTools(_contextId, Arg.Any<CancellationToken>()).Returns(
            Task.FromResult<IReadOnlyList<ToolDto>>(
            [
                new ToolDto("get_project", "Details of a project", [new ToolParameterDto("id", "long", "Project id", required: true)]),
            ]));

        _handler = new StreamToolResultsCommandHandler(_providerModule, _contextModule, _repository, _settings);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    private static async IAsyncEnumerable<ProviderStreamEvent> Stream(params ProviderStreamEvent[] events)
    {
        foreach (var item in events)
            yield return item;

        await Task.CompletedTask;
    }

    private async Task<Completion> SeedWaitingCompletionAsync(Guid? contextId = null)
    {
        var completion = new Completion(new Message(MessageRole.User, "project 5?"), new Message(MessageRole.System, "context"), contextId);
        completion.AddMessage(Message.ForToolCalls(string.Empty, [new ToolCall("call_1", "get_project", "{\"id\":5}")]));

        await _repository.AddAsync(completion);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        return completion;
    }

    private static StreamToolResultsCommand Results(Guid completionId, params ToolResultDto[] results)
        => new(completionId, results);

    private static ToolResultDto Result(string id, string content, bool isError = false)
        => new() { ToolCallId = id, Content = content, IsError = isError };

    private static async Task<List<CompletionStreamEvent>> RunAsync(IAsyncEnumerable<CompletionStreamEvent> stream)
    {
        var all = new List<CompletionStreamEvent>();
        await foreach (var item in stream)
            all.Add(item);
        return all;
    }

    [Fact]
    public async Task The_model_writes_the_answer_from_the_results_and_it_is_kept()
    {
        var completion = await SeedWaitingCompletionAsync(_contextId);
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(Stream(ProviderStreamEvent.ForToken("It is "), ProviderStreamEvent.ForToken("a site.")));

        var events = await RunAsync(_handler.Handle(Results(completion.Id, Result("call_1", "{\"name\":\"X\"}")), CancellationToken.None));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        Assert.Equal(["It is ", "a site."], events.Select(e => e.Data).ToArray());
        var saved = await _repository.GetByIdAsync(completion.Id);
        var messages = saved!.Messages.OrderBy(m => m.Index).ToList();
        Assert.Equal([MessageRole.System, MessageRole.User, MessageRole.Assistant, MessageRole.Tool, MessageRole.Assistant], messages.Select(m => m.Role).ToArray());
        Assert.Equal("call_1", messages[3].ToolCallId);
        Assert.Equal("{\"name\":\"X\"}", messages[3].Content);
        Assert.Equal("It is a site.", messages[4].Content);
        Assert.Equal("call_1", messages[2].ToolCalls.Single().Id);
    }

    [Fact]
    public async Task The_provider_gets_the_whole_conversation_and_the_tools_of_the_context()
    {
        var completion = await SeedWaitingCompletionAsync(_contextId);
        ProviderCompletionDto? sent = null;
        _providerModule
            .StreamCompletionAsync(Arg.Do<ProviderCompletionDto>(d => sent = d), Arg.Any<CancellationToken>())
            .Returns(Stream(ProviderStreamEvent.ForToken("ok")));

        await RunAsync(_handler.Handle(Results(completion.Id, Result("call_1", "data")), CancellationToken.None));

        Assert.NotNull(sent);
        Assert.Equal(["System", "User", "Assistant", "Tool"], sent!.Messages.Select(m => m.Role).ToArray());
        Assert.Equal("call_1", sent.Messages[2].ToolCalls!.Single().Id);
        Assert.Equal("call_1", sent.Messages[3].ToolCallId);
        var tool = Assert.Single(sent.Tools!);
        Assert.Equal("get_project", tool.Name);
        Assert.True(tool.Parameters.Single().Required);
    }

    [Fact]
    public async Task A_failed_tool_is_told_to_the_model_as_an_error()
    {
        var completion = await SeedWaitingCompletionAsync(_contextId);
        ProviderCompletionDto? sent = null;
        _providerModule
            .StreamCompletionAsync(Arg.Do<ProviderCompletionDto>(d => sent = d), Arg.Any<CancellationToken>())
            .Returns(Stream(ProviderStreamEvent.ForToken("sorry")));

        await RunAsync(_handler.Handle(Results(completion.Id, Result("call_1", "not found", isError: true)), CancellationToken.None));

        Assert.Equal("Error: not found", sent!.Messages[3].Content);
    }

    [Fact]
    public async Task The_model_may_ask_for_more_tools_and_the_completion_waits_again()
    {
        var completion = await SeedWaitingCompletionAsync(_contextId);
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(Stream(ProviderStreamEvent.ForToolCall(new ProviderToolCallDto { Id = "call_2", Name = "get_project", Arguments = "{\"id\":6}" })));

        var events = await RunAsync(_handler.Handle(Results(completion.Id, Result("call_1", "data")), CancellationToken.None));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var asked = Assert.Single(events);
        Assert.Equal(CompletionStreamEventType.ToolCall, asked.Type);
        Assert.Equal("call_2", asked.ToolCall.Id);
        var saved = await _repository.GetByIdAsync(completion.Id);
        Assert.Equal("call_2", saved!.PendingToolCalls.Single().Id);
    }

    [Fact]
    public async Task The_completion_is_not_found_when_it_does_not_exist()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => RunAsync(_handler.Handle(Results(Guid.NewGuid(), Result("call_1", "x")), CancellationToken.None)));
    }

    [Fact]
    public async Task Results_for_a_completion_that_is_not_waiting_are_a_conflict()
    {
        var completion = new Completion(new Message(MessageRole.User, "hi"));
        completion.AddMessage(new Message(MessageRole.Assistant, "hello"));
        await _repository.AddAsync(completion);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<ConflictException>(
            () => RunAsync(_handler.Handle(Results(completion.Id, Result("call_1", "x")), CancellationToken.None)));
    }

    [Fact]
    public async Task Results_for_another_call_are_a_conflict_and_the_provider_is_not_asked()
    {
        var completion = await SeedWaitingCompletionAsync(_contextId);

        await Assert.ThrowsAsync<ConflictException>(
            () => RunAsync(_handler.Handle(Results(completion.Id, Result("invented", "x")), CancellationToken.None)));

        _providerModule.DidNotReceiveWithAnyArgs().StreamCompletionAsync(default!, default);
    }

    [Fact]
    public async Task The_same_call_answered_twice_is_a_conflict()
    {
        var completion = await SeedWaitingCompletionAsync(_contextId);

        await Assert.ThrowsAsync<ConflictException>(
            () => RunAsync(_handler.Handle(Results(completion.Id, Result("call_1", "a"), Result("call_1", "b")), CancellationToken.None)));
    }

    [Fact]
    public async Task A_new_prompt_cannot_continue_a_completion_that_waits_for_tools()
    {
        var completion = await SeedWaitingCompletionAsync(_contextId);
        var continueHandler = new StreamContinueCompletionCommandHandler(_providerModule, _contextModule, _repository, _settings);

        await Assert.ThrowsAsync<ConflictException>(
            () => RunAsync(continueHandler.Handle(new StreamContinueCompletionCommand(completion.Id, "and another?"), CancellationToken.None)));
    }

    [Fact]
    public async Task A_completion_created_with_a_context_remembers_it_and_passes_its_tools_to_the_model()
    {
        _providerModule
            .StreamCompletionAsync(Arg.Any<ProviderCompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(Stream(ProviderStreamEvent.ForToolCall(new ProviderToolCallDto { Id = "call_1", Name = "get_project", Arguments = "{\"id\":5}" })));
        _contextModule.GetContextPrompt(_contextId, Arg.Any<CancellationToken>()).Returns("prompt");
        var createHandler = new StreamCreateCompletionCommandHandler(_providerModule, _contextModule, _repository);

        var events = await RunAsync(createHandler.Handle(new StreamCreateCompletionCommand("project 5?", _contextId), CancellationToken.None));
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var completionId = Guid.Parse(events[0].Data);
        var saved = await _repository.GetByIdAsync(completionId);
        Assert.Equal(_contextId, saved!.ContextId);
        Assert.Equal("get_project", saved.PendingToolCalls.Single().Name);
        _providerModule.Received(1).StreamCompletionAsync(
            Arg.Is<ProviderCompletionDto>(d => d.Tools != null && d.Tools.Length == 1), Arg.Any<CancellationToken>());
    }
}

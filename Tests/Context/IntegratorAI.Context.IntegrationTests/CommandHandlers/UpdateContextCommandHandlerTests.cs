using IntegratorAI.Context.Application.CommandHandlers;
using IntegratorAI.Context.Application.Commands;
using IntegratorAI.Context.Contracts.Models;
using IntegratorAI.Context.Persistence;
using IntegratorAI.Context.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using RescuePC.Software.Caching;
using RescuePC.Software.Domain.Exceptions;

namespace IntegratorAI.Context.IntegrationTests.CommandHandlers;

public class UpdateContextCommandHandlerTests : IDisposable
{
    private readonly ContextDbContext _context;
    private readonly ContextRepository _repository;
    private readonly ICacheProvider _cacheProvider;
    private readonly CreateContextCommandHandler _createHandler;
    private readonly UpdateContextCommandHandler _handler;

    public UpdateContextCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ContextDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ContextDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new ContextRepository(_context);
        _cacheProvider = Substitute.For<ICacheProvider>();
        _createHandler = new CreateContextCommandHandler(_repository);
        _handler = new UpdateContextCommandHandler(_repository, _cacheProvider);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    private async Task<Guid> CreateContextAsync(IEnumerable<ToolDto>? tools = null, IEnumerable<ExampleDto>? examples = null)
    {
        var id = await _createHandler.Handle(
            new CreateContextCommand("old name", "old role", "old domain", "old policy", "old rules", "old format", tools!, examples!),
            CancellationToken.None);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return id;
    }

    [Fact]
    public async Task Handle_UpdatesScalarFields()
    {
        var id = await CreateContextAsync();

        await _handler.Handle(
            new UpdateContextCommand(id, "new name", "new role", "new domain", "new policy", "new rules", "new format"),
            CancellationToken.None);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var saved = await _repository.GetByIdAsync(id);
        Assert.NotNull(saved);
        Assert.Equal("new name", saved.Name);
        Assert.Equal("new role", saved.SystemRole);
        Assert.Equal("new domain", saved.DomainContext);
        Assert.Equal("new policy", saved.DecisionPolicy);
        Assert.Equal("new rules", saved.OperatingRules);
        Assert.Equal("new format", saved.OutputFormat);
    }

    [Fact]
    public async Task Handle_ReplacesToolsAndExamples_AndRemovesOldOnes()
    {
        var id = await CreateContextAsync(
            tools: [new ToolDto("old-tool", "old", [new ToolParameterDto("p", "string", "param")], [new GuardrailDto("g", false)])],
            examples: [new ExampleDto("old input", "old output")]);

        await _handler.Handle(
            new UpdateContextCommand(id, "n", "r", null!, null!, null!, null!,
                tools: [new ToolDto("new-tool", "new")],
                examples: [new ExampleDto("new input", "new output"), new ExampleDto("second", "answer")]),
            CancellationToken.None);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var saved = await _repository.GetByIdAsync(id);
        Assert.NotNull(saved);
        Assert.Equal("new-tool", Assert.Single(saved.Tools).Name);
        Assert.Equal(2, saved.Examples.Count);
        Assert.DoesNotContain(saved.Examples, e => e.Input == "old input");
        Assert.Equal(1, await _context.Set<Domain.Tool>().CountAsync());
        Assert.Equal(0, await _context.Set<Domain.ToolParameter>().CountAsync());
        Assert.Equal(0, await _context.Set<Domain.Guardrail>().CountAsync());
    }

    [Fact]
    public async Task Handle_ClearsToolsAndExamples_WhenNullProvided()
    {
        var id = await CreateContextAsync(
            tools: [new ToolDto("tool", "desc")],
            examples: [new ExampleDto("in", "out")]);

        await _handler.Handle(
            new UpdateContextCommand(id, "n", "r", null!, null!, null!, null!),
            CancellationToken.None);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var saved = await _repository.GetByIdAsync(id);
        Assert.NotNull(saved);
        Assert.Empty(saved.Tools);
        Assert.Empty(saved.Examples);
    }

    [Fact]
    public async Task Handle_RemovesCachedPrompt()
    {
        var id = await CreateContextAsync();

        await _handler.Handle(
            new UpdateContextCommand(id, "n", "r", null!, null!, null!, null!),
            CancellationToken.None);

        await _cacheProvider.Received(1).RemoveAsync($"context-prompt:{id}");
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenContextDoesNotExist()
    {
        var command = new UpdateContextCommand(Guid.NewGuid(), "n", "r", null!, null!, null!, null!);

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        await _cacheProvider.DidNotReceive().RemoveAsync(Arg.Any<string>());
    }
}

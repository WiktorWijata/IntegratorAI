using IntegratorAI.Context.Application.CommandHandlers;
using IntegratorAI.Context.Application.Commands;
using IntegratorAI.Context.Contracts.Models;
using IntegratorAI.Context.Domain;
using IntegratorAI.Context.Persistence;
using IntegratorAI.Context.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Context.IntegrationTests.CommandHandlers;

public class CreateContextCommandHandlerTests : IDisposable
{
    private readonly ContextDbContext _context;
    private readonly ContextRepository _repository;
    private readonly CreateContextCommandHandler _handler;

    public CreateContextCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ContextDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ContextDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new ContextRepository(_context);
        _handler = new CreateContextCommandHandler(_repository);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ReturnsNonEmptyContextId()
    {
        var command = new CreateContextCommand("test", "You are helpful", null, null, null, null);

        var id = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Handle_PersistsContextInDatabase()
    {
        var command = new CreateContextCommand("my context", "You are an assistant", "domain info", null, null, null);

        var id = await _handler.Handle(command, CancellationToken.None);

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var saved = await _repository.GetByIdAsync(id);

        Assert.NotNull(saved);
        Assert.Equal("my context", saved.Name);
        Assert.Equal("You are an assistant", saved.SystemRole);
        Assert.Equal("domain info", saved.DomainContext);
    }

    [Fact]
    public async Task Handle_PersistsTools_WithParametersAndGuardrails()
    {
        var tools = new List<ToolDto>
        {
            new ToolDto(
                name: "search",
                description: "Search the web",
                parameters: [new ToolParameterDto("query", "string", "Search query")],
                guardrails: [new GuardrailDto("No PII", requiresConfirmation: true)])
        };
        var command = new CreateContextCommand("ctx", "role", null, null, null, null, tools);

        var id = await _handler.Handle(command, CancellationToken.None);

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var saved = await _repository.GetByIdAsync(id);

        Assert.NotNull(saved);
        Assert.Single(saved.Tools);
        var tool = saved.Tools.First();
        Assert.Equal("search", tool.Name);
        Assert.Single(tool.Parameters);
        Assert.Single(tool.Guardrails);
    }

    [Fact]
    public async Task Handle_HandlesNullTools_AndNullExamples()
    {
        var command = new CreateContextCommand("ctx", "role", null, null, null, null, null, null);

        var id = await _handler.Handle(command, CancellationToken.None);

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var saved = await _repository.GetByIdAsync(id);

        Assert.NotNull(saved);
        Assert.Empty(saved.Tools);
        Assert.Empty(saved.Examples);
    }
}

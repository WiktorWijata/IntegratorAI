using IntegratorAI.Context.Domain;
using IntegratorAI.Context.Persistence;
using IntegratorAI.Context.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Context.IntegrationTests;

public class ContextRepositoryTests : IDisposable
{
    private readonly ContextDbContext _context;
    private readonly ContextRepository _repository;

    public ContextRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ContextDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ContextDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new ContextRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    private async Task SaveContextAsync(Domain.Context context)
    {
        await _repository.AddAsync(context);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_PersistsContext()
    {
        var context = new Domain.Context("test context", "You are helpful", null, null, null, null, null, null);
        await SaveContextAsync(context);

        var saved = await _repository.GetByIdAsync(context.Id);

        Assert.NotNull(saved);
        Assert.Equal("test context", saved.Name);
        Assert.Equal("You are helpful", saved.SystemRole);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsContextWithTools()
    {
        var tools = new List<Tool> { new Tool("search", "Search the web", null, null) };
        var context = new Domain.Context("ctx", "role", null, null, null, null, tools, null);
        await SaveContextAsync(context);

        var saved = await _repository.GetByIdAsync(context.Id);

        Assert.NotNull(saved);
        Assert.Single(saved.Tools);
        Assert.Equal("search", saved.Tools.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsContextWithToolParameters()
    {
        var parameters = new List<ToolParameter>
        {
            new ToolParameter { Name = "query", Type = "string", Description = "The search query" }
        };
        var tools = new List<Tool> { new Tool("search", "Search", parameters, null) };
        var context = new Domain.Context("ctx", "role", null, null, null, null, tools, null);
        await SaveContextAsync(context);

        var saved = await _repository.GetByIdAsync(context.Id);

        Assert.NotNull(saved);
        var tool = saved.Tools.First();
        Assert.Single(tool.Parameters);
        Assert.Equal("query", tool.Parameters.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsContextWithGuardrails()
    {
        var guardrails = new List<Guardrail> { new Guardrail("No PII", requiresConfirmation: true) };
        var tools = new List<Tool> { new Tool("tool1", "desc", null, guardrails) };
        var context = new Domain.Context("ctx", "role", null, null, null, null, tools, null);
        await SaveContextAsync(context);

        var saved = await _repository.GetByIdAsync(context.Id);

        Assert.NotNull(saved);
        var tool = saved.Tools.First();
        Assert.Single(tool.Guardrails);
        Assert.Equal("No PII", tool.Guardrails.First().Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsContextWithExamples()
    {
        var examples = new List<Example> { new Example("What is 2+2?", "4") };
        var context = new Domain.Context("ctx", "role", null, null, null, null, null, examples);
        await SaveContextAsync(context);

        var saved = await _repository.GetByIdAsync(context.Id);

        Assert.NotNull(saved);
        Assert.Single(saved.Examples);
        Assert.Equal("What is 2+2?", saved.Examples.First().Input);
        Assert.Equal("4", saved.Examples.First().ExpectedResponse);
    }
}

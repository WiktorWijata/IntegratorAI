using IntegratorAI.BuildingBlocks.Domain;
using IntegratorAI.Context.Application.PromptBuilder;
using IntegratorAI.Context.Application.Queries;
using IntegratorAI.Context.Application.QueryHandlers;
using IntegratorAI.Context.Domain;
using IntegratorAI.Context.Persistence;
using IntegratorAI.Context.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Context.IntegrationTests.QueryHandlers;

public class GetContextPromptQueryHandlerTests : IDisposable
{
    private readonly ContextDbContext _context;
    private readonly ContextRepository _repository;
    private readonly GetContextPromptQueryHandler _handler;

    public GetContextPromptQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ContextDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ContextDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new ContextRepository(_context);
        _handler = new GetContextPromptQueryHandler(_repository);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    private async Task<Domain.Context> SeedContextAsync(Domain.Context context)
    {
        await _repository.AddAsync(context);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return context;
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenContextNotFound()
    {
        var query = new GetContextPromptQuery(Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsNonEmptyYaml()
    {
        var context = await SeedContextAsync(
            new Domain.Context("test", "You are helpful", null, null, null, null, null, null));

        var yaml = await _handler.Handle(new GetContextPromptQuery(context.Id), CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(yaml));
    }

    [Fact]
    public async Task Handle_ReturnsYaml_ContainingSystemRole()
    {
        var context = await SeedContextAsync(
            new Domain.Context("test", "You are a customer support agent", null, null, null, null, null, null));

        var yaml = await _handler.Handle(new GetContextPromptQuery(context.Id), CancellationToken.None);

        Assert.Contains("SYSTEM_ROLE", yaml);
        Assert.Contains("You are a customer support agent", yaml);
    }
}

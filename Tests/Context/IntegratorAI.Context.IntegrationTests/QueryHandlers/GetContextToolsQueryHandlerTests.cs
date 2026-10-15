using IntegratorAI.Context.Application.Queries;
using IntegratorAI.Context.Application.QueryHandlers;
using IntegratorAI.Context.Domain;
using IntegratorAI.Context.Persistence;
using IntegratorAI.Context.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using RescuePC.Software.Domain.Exceptions;

namespace IntegratorAI.Context.IntegrationTests.QueryHandlers;

public class GetContextToolsQueryHandlerTests : IDisposable
{
    private readonly ContextDbContext _context;
    private readonly ContextRepository _repository;
    private readonly GetContextToolsQueryHandler _handler;

    public GetContextToolsQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ContextDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ContextDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new ContextRepository(_context);
        _handler = new GetContextToolsQueryHandler(_repository);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public async Task The_tools_come_with_their_parameters_and_whether_they_are_required()
    {
        var context = new Domain.Context(
            name: "ctx",
            systemRole: "role",
            domainContext: null,
            decisionPolicy: null,
            operatingRules: null,
            outputFormat: null,
            tools:
            [
                new Tool("get_project", "Details of a project",
                [
                    new ToolParameter { Name = "id", Type = "long", Description = "Project id", Required = true },
                    new ToolParameter { Name = "language", Type = "string", Description = "Language" },
                ], guardrails: null),
                new Tool("get_experiences", "Experience", parameters: null, guardrails: null),
            ],
            examples: null);
        await _repository.AddAsync(context);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var tools = await _handler.Handle(new GetContextToolsQuery(context.Id), CancellationToken.None);

        Assert.Equal(["get_experiences", "get_project"], tools.Select(t => t.Name).ToArray());
        var project = tools.Single(t => t.Name == "get_project");
        Assert.Equal(["id", "language"], project.Parameters.Select(p => p.Name).OrderBy(n => n).ToArray());
        Assert.True(project.Parameters.Single(p => p.Name == "id").Required);
        Assert.False(project.Parameters.Single(p => p.Name == "language").Required);
        Assert.Empty(tools.Single(t => t.Name == "get_experiences").Parameters);
    }

    [Fact]
    public async Task A_missing_context_is_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new GetContextToolsQuery(Guid.NewGuid()), CancellationToken.None));
    }
}

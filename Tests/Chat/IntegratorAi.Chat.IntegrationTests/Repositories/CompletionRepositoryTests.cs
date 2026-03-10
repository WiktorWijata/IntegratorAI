using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Persistence;
using IntegratorAI.Chat.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Chat.IntegrationTests.Repositories;

public class CompletionRepositoryTests : IDisposable
{
    private readonly ChatDbContext _context;
    private readonly CompletionRepository _repository;

    public CompletionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ChatDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new CompletionRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    private async Task SaveCompletionAsync(Completion completion)
    {
        await _repository.AddAsync(completion);
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
    public async Task AddAsync_PersistsCompletion()
    {
        var completion = new Completion(new Message(MessageRole.User, "hello"));
        await SaveCompletionAsync(completion);

        var result = await _repository.GetByIdAsync(completion.Id);

        Assert.NotNull(result);
        Assert.Equal(completion.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCompletionWithMessages()
    {
        var completion = new Completion(new Message(MessageRole.User, "hello"));
        completion.AddMessage(new Message(MessageRole.Assistant, "reply"));
        await SaveCompletionAsync(completion);

        var result = await _repository.GetByIdAsync(completion.Id);

        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCompletionWithSummary()
    {
        var completion = new Completion(new Message(MessageRole.User, "hello"));
        completion.SetSummary("summary content", summarizedUpToIndex: 1);
        await SaveCompletionAsync(completion);

        var result = await _repository.GetByIdAsync(completion.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.Summary);
        Assert.Equal("summary content", result.Summary.Content);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenSummaryNotSet()
    {
        var completion = new Completion(new Message(MessageRole.User, "hello"));
        await SaveCompletionAsync(completion);

        var result = await _repository.GetByIdAsync(completion.Id);

        Assert.NotNull(result);
        Assert.Null(result.Summary);
    }

    [Fact]
    public async Task GetByIdAsync_PreservesMessageIndexes()
    {
        var completion = new Completion(new Message(MessageRole.User, "first"));
        completion.AddMessage(new Message(MessageRole.Assistant, "second"));
        completion.AddMessage(new Message(MessageRole.User, "third"));
        await SaveCompletionAsync(completion);

        var result = await _repository.GetByIdAsync(completion.Id);

        var messages = result!.Messages.OrderBy(m => m.Index).ToList();
        Assert.Equal(1, messages[0].Index);
        Assert.Equal(2, messages[1].Index);
        Assert.Equal(3, messages[2].Index);
    }

    [Fact]
    public async Task GetByIdAsync_PreservesMessageRolesAndContent()
    {
        var completion = new Completion(new Message(MessageRole.User, "hello"));
        completion.AddMessage(new Message(MessageRole.Assistant, "reply"));
        await SaveCompletionAsync(completion);

        var result = await _repository.GetByIdAsync(completion.Id);

        var messages = result!.Messages.OrderBy(m => m.Index).ToList();
        Assert.Equal(MessageRole.User, messages[0].Role);
        Assert.Equal("hello", messages[0].Content);
        Assert.Equal(MessageRole.Assistant, messages[1].Role);
        Assert.Equal("reply", messages[1].Content);
    }
}

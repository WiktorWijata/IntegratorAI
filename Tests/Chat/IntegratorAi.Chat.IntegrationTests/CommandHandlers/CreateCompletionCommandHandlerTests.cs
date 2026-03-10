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

public class CreateCompletionCommandHandlerTests : IDisposable
{
    private readonly ChatDbContext _context;
    private readonly CompletionRepository _repository;
    private readonly IProviderModule _providerModule;
    private readonly CreateCompletionCommandHandler _handler;

    public CreateCompletionCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ChatDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new CompletionRepository(_context);
        _providerModule = Substitute.For<IProviderModule>();

        _handler = new CreateCompletionCommandHandler(_providerModule, _repository);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ReturnsDtoWithUserAndAssistantMessages()
    {
        _providerModule
            .CompletionAsync(Arg.Any<CompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new MessageDto { Role = "Assistant", Content = "AI response" });

        var command = new CreateCompletionCommand("Hello");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Messages.Length);
        Assert.Equal((string?)"Hello", result.Messages[0].Content);
        Assert.Equal((string?)"AI response", result.Messages[1].Content);
    }

    [Fact]
    public async Task Handle_ReturnsNonEmptyCompletionId()
    {
        _providerModule
            .CompletionAsync(Arg.Any<CompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new MessageDto { Role = "Assistant", Content = "reply" });

        var result = await _handler.Handle(new CreateCompletionCommand("Hi"), CancellationToken.None);

        Assert.True(Guid.TryParse(result.CompletionId, out var id));
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Handle_PersistsCompletionInDatabase()
    {
        _providerModule
            .CompletionAsync(Arg.Any<CompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new MessageDto { Role = "Assistant", Content = "reply" });

        var result = await _handler.Handle(new CreateCompletionCommand("Hello"), CancellationToken.None);

        // Handler adds entities to the EF context but does not commit —
        // persistence is the responsibility of the UoW / EF interceptor.
        // We flush changes here to verify the tracked state is correct.
        await _context.SaveChangesAsync();

        var completionId = Guid.Parse(result.CompletionId);
        _context.ChangeTracker.Clear();
        var saved = await _repository.GetByIdAsync(completionId);

        Assert.NotNull(saved);
        Assert.Equal(2, saved.Messages.Count);
    }

    [Fact]
    public async Task Handle_SendsUserMessageToProvider()
    {
        _providerModule
            .CompletionAsync(Arg.Any<CompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new MessageDto { Role = "Assistant", Content = "reply" });

        await _handler.Handle(new CreateCompletionCommand("Test prompt"), CancellationToken.None);

        await _providerModule
            .Received(1)
            .CompletionAsync(
                Arg.Is<CompletionDto>(dto =>
                    dto.Messages.Length == 1 &&
                    dto.Messages[0].Content == "Test prompt"),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SetsCorrectRolesInResponse()
    {
        _providerModule
            .CompletionAsync(Arg.Any<CompletionDto>(), Arg.Any<CancellationToken>())
            .Returns(new MessageDto { Role = "Assistant", Content = "AI reply" });

        var result = await _handler.Handle(new CreateCompletionCommand("Hello"), CancellationToken.None);

        Assert.Equal("User", result.Messages[0].Role.ToString());
        Assert.Equal("Assistant", result.Messages[1].Role.ToString());
    }
}




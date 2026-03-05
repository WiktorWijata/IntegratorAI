using IntegratorAI.BuildingBlocks.Application.Caching;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Domain.Repositories;
using IntegratorAI.Providers.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntegratorAI.Providers.UnitTests;

public class ProviderModuleTests
{
    private readonly Mock<IProviderRepository> _repositoryMock = new();
    private readonly Mock<IKeyedServiceProvider> _serviceProviderMock = new();
    private readonly Mock<ICacheProvider> _cacheMock = new();
    private readonly FakeProvider _fakeProvider = new();
    private readonly ProviderModule _module;

    private static readonly Provider ActiveProvider = new()
    {
        Id = 1,
        Type = ProviderType.HuggingFace,
        PrimaryModel = "primary-model",
        SummarizationModel = "summary-model",
        IsActive = true
    };

    private static readonly CompletionDto EmptyCompletion = new() { Messages = [] };

    public ProviderModuleTests()
    {
        _serviceProviderMock
            .Setup(sp => sp.GetKeyedService(typeof(IProvider), ProviderType.HuggingFace))
            .Returns(_fakeProvider);

        _module = new ProviderModule(
            _repositoryMock.Object,
            _serviceProviderMock.Object,
            _cacheMock.Object);
    }

    private void SetupCacheHit()
        => _cacheMock
            .Setup(c => c.GetAsync<Provider>(It.IsAny<string>()))
            .ReturnsAsync(ActiveProvider);

    private void SetupCacheMiss()
        => _cacheMock
            .Setup(c => c.GetAsync<Provider>(It.IsAny<string>()))
            .ReturnsAsync((Provider?)null);


    [Fact]
    public async Task CompletionAsync_ReturnsDelegatedResult()
    {
        SetupCacheHit();
        var expected = new MessageDto { Role = "assistant", Content = "reply" };
        _fakeProvider.CompletionResult = expected;

        var result = await _module.CompletionAsync(EmptyCompletion);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task SummaryCompletionAsync_ReturnsDelegatedResult()
    {
        SetupCacheHit();
        var expected = new MessageDto { Role = "system", Content = "summary" };
        _fakeProvider.SummaryResult = expected;

        var result = await _module.SummaryCompletionAsync(EmptyCompletion);

        Assert.Equal(expected, result);
    }


    [Fact]
    public async Task CompletionAsync_UsesCache_WhenProviderIsCached()
    {
        SetupCacheHit();

        await _module.CompletionAsync(EmptyCompletion);

        _repositoryMock.Verify(r => r.GetActiveProviderAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CompletionAsync_FetchesFromRepository_WhenCacheIsEmpty()
    {
        SetupCacheMiss();
        _repositoryMock
            .Setup(r => r.GetActiveProviderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ActiveProvider);

        await _module.CompletionAsync(EmptyCompletion);

        _repositoryMock.Verify(r => r.GetActiveProviderAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompletionAsync_SavesProviderToCache_WhenFetchedFromRepository()
    {
        SetupCacheMiss();
        _repositoryMock
            .Setup(r => r.GetActiveProviderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ActiveProvider);

        await _module.CompletionAsync(EmptyCompletion);

        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), ActiveProvider, It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task CompletionAsync_ReusesProvider_OnSubsequentCalls()
    {
        SetupCacheHit();

        await _module.CompletionAsync(EmptyCompletion);
        await _module.CompletionAsync(EmptyCompletion);

        _cacheMock.Verify(c => c.GetAsync<Provider>(It.IsAny<string>()), Times.Once);
    }


    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenNoProviderRegistered()
    {
        SetupCacheHit();
        _serviceProviderMock
            .Setup(sp => sp.GetKeyedService(typeof(IProvider), ProviderType.HuggingFace))
            .Returns((object?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _module.CompletionAsync(EmptyCompletion));
    }


    [Fact]
    public async Task CompletionAsync_SetsPrimaryModel_OnProvider()
    {
        SetupCacheHit();

        await _module.CompletionAsync(EmptyCompletion);

        Assert.Equal(ActiveProvider.PrimaryModel, _fakeProvider.PrimaryModel);
    }

    [Fact]
    public async Task CompletionAsync_SetsSummarizationModel_OnProvider()
    {
        SetupCacheHit();

        await _module.CompletionAsync(EmptyCompletion);

        Assert.Equal(ActiveProvider.SummarizationModel, _fakeProvider.SummarizationModel);
    }

    [Fact]
    public async Task SummaryCompletionAsync_UsesCache_WhenProviderIsCached()
    {
        SetupCacheHit();

        await _module.SummaryCompletionAsync(EmptyCompletion);

        _repositoryMock.Verify(r => r.GetActiveProviderAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SummaryCompletionAsync_FetchesFromRepository_WhenCacheIsEmpty()
    {
        SetupCacheMiss();
        _repositoryMock
            .Setup(r => r.GetActiveProviderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ActiveProvider);

        await _module.SummaryCompletionAsync(EmptyCompletion);

        _repositoryMock.Verify(r => r.GetActiveProviderAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SummaryCompletionAsync_SetsPrimaryModel_OnProvider()
    {
        SetupCacheHit();

        await _module.SummaryCompletionAsync(EmptyCompletion);

        Assert.Equal(ActiveProvider.PrimaryModel, _fakeProvider.PrimaryModel);
    }

    [Fact]
    public async Task SummaryCompletionAsync_SetsSummarizationModel_OnProvider()
    {
        SetupCacheHit();

        await _module.SummaryCompletionAsync(EmptyCompletion);

        Assert.Equal(ActiveProvider.SummarizationModel, _fakeProvider.SummarizationModel);
    }

    [Fact]
    public async Task CompletionAsync_PassesCancellationToken_ToRepository()
    {
        SetupCacheMiss();
        var cts = new CancellationTokenSource();
        _repositoryMock
            .Setup(r => r.GetActiveProviderAsync(cts.Token))
            .ReturnsAsync(ActiveProvider);

        await _module.CompletionAsync(EmptyCompletion, cts.Token);

        _repositoryMock.Verify(r => r.GetActiveProviderAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenRepositoryReturnsNull()
    {
        SetupCacheMiss();
        _repositoryMock
            .Setup(r => r.GetActiveProviderAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("No active provider found."));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _module.CompletionAsync(EmptyCompletion));
    }

    [Fact]
    public async Task SummaryCompletionAsync_ThrowsInvalidOperationException_WhenNoProviderRegistered()
    {
        SetupCacheHit();
        _serviceProviderMock
            .Setup(sp => sp.GetKeyedService(typeof(IProvider), ProviderType.HuggingFace))
            .Returns((object?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _module.SummaryCompletionAsync(EmptyCompletion));
    }

    [Fact]
    public async Task SummaryCompletionAsync_SavesProviderToCache_WhenFetchedFromRepository()
    {
        SetupCacheMiss();
        _repositoryMock
            .Setup(r => r.GetActiveProviderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ActiveProvider);

        await _module.SummaryCompletionAsync(EmptyCompletion);

        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), ActiveProvider, It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task SummaryCompletionAsync_ReusesProvider_OnSubsequentCalls()
    {
        SetupCacheHit();

        await _module.SummaryCompletionAsync(EmptyCompletion);
        await _module.SummaryCompletionAsync(EmptyCompletion);

        _cacheMock.Verify(c => c.GetAsync<Provider>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SummaryCompletionAsync_ReusesProvider_AfterCompletionAsync()
    {
        SetupCacheHit();

        await _module.CompletionAsync(EmptyCompletion);
        await _module.SummaryCompletionAsync(EmptyCompletion);

        _cacheMock.Verify(c => c.GetAsync<Provider>(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CompletionAsync_ReusesProvider_AfterSummaryCompletionAsync()
    {
        SetupCacheHit();

        await _module.SummaryCompletionAsync(EmptyCompletion);
        await _module.CompletionAsync(EmptyCompletion);

        _cacheMock.Verify(c => c.GetAsync<Provider>(It.IsAny<string>()), Times.Once);
    }


    private sealed class FakeProvider : IProvider, IProviderInitalizable
    {
        public string PrimaryModel { get; set; } = string.Empty;
        public string SummarizationModel { get; set; } = string.Empty;

        public MessageDto? CompletionResult { get; set; }
        public MessageDto? SummaryResult { get; set; }

        public Task<MessageDto> CompletionAsync(CompletionDto completion)
            => Task.FromResult(CompletionResult ?? new MessageDto());

        public Task<MessageDto> SummaryCompletionAsync(CompletionDto completion)
            => Task.FromResult(SummaryResult ?? new MessageDto());
    }
}

using Microsoft.Extensions.DependencyInjection;
using IntegratorAI.Providers.Contracts;
using IntegratorAI.Providers.Contracts.Models;
using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Domain.Repositories;
using IntegratorAI.Providers.Infrastructure;
using NSubstitute;
using RescuePC.Software.Caching;

namespace IntegratorAI.Providers.UnitTests;

public class ProviderModuleTests
{
    private readonly IProviderRepository _repository = Substitute.For<IProviderRepository>();
    private readonly IKeyedServiceProvider _serviceProvider = Substitute.For<IKeyedServiceProvider>();
    private readonly ICacheProvider _cache = Substitute.For<ICacheProvider>();
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

    private static readonly ProviderCompletionDto EmptyCompletion = new() { Messages = [] };

    public ProviderModuleTests()
    {
        _serviceProvider
            .GetKeyedService(typeof(IProvider), Arg.Any<object?>())
            .Returns(callInfo => (object)_fakeProvider);

        _module = new ProviderModule(
            _repository,
            _serviceProvider,
            _cache);
    }

    private void SetupCacheHit()
        => _cache
            .GetAsync<Provider>(Arg.Any<string>())
            .Returns(ActiveProvider);

    private void SetupCacheMiss()
        => _cache
            .GetAsync<Provider>(Arg.Any<string>())
            .Returns((Provider?)null);


    [Fact]
    public async Task CompletionAsync_ReturnsDelegatedResult()
    {
        SetupCacheHit();
        var expected = new ProviderMessageDto { Role = "assistant", Content = "reply" };
        _fakeProvider.CompletionResult = expected;

        var result = await _module.CompletionAsync(EmptyCompletion);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task SummaryCompletionAsync_ReturnsDelegatedResult()
    {
        SetupCacheHit();
        var expected = new ProviderMessageDto { Role = "system", Content = "summary" };
        _fakeProvider.SummaryResult = expected;

        var result = await _module.SummaryCompletionAsync(EmptyCompletion);

        Assert.Equal(expected, result);
    }


    [Fact]
    public async Task CompletionAsync_UsesCache_WhenProviderIsCached()
    {
        SetupCacheHit();

        await _module.CompletionAsync(EmptyCompletion);

        await _repository.DidNotReceive().GetActiveProviderAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompletionAsync_FetchesFromRepository_WhenCacheIsEmpty()
    {
        SetupCacheMiss();
        _repository
            .GetActiveProviderAsync(Arg.Any<CancellationToken>())
            .Returns(ActiveProvider);

        await _module.CompletionAsync(EmptyCompletion);

        await _repository.Received(1).GetActiveProviderAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompletionAsync_SavesProviderToCache_WhenFetchedFromRepository()
    {
        SetupCacheMiss();
        _repository
            .GetActiveProviderAsync(Arg.Any<CancellationToken>())
            .Returns(ActiveProvider);

        await _module.CompletionAsync(EmptyCompletion);

        await _cache.Received(1).SetAsync(Arg.Any<string>(), ActiveProvider, Arg.Any<TimeSpan?>());
    }

    [Fact]
    public async Task CompletionAsync_ReusesProvider_OnSubsequentCalls()
    {
        SetupCacheHit();

        await _module.CompletionAsync(EmptyCompletion);
        await _module.CompletionAsync(EmptyCompletion);

        await _cache.Received(1).GetAsync<Provider>(Arg.Any<string>());
    }


    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenNoProviderRegistered()
    {
        SetupCacheHit();
        _serviceProvider
            .GetKeyedService(typeof(IProvider), Arg.Any<object?>())
            .Returns(callInfo => (object?)null);

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

        await _repository.DidNotReceive().GetActiveProviderAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SummaryCompletionAsync_FetchesFromRepository_WhenCacheIsEmpty()
    {
        SetupCacheMiss();
        _repository
            .GetActiveProviderAsync(Arg.Any<CancellationToken>())
            .Returns(ActiveProvider);

        await _module.SummaryCompletionAsync(EmptyCompletion);

        await _repository.Received(1).GetActiveProviderAsync(Arg.Any<CancellationToken>());
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
        _repository
            .GetActiveProviderAsync(cts.Token)
            .Returns(ActiveProvider);

        await _module.CompletionAsync(EmptyCompletion, cts.Token);

        await _repository.Received(1).GetActiveProviderAsync(cts.Token);
    }

    [Fact]
    public async Task CompletionAsync_ThrowsInvalidOperationException_WhenRepositoryReturnsNull()
    {
        SetupCacheMiss();
        _repository
            .GetActiveProviderAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Provider>(new InvalidOperationException("No active provider found.")));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _module.CompletionAsync(EmptyCompletion));
    }

    [Fact]
    public async Task SummaryCompletionAsync_ThrowsInvalidOperationException_WhenNoProviderRegistered()
    {
        SetupCacheHit();
        _serviceProvider
            .GetKeyedService(typeof(IProvider), Arg.Any<object?>())
            .Returns(callInfo => (object?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _module.SummaryCompletionAsync(EmptyCompletion));
    }

    [Fact]
    public async Task SummaryCompletionAsync_SavesProviderToCache_WhenFetchedFromRepository()
    {
        SetupCacheMiss();
        _repository
            .GetActiveProviderAsync(Arg.Any<CancellationToken>())
            .Returns(ActiveProvider);

        await _module.SummaryCompletionAsync(EmptyCompletion);

        await _cache.Received(1).SetAsync(Arg.Any<string>(), ActiveProvider, Arg.Any<TimeSpan?>());
    }

    [Fact]
    public async Task SummaryCompletionAsync_ReusesProvider_OnSubsequentCalls()
    {
        SetupCacheHit();

        await _module.SummaryCompletionAsync(EmptyCompletion);
        await _module.SummaryCompletionAsync(EmptyCompletion);

        await _cache.Received(1).GetAsync<Provider>(Arg.Any<string>());
    }

    [Fact]
    public async Task SummaryCompletionAsync_ReusesProvider_AfterCompletionAsync()
    {
        SetupCacheHit();

        await _module.CompletionAsync(EmptyCompletion);
        await _module.SummaryCompletionAsync(EmptyCompletion);

        await _cache.Received(1).GetAsync<Provider>(Arg.Any<string>());
    }

    [Fact]
    public async Task CompletionAsync_ReusesProvider_AfterSummaryCompletionAsync()
    {
        SetupCacheHit();

        await _module.SummaryCompletionAsync(EmptyCompletion);
        await _module.CompletionAsync(EmptyCompletion);

        await _cache.Received(1).GetAsync<Provider>(Arg.Any<string>());
    }


    private sealed class FakeProvider : IProvider, IProviderInitalizable 
    {
        public string PrimaryModel { get; set; } = string.Empty;
        public string SummarizationModel { get; set; } = string.Empty;

        public ProviderMessageDto? CompletionResult { get; set; }
        public ProviderMessageDto? SummaryResult { get; set; }

        public Task<ProviderMessageDto> CompletionAsync(ProviderCompletionDto completion)
            => Task.FromResult(CompletionResult ?? new ProviderMessageDto());

        public Task<ProviderMessageDto> SummaryCompletionAsync(ProviderCompletionDto completion)
            => Task.FromResult(SummaryResult ?? new ProviderMessageDto());

        public IAsyncEnumerable<string> StreamCompletionAsync(ProviderCompletionDto completion, CancellationToken cancellationToken = default)
            => AsyncEnumerable.Empty<string>();
    }
}

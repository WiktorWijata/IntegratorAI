using IntegratorAI.Providers.Domain;
using IntegratorAI.Providers.Persistence;
using IntegratorAI.Providers.Persistence.Reposiitories;
using Microsoft.EntityFrameworkCore;

namespace IntegratorAI.Providers.IntegrationTests.Repositories;

public class ProviderRepositoryTests : IDisposable
{
    private readonly ProvidersDbContext _context;
    private readonly ProviderRepository _repository;

    public ProviderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new ProvidersDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new ProviderRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    private async Task SeedAsync(params Provider[] providers)
    {
        _context.Providers.AddRange(providers);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task GetActiveProviderAsync_ReturnsActiveProvider()
    {
        var provider = new Provider
        {
            Type = ProviderType.HuggingFace,
            PrimaryModel = "primary-model",
            IsActive = true
        };
        await SeedAsync(provider);

        var result = await _repository.GetActiveProviderAsync();

        Assert.NotNull(result);
        Assert.Equal(provider.Id, result.Id);
    }

    [Fact]
    public async Task GetActiveProviderAsync_ThrowsInvalidOperationException_WhenNoActiveProvider()
    {
        var provider = new Provider
        {
            Type = ProviderType.HuggingFace,
            PrimaryModel = "primary-model",
            IsActive = false
        };
        await SeedAsync(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.GetActiveProviderAsync());
    }

    [Fact]
    public async Task GetActiveProviderAsync_ThrowsInvalidOperationException_WhenTableIsEmpty()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.GetActiveProviderAsync());
    }

    [Fact]
    public async Task GetActiveProviderAsync_ReturnsPrimaryModel()
    {
        await SeedAsync(new Provider
        {
            Type = ProviderType.HuggingFace,
            PrimaryModel = "meta-llama/Llama-3.1-8B",
            IsActive = true
        });

        var result = await _repository.GetActiveProviderAsync();

        Assert.Equal("meta-llama/Llama-3.1-8B", result.PrimaryModel);
    }

    [Fact]
    public async Task GetActiveProviderAsync_ReturnsSummarizationModel_WhenSet()
    {
        await SeedAsync(new Provider
        {
            Type = ProviderType.HuggingFace,
            PrimaryModel = "primary-model",
            SummarizationModel = "facebook/bart-large-cnn",
            IsActive = true
        });

        var result = await _repository.GetActiveProviderAsync();

        Assert.Equal("facebook/bart-large-cnn", result.SummarizationModel);
    }

    [Fact]
    public async Task GetActiveProviderAsync_ReturnsSummarizationModelAsNull_WhenNotSet()
    {
        await SeedAsync(new Provider
        {
            Type = ProviderType.HuggingFace,
            PrimaryModel = "primary-model",
            SummarizationModel = null,
            IsActive = true
        });

        var result = await _repository.GetActiveProviderAsync();

        Assert.Null(result.SummarizationModel);
    }

    [Fact]
    public async Task GetActiveProviderAsync_ReturnsCorrectProviderType()
    {
        await SeedAsync(new Provider
        {
            Type = ProviderType.HuggingFace,
            PrimaryModel = "primary-model",
            IsActive = true
        });

        var result = await _repository.GetActiveProviderAsync();

        Assert.Equal(ProviderType.HuggingFace, result.Type);
    }

    [Fact]
    public async Task GetActiveProviderAsync_ThrowsInvalidOperationException_WhenMultipleActiveProviders()
    {
        await SeedAsync(
            new Provider { Type = ProviderType.HuggingFace, PrimaryModel = "model-a", IsActive = true },
            new Provider { Type = ProviderType.HuggingFace, PrimaryModel = "model-b", IsActive = true }
        );

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.GetActiveProviderAsync());
    }

    [Fact]
    public async Task GetActiveProviderAsync_ReturnsOnlyActiveProvider_WhenMixedExists()
    {
        var activeProvider = new Provider
        {
            Type = ProviderType.HuggingFace,
            PrimaryModel = "active-model",
            IsActive = true
        };
        await SeedAsync(
            new Provider { Type = ProviderType.HuggingFace, PrimaryModel = "inactive-model", IsActive = false },
            activeProvider
        );

        var result = await _repository.GetActiveProviderAsync();

        Assert.Equal("active-model", result.PrimaryModel);
    }

    [Fact]
    public async Task GetActiveProviderAsync_RespectsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _repository.GetActiveProviderAsync(cts.Token));
    }
}


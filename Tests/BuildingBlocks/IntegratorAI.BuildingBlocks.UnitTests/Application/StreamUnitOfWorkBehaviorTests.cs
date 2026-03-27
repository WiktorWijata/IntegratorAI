using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.BuildingBlocks.Application.Behaviors;
using MediatR;
using NSubstitute;

namespace IntegratorAI.BuildingBlocks.UnitTests.Application;

public class StreamUnitOfWorkBehaviorTests
{
    private record TestStreamRequest : IStreamRequest<string>;

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly StreamUnitOfWorkBehavior<TestStreamRequest, string, IUnitOfWork> _behavior;

    public StreamUnitOfWorkBehaviorTests()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(0);
        _behavior = new StreamUnitOfWorkBehavior<TestStreamRequest, string, IUnitOfWork>(_unitOfWork);
    }

    private static async IAsyncEnumerable<string> NextItems(params string[] items)
    {
        foreach (var item in items)
            yield return item;
    }

    [Fact]
    public async Task Handle_YieldsAllItemsFromNext()
    {
        var items = new List<string>();

        await foreach (var item in _behavior.Handle(new TestStreamRequest(), () => NextItems("a", "b", "c"), CancellationToken.None))
            items.Add(item);

        Assert.Equal(["a", "b", "c"], items);
    }

    [Fact]
    public async Task Handle_CallsSaveChangesAsync_AfterStreaming()
    {
        await foreach (var _ in _behavior.Handle(new TestStreamRequest(), () => NextItems("item"), CancellationToken.None)) { }

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PassesCancellationToken_ToSaveChangesAsync()
    {
        using var cts = new CancellationTokenSource();

        await foreach (var _ in _behavior.Handle(new TestStreamRequest(), () => NextItems("item"), cts.Token)) { }

        await _unitOfWork.Received(1).SaveChangesAsync(cts.Token);
    }

    [Fact]
    public async Task Handle_CallsSaveChangesAsync_OnlyAfterAllItemsYielded()
    {
        var callLog = new List<string>();
        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(callInfo => { callLog.Add("save"); return Task.FromResult(0); });

        await foreach (var item in _behavior.Handle(new TestStreamRequest(), () => NextItems("a", "b"), CancellationToken.None))
            callLog.Add($"item:{item}");

        Assert.Equal(["item:a", "item:b", "save"], callLog);
    }
}

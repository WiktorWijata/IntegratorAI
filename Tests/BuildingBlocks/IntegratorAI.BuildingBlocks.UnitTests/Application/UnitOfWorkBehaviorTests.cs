using MediatR;
using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.BuildingBlocks.Application.Behaviors;
using Moq;

namespace IntegratorAI.BuildingBlocks.UnitTests.Application;

public class UnitOfWorkBehaviorTests
{
    private record TestRequest : IRequest<TestResponse>;
    private record TestResponse;

    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UnitOfWorkBehavior<TestRequest, TestResponse> _behavior;

    public UnitOfWorkBehaviorTests()
    {
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _behavior = new UnitOfWorkBehavior<TestRequest, TestResponse>(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsResponseFromNext()
    {
        var expected = new TestResponse();
        RequestHandlerDelegate<TestResponse> next = _ => Task.FromResult(expected);

        var result = await _behavior.Handle(new TestRequest(), next, CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Handle_InvokesNextDelegate()
    {
        var nextCalled = false;
        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(new TestResponse());
        };

        await _behavior.Handle(new TestRequest(), next, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_CallsSaveChangesAsync_AfterNext()
    {
        var callLog = new List<string>();
        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            callLog.Add("next");
            return Task.FromResult(new TestResponse());
        };
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callLog.Add("save"))
            .ReturnsAsync(0);

        await _behavior.Handle(new TestRequest(), next, CancellationToken.None);

        Assert.Equal(["next", "save"], callLog);
    }

    [Fact]
    public async Task Handle_PassesCancellationToken_ToSaveChangesAsync()
    {
        RequestHandlerDelegate<TestResponse> next = _ => Task.FromResult(new TestResponse());
        using var cts = new CancellationTokenSource();

        await _behavior.Handle(new TestRequest(), next, cts.Token);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once());
    }
}

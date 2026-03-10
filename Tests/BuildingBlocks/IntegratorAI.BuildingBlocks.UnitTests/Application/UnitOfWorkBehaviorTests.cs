using IntegratorAI.BuildingBlocks.Application;
using IntegratorAI.BuildingBlocks.Application.Behaviors;
using MediatR;
using NSubstitute;

namespace IntegratorAI.BuildingBlocks.UnitTests.Application;

public class UnitOfWorkBehaviorTests
{
    private record TestRequest : IRequest<TestResponse>;
    private record TestResponse;

    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UnitOfWorkBehavior<TestRequest, TestResponse> _behavior;

    public UnitOfWorkBehaviorTests()
    {
        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(0);

        _behavior = new UnitOfWorkBehavior<TestRequest, TestResponse>(_unitOfWork);
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
        _unitOfWork
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(callInfo => 
            { 
                callLog.Add("save");
                return Task.FromResult(0); 
            });

        await _behavior.Handle(new TestRequest(), next, CancellationToken.None);

        Assert.Equal(["next", "save"], callLog);
    }

    [Fact]
    public async Task Handle_PassesCancellationToken_ToSaveChangesAsync()
    {
        RequestHandlerDelegate<TestResponse> next = _ => Task.FromResult(new TestResponse());
        using var cts = new CancellationTokenSource();

        await _behavior.Handle(new TestRequest(), next, cts.Token);

        await _unitOfWork.Received(1).SaveChangesAsync(cts.Token);
    }
}

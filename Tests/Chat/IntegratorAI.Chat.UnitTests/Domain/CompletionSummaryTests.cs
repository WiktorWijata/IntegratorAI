using IntegratorAI.Chat.Domain;

namespace IntegratorAI.Chat.UnitTests.Domain;

public class CompletionSummaryTests
{
    [Fact]
    public void Constructor_SetsContentAndIndex()
    {
        var summary = new CompletionSummary("initial content", summarizedUpToIndex: 3);

        Assert.Equal("initial content", summary.Content);
        Assert.Equal(3, summary.SummarizedUpToIndex);
    }

    [Fact]
    public void Constructor_SetsCreatedAt()
    {
        var before = DateTime.UtcNow;
        var summary = new CompletionSummary("content", summarizedUpToIndex: 1);
        var after = DateTime.UtcNow;

        Assert.InRange(summary.CreatedAt, before, after);
    }

    [Fact]
    public void Constructor_LeavesModifiedAtNull()
    {
        var summary = new CompletionSummary("content", summarizedUpToIndex: 1);

        Assert.Null(summary.ModifiedAt);
    }

    [Fact]
    public void Update_SetsNewContent()
    {
        var summary = new CompletionSummary("old content", summarizedUpToIndex: 1);

        summary.Update("new content", summarizedUpToIndex: 2);

        Assert.Equal("new content", summary.Content);
    }

    [Fact]
    public void Update_SetsNewSummarizedUpToIndex()
    {
        var summary = new CompletionSummary("content", summarizedUpToIndex: 1);

        summary.Update("updated", summarizedUpToIndex: 5);

        Assert.Equal(5, summary.SummarizedUpToIndex);
    }

    [Fact]
    public void Update_SetsModifiedAt()
    {
        var summary = new CompletionSummary("content", summarizedUpToIndex: 1);
        var before = DateTime.UtcNow;

        summary.Update("updated", summarizedUpToIndex: 2);

        var after = DateTime.UtcNow;
        Assert.NotNull(summary.ModifiedAt);
        Assert.InRange(summary.ModifiedAt!.Value, before, after);
    }
}


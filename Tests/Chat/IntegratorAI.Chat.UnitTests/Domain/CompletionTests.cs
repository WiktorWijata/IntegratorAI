using IntegratorAI.Chat.Domain;
using IntegratorAI.Chat.Domain.Events;

namespace IntegratorAI.Chat.UnitTests.Domain;

public class CompletionTests
{
    private static Message UserMessage(string content = "hello") => new(MessageRole.User, content);

    [Fact]
    public void Constructor_AddsInitialMessage()
    {
        var message = UserMessage();

        var completion = new Completion(message);

        Assert.Single(completion.Messages);
        Assert.Contains(completion.Messages, m => m == message);
    }

    [Fact]
    public void AddMessage_SetsIndexSequentially()
    {
        var completion = new Completion(UserMessage());
        var second = new Message(MessageRole.Assistant, "reply");
        var third = new Message(MessageRole.User, "follow up");

        completion.AddMessage(second);
        completion.AddMessage(third);

        Assert.Equal(1, completion.Messages.First().Index);
        Assert.Equal(2, second.Index);
        Assert.Equal(3, third.Index);
    }

    [Fact]
    public void AddMessage_AddsMessageToCollection()
    {
        var completion = new Completion(UserMessage());
        var newMessage = new Message(MessageRole.Assistant, "reply");

        completion.AddMessage(newMessage);

        Assert.Equal(2, completion.Messages.Count);
        Assert.Contains(completion.Messages, m => m == newMessage);
    }

    [Fact]
    public void UnsummarizedMessages_ReturnsAllMessages_WhenNoSummary()
    {
        var completion = new Completion(UserMessage());
        completion.AddMessage(new Message(MessageRole.Assistant, "reply"));

        Assert.Equal(2, completion.UnsummarizedMessages.Count());
    }

    [Fact]
    public void UnsummarizedMessages_ReturnsOnlyMessagesAfterSummarizedIndex()
    {
        var completion = new Completion(UserMessage("msg1"));
        completion.AddMessage(new Message(MessageRole.Assistant, "msg2"));
        completion.AddMessage(new Message(MessageRole.User, "msg3"));
        completion.AddMessage(new Message(MessageRole.Assistant, "msg4"));

        completion.SetSummary("summary", summarizedUpToIndex: 2);

        var unsummarized = completion.UnsummarizedMessages.ToList();

        Assert.Equal(2, unsummarized.Count);
        Assert.All(unsummarized, m => Assert.True(m.Index > 2));
    }

    [Fact]
    public void TrySummarize_ReturnsFalse_WhenBelowThreshold()
    {
        var completion = new Completion(UserMessage());

        var result = completion.TrySummarize(threshold: 5);

        Assert.False(result);
    }

    [Fact]
    public void TrySummarize_ReturnsFalse_WhenExactlyAtThreshold()
    {
        var completion = new Completion(UserMessage());
        foreach (var i in Enumerable.Range(0, 4))
            completion.AddMessage(new Message(MessageRole.Assistant, $"msg{i}"));

        var result = completion.TrySummarize(threshold: 5);

        Assert.False(result);
    }

    [Fact]
    public void TrySummarize_ReturnsTrue_WhenAboveThreshold()
    {
        var completion = new Completion(UserMessage());
        foreach (var i in Enumerable.Range(0, 5))
            completion.AddMessage(new Message(MessageRole.Assistant, $"msg{i}"));

        var result = completion.TrySummarize(threshold: 5);

        Assert.True(result);
    }

    [Fact]
    public void TrySummarize_RaisesCompletionSummaryRequiredEvent_WhenAboveThreshold()
    {
        var completion = new Completion(UserMessage());
        foreach (var i in Enumerable.Range(0, 5))
            completion.AddMessage(new Message(MessageRole.Assistant, $"msg{i}"));

        completion.TrySummarize(threshold: 5);

        var events = completion.GetDomainEvents();
        Assert.Single(events);
        Assert.IsType<CompletionSummaryRequiredEvent>(events.First());
    }

    [Fact]
    public void TrySummarize_RaisedEventContainsCorrectCompletion()
    {
        var completion = new Completion(UserMessage());
        foreach (var i in Enumerable.Range(0, 5))
            completion.AddMessage(new Message(MessageRole.Assistant, $"msg{i}"));

        completion.TrySummarize(threshold: 5);

        var @event = (CompletionSummaryRequiredEvent)completion.GetDomainEvents().First();
        Assert.Same(completion, @event.Completion);
    }

    [Fact]
    public void TrySummarize_DoesNotRaiseEvent_WhenBelowThreshold()
    {
        var completion = new Completion(UserMessage());

        completion.TrySummarize(threshold: 5);

        Assert.Empty(completion.GetDomainEvents());
    }

    [Fact]
    public void SetSummary_CreatesSummary_WhenNoneExists()
    {
        var completion = new Completion(UserMessage());

        completion.SetSummary("summary content", summarizedUpToIndex: 1);

        Assert.NotNull(completion.Summary);
        Assert.Equal("summary content", completion.Summary!.Content);
        Assert.Equal(1, completion.Summary.SummarizedUpToIndex);
    }

    [Fact]
    public void SetSummary_UpdatesSummary_WhenAlreadyExists()
    {
        var completion = new Completion(UserMessage());
        completion.AddMessage(new Message(MessageRole.Assistant, "reply"));
        completion.SetSummary("old summary", summarizedUpToIndex: 1);

        completion.SetSummary("new summary", summarizedUpToIndex: 2);

        Assert.Equal("new summary", completion.Summary!.Content);
        Assert.Equal(2, completion.Summary.SummarizedUpToIndex);
    }

    [Fact]
    public void SetSummary_DoesNotCreateNewInstance_WhenUpdating()
    {
        var completion = new Completion(UserMessage());
        completion.SetSummary("first", summarizedUpToIndex: 1);
        var originalSummary = completion.Summary;

        completion.SetSummary("second", summarizedUpToIndex: 2);

        Assert.Same(originalSummary, completion.Summary);
    }
}




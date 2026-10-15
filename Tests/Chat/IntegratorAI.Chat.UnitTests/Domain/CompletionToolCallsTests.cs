using IntegratorAI.Chat.Domain;

namespace IntegratorAI.Chat.UnitTests.Domain;

public class CompletionToolCallsTests
{
    private static Completion AskingForTools(params ToolCall[] calls)
    {
        var completion = new Completion(new Message(MessageRole.User, "tell me about project 5"), new Message(MessageRole.System, "context"));
        completion.AddMessage(Message.ForToolCalls(string.Empty, calls));
        return completion;
    }

    private static ToolCall Call(string id, string name = "get_project") => new(id, name, "{\"id\":5}");

    [Fact]
    public void A_completion_has_no_pending_tool_calls_until_the_model_asks_for_one()
    {
        var completion = new Completion(new Message(MessageRole.User, "hi"));
        completion.AddMessage(new Message(MessageRole.Assistant, "hello"));

        Assert.Empty(completion.PendingToolCalls);
    }

    [Fact]
    public void The_tool_calls_of_the_last_assistant_message_are_pending()
    {
        var completion = AskingForTools(Call("a"), Call("b", "get_skills"));

        Assert.Equal(["a", "b"], completion.PendingToolCalls.Select(c => c.Id).ToArray());
        Assert.Equal("get_skills", completion.PendingToolCalls[1].Name);
        Assert.Equal("{\"id\":5}", completion.PendingToolCalls[0].Arguments);
    }

    [Fact]
    public void Adding_the_results_answers_the_calls_in_the_order_they_were_asked()
    {
        var completion = AskingForTools(Call("a"), Call("b"));

        completion.AddToolResults(new Dictionary<string, string> { ["b"] = "second", ["a"] = "first" });

        var tools = completion.Messages.Where(m => m.Role == MessageRole.Tool).OrderBy(m => m.Index).ToList();
        Assert.Equal(["a", "b"], tools.Select(m => m.ToolCallId!).ToArray());
        Assert.Equal(["first", "second"], tools.Select(m => m.Content).ToArray());
        Assert.Empty(completion.PendingToolCalls);
    }

    [Fact]
    public void Results_for_a_completion_that_waits_for_nothing_are_rejected()
    {
        var completion = new Completion(new Message(MessageRole.User, "hi"));

        Assert.Throws<InvalidOperationException>(() => completion.AddToolResults(new Dictionary<string, string> { ["a"] = "x" }));
    }

    [Fact]
    public void Results_that_miss_a_call_are_rejected_and_nothing_is_added()
    {
        var completion = AskingForTools(Call("a"), Call("b"));
        var before = completion.Messages.Count;

        Assert.Throws<InvalidOperationException>(() => completion.AddToolResults(new Dictionary<string, string> { ["a"] = "x" }));

        Assert.Equal(before, completion.Messages.Count);
    }

    [Fact]
    public void Results_for_a_call_the_model_did_not_make_are_rejected()
    {
        var completion = AskingForTools(Call("a"));

        Assert.Throws<InvalidOperationException>(() => completion.AddToolResults(new Dictionary<string, string> { ["other"] = "x" }));
    }

    [Fact]
    public void The_context_messages_carry_the_tool_calls_and_the_call_a_result_answers()
    {
        var completion = AskingForTools(Call("a"));
        completion.AddToolResults(new Dictionary<string, string> { ["a"] = "result" });

        var messages = completion.ContextMessages.ToList();

        Assert.Equal([MessageRole.System, MessageRole.User, MessageRole.Assistant, MessageRole.Tool], messages.Select(m => m.Role).ToArray());
        Assert.NotNull(messages[2].ToolCallsJson);
        Assert.Equal("a", messages[3].ToolCallId);
    }

    [Fact]
    public void A_completion_that_waits_for_tools_is_not_summarized()
    {
        var completion = AskingForTools(Call("a"));

        Assert.False(completion.TrySummarize(threshold: 1));
    }

    [Fact]
    public void A_completion_remembers_its_context()
    {
        var contextId = Guid.NewGuid();

        var completion = new Completion(new Message(MessageRole.User, "hi"), contextId: contextId);

        Assert.Equal(contextId, completion.ContextId);
    }
}

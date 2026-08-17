namespace IntegratorAI.Context.Application;

internal static class CacheKeys
{
    public static string ContextPrompt(Guid contextId) => $"context-prompt:{contextId}";
}

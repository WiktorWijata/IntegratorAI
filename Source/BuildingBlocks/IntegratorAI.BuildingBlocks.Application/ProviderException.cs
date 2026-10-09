namespace IntegratorAI.BuildingBlocks.Application;

/// <summary>
/// A language model provider could not answer (it rejected the request, is out of credit, is down, and so on).
/// The message is for the logs only: the API answers the client with a generic 502 so that provider details,
/// such as the state of the account, are not exposed.
/// </summary>
public class ProviderException : Exception
{
    public ProviderException(string message, int? providerStatusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ProviderStatusCode = providerStatusCode;
    }

    /// <summary>The HTTP status the provider returned, when there was one.</summary>
    public int? ProviderStatusCode { get; }
}

namespace IntegratorAI.BuildingBlocks.Application;

/// <summary>
/// The request cannot be handled in the current state of the resource (for example, tool results sent for a
/// completion that is not waiting for any). The API answers 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}

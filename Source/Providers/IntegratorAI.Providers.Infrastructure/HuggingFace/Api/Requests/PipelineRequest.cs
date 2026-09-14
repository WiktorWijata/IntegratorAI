namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Requests;

public class PipelineRequest
{
    public required string Inputs { get; set; }

    public PipelineParameters? Parameters { get; set; }
}

public class PipelineParameters
{
    public bool Truncation { get; set; }
}

using System.Text.Json.Serialization;

namespace IntegratorAI.Providers.Infrastructure.HuggingFace.Api.Responses;

public class SummarizationResponse
{
    [JsonPropertyName("summary_text")]
    public string SummaryText { get; set; }
}

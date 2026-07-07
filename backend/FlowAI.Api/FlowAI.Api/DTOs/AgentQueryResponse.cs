namespace FlowAI.Api.DTOs;

public class AgentQueryResponse
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public string Source { get; set; } = "mock";

    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}

namespace FlowAI.Api.Options;

public class AgentOptions
{
    public string Mode { get; set; } = "RuleBased";

    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";

    public string OllamaModel { get; set; } = "llama3.2:3b";
}

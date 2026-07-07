namespace FlowAI.Api.Options;

public class ApiKeyOptions
{
    public string HeaderName { get; set; } = "X-FlowAI-Api-Key";

    public string ApiKey { get; set; } = string.Empty;
}

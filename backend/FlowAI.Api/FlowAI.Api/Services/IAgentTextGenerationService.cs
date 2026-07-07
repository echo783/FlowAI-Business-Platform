namespace FlowAI.Api.Services;

public interface IAgentTextGenerationService
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}

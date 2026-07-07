namespace FlowAI.Api.Services;

public class RuleBasedAgentTextGenerationService : IAgentTextGenerationService
{
    public Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        const string marker = "RULE_BASED_ANSWER:";
        var markerIndex = prompt.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

        if (markerIndex < 0)
        {
            return Task.FromResult("현재는 계약, 작업, 정산, 대시보드 현황 조회를 지원합니다.");
        }

        var answer = prompt[(markerIndex + marker.Length)..].Trim();

        return Task.FromResult(answer);
    }
}

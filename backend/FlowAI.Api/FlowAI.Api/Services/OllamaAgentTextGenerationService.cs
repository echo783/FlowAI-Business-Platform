using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FlowAI.Api.Options;
using Microsoft.Extensions.Options;

namespace FlowAI.Api.Services;

public class OllamaAgentTextGenerationService : IAgentTextGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly AgentOptions _options;

    public OllamaAgentTextGenerationService(
        HttpClient httpClient,
        IOptions<AgentOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = _options.OllamaBaseUrl.TrimEnd('/');
            var request = new OllamaGenerateRequest(
                Model: _options.OllamaModel,
                Prompt: prompt,
                Stream: false,
                Options: new OllamaGenerateOptions(
                    Temperature: 0.2,
                    NumPredict: 200));

            using var response = await _httpClient.PostAsJsonAsync(
                $"{baseUrl}/api/generate",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return "Ollama 호출에 실패했습니다. 현재는 로컬 업무 데이터 기준 요약을 사용할 수 있습니다.";
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
                cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(result?.Response))
            {
                return "Ollama 응답이 비어 있습니다. 현재는 계약, 작업, 정산, 대시보드 현황 조회를 지원합니다.";
            }

            return result.Response.Trim();
        }
        catch
        {
            return "Ollama 연결에 실패했습니다. Ollama가 실행 중인지 확인해 주세요.";
        }
    }

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("options")] OllamaGenerateOptions Options);

    private sealed record OllamaGenerateOptions(
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("num_predict")] int NumPredict);

    private sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }
    }
}

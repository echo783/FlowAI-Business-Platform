using FlowAI.Api.Options;
using Microsoft.Extensions.Options;

namespace FlowAI.Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiKeyOptions _options;
    private readonly IWebHostEnvironment _environment;

    public ApiKeyMiddleware(
        RequestDelegate next,
        IOptions<ApiKeyOptions> options,
        IWebHostEnvironment environment)
    {
        _next = next;
        _options = options.Value;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var configuredApiKey = _options.ApiKey;

        if (_environment.IsDevelopment() && string.IsNullOrWhiteSpace(configuredApiKey))
        {
            // TODO: Require an API key for shared development tunnel scenarios.
            await _next(context);
            return;
        }

        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API key is not configured.");
            return;
        }

        var headerName = string.IsNullOrWhiteSpace(_options.HeaderName)
            ? "X-FlowAI-Api-Key"
            : _options.HeaderName;

        if (!context.Request.Headers.TryGetValue(headerName, out var providedApiKey) ||
            providedApiKey != configuredApiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or missing API key.");
            return;
        }

        await _next(context);
    }
}

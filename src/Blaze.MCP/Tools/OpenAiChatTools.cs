using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using ModelContextProtocol.Server;

namespace Blaze.MCP.Tools;

/// <summary>
/// Tools that call an OpenAI-compatible chat completion endpoint.
/// Point OPENAI_BASE_URL to an endpoint like LM Studio or Ollama with OpenAI API shim.
/// </summary>
internal class OpenAiChatTools
{
    private readonly HttpClient _http;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAiChatTools"/> class.
    /// </summary>
    public OpenAiChatTools()
    {
        this._http = new();
    }

    // For tests
    internal OpenAiChatTools(HttpClient http)
    {
        this._http = http;
    }

    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("OPENAI_BASE_URL")?.TrimEnd('/')
        ?? Environment.GetEnvironmentVariable("LMSTUDIO_BASE_URL")?.TrimEnd('/')
        ?? "http://127.0.0.1:1234/v1";

    private static string? ApiKey => Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    /// <summary>
    /// Calls the chat completion endpoint with a simple system+user prompt.
    /// </summary>
    /// <param name="model">Model name (e.g., lmstudio-community/Meta-Llama-3.1-8B-Instruct or ollama model name)</param>
    /// <param name="prompt">User prompt text.</param>
    /// <param name="temperature">Sampling temperature.</param>
    /// <returns>Assistant content string.</returns>
    [McpServerTool]
    [Description("Call an OpenAI-compatible chat completion endpoint with a prompt.")]
    public async Task<string> ChatCompleteAsync(
        [Description("Model name")] string model,
        [Description("Prompt text")] string prompt,
        [Description("Temperature (0..2)")] double temperature = 0.2)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/chat/completions");
        if (!string.IsNullOrEmpty(ApiKey))
        {
            req.Headers.Authorization = new("Bearer", ApiKey);
        }

        var body = new
        {
            model,
            temperature,
            messages = new object[]
            {
                new { role = "system", content = "You are a concise assistant." },
                new { role = "user", content = prompt },
            },
        };

        req.Content = JsonContent.Create(body);
        using var resp = await this._http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var doc = await resp.Content.ReadFromJsonAsync<ChatResponse>();
        return doc?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }

    private sealed record ChatResponse
    {
        [JsonPropertyName("choices")]
        public Choice[]? Choices { get; set; }
    }

    private sealed record Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private sealed record Message
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}

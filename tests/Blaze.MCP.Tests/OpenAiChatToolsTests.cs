using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using Blaze.MCP;
using Blaze.MCP.Tools;

using FluentAssertions;
using Xunit;

namespace Blaze.MCP.Tests;

public class OpenAiChatToolsTests
{
    [Fact]
    public async Task ChatCompleteAsync_Returns_FirstChoiceMessageContent()
    {
        // Arrange a fake response matching OpenAI schema
        var fake = new
        {
            choices = new[]
            {
                new
                {
                    message = new { role = "assistant", content = "Hello from fake!" },
                },
            },
        };

        var json = JsonSerializer.Serialize(fake, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });

        var handler = new FakeHttpMessageHandler(_ =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            };
            return resp;
        });

        var http = new HttpClient(handler);
        var tools = new OpenAiChatTools(http);

        // Act
        var result = await tools.ChatCompleteAsync("test-model", "say hi");

        // Assert
        result.Should().Be("Hello from fake!");
    }
}

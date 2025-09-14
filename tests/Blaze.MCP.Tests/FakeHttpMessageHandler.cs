using System.Net;

namespace Blaze.MCP.Tests;

/// <inheritdoc />
public sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
{
    public Func<HttpRequestMessage, HttpResponseMessage> Handler { get; } = handler;

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Handler(request));
    }
}

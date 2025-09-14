using Blaze.MCP.Tools;

using dotenv.net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Load .env (and .env.local if present) early, without failing if missing.
DotEnv.Load(new DotEnvOptions(envFilePaths: [".env", ".env.local"], probeForEnv: true, ignoreExceptions: true));

var builder = Host.CreateApplicationBuilder(args);

// Optional: include user-secrets in development.
try
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}
catch
{
    // ignored in non-dev environments
}

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<OpenAiChatTools>();

await builder.Build().RunAsync();

// Needed for AddUserSecrets<Program>() with top-level statements
public partial class Program
{
}

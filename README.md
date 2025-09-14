# Blaze.MCP

Private learning project: an MCP server built with modern C#/.NET 10 and ModelContextProtocol, focusing on the Microsoft.Extensions.* stack. Usable from local MCP clients (e.g., LM Studio, Warp, VS Code/Visual Studio MCP previews) over stdio.

Author: Blaze.MCP maintainers
License: MIT

## Quick start

Prereqs:
- .NET SDK 10 preview (pinned via global.json)
- Windows 11 (dev), Linux/macOS supported for runtime

Restore and run the server (stdio):

```powershell
# from repo root
$env:DOTNET_CLI_TELEMETRY_OPTOUT=1
$env:DOTNET_NOLOGO=1

# restore and build
dotnet restore
dotnet build -c Debug

# run MCP server (stdio)
dotnet run --project src/Blaze.MCP/Blaze.MCP.csproj
```

Environment variables are loaded from .env and user secrets (if configured). See .env.example.

## Configure in LM Studio (MCP)

Add a new MCP server of type Command (stdio):

```json
{
  "command": "dotnet",
  "args": ["run", "--project", "src/Blaze.MCP/Blaze.MCP.csproj"],
  "env": {
    "OPENAI_API_KEY": "{{OPENAI_API_KEY}}",
    "OPENAI_BASE_URL": "http://127.0.0.1:1234/v1",
    "OLLAMA_HOST": "http://127.0.0.1:11434",
    "LMSTUDIO_BASE_URL": "http://127.0.0.1:1234/v1"
  },
  "working_directory": "{{REPO_DIR}}"
}
```

Tip: Replace placeholders with your actual values or set them in .env.

### How to connect from LM Studio (persistent server)

- Preferred (URL): If your LM Studio build supports connecting to an MCP server by URL, run Blaze.MCP in persistent mode (HTTP+SSE) and point LM Studio at http://127.0.0.1:34344. Add header Authorization: Bearer {{BLAZE_MCP_TOKEN}}. See docs/adr/0001-blaze-mcp-integration-and-transport.md for details.

  Example startup flags (subject to implementation):

```powershell
# persistent server on localhost:34344, token saved to %LOCALAPPDATA%/Blaze.MCP/token
Blaze.MCP --mcp-sse-enabled=true --mcp-sse-port=34344 --mcp-sse-bind=127.0.0.1 --mcp-token-file="$env:LOCALAPPDATA/Blaze.MCP/token"
```

- Fallback (stdio proxy): If LM Studio only supports spawning a command, use the stdio proxy binary that bridges to the always-on service via a Windows Named Pipe (\\.\pipe\blaze-mcp) or TCP 127.0.0.1:34345.

  Example mcp.json entry:

```json
{
  "command": "blaze-mcp-proxy.exe",
  "args": [],
  "working_directory": "{{REPO_DIR}}"
}
```

Note: Until persistent mode is enabled in this repo, continue using the Command (stdio) configuration above.

### LM Studio walkthrough (with screenshots)

This walkthrough uses today's screenshots captured on your machine and shows the setup and tool usage flow in order.

1) Program > Integrations: Add or enable the Command (stdio) MCP server and confirm it appears with tools on the right.

![LM Studio – Program/Integrations with mcp/blaze.mcp listed](assets/Screenshot%202025-09-14%20191719.png)

2) Open the mcp.json editor in LM Studio and add an entry for Blaze.MCP similar to the JSON in the previous section.

![LM Studio – mcp.json editor showing Blaze.MCP configuration](assets/Screenshot%202025-09-14%20191732.png)

3) Verify the tools are registered for the server (get_random_number and chat_complete).

![LM Studio – Tools list showing get_random_number and chat_complete](assets/Screenshot%202025-09-14%20191736.png)

4) In a chat, ask the model to call a tool, e.g. “Call GetRandomNumber with min=1 and max=10.”

![LM Studio – Chat prompting a tool call](assets/Screenshot%202025-09-14%20191840.png)

![LM Studio – Alternate chat view (same step)](assets/Screenshot%202025-09-14%20191850.png)

5) Approve the tool call when LM Studio asks for permission.

![LM Studio – Permission prompt to call get_random_number](assets/Screenshot%202025-09-14%20191935.png)

6) You can also call ChatComplete from the tool. Approve the request and observe the response.

![LM Studio – Request to call chat_complete](assets/Screenshot%202025-09-14%20192021.png)

![LM Studio – chat_complete result](assets/Screenshot%202025-09-14%20192033.png)

Additional reference: Integrations popover with available MCP servers.

![LM Studio – Integrations popover](assets/Screenshot%202025-09-14%20195158.png)

Notes:
- Ensure LM Studio’s OpenAI-compatible server is enabled (OpenAI API tab) at http://127.0.0.1:1234/v1.
- If your endpoint requires a key, set OPENAI_API_KEY in mcp.json or via user-secrets.

## Configure in Warp (MCP)

You can add the server as a CLI MCP (Command):

```json
{
  "Blaze.MCP": {
    "command": "dotnet",
    "args": ["run", "--project", "{{REPO_DIR}}/src/Blaze.MCP/Blaze.MCP.csproj"],
    "env": {
      "OPENAI_API_KEY": "{{OPENAI_API_KEY}}",
      "OPENAI_BASE_URL": "http://127.0.0.1:1234/v1",
      "OLLAMA_HOST": "http://127.0.0.1:11434",
      "LMSTUDIO_BASE_URL": "http://127.0.0.1:1234/v1"
    },
    "working_directory": "{{REPO_DIR}}"
  }
}
```

## Project layout

- BlazeMCP.csproj — .NET 10 console (MCP stdio)
- Program.cs — host builder, logging to stderr, dotenv + user-secrets
- Tools/RandomNumberTools.cs — sample tool for quick validation
- Tools/OpenAiChatTools.cs — calls OpenAI-compatible chat completions
- .mcp/server.json — package metadata for discovery
- tests/Blaze.MCP.Tests — xUnit tests
- scripts/*.ps1 — build/test/format helpers

## Tests

```powershell
# run tests
pwsh -File .\scripts\test.ps1
```

## Formatting and linting

- dotnet format (via local tool manifest)
- AnalysisLevel=latest and EnforceCodeStyleInBuild enabled

```powershell
pwsh -File .\scripts\format.ps1
```

## .env and secrets

- Put local settings in .env (see .env.example)
- Optionally store secrets in user-secrets for dev-only:

```powershell
# initialize user-secrets store once (auto-generated ID in csproj)
dotnet user-secrets init
# set secrets
 dotnet user-secrets set OPENAI_API_KEY "{{OPENAI_API_KEY}}"
```

Supported variables:
- OPENAI_API_KEY
- OPENAI_BASE_URL (LM Studio or OpenAI-compatible endpoint)
- OLLAMA_HOST
- LMSTUDIO_BASE_URL

## Neovim integration (C# LSP)

Recommended:
- Mason.nvim + omnisharp or roslyn-language-server
- null-ls / conform.nvim for format via dotnet format

Steps:
- Ensure .NET SDK 10 is installed (global.json pins preview)
- Install omnisharp (Mason: :Mason -> omnisharp)
- Configure Neovim to run dotnet format on save for C# files

Example conform.nvim command:
```lua
require("conform").setup({
  formatters_by_ft = { cs = { "dotnet-format" } },
  format_on_save = { timeout_ms = 2000, lsp_fallback = true },
})
```

## Agentic automation

- This repo is suitable as a minimal MCP server for experimentation with tools. Start by adding additional tools under Tools/*.cs with [McpServerTool] attributes.
- For local LLMs (Ollama/LM Studio), point OPENAI_BASE_URL to the local OpenAI-compatible endpoint; your MCP tools can call those endpoints using HttpClient.

## Architecture decisions

- ADR 0001 — Blaze.MCP transport and integration strategy: docs/adr/0001-blaze-mcp-integration-and-transport.md

## License

MIT © 2025 Piotr Błażejewicz

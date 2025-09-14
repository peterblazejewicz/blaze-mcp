# ADR 0001: Blaze.MCP transport and integration strategy

Status: Accepted
Date: 2025-09-14

Context
Blaze.MCP is being integrated into a standalone gateway client that runs continually on users’ machines to provide LLM/agent tooling for locally connected devices. Current MCP integrations in some hosts (e.g., LM Studio) often spawn a one-shot server over stdio. We need a strategy that supports a long-running local service, multiple concurrent clients, and future cloud (service-to-service) use, while remaining compatible with stdio-only hosts.

Decision
Adopt a dual-transport strategy:
- Primary: Persistent MCP over HTTP + SSE on localhost
  - Blaze.MCP runs continuously, exposes an SSE endpoint for server→client events and a POST endpoint for client→server messages.
  - Supports multiple concurrent clients, session scoping per client, and reconnection.
  - Local auth via short-lived token passed in an Authorization header; bind to 127.0.0.1 by default.
- Compatibility: Stdio proxy bridging to the long-running service
  - Provide a small proxy executable that a host can spawn. The proxy speaks stdio to the host and forwards JSON-RPC to Blaze.MCP via a secure local IPC channel.
  - Preferred IPC on Windows: Named Pipe (\\.\pipe\blaze-mcp). Fallback: TCP on 127.0.0.1:<port> for cross-platform parity.

This preserves a single long-running Blaze.MCP instance (stateful caching, background jobs) while ensuring compatibility with hosts that only know stdio.

Options considered
- Stdio-only per-session process
  - Pros: Simplest for hosts. Cons: No persistent state/background work; each client requires full spawn/init.
- HTTP + SSE on localhost (chosen as primary)
  - Pros: Clean persistent transport, multi-client, reconnect-friendly. Cons: Requires host URL support.
- WebSocket/Raw TCP server
  - Pros: Simple sockets, works well with proxies. Cons: Not all MCP hosts support direct WS/TCP; still might need a stdio proxy.
- Windows Named Pipes (used behind the proxy)
  - Pros: Native, secure local ACLs, no ports. Cons: Hosts don’t speak pipes directly; needs proxy.

Consequences
- Additional artifact: a tiny stdio proxy binary.
- Better UX: fast, always-on service; multiple clients supported concurrently.
- Clear security posture: loopback-only by default; token for HTTP; OS ACLs for pipes.
- Slightly more infra: health/readiness endpoints, session management, reconnection logic.

Implementation plan
1) HTTP + SSE transport in Blaze.MCP
   - Endpoints: GET /events (SSE, server→client), POST /send (client→server), GET /healthz, GET /readyz.
   - Config: port, bind address, token file path, enabled/disabled via flags/env.
   - Session model: create per-connection session; cleanly close on disconnect.
2) Local IPC listener
   - Windows Named Pipe: \\.\pipe\blaze-mcp with appropriate ACLs; message framing uses line-delimited JSON-RPC.
   - Optional TCP fallback on 127.0.0.1:34345 (configurable).
3) Stdio proxy executable
   - stdin/stdout ↔ named pipe/TCP bridge with reconnect and exponential backoff.
   - Process exits on stdin EOF; surfaces service unavailability with clear errors.
4) Concurrency and cancellation
   - Support parallel tool calls; respect JSON-RPC cancellations; implement backpressure.
5) Security
   - Local HTTP: random token on startup; store in user-only readable file; validate Authorization header.
   - Pipes: rely on OS ACLs; run under current user; no token required.
   - Cloud: require HTTPS + token or mTLS, see below.
6) Observability
   - Structured logs for initialize, tools/list, tools/call, cancellations, errors.
   - Basic metrics: request latency, error rates, active sessions, queue depth.
7) Configuration surface (examples)
   - Flags/env for transport toggles and settings:
     ```bash path=null start=null
     Blaze.MCP --mcp-sse-enabled=true \
               --mcp-sse-port=34344 \
               --mcp-sse-bind=127.0.0.1 \
               --mcp-token-file="%LOCALAPPDATA%/Blaze.MCP/token" \
               --mcp-pipe-name="blaze-mcp" \
               --mcp-tcp-port=34345
     ```
   - LM Studio can either connect via URL (if supported) or spawn the stdio proxy.

LM Studio and local agent integration
- Preferred: Configure host to connect to http://127.0.0.1:<port> with Authorization header from the token file.
- Compatibility: Configure host to execute the stdio proxy binary; the proxy connects to the always-on Blaze.MCP via named pipe or TCP.
- Multi-client: Allow many concurrent sessions; isolate per-session state.

Cloud (future)
- Run the same SSE transport behind HTTPS with a reverse proxy (Caddy/NGINX).
- AuthZ/AuthN via mTLS or signed tokens (e.g., JWT). Restrict network via firewall/VPN/mesh (Tailscale).
- Consider tenant isolation by instance or strict authorization per session.

Open questions
- Does LM Studio support URL-based MCP connections in the target release? If not, ship proxy by default.
- Default ports and token location on Windows/macOS/Linux.
- Service management: integrate into the existing gateway process or install as a background service.
- Cross-platform transport parity (pipes vs Unix sockets).

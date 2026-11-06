# prompt-magic

Prompt generation web app: an ASP.NET Core (.NET 10) API plus a SvelteKit (Svelte 5, TypeScript) single-page app.

## Layout

| Path | What |
| --- | --- |
| `src/PromptMagic.Api` | Minimal API host, OpenAPI, health check, serves the built SPA from `wwwroot` |
| `src/PromptMagic.Core` | Prompt-generation logic, no web dependencies (`IPromptGenerator`) |
| `tests/` | xUnit tests (`Core.Tests`, `Api.Tests` with `WebApplicationFactory`) |
| `web/` | SvelteKit app (adapter-static, SPA mode), pnpm |

## Prerequisites

.NET SDK 10 (see `global.json`), Node 24+, pnpm.

## Develop

Run the two halves in separate terminals:

```bash
dotnet run --project src/PromptMagic.Api     # API on http://localhost:5080
cd web && pnpm install && pnpm dev           # UI on http://localhost:5173, /api proxied to the API
```

OpenAPI document (Development only): http://localhost:5080/openapi/v1.json

### Regenerate the typed API client

With the API running:

```bash
cd web && pnpm gen:api    # writes src/lib/api/schema.d.ts, commit the result
```

## Test and check

```bash
dotnet test PromptMagic.slnx
cd web && pnpm lint && pnpm check && pnpm test
```

## Docker

```bash
docker build -t prompt-magic .
docker run -p 8080:8080 prompt-magic
```

Builds the SPA, publishes the API, and serves both from one container on port 8080.

## Secrets

Never put LLM API keys in the frontend or in git. Locally use `dotnet user-secrets` in `src/PromptMagic.Api`; in production use environment variables or a secret store.

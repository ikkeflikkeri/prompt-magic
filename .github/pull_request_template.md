## What and why

<!-- What does this change, and why? Link the issue or task if there is one. -->

## How to verify

<!-- Commands to run, screenshots, or the behavior to check. -->

## Checklist

- [ ] `dotnet test PromptMagic.slnx` passes
- [ ] `pnpm lint && pnpm check && pnpm test` pass (when `web/` changed)
- [ ] API changes: regenerated the client with `pnpm gen:api` and committed `schema.d.ts`
- [ ] No secrets, API keys or `.env` files included
- [ ] README or docs updated if behavior or configuration changed

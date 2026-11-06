# Contributing

All changes reach `main` through a pull request. Do not push to `main` directly.

## Flow

1. Branch from an up-to-date `main`: `git switch -c <type>/<short-description>`
   (types: `feat`, `fix`, `chore`, `docs`, `ci`, `refactor`, `test`).
2. Commit in small, logical steps. Messages are short and imperative ("Add rate limiting").
3. Push the branch and open a PR. Fill in the template.
4. CI must pass. The one check that matters is **CI passed**; it fails if any job failed.
5. Squash-merge. The PR title becomes the commit message on `main`, so make it a good one.
   The branch is deleted automatically after the merge.

## Checks to run locally

```bash
dotnet test PromptMagic.slnx
dotnet format PromptMagic.slnx --verify-no-changes
cd web && pnpm lint && pnpm check && pnpm test
```

If you change the API's request or response shapes, regenerate the typed client with the API running,
and commit the result. CI fails when the committed client is stale:

```bash
cd web && pnpm gen:api
```

## What CI does

| Job | Checks |
| --- | --- |
| Backend | restore, build (warnings are errors), tests, formatting |
| Frontend | lint, type-check, tests, production build |
| API contract | the committed TypeScript client matches the API's OpenAPI document |
| Docker build | the production image builds and the running container answers `/healthz`, `/` and the API |
| CI passed | aggregates the jobs above; the check to require on `main` |

## Releases and images

- Every commit that lands on `main` publishes `ghcr.io/ikkeflikkeri/prompt-magic` tagged `latest` and `sha-<short>`.
- Tagging `vX.Y.Z` also publishes `X.Y.Z` and `X.Y`:

  ```bash
  git tag v0.1.0 && git push origin v0.1.0
  ```

- Deployment is not automated yet.

## Dependencies

Dependabot opens weekly PRs. Minor and patch updates are grouped; major updates arrive one per PR.
Review the changelog for majors, and check that the lockfile only changes what you expect.

## Secrets

Never commit API keys, `.env` files or credentials. Use `dotnet user-secrets` locally and environment
variables or a secret store in production.

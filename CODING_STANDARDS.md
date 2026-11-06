# Coding Standards

Read during review (the CI reviewer and `/code-review`), not during implementation. Mechanical rules live in CI (`dotnet format`, `pnpm lint`); this file holds judgement calls only.

## Domain language

New or changed identifiers, UI copy, test names and comments use the terms in [GLOSSARY.md](GLOSSARY.md). Flag any alias listed under a term's `_Avoid_`.

Names that predate the glossary are known drift (`LLM*` classes, `PromptRequest`, the `# Task` heading): flag them only when the change touches that line. Public config keys (`Llm:`, `SystemPrompt`) and API routes keep their names.

namespace PromptMagic.Core.Prompts;

public sealed record PromptRequest(string Goal, string? Audience = null, string? Tone = null);

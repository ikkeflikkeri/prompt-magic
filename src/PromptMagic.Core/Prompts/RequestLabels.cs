namespace PromptMagic.Core.Prompts;

/// <summary>The wording a Generator puts in front of each part of a Request, for example <c>"# Task"</c> or <c>"Goal:"</c>.</summary>
public readonly record struct RequestLabels(string Goal, string Audience, string Tone);

namespace PromptMagic.Core.Prompts;

public sealed class TemplatePromptGenerator : IPromptGenerator
{
    public Task<GeneratedPrompt> GenerateAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Goal))
        {
            throw new ArgumentException("Goal is required.", nameof(request));
        }

        // Always "\n" (not Environment.NewLine) so output is identical on every OS.
        var sections = new List<string> { $"# Task\n{request.Goal.Trim()}" };

        if (!string.IsNullOrWhiteSpace(request.Audience))
        {
            sections.Add($"# Audience\n{request.Audience.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(request.Tone))
        {
            sections.Add($"# Tone\n{request.Tone.Trim()}");
        }

        sections.Add(
            "# Instructions\n" +
            "- State any assumptions before answering.\n" +
            "- Ask a clarifying question if the task is ambiguous.\n" +
            "- Keep the answer concise and structured.");

        return Task.FromResult(new GeneratedPrompt(string.Join("\n\n", sections)));
    }
}

using System.Text;

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

        var sb = new StringBuilder();
        sb.AppendLine("# Task");
        sb.AppendLine(request.Goal.Trim());

        if (!string.IsNullOrWhiteSpace(request.Audience))
        {
            sb.AppendLine().AppendLine("# Audience").AppendLine(request.Audience.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Tone))
        {
            sb.AppendLine().AppendLine("# Tone").AppendLine(request.Tone.Trim());
        }

        sb.AppendLine().AppendLine("# Instructions");
        sb.AppendLine("- State any assumptions before answering.");
        sb.AppendLine("- Ask a clarifying question if the task is ambiguous.");
        sb.AppendLine("- Keep the answer concise and structured.");

        return Task.FromResult(new GeneratedPrompt(sb.ToString().TrimEnd()));
    }
}

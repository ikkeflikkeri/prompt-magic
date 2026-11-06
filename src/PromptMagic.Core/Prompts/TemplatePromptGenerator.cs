namespace PromptMagic.Core.Prompts;

public sealed class TemplatePromptGenerator : IPromptGenerator
{
    private static readonly RequestLabels Labels = new("# Task", "# Audience", "# Tone");

    private const string Instructions =
        "# Instructions\n" +
        "- State any assumptions before answering.\n" +
        "- Ask a clarifying question if the task is ambiguous.\n" +
        "- Keep the answer concise and structured.";

    public Task<GeneratedPrompt> GenerateAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Task.FromResult(new GeneratedPrompt(request.Render(Labels, Instructions)));
    }
}

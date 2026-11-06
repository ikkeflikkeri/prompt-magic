namespace PromptMagic.Core.Prompts;

public sealed record PromptRequest
{
    public PromptRequest(string goal, string? audience = null, string? tone = null)
    {
        if (string.IsNullOrWhiteSpace(goal))
        {
            throw new ArgumentException("Goal is required.", nameof(goal));
        }

        Goal = goal.Trim();
        Audience = TrimToNull(audience);
        Tone = TrimToNull(tone);
    }

    /// <summary>
    /// Renders the Goal, then the Audience and Tone that are present, each as its label, a line break and its value.
    /// Trailing sections follow verbatim. Sections are separated by a blank line. The separators are always "\n",
    /// never <see cref="Environment.NewLine"/>; the user's own text is left as typed.
    /// </summary>
    public string Render(RequestLabels labels, params string[] trailingSections)
    {
        var sections = new List<string> { $"{labels.Goal}\n{Goal}" };

        if (Audience is not null)
        {
            sections.Add($"{labels.Audience}\n{Audience}");
        }

        if (Tone is not null)
        {
            sections.Add($"{labels.Tone}\n{Tone}");
        }

        sections.AddRange(trailingSections);

        return string.Join("\n\n", sections);
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public string Goal { get; }

    public string? Audience { get; }

    public string? Tone { get; }
}

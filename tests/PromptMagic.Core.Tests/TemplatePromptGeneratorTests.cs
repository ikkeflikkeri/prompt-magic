using PromptMagic.Core.Prompts;

namespace PromptMagic.Core.Tests;

public class TemplatePromptGeneratorTests
{
    private readonly TemplatePromptGenerator _sut = new();

    [Fact]
    public async Task Includes_goal_audience_and_tone()
    {
        var result = await _sut.GenerateAsync(new PromptRequest("Write a haiku", "kids", "playful"));

        Assert.Contains("Write a haiku", result.Text);
        Assert.Contains("# Audience", result.Text);
        Assert.Contains("# Tone", result.Text);
    }

    [Fact]
    public async Task Omits_optional_sections_when_missing()
    {
        var result = await _sut.GenerateAsync(new PromptRequest("Write a haiku"));

        Assert.DoesNotContain("# Audience", result.Text);
        Assert.DoesNotContain("# Tone", result.Text);
    }

    [Fact]
    public async Task Produces_headed_sections_followed_by_stock_instructions()
    {
        var result = await _sut.GenerateAsync(new PromptRequest("Write a haiku", "kids", "playful"));

        Assert.Equal(
            "# Task\nWrite a haiku\n\n# Audience\nkids\n\n# Tone\nplayful\n\n" +
            "# Instructions\n" +
            "- State any assumptions before answering.\n" +
            "- Ask a clarifying question if the task is ambiguous.\n" +
            "- Keep the answer concise and structured.",
            result.Text);
    }
}

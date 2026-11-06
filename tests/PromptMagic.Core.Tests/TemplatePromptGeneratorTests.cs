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

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Rejects_blank_goal(string goal)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GenerateAsync(new PromptRequest(goal)));
    }
}

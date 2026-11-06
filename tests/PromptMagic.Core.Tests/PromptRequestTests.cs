using PromptMagic.Core.Prompts;

namespace PromptMagic.Core.Tests;

public class PromptRequestTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Rejects_blank_goal(string? goal)
    {
        Assert.Throws<ArgumentException>(() => new PromptRequest(goal!));
    }

    [Fact]
    public void Trims_goal_audience_and_tone()
    {
        var request = new PromptRequest("  Write a haiku  ", "\tkids ", " playful\n");

        Assert.Equal("Write a haiku", request.Goal);
        Assert.Equal("kids", request.Audience);
        Assert.Equal("playful", request.Tone);
    }

    private static readonly RequestLabels Labels = new("# Task", "# Audience", "# Tone");

    [Fact]
    public void Renders_goal_audience_and_tone_in_order_with_blank_line_between_sections()
    {
        var text = new PromptRequest("Write a haiku", "kids", "playful").Render(Labels);

        Assert.Equal("# Task\nWrite a haiku\n\n# Audience\nkids\n\n# Tone\nplayful", text);
    }

    [Fact]
    public void Omits_absent_audience_and_tone_together_with_their_labels()
    {
        var text = new PromptRequest("Write a haiku", audience: null, tone: "  ").Render(Labels);

        Assert.Equal("# Task\nWrite a haiku", text);
    }

    [Fact]
    public void Appends_trailing_sections_verbatim_after_the_request()
    {
        var text = new PromptRequest("Write a haiku", "kids").Render(Labels, "# Instructions\n- Be brief.", "# Extra\nmore");

        Assert.Equal("# Task\nWrite a haiku\n\n# Audience\nkids\n\n# Instructions\n- Be brief.\n\n# Extra\nmore", text);
    }

    [Fact]
    public void Adds_only_lf_separators_and_leaves_the_users_own_line_endings_alone()
    {
        var text = new PromptRequest("line one\r\nline two", "kids").Render(Labels);

        Assert.Equal("# Task\nline one\r\nline two\n\n# Audience\nkids", text);
        Assert.DoesNotContain('\r', new PromptRequest("goal", "kids", "playful").Render(Labels));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Treats_blank_audience_and_tone_as_absent(string? blank)
    {
        var request = new PromptRequest("goal", blank, blank);

        Assert.Null(request.Audience);
        Assert.Null(request.Tone);
    }
}

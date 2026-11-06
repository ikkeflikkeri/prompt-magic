using PromptMagic.Core.Prompts;

namespace PromptMagic.Core.Tests;

public class LineEndingTests
{
    [Fact]
    public async Task Template_output_uses_lf_only_on_every_os()
    {
        var result = await new TemplatePromptGenerator().GenerateAsync(new PromptRequest("goal", "audience", "tone"));

        Assert.DoesNotContain('\r', result.Text);
        Assert.Contains("\n", result.Text);
    }
}

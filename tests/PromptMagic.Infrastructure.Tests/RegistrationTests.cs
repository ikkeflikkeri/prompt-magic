using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PromptMagic.Core.Prompts;
using PromptMagic.Infrastructure.Llm;

namespace PromptMagic.Infrastructure.Tests;

public class RegistrationTests
{
    private static ServiceProvider Build(Dictionary<string, string?> settings)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPromptGeneration(config);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Falls_back_to_templates_when_llm_not_configured()
    {
        using var sp = Build(new() { ["Llm:BaseUrl"] = "https://api.openai.com/v1", ["Llm:Model"] = "" });

        Assert.IsType<TemplatePromptGenerator>(sp.GetRequiredService<IPromptGenerator>());
    }

    [Fact]
    public void Uses_llm_client_when_model_configured()
    {
        using var sp = Build(new() { ["Llm:Model"] = "gpt-test", ["Llm:ApiKey"] = "sk-test" });

        Assert.IsType<OpenAiCompatiblePromptGenerator>(sp.GetRequiredService<IPromptGenerator>());
    }

    [Fact]
    public void Api_key_alone_without_model_fails_validation()
    {
        using var sp = Build(new() { ["Llm:ApiKey"] = "sk-test" });

        var ex = Assert.Throws<OptionsValidationException>(() => sp.GetRequiredService<IOptions<LlmOptions>>().Value);
        Assert.Contains("Llm:Model", string.Join(" ", ex.Failures));
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("ftp://example.com")]
    public void Invalid_base_url_fails_validation(string baseUrl)
    {
        var result = new LlmOptionsValidator().Validate(null, new LlmOptions { Model = "m", BaseUrl = baseUrl });

        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(601)]
    public void Out_of_range_timeout_fails_validation(int seconds)
    {
        var result = new LlmOptionsValidator().Validate(null, new LlmOptions { Model = "m", TimeoutSeconds = seconds });

        Assert.True(result.Failed);
    }

    [Fact]
    public void Disabled_options_are_always_valid()
    {
        var result = new LlmOptionsValidator().Validate(null, new LlmOptions { BaseUrl = "garbage", TimeoutSeconds = 0 });

        Assert.True(result.Succeeded);
    }
}

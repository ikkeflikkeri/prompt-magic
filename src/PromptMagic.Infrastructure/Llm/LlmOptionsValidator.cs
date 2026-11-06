using Microsoft.Extensions.Options;

namespace PromptMagic.Infrastructure.Llm;

public sealed class LlmOptionsValidator : IValidateOptions<LlmOptions>
{
    public ValidateOptionsResult Validate(string? name, LlmOptions options)
    {
        if (!options.IsEnabled)
        {
            return ValidateOptionsResult.Success;
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Model))
        {
            errors.Add("Llm:Model is required when the LLM is enabled.");
        }

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            errors.Add("Llm:BaseUrl must be an absolute http(s) URL.");
        }

        if (string.IsNullOrWhiteSpace(options.SystemPrompt))
        {
            errors.Add("Llm:SystemPrompt must not be empty.");
        }

        if (options.TimeoutSeconds is < 5 or > 600)
        {
            errors.Add("Llm:TimeoutSeconds must be between 5 and 600.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}

namespace PromptMagic.Infrastructure.Llm;

/// <summary>Settings for any OpenAI-compatible chat completions endpoint (OpenAI, Ollama, OpenRouter, vLLM, ...).</summary>
public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public const string DefaultSystemPrompt =
        "You are an expert prompt engineer. Turn the user's rough request into a clear, well-structured " +
        "prompt that another AI model can follow. Include the task, relevant context, constraints and the " +
        "desired output format where useful. Reply with the finished prompt text only, with no preamble " +
        "and no commentary.";

    /// <summary>API root, without the trailing /chat/completions.</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>Bearer token. Optional for local servers such as Ollama. Keep it in user-secrets or environment variables.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Model name understood by the endpoint. The LLM is used when this or ApiKey is set.</summary>
    public string Model { get; set; } = "";

    public string SystemPrompt { get; set; } = DefaultSystemPrompt;

    /// <summary>Timeout for one attempt, in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 60;

    public bool IsEnabled => !string.IsNullOrWhiteSpace(Model) || !string.IsNullOrWhiteSpace(ApiKey);
}

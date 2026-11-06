namespace PromptMagic.Core.Prompts;

/// <summary>The prompt could not be generated (for example the upstream LLM failed or returned nothing usable).</summary>
public sealed class PromptGenerationException : Exception
{
    public PromptGenerationException(string message) : base(message)
    {
    }

    public PromptGenerationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

namespace PromptMagic.Core.Prompts;

/// <summary>Turns a rough request into a well-structured prompt. Swap the implementation to use an LLM.</summary>
public interface IPromptGenerator
{
    Task<GeneratedPrompt> GenerateAsync(PromptRequest request, CancellationToken cancellationToken = default);
}

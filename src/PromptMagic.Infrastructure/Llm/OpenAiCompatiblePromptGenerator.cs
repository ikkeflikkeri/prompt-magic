using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PromptMagic.Core.Prompts;

namespace PromptMagic.Infrastructure.Llm;

/// <summary>Generates prompts through an OpenAI-compatible <c>/chat/completions</c> endpoint.</summary>
public sealed class OpenAiCompatiblePromptGenerator(HttpClient http, IOptions<LlmOptions> options) : IPromptGenerator
{
    private static readonly RequestLabels Labels = new("Goal:", "Audience:", "Tone:");

    private readonly LlmOptions _options = options.Value;

    public async Task<GeneratedPrompt> GenerateAsync(PromptRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var body = new ChatRequest(
            _options.Model,
            [
                new ChatMessage("system", _options.SystemPrompt),
                new ChatMessage("user", request.Render(Labels)),
            ]);

        try
        {
            using var response = await http.PostAsJsonAsync("chat/completions", body, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Deliberately do not include the response body: it may echo request data.
                throw new PromptGenerationException($"The LLM endpoint returned HTTP {(int)response.StatusCode}.");
            }

            var completion = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken);
            var text = completion?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

            if (string.IsNullOrEmpty(text))
            {
                throw new PromptGenerationException("The LLM endpoint returned an empty response.");
            }

            return new GeneratedPrompt(text);
        }
        catch (HttpRequestException ex)
        {
            throw new PromptGenerationException("Could not reach the LLM endpoint.", ex);
        }
        catch (JsonException ex)
        {
            throw new PromptGenerationException("The LLM endpoint returned an unreadable response.", ex);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PromptGenerationException("The LLM endpoint timed out.", ex);
        }
    }

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record ChatResponse(
        [property: JsonPropertyName("choices")] IReadOnlyList<ChatChoice>? Choices);

    private sealed record ChatChoice(
        [property: JsonPropertyName("message")] ChatResponseMessage? Message);

    private sealed record ChatResponseMessage(
        [property: JsonPropertyName("content")] string? Content);
}

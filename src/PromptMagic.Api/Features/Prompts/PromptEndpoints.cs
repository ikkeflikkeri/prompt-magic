using System.ComponentModel.DataAnnotations;
using PromptMagic.Core.Prompts;

namespace PromptMagic.Api.Features.Prompts;

public sealed record GeneratePromptRequest(
    [property: Required, StringLength(2000, MinimumLength = 3)] string Goal,
    [property: StringLength(200)] string? Audience,
    [property: StringLength(200)] string? Tone);

public sealed record GeneratePromptResponse(string Prompt);

public static class PromptEndpoints
{
    public static IEndpointRouteBuilder MapPromptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/prompts").WithTags("Prompts");

        group.MapPost("/generate", async (GeneratePromptRequest body, IPromptGenerator generator, CancellationToken ct) =>
        {
            var result = await generator.GenerateAsync(new PromptRequest(body.Goal, body.Audience, body.Tone), ct);
            return TypedResults.Ok(new GeneratePromptResponse(result.Text));
        })
        .WithName("GeneratePrompt");

        return app;
    }
}

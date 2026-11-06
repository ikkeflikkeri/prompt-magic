using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
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

        group.MapPost("/generate", GenerateAsync)
            .WithName("GeneratePrompt")
            .ProducesProblem(StatusCodes.Status502BadGateway);

        return app;
    }

    private static async Task<Results<Ok<GeneratePromptResponse>, ProblemHttpResult>> GenerateAsync(
        GeneratePromptRequest body,
        IPromptGenerator generator,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        try
        {
            var result = await generator.GenerateAsync(new PromptRequest(body.Goal, body.Audience, body.Tone), ct);
            return TypedResults.Ok(new GeneratePromptResponse(result.Text));
        }
        catch (PromptGenerationException ex)
        {
            logger.LogWarning(ex, "Prompt generation failed");
            return TypedResults.Problem(
                title: "Prompt generation failed",
                detail: "The language model could not produce a prompt. Please try again.",
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}

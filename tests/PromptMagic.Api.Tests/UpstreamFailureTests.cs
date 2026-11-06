using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PromptMagic.Api.Features.Prompts;
using PromptMagic.Core.Prompts;

namespace PromptMagic.Api.Tests;

public class UpstreamFailureTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Llm_failure_returns_502_problem_details_without_internal_detail()
    {
        var client = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddSingleton<IPromptGenerator>(new ThrowingGenerator()))).CreateClient();

        var response = await client.PostAsJsonAsync("/api/prompts/generate",
            new GeneratePromptRequest("Write a haiku about rain", null, null));

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Prompt generation failed", body);
        Assert.DoesNotContain("super-secret-upstream-detail", body);
    }

    private sealed class ThrowingGenerator : IPromptGenerator
    {
        public Task<GeneratedPrompt> GenerateAsync(PromptRequest request, CancellationToken cancellationToken = default) =>
            throw new PromptGenerationException("super-secret-upstream-detail");
    }
}

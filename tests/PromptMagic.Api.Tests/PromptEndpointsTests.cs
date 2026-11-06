using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PromptMagic.Api.Features.Prompts;

namespace PromptMagic.Api.Tests;

public class PromptEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_endpoint_returns_ok()
    {
        var response = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Generate_returns_prompt()
    {
        var response = await _client.PostAsJsonAsync("/api/prompts/generate",
            new GeneratePromptRequest("Write a haiku about rain", null, null));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<GeneratePromptResponse>();
        Assert.Contains("Write a haiku about rain", body!.Prompt);
    }

    [Fact]
    public async Task Generate_rejects_too_short_goal()
    {
        var response = await _client.PostAsJsonAsync("/api/prompts/generate",
            new GeneratePromptRequest("hi", null, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

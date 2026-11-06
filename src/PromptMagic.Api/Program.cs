using PromptMagic.Api.Features.Prompts;
using PromptMagic.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddHealthChecks();
// OpenAI-compatible LLM when Llm:Model / Llm:ApiKey is configured, offline templates otherwise.
builder.Services.AddPromptGeneration(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/healthz");
app.MapPromptEndpoints();

// Serve the built SvelteKit SPA (adapter-static output copied to wwwroot).
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;

using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using PromptMagic.Core.Prompts;
using PromptMagic.Infrastructure.Llm;

namespace PromptMagic.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers <see cref="IPromptGenerator"/>: an OpenAI-compatible LLM client when <c>Llm:Model</c> or
    /// <c>Llm:ApiKey</c> is configured, otherwise the offline <see cref="TemplatePromptGenerator"/>.
    /// </summary>
    public static IServiceCollection AddPromptGeneration(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(LlmOptions.SectionName);
        var llm = section.Get<LlmOptions>() ?? new LlmOptions();

        services.AddOptions<LlmOptions>().Bind(section).ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<LlmOptions>, LlmOptionsValidator>());

        if (!llm.IsEnabled)
        {
            services.AddSingleton<IPromptGenerator, TemplatePromptGenerator>();
            return services;
        }

        var attemptTimeout = TimeSpan.FromSeconds(Math.Clamp(llm.TimeoutSeconds, 5, 600));

        services
            .AddHttpClient<IPromptGenerator, OpenAiCompatiblePromptGenerator>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<LlmOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                // Timeouts are enforced by the resilience handler below.
                client.Timeout = Timeout.InfiniteTimeSpan;

                if (!string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
                }
            })
            .AddStandardResilienceHandler(o =>
            {
                o.Retry.MaxRetryAttempts = 2;
                o.AttemptTimeout.Timeout = attemptTimeout;
                o.TotalRequestTimeout.Timeout = attemptTimeout * 3 + TimeSpan.FromSeconds(10);
                o.CircuitBreaker.SamplingDuration = attemptTimeout * 3;
            });

        return services;
    }
}

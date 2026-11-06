using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PromptMagic.Core.Prompts;
using PromptMagic.Infrastructure.Llm;

namespace PromptMagic.Infrastructure.Tests;

public class OpenAiCompatiblePromptGeneratorTests
{
    private const string OkBody = """{"choices":[{"message":{"role":"assistant","content":"  Write a haiku about rain.  "},"finish_reason":"stop"}]}""";

    private static OpenAiCompatiblePromptGenerator Create(FakeHandler handler, LlmOptions? options = null)
    {
        options ??= new LlmOptions { Model = "test-model" };
        var http = new HttpClient(handler) { BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/") };
        return new OpenAiCompatiblePromptGenerator(http, Options.Create(options));
    }

    [Fact]
    public async Task Posts_chat_completion_and_returns_trimmed_content()
    {
        var handler = new FakeHandler(HttpStatusCode.OK, OkBody);
        var sut = Create(handler, new LlmOptions { Model = "test-model", BaseUrl = "http://llm.test/v1/" });

        var result = await sut.GenerateAsync(new PromptRequest("rain haiku", "kids", "playful"));

        Assert.Equal("Write a haiku about rain.", result.Text);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("http://llm.test/v1/chat/completions", request.Uri);

        using var json = JsonDocument.Parse(request.Body);
        Assert.Equal("test-model", json.RootElement.GetProperty("model").GetString());

        var messages = json.RootElement.GetProperty("messages");
        Assert.Equal(2, messages.GetArrayLength());
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());

        var user = messages[1].GetProperty("content").GetString()!;
        Assert.Contains("rain haiku", user);
        Assert.Contains("kids", user);
        Assert.Contains("playful", user);
        Assert.DoesNotContain('\r', user);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Non_success_status_becomes_PromptGenerationException_without_leaking_body(HttpStatusCode status)
    {
        var sut = Create(new FakeHandler(status, """{"error":{"message":"secret detail"}}"""));

        var ex = await Assert.ThrowsAsync<PromptGenerationException>(() => sut.GenerateAsync(new PromptRequest("goal")));

        Assert.Contains(((int)status).ToString(), ex.Message);
        Assert.DoesNotContain("secret detail", ex.Message);
    }

    [Theory]
    [InlineData("""{"choices":[]}""")]
    [InlineData("""{"choices":[{"message":{"content":"   "}}]}""")]
    [InlineData("""{}""")]
    public async Task Empty_completion_becomes_PromptGenerationException(string body)
    {
        var sut = Create(new FakeHandler(HttpStatusCode.OK, body));

        await Assert.ThrowsAsync<PromptGenerationException>(() => sut.GenerateAsync(new PromptRequest("goal")));
    }

    [Fact]
    public async Task Invalid_json_becomes_PromptGenerationException()
    {
        var sut = Create(new FakeHandler(HttpStatusCode.OK, "<html>not json</html>"));

        await Assert.ThrowsAsync<PromptGenerationException>(() => sut.GenerateAsync(new PromptRequest("goal")));
    }

    [Fact]
    public async Task Network_failure_becomes_PromptGenerationException()
    {
        var sut = Create(new FakeHandler(new HttpRequestException("connection refused")));

        await Assert.ThrowsAsync<PromptGenerationException>(() => sut.GenerateAsync(new PromptRequest("goal")));
    }

    [Fact]
    public async Task Timeout_becomes_PromptGenerationException()
    {
        var sut = Create(new FakeHandler(new TaskCanceledException("timed out")));

        await Assert.ThrowsAsync<PromptGenerationException>(() => sut.GenerateAsync(new PromptRequest("goal")));
    }

    [Fact]
    public async Task Caller_cancellation_is_not_wrapped()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var sut = Create(new FakeHandler(HttpStatusCode.OK, OkBody));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.GenerateAsync(new PromptRequest("goal"), cts.Token));
    }

    [Fact]
    public async Task Sends_the_request_as_labelled_sections_with_lf_separators()
    {
        var handler = new FakeHandler(HttpStatusCode.OK, OkBody);
        var sut = Create(handler);

        await sut.GenerateAsync(new PromptRequest("rain haiku", "kids", "playful"));

        using var json = JsonDocument.Parse(Assert.Single(handler.Requests).Body);
        var user = json.RootElement.GetProperty("messages")[1].GetProperty("content").GetString();
        Assert.Equal("Goal:\nrain haiku\n\nAudience:\nkids\n\nTone:\nplayful", user);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body = "";
        private readonly Exception? _throw;

        public FakeHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        public FakeHandler(Exception toThrow) => _throw = toThrow;

        public List<CapturedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new CapturedRequest(request.Method, request.RequestUri!.ToString(), body));

            if (_throw is not null)
            {
                throw _throw;
            }

            return new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed record CapturedRequest(HttpMethod Method, string Uri, string Body);
}

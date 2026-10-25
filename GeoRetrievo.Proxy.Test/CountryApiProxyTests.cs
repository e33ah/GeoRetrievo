namespace GeoRetrievo.Proxy.Test;

public class CountryApiProxyTests {
    [Fact]
    public async Task ForwardsCountryRequestWithBearerTokenAndPreservesResponse() {
        HttpRequestMessage? capturedRequest = null;
        using var apiClient = new HttpClient(new ApiHttpHandler(request => {
            capturedRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) {
                Content = new StringContent("""{"error":"rate limited"}"""),
            };
            response.Content.Headers.ContentType = new("application/json");
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(
                TimeSpan.FromSeconds(10));
            return response;
        })) {
            BaseAddress = new Uri("https://private-api.example.run.app/"),
        };
        var proxy = CreateProxy(apiClient, new FixedTokenProvider("test-id-token"));
        var context = CreateContext();

        await proxy.ForwardAsync(context, "/countries/CA");

        capturedRequest.ShouldNotBeNull();
        capturedRequest!.Method.ShouldBe(HttpMethod.Get);
        capturedRequest.RequestUri!.AbsoluteUri.ShouldBe(
            "https://private-api.example.run.app/countries/CA");
        capturedRequest.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        capturedRequest.Headers.Authorization.Parameter.ShouldBe("test-id-token");
        context.Response.StatusCode.ShouldBe(StatusCodes.Status429TooManyRequests);
        context.Response.ContentType.ShouldBe("application/json");
        context.Response.Headers.CacheControl.ToString().ShouldBe("no-store");
        context.Response.Headers.RetryAfter.ToString().ShouldBe("10");
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        (await reader.ReadToEndAsync(TestContext.Current.CancellationToken))
            .ShouldBe("""{"error":"rate limited"}""");
    }

    [Fact]
    public async Task ReturnsServiceUnavailableWhenIdentityTokenCannotBeFetched() {
        bool apiCalled = false;
        using var apiClient = new HttpClient(new ApiHttpHandler(_ => {
            apiCalled = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        }));
        var proxy = CreateProxy(apiClient, new FixedTokenProvider(new HttpRequestException()));
        var context = CreateContext();

        await proxy.ForwardAsync(context, "/countries");

        context.Response.StatusCode.ShouldBe(StatusCodes.Status503ServiceUnavailable);
        apiCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ReturnsBadGatewayWhenApiRequestFails() {
        using var apiClient = new HttpClient(new ApiHttpHandler(
            _ => throw new HttpRequestException("API unavailable")));
        var proxy = CreateProxy(apiClient, new FixedTokenProvider("test-id-token"));
        var context = CreateContext();

        await proxy.ForwardAsync(context, "/countries");

        context.Response.StatusCode.ShouldBe(StatusCodes.Status502BadGateway);
    }

    [Fact]
    public async Task ReturnsGatewayTimeoutWhenApiRequestTimesOut() {
        using var apiClient = new HttpClient(new ApiHttpHandler(
            _ => throw new TaskCanceledException("API timed out")));
        var proxy = CreateProxy(apiClient, new FixedTokenProvider("test-id-token"));
        var context = CreateContext();

        await proxy.ForwardAsync(context, "/countries");

        context.Response.StatusCode.ShouldBe(StatusCodes.Status504GatewayTimeout);
    }

    private static CountryApiProxy CreateProxy(
        HttpClient client,
        ICloudRunIdentityTokenProvider tokenProvider) {
        client.BaseAddress ??= new Uri("https://private-api.example.run.app/");
        return new CountryApiProxy(client, tokenProvider, NullLogger<CountryApiProxy>.Instance);
    }

    private static DefaultHttpContext CreateContext() {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private sealed class FixedTokenProvider(string token) : ICloudRunIdentityTokenProvider {
        public FixedTokenProvider(Exception exception) : this(string.Empty) {
            this.exception = exception;
        }

        private readonly Exception? exception;

        public Task<string> GetTokenAsync(CancellationToken cancellationToken) =>
            exception is null ? Task.FromResult(token) : Task.FromException<string>(exception);
    }

    private sealed class ApiHttpHandler(
        Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(send(request));
    }
}

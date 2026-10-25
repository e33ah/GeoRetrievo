namespace GeoRetrievo.Api.Test;

public class CountryRateLimitingTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>> {
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task CountryEndpointsReturnTooManyRequestsAfterSixtyRequestsPerMinute() {
        for (int requestNumber = 0; requestNumber < 60; requestNumber++) {
            using var response = await client.GetAsync("/countries", TestContext.Current.CancellationToken);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        using var limitedResponse = await client.GetAsync("/countries/CA", TestContext.Current.CancellationToken);
        limitedResponse.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        int.TryParse(limitedResponse.Headers.RetryAfter?.ToString(), out int retryAfterSeconds).ShouldBeTrue();
        retryAfterSeconds.ShouldBeGreaterThanOrEqualTo(1);
        (await limitedResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))
            .ShouldBe("Too many requests. Please try again later.");

        using var healthResponse = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        healthResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

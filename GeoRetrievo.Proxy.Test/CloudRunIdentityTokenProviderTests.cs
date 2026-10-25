namespace GeoRetrievo.Proxy.Test;

public class CloudRunIdentityTokenProviderTests {
    [Fact]
    public async Task RequestsAnIdTokenForTheApiAudience() {
        HttpRequestMessage? capturedRequest = null;
        using var client = new HttpClient(new MetadataHttpHandler(request => {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("signed-id-token"),
            };
        }));
        client.DefaultRequestHeaders.Add("Metadata-Flavor", "Google");
        var provider = new CloudRunIdentityTokenProvider(
            new Uri("https://private-api-123-uc.a.run.app/"),
            client);

        string token = await provider.GetTokenAsync(TestContext.Current.CancellationToken);

        token.ShouldBe("signed-id-token");
        capturedRequest.ShouldNotBeNull();
        capturedRequest!.RequestUri!.Host.ShouldBe("metadata.google.internal");
        capturedRequest.RequestUri.AbsolutePath.ShouldBe(
            "/computeMetadata/v1/instance/service-accounts/default/identity");
        capturedRequest.RequestUri.Query.ShouldContain(
            "audience=https%3A%2F%2Fprivate-api-123-uc.a.run.app");
        capturedRequest.RequestUri.Query.ShouldContain("format=full");
        capturedRequest.Headers.GetValues("Metadata-Flavor").ShouldContain("Google");
    }
}

internal sealed class MetadataHttpHandler(
    Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler {
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        Task.FromResult(send(request));
}

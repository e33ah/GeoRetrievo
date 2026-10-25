namespace GeoRetrievo.Proxy.Services;

public sealed class CloudRunIdentityTokenProvider(Uri audience, HttpClient metadataClient) : ICloudRunIdentityTokenProvider {
    private const string GOOGLE_METADATA_URL = "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/identity";

    // <summary>
    // Gets a signed identity token for the Cloud Run service account, which can be used to authenticate requests to a private Cloud Run service.
    // </summary>
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken) {
        string metadataUrl =
            GOOGLE_METADATA_URL +
            $"?audience={Uri.EscapeDataString(audience.GetLeftPart(UriPartial.Authority))}&format=full";
        return await metadataClient.GetStringAsync(metadataUrl, cancellationToken);
    }
}

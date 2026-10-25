namespace GeoRetrievo.Proxy.Services;

public interface ICloudRunIdentityTokenProvider {
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}

namespace GeoRetrievo.Api.Services;

public interface ICountryDataSource {
    Task<Stream> DownloadAsync(
        string bucket,
        string objectName,
        CancellationToken cancellationToken = default);
}

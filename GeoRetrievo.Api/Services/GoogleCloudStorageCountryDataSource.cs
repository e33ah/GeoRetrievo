using Google.Cloud.Storage.V1;

namespace GeoRetrievo.Api.Services;

public sealed class GoogleCloudStorageCountryDataSource : ICountryDataSource {
    public async Task<Stream> DownloadAsync(
        string bucket,
        string objectName,
        CancellationToken cancellationToken = default) {
        using var storageClient = await StorageClient.CreateAsync();
        var content = new MemoryStream();
        await storageClient.DownloadObjectAsync(
            bucket,
            objectName,
            content,
            cancellationToken: cancellationToken);
        content.Position = 0;
        return content;
    }
}

using Google.Cloud.Storage.V1;

namespace GeoRetrievo.Api.Services;

public sealed class GoogleCloudStorageCountryDataSource(Func<string, string, CancellationToken, Task<Stream>> downloadObjectAsync) : ICountryDataSource {
    private readonly Func<string, string, CancellationToken, Task<Stream>> downloadObjectAsync = downloadObjectAsync ?? throw new ArgumentNullException(nameof(downloadObjectAsync));

    public GoogleCloudStorageCountryDataSource() : this(async (bucket, objectName, cancellationToken) => {
            using var storageClient = await StorageClient.CreateAsync();
            var content = new MemoryStream();
            await storageClient.DownloadObjectAsync(
                bucket,
                objectName,
                content,
                cancellationToken: cancellationToken);
            content.Position = 0;
            return content;
        }) {
    }

    public async Task<Stream> DownloadAsync(
        string bucket,
        string objectName,
        CancellationToken cancellationToken = default) {
        var content = await downloadObjectAsync(bucket, objectName, cancellationToken);
        content.Position = 0;
        return content;
    }
}

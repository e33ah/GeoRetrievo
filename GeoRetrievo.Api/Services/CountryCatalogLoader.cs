using Google.Cloud.Storage.V1;

namespace GeoRetrievo.Api.Services;

public static class CountryCatalogLoader {
    public static async Task<CountryCatalog> LoadAsync(IConfiguration configuration, CancellationToken cancellationToken = default) {
        string? bucket = configuration["CountryData:Bucket"];

        if (string.IsNullOrWhiteSpace(bucket)) {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Data", "countries.json");
            await using var content = File.OpenRead(filePath);
            return await CountryCatalog.FromJsonAsync(content, cancellationToken);
        }

        string objectName = configuration["CountryData:Object"] ?? "countries.json";
        using var storageClient = await StorageClient.CreateAsync();

        await using var downloadedContent = new MemoryStream();
        await storageClient.DownloadObjectAsync(
            bucket,
            objectName,
            downloadedContent,
            cancellationToken: cancellationToken);
        downloadedContent.Position = 0;
        return await CountryCatalog.FromJsonAsync(downloadedContent, cancellationToken);
    }
}

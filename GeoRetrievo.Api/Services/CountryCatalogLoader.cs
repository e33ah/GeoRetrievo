namespace GeoRetrievo.Api.Services;

public sealed class CountryCatalogLoader(IConfiguration configuration, ICountryDataSource countryDataSource) {
    private const string DEFAULT_JSON_FILE_NAME = "countries.json";
    private readonly IConfiguration configuration = configuration;
    private readonly ICountryDataSource countryDataSource = countryDataSource;

    public async Task<CountryCatalog> LoadAsync(CancellationToken cancellationToken = default) {
        string? bucket = configuration["CountryData:Bucket"];

        if (string.IsNullOrWhiteSpace(bucket)) {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Data", DEFAULT_JSON_FILE_NAME);
            await using var content = File.OpenRead(filePath);
            return await CountryCatalog.FromJsonAsync(content, cancellationToken);
        }

        string objectName = configuration["CountryData:Object"] ?? DEFAULT_JSON_FILE_NAME;
        await using var downloadedContent = await countryDataSource.DownloadAsync(bucket, objectName, cancellationToken);

        return await CountryCatalog.FromJsonAsync(downloadedContent, cancellationToken);
    }
}

using System.Diagnostics;

namespace GeoRetrievo.Api.Services;

public sealed class CountryCatalogLoader(IConfiguration configuration,ICountryDataSource countryDataSource,ILogger<CountryCatalogLoader> logger) {
    private const string DEFAULT_JSON_FILE_NAME = "countries.json";
    private readonly IConfiguration configuration = configuration;
    private readonly ICountryDataSource countryDataSource = countryDataSource;
    private readonly ILogger<CountryCatalogLoader> logger = logger;

    public async Task<CountryCatalog> LoadAsync(CancellationToken cancellationToken = default) {
        string? bucket = configuration["CountryData:Bucket"];
        string objectName = configuration["CountryData:Object"] ?? DEFAULT_JSON_FILE_NAME;
        string source = string.IsNullOrWhiteSpace(bucket) ? "local-file" : "cloud-storage";
        var stopwatch = Stopwatch.StartNew();

        try {
            Stream content;
            if (string.IsNullOrWhiteSpace(bucket)) {
                logger.LogInformation("Loading country catalog from {Source}", source);

                string filePath = Path.Combine(AppContext.BaseDirectory, "Data", DEFAULT_JSON_FILE_NAME);
                content = File.OpenRead(filePath);

            } else {
                logger.LogInformation("Loading country catalog from {Source}, bucket {Bucket}, object {ObjectName}", source, bucket, objectName);

                content = await countryDataSource.DownloadAsync(bucket, objectName, cancellationToken);
            }

            await using (content) {
                var catalog = await CountryCatalog.FromJsonAsync(content, cancellationToken);

                logger.LogInformation("Loaded country catalog from {Source} with {CountryCount} countries in {ElapsedMilliseconds} ms", source, catalog.GetAll().Count, stopwatch.ElapsedMilliseconds);
                return catalog;
            }
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            logger.LogDebug("Country catalog load was cancelled from {Source}", source);
            throw;
        } catch (Exception exception) {
            logger.LogError( exception, "Failed to load country catalog from {Source}, bucket {Bucket}, object {ObjectName}", source, bucket, objectName);
            throw;
        }
    }
}

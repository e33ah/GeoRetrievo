namespace GeoRetrievo.Test;

public class CountryCatalogLoaderTests {
    private const string JSON = """
        [{"name":"Canada","code":"CA","capital":"Ottawa","region":"Americas","population":35540419,"currencyCode":"CAD","currencySymbol":"CA$","languages":["en","fr"],"callingCode":"+1","flag":"https://flagcdn.com/w320/ca.png"}]
        """;

    [Fact]
    public async Task LoadAsyncDownloadsConfiguredBucketObjectAndParsesCatalog() {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["CountryData:Bucket"] = "countries-bucket",
                ["CountryData:Object"] = "data/countries.json"
            })
            .Build();
        var dataSource = new FakeCountryDataSource(JSON);
        var loader = new CountryCatalogLoader(configuration, dataSource);

        var catalog = await loader.LoadAsync(TestContext.Current.CancellationToken);

        dataSource.Bucket.ShouldBe("countries-bucket");
        dataSource.ObjectName.ShouldBe("data/countries.json");
        catalog.GetByCode("CA")!.Name.ShouldBe("Canada");
    }

    [Fact]
    public async Task LoadAsyncUsesDefaultObjectNameWhenNotConfigured() {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["CountryData:Bucket"] = "countries-bucket"
            })
            .Build();
        var dataSource = new FakeCountryDataSource(JSON);
        var loader = new CountryCatalogLoader(configuration, dataSource);

        await loader.LoadAsync(TestContext.Current.CancellationToken);

        dataSource.ObjectName.ShouldBe("countries.json");
    }

    private sealed class FakeCountryDataSource(string json) : ICountryDataSource {
        public string? Bucket { get; private set; }
        public string? ObjectName { get; private set; }

        public Task<Stream> DownloadAsync(
            string bucket,
            string objectName,
            CancellationToken cancellationToken = default) {
            Bucket = bucket;
            ObjectName = objectName;
            Stream content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            return Task.FromResult(content);
        }
    }
}

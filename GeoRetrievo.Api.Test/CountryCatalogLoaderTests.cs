namespace GeoRetrievo.Api.Test;

public class CountryCatalogLoaderTests {

    [Fact]
    public async Task LoadAsyncDownloadsConfiguredBucketObjectAndParsesCatalog() {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["CountryData:Bucket"] = "countries-bucket",
                ["CountryData:Object"] = "data/countries.json"
            })
            .Build();
        var dataSource = new FakeCountryDataSource(JSON);
        var loader = new CountryCatalogLoader(
            configuration,
            dataSource,
            NullLogger<CountryCatalogLoader>.Instance);

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
        var loader = new CountryCatalogLoader(configuration, dataSource, NullLogger<CountryCatalogLoader>.Instance);

        await loader.LoadAsync(TestContext.Current.CancellationToken);

        dataSource.ObjectName.ShouldBe("countries.json");
    }
}

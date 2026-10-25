namespace GeoRetrievo.Test;

internal static class TestHelper {
    public const string JSON = """
        [{"name":"Canada","code":"CA","capital":"Ottawa","region":"North America","population":35540419,"currencyCode":"CAD","currencySymbol":"CA$","languages":["en","fr"],"callingCode":"+1","flag":"https://flagcdn.com/w320/ca.png"}]
        """;

    public static Country ExpectedCountry => new("Canada", "CA", "Ottawa", "North America", 35540419, "CAD", "CA$", ["en", "fr"], "+1", "https://flagcdn.com/w320/ca.png");

    internal sealed class FakeCountryDataSource(string json) : ICountryDataSource {
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

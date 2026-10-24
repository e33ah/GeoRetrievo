namespace GeoRetrievo.Test;

public class CountryCatalogTests {
    const string JSON = """
            [{"name":"Canada","code":"CA","capital":"Ottawa","region":"Americas","population":38246108,"currencyCode":"CAD","currencySymbol":"CA$","languages":["en","fr"],"callingCode":"+1","flag":"https://flagcdn.com/w320/ca.png"}]
            """;

    [Fact]
    public void FromJsonDeserializesCountryProperties() {
        var catalog = CountryCatalog.FromJson(JSON);
        var expected = new Country("Canada", "CA", "Ottawa", "Americas", 38246108, "CAD", "CA$", ["en", "fr"], "+1", "https://flagcdn.com/w320/ca.png");

        var actual = catalog.GetAll().ShouldHaveSingleItem();

        actual.Languages.ShouldBe(expected.Languages);
        (actual with { Languages = expected.Languages }).ShouldBe(expected, "Deserialized country should match the JSON data");
    }

    [Fact]
    public async Task FromJsonAsyncDeserializesCountryProperties() {
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(JSON));

        var catalog = await CountryCatalog.FromJsonAsync(stream, TestContext.Current.CancellationToken);

        catalog.GetByCode("CA")!.Name.ShouldBe("Canada");
    }

    [Fact]
    public void FromJsonRejectsNullInsteadOfAnArray() {
        var exception = Should.Throw<InvalidDataException>(() => CountryCatalog.FromJson("null"));

        exception.Message.ShouldBe("Country data must contain a JSON array, not null.");
    }

    [Fact]
    public void GetByCodeReturnsMatchingCountry() {
        var expected = new Country("Canada", "CA", "Ottawa", "Americas", 38246108, "CAD", "CA$", ["en", "fr"], "+1", "https://flagcdn.com/w320/ca.png");
        var catalog = CountryCatalog.FromJson(JSON);

        var actual = catalog.GetByCode("CA");

        actual.ShouldNotBeNull();
        actual!.Languages.ShouldBe(expected.Languages);
        (actual with { Languages = expected.Languages }).ShouldBe(expected);
    }

    [Fact]
    public void GetByCodeReturnsNullWhenCodeDoesNotMatch() {
        var catalog = CountryCatalog.FromJson(JSON);

        catalog.GetByCode("US").ShouldBeNull();
        catalog.GetByCode("ca").ShouldBeNull();
    }
}

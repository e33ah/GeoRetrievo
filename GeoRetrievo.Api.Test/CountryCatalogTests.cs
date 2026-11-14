namespace GeoRetrievo.Api.Test;

public class CountryCatalogTests {
    [Fact]
    public void FromJsonDeserializesCountryProperties() {
        var catalog = CountryCatalog.FromJson(JSON);
        var expected = ExpectedCountry;

        var actual = catalog.GetAll().ShouldHaveSingleItem();

        actual.Languages.ShouldBe(expected.Languages);
        (actual with { Languages = expected.Languages }).ShouldBe(expected, "Deserialized country should match the JSON data");
    }

    [Fact]
    public async Task FromJsonAsyncDeserializesCountryProperties() {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JSON));

        var catalog = await CountryCatalog.FromJsonAsync(stream, TestContext.Current.CancellationToken);

        catalog.GetByCode("CA")!.Name.ShouldBe("Canada");
    }

    [Fact]
    public void FromJsonRejectsNullInsteadOfAnArray() {
        var exception = Should.Throw<InvalidDataException>(() => CountryCatalog.FromJson("null"));

        exception.Message.ShouldBe("Country data must contain a JSON array, not null.");
    }

    [Fact]
    public async Task FromJsonAsyncRejectsNullInsteadOfAnArray() {
        var exception = Should.Throw<InvalidDataException>(() => CountryCatalog.FromJsonAsync(new MemoryStream(Encoding.UTF8.GetBytes("null")), TestContext.Current.CancellationToken));

        exception.Message.ShouldBe("Country data must contain a JSON array, not null.");
    }

    [Fact]
    public void GetByCodeReturnsMatchingCountry() {
        var expected = ExpectedCountry;
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

    [Fact]
    public void FromJsonMatchesPropertyNamesCaseInsensitively() {
        string json = JSON.Replace("\"name\"", "\"NAME\"", StringComparison.Ordinal);

        var catalog = CountryCatalog.FromJson(json);

        catalog.GetByCode("CA")!.Name.ShouldBe("Canada");
    }
}

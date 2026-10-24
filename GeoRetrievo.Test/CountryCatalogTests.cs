namespace GeoRetrievo.Test;

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
}

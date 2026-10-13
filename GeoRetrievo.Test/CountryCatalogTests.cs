using GeoRetrievo.Api.Models;
using GeoRetrievo.Api.Services;
using Shouldly;

namespace GeoRetrievo.Test;

public class CountryCatalogTests {
    [Fact]
    public void FromJsonDeserializesCountryPropertiesCaseInsensitively() {
        const string JSON = """
            [{"name":"Canada","code":"CA","capital":"Ottawa","region":"Americas","population":38246108,"currency":"CAD","callingCode":"+1"}]
            """;

        var catalog = CountryCatalog.FromJson(JSON);

        catalog.GetAll().ShouldHaveSingleItem().ShouldBe(
            new Country("Canada", "CA", "Ottawa", "Americas", 38246108, "CAD", "+1"),
            "Deserialized country should match the JSON data");
    }

    [Fact]
    public void FromJsonRejectsNullInsteadOfAnArray() {
        var exception = Should.Throw<InvalidDataException>(() => CountryCatalog.FromJson("null"));

        exception.Message.ShouldBe("Country data must contain a JSON array, not null.");
    }

    [Fact]
    public void GetByCodeReturnsMatchingCountry() {
        var expected = new Country("Canada", "CA", "Ottawa", "Americas", 38246108, "CAD", "+1");
        var catalog = CountryCatalog.FromJson(
            """[{"Name":"Canada","Code":"CA","Capital":"Ottawa","Region":"Americas","Population":38246108,"Currency":"CAD","CallingCode":"+1"}]""");

        var actual = catalog.GetByCode("CA");

        actual.ShouldBe(expected);
    }

    [Fact]
    public void GetByCodeReturnsNullWhenCodeDoesNotMatch() {
        var catalog = CountryCatalog.FromJson(
            """[{"Name":"Canada","Code":"CA","Capital":"Ottawa","Region":"Americas","Population":38246108,"Currency":"CAD","CallingCode":"+1"}]""");

        catalog.GetByCode("US").ShouldBeNull();
        catalog.GetByCode("ca").ShouldBeNull();
    }
}

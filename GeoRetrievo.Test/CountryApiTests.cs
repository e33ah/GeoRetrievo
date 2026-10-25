namespace GeoRetrievo.Test;

public class CountryApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>> {
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task HealthEndpointReturnsHealthyStatus() {
        using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HealthResponse>(TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.Status.ShouldBe("healthy");
    }

    [Fact]
    public async Task CountriesEndpointReturnsCountryList() {
        var countries = await client.GetFromJsonAsync<Country[]>(
            "/countries",
            TestContext.Current.CancellationToken);

        countries.ShouldNotBeNull();
        countries!.ShouldContain(country => country.Code == "CA" && country.Name == "Canada");
    }

    [Fact]
    public async Task CountryByCodeEndpointReturnsMatchingCountry() {
        var expected = new Country("Canada", "CA", "Ottawa", "North America", 35540419, "CAD", "CA$", ["en", "fr"], "1", "https://flagcdn.com/w320/ca.png");

        using var response = await client.GetAsync("/countries/CA", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var country = await response.Content.ReadFromJsonAsync<Country>(TestContext.Current.CancellationToken);

        country.ShouldNotBeNull();
        country!.Languages.ShouldBe(expected.Languages);
        (country with { Languages = expected.Languages }).ShouldBe(expected);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("123")]
    [InlineData("CAN")]
    [InlineData("ca")]
    [InlineData("XX")]
    public async Task CountryByCodeEndpointReturnsNotFoundForInvalidOrMissingCode(string code) {
        using var response = await client.GetAsync($"/countries/{code}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        body.ShouldBeEmpty();
    }

    private sealed record HealthResponse(string Status);
}

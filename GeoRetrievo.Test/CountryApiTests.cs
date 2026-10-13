using System.Net;
using System.Net.Http.Json;
using GeoRetrievo.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace GeoRetrievo.Test;

public class CountryApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>> {
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task HealthEndpointReturnsHealthyStatus() {
        using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HealthResponse>(
            TestContext.Current.CancellationToken);
        result?.Status.ShouldBe("healthy");
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
        using var response = await client.GetAsync(
            "/countries/CA",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var country = await response.Content.ReadFromJsonAsync<Country>(
            TestContext.Current.CancellationToken);

        country.ShouldBe(new Country("Canada", "CA", "Ottawa", "Americas", 38246108, "CAD", "+1"));
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("123")]
    [InlineData("CAN")]
    [InlineData("ca")]
    [InlineData("NZ")]
    public async Task CountryByCodeEndpointReturnsNotFoundForInvalidOrMissingCode(string code) {
        using var response = await client.GetAsync(
            $"/countries/{code}",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.ShouldBeEmpty();
    }

    private sealed record HealthResponse(string Status);
}

using System.Text.Json;
using GeoRetrievo.Api.Models;

namespace GeoRetrievo.Api.Services;

public sealed class CountryCatalog {
    private static readonly JsonSerializerOptions serializerOptions = new() {
        PropertyNameCaseInsensitive = true
    };

    private readonly List<Country> countries;

    private CountryCatalog(List<Country> countries) {
        this.countries = countries;
    }

    public static CountryCatalog FromJson(string json) {
        var countries = JsonSerializer.Deserialize<List<Country>>(json, serializerOptions)
            ?? throw new InvalidDataException("Country data must contain a JSON array, not null.");

        return new CountryCatalog(countries);
    }

    public IReadOnlyList<Country> GetAll() => countries;

    public Country? GetByCode(string code) =>
        countries.FirstOrDefault(country => country.Code == code);
}

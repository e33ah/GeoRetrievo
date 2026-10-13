namespace GeoRetrievo.Api.Models;


public record Country(
    string Name,
    string Code,
    string Capital,
    string Region,
    long Population,
    string CurrencyCode,
    string CurrencySymbol,
    IEnumerable<string> Languages,
    string CallingCode,
    string Flag
);


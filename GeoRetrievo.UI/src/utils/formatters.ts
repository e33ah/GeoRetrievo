const languageNames = new Intl.DisplayNames(["en"], { type: "language" });
const currencyNames = new Intl.DisplayNames(["en"], { type: "currency" });

export function getFlagEmoji(countryCode?: string): string {
  if (!countryCode || countryCode.length !== 2) return "🌐";
  try {
    const codePoints = countryCode
      .toUpperCase()
      .split("")
      .map((char) => 127397 + char.charCodeAt(0));
    return String.fromCodePoint(...codePoints);
  } catch {
    return "🌐";
  }
}

export function formatLanguageName(languageCode: string): string {
  try {
    return languageNames.of(languageCode) ?? languageCode;
  } catch (error) {
    if (error instanceof RangeError) return languageCode;
    throw error;
  }
}

export function formatCurrencyName(currencyCode: string): string {
  try {
    return currencyNames.of(currencyCode) ?? currencyCode;
  } catch (error) {
    if (error instanceof RangeError) return currencyCode;
    throw error;
  }
}

export function formatCompactNumber(num: number): string {
  const val = Number.isFinite(num) ? num : 0;
  if (val >= 1_000_000_000) {
    return `${(val / 1_000_000_000).toFixed(2)}B`;
  }
  if (val >= 1_000_000) {
    return `${(val / 1_000_000).toFixed(1)}M`;
  }
  return val.toLocaleString();
}

export function getRegionClass(region?: string): string {
  switch (region?.toLowerCase()) {
    case "africa":
      return "badge-region-africa";
    case "americas":
    case "north america":
    case "south america":
      return "badge-region-americas";
    case "asia":
      return "badge-region-asia";
    case "europe":
      return "badge-region-europe";
    case "oceania":
      return "badge-region-oceania";
    default:
      return "badge-region-default";
  }
}

import type { Country } from "../types/Country";

export const API_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5294";

export function parseCountry(data: unknown): Country | null {
  if (!data || typeof data !== "object") return null;
  const record = data as Record<string, unknown>;

  const name = typeof record.name === "string" ? record.name.trim() : "";
  const code = typeof record.code === "string" ? record.code.trim().toUpperCase() : "";

  // Require name and code for valid country entries
  if (!name || !code) return null;

  const capital = typeof record.capital === "string" ? record.capital : "";
  const region = typeof record.region === "string" ? record.region : "Unknown";
  const rawPop =
    typeof record.population === "number"
      ? record.population
      : Number(record.population);
  const population = Number.isFinite(rawPop) ? Math.max(0, rawPop) : 0;
  const currency = typeof record.currency === "string" ? record.currency : "";
  const callingCode = typeof record.callingCode === "string" ? record.callingCode : "";
  const flag = typeof record.flag === "string" ? record.flag : "";

  return {
    name,
    code,
    capital,
    region,
    population,
    currency,
    callingCode,
    flag
  };
}

export function parseCountriesResponse(data: unknown): Country[] {
  if (!Array.isArray(data)) {
    throw new Error("Invalid API response format: expected an array of countries");
  }

  const validCountries: Country[] = [];
  for (const item of data) {
    const parsed = parseCountry(item);
    if (parsed) {
      validCountries.push(parsed);
    }
  }

  return validCountries;
}

export async function fetchCountries(signal?: AbortSignal): Promise<Country[]> {
  const response = await fetch(`${API_URL}/countries`, {
    signal,
  });

  if (!response.ok) {
    throw new Error(`API returned HTTP ${response.status}`);
  }

  const data: unknown = await response.json();
  return parseCountriesResponse(data);
}

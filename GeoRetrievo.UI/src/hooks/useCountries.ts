import { useEffect, useState, useCallback } from "react";
import type { Country } from "../types/Country";
import { API_URL, fetchCountries } from "../services/countriesApi";

export interface UseCountriesResult {
  countries: Country[];
  loading: boolean;
  error: string;
  retry: () => void;
}

export function useCountries(): UseCountriesResult {
  const [countries, setCountries] = useState<Country[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  const retry = useCallback(() => {
    setReloadKey((prev) => prev + 1);
  }, []);

  useEffect(() => {
    const controller = new AbortController();

    async function loadCountries() {
      try {
        setLoading(true);
        setError("");

        const data = await fetchCountries(controller.signal);
        setCountries(data);
      } catch (err) {
        if (err instanceof Error && err.name === "AbortError") {
          return;
        }

        setError(
          "Could not connect to the GeoRetrievo API. Please ensure your .NET service is running at " +
            API_URL
        );
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    loadCountries();

    return () => controller.abort();
  }, [reloadKey]);

  return { countries, loading, error, retry };
}

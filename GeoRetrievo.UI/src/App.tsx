import { useMemo, useRef, useState, useCallback } from "react";
import { useCountries } from "./hooks/useCountries";
import { useSearchShortcut } from "./hooks/useSearchShortcut";
import { HeroHeader } from "./components/HeroHeader";
import { ControlPanel, type SortOption } from "./components/ControlPanel";
import { CountryCard } from "./components/CountryCard";
import { CountryDetailsModal } from "./components/CountryDetailsModal";
import { LoadingSkeleton } from "./components/LoadingSkeleton";
import { ErrorState } from "./components/ErrorState";
import { EmptyState } from "./components/EmptyState";
import type { Country } from "./types/Country";
import "./App.css";

function App() {
  const { countries, loading, error, retry } = useCountries();
  const [search, setSearch] = useState("");
  const [region, setRegion] = useState("All");
  const [sortBy, setSortBy] = useState<SortOption>("name-asc");
  const [selectedCountry, setSelectedCountry] = useState<Country | null>(null);
  const closeCountryDetails = useCallback(() => setSelectedCountry(null), []);

  const searchInputRef = useRef<HTMLInputElement>(null);

  const handleClearSearch = useCallback(() => {
    setSearch("");
  }, []);

  useSearchShortcut(searchInputRef, handleClearSearch);

  const regions = useMemo(
    () => ["All", ...new Set(countries.map((c) => c.region).filter(Boolean).sort())],
    [countries]
  );

  const totalPopulation = useMemo(
    () => countries.reduce((sum, c) => sum + (c.population || 0), 0),
    [countries]
  );

  const filteredCountries = useMemo(() => {
    const query = search.trim().toLowerCase();

    const matches = countries.filter((country) => {
      const matchesSearch = [
        country.name,
        country.code,
        country.capital,
        country.region,
        country.currency,
        country.callingCode,
      ].some((value) => (value ?? "").toLowerCase().includes(query));

      const matchesRegion =
        region === "All" || country.region === region;

      return matchesSearch && matchesRegion;
    });

    return matches.sort((a, b) => {
      switch (sortBy) {
        case "name-asc":
          return a.name.localeCompare(b.name);
        case "name-desc":
          return b.name.localeCompare(a.name);
        case "pop-desc":
          return b.population - a.population;
        case "pop-asc":
          return a.population - b.population;
        default:
          return 0;
      }
    });
  }, [countries, search, region, sortBy]);

  const handleResetFilters = useCallback(() => {
    setSearch("");
    setRegion("All");
    setSortBy("name-asc");
    searchInputRef.current?.focus();
  }, []);

  const handleBackToTop = useCallback(() => {
    window.scrollTo({
      top: 0,
      behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches
        ? "instant"
        : "smooth",
    });
  }, []);

  const isFiltered = search.trim() !== "" || region !== "All" || sortBy !== "name-asc";

  return (
    <main className="container">
      {/* Hero Header */}
      <HeroHeader
        totalCountries={countries.length}
        totalRegions={regions.length - 1}
        totalPopulation={totalPopulation}
        showStats={!loading && !error && countries.length > 0}
      />

      {/* Control Panel */}
      <ControlPanel
        search={search}
        onSearchChange={setSearch}
        onClearSearch={handleClearSearch}
        searchInputRef={searchInputRef}
        sortBy={sortBy}
        onSortChange={setSortBy}
        regions={regions}
        selectedRegion={region}
        onRegionChange={setRegion}
      />

      {/* Result Meta Bar */}
      {!loading && !error && (
        <div className="results-header">
          <p className="result-count" aria-live="polite" aria-atomic="true">
            Showing <strong>{filteredCountries.length}</strong> of{" "}
            <strong>{countries.length}</strong> countries
          </p>

          {isFiltered && (
            <button
              type="button"
              className="reset-filters-btn"
              onClick={handleResetFilters}
            >
              Reset filters
            </button>
          )}
        </div>
      )}

      {/* Loading Skeleton */}
      {loading && <LoadingSkeleton />}

      {/* Error State */}
      {error && <ErrorState error={error} onRetry={retry} />}

      {/* Empty Search State */}
      {!loading && !error && filteredCountries.length === 0 && (
        <EmptyState
          search={search}
          region={region}
          onResetFilters={handleResetFilters}
        />
      )}

      {/* Countries Grid */}
      {!loading && !error && filteredCountries.length > 0 && (
        <section className="grid" aria-label="Countries list">
          {filteredCountries.map((country) => (
            <CountryCard
              key={country.code}
              country={country}
              onOpenDetails={setSelectedCountry}
            />
          ))}
        </section>
      )}

      {selectedCountry && (
        <CountryDetailsModal
          country={selectedCountry}
          onClose={closeCountryDetails}
        />
      )}

      {/* Footer */}
      <footer className="footer">
        <button type="button" className="back-to-top-btn" onClick={handleBackToTop}>
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="16"
            height="16"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden="true"
          >
            <path d="m18 15-6-6-6 6" />
          </svg>
          Back to top
        </button>
        <p>&copy; 2026 GeoRetrievo</p>
      </footer>
    </main>
  );
}

export default App;
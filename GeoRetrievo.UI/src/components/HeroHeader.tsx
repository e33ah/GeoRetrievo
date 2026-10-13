interface HeroHeaderProps {
  totalCountries: number;
  totalRegions: number;
  totalPopulation: number;
  showStats: boolean;
}

export function HeroHeader({
  totalCountries,
  totalRegions,
  showStats,
}: HeroHeaderProps) {
  return (
    <header className="hero">
      <div className="hero-badge">
        <span className="badge-pulse-dot" aria-hidden="true" />
        <span className="badge-text">GLOBAL EXPLORATION PLATFORM</span>
      </div>
      <h1 className="hero-title">GeoRetrievo</h1>
      <p className="hero-subtitle">
        Discover countries, capitals, populations, currencies and regional demographics in an intuitive modern interface.
      </p>

      {showStats && totalCountries > 0 && (
        <div className="stats-row" aria-label="Global statistics summary">
          <div className="stat-card">
            <span className="stat-value">{totalCountries}</span>
            <span className="stat-label">Countries Cataloged</span>
          </div>
          <div className="stat-divider" aria-hidden="true" />
          <div className="stat-card">
            <span className="stat-value">{totalRegions}</span>
            <span className="stat-label">Global Regions</span>
          </div>
        </div>
      )}
    </header>
  );
}

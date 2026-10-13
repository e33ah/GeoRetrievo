import type { RefObject } from "react";

export type SortOption = "name-asc" | "name-desc" | "pop-desc" | "pop-asc";

interface ControlPanelProps {
  search: string;
  onSearchChange: (value: string) => void;
  onClearSearch: () => void;
  searchInputRef: RefObject<HTMLInputElement | null>;
  sortBy: SortOption;
  onSortChange: (value: SortOption) => void;
  regions: string[];
  selectedRegion: string;
  onRegionChange: (region: string) => void;
}

export function ControlPanel({
  search,
  onSearchChange,
  onClearSearch,
  searchInputRef,
  sortBy,
  onSortChange,
  regions,
  selectedRegion,
  onRegionChange,
}: ControlPanelProps) {
  return (
    <section className="control-panel" aria-label="Country filters and controls">
      <div className="control-bar-main">
        {/* Search Box */}
        <div className="search-wrapper">
          <svg
            className="search-icon"
            xmlns="http://www.w3.org/2000/svg"
            width="18"
            height="18"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden="true"
          >
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input
            ref={searchInputRef}
            type="search"
            placeholder="Search by name, capital, code or currency..."
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
            aria-label="Search countries"
            className="search-input"
          />
          {search ? (
            <button
              type="button"
              className="clear-search-btn"
              onClick={onClearSearch}
              aria-label="Clear search query"
            >
              ✕
            </button>
          ) : (
            <kbd className="search-shortcut" title="Press / to focus">
              /
            </kbd>
          )}
        </div>

        {/* Sort Selector */}
        <div className="sort-wrapper">
          <label htmlFor="sort-select" className="sort-label">
            Sort:
          </label>
          <select
            id="sort-select"
            value={sortBy}
            onChange={(e) => onSortChange(e.target.value as SortOption)}
            aria-label="Sort countries by"
            className="sort-select"
          >
            <option value="name-asc">Name (A → Z)</option>
            <option value="name-desc">Name (Z → A)</option>
            <option value="pop-desc">Population (High → Low)</option>
            <option value="pop-asc">Population (Low → High)</option>
          </select>
        </div>
      </div>

      {/* Region Filter Chips */}
      <div className="region-chips-wrapper" role="group" aria-label="Region filters">
        <span className="region-filter-label">Regions:</span>
        <div className="region-chips-list">
          {regions.map((item) => {
            const isActive = selectedRegion === item;
            return (
              <button
                key={item}
                type="button"
                onClick={() => onRegionChange(item)}
                className={`region-chip ${isActive ? "active" : ""}`}
                aria-pressed={isActive}
              >
                {item === "All" ? "All Regions" : item}
              </button>
            );
          })}
        </div>
      </div>
    </section>
  );
}

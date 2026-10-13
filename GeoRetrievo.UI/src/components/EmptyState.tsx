interface EmptyStateProps {
  search: string;
  region: string;
  onResetFilters: () => void;
}

export function EmptyState({ search, region, onResetFilters }: EmptyStateProps) {
  return (
    <div className="empty-state">
      <div className="empty-icon-box" aria-hidden="true">
        <svg
          width="36"
          height="36"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <circle cx="12" cy="12" r="10" />
          <polygon points="16.24 7.76 14.12 14.12 7.76 16.24 9.88 9.88 16.24 7.76" />
        </svg>
      </div>
      <h2 className="empty-title">No matching countries found</h2>
      <p className="empty-description">
        We couldn't find any country matching{" "}
        {search ? <strong>"{search}"</strong> : "the selected criteria"}
        {region !== "All" ? ` in ${region}` : ""}.
      </p>
      <button
        type="button"
        className="empty-action-btn"
        onClick={onResetFilters}
      >
        Clear all filters
      </button>
    </div>
  );
}

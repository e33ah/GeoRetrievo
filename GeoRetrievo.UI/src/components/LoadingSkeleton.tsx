interface LoadingSkeletonProps {
  count?: number;
}

export function LoadingSkeleton({ count = 8 }: LoadingSkeletonProps) {
  return (
    <section className="grid" aria-label="Loading countries">
      {Array.from({ length: count }).map((_, index) => (
        <div key={index} className="country-card skeleton-card" aria-hidden="true">
          <div className="skeleton-header">
            <div className="skeleton-flag" />
            <div className="skeleton-title" />
            <div className="skeleton-badge" />
          </div>
          <div className="skeleton-region" />
          <div className="skeleton-details">
            <div className="skeleton-row" />
            <div className="skeleton-row" />
            <div className="skeleton-row" />
            <div className="skeleton-row" />
          </div>
        </div>
      ))}
    </section>
  );
}

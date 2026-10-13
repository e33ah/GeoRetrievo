import type { Country } from "../types/Country";
import { getFlagEmoji, getRegionClass } from "../utils/formatters";

interface CountryCardProps {
  country: Country;
}

export function CountryCard({ country }: CountryCardProps) {
  const populationText =
    typeof country.population === "number" && Number.isFinite(country.population)
      ? country.population.toLocaleString()
      : "0";

  const flagSrc = country.flag || (country.code ? `https://flagcdn.com/w320/${country.code.toLowerCase()}.png` : "");
  return (
    <article className="country-card">
      <div className="card-heading">
        <div className="country-identity">

          <span className="country-flag" aria-hidden="true">
            {flagSrc ? (
              <img
                src={flagSrc}
                alt={`${country.name} flag`}
                className="country-flag-img"
                loading="lazy"
              />
            ) : (
              getFlagEmoji(country.code)
            )}
          </span>
          <div>
            <h2 className="country-name">{country.name}</h2>
            <span className={`country-region-badge ${getRegionClass(country.region)}`}>
              {country.region || "Unknown"}
            </span>
          </div>
        </div>
        <span className="country-code" title={`ISO Alpha-2: ${country.code}`}>
          {country.code}
        </span>
      </div>

      <dl className="card-specs">
        <div className="spec-item">
          <dt className="spec-label">
            <svg
              className="spec-icon"
              xmlns="http://www.w3.org/2000/svg"
              width="15"
              height="15"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
            >
              <path d="M3 21h18M5 21V7l8-4v18M19 21V11l-6-3" />
              <path d="M9 9h1M9 13h1M9 17h1" />
            </svg>
            Capital
          </dt>
          <dd className="spec-value">{country.capital || "Unknown"}</dd>
        </div>

        <div className="spec-item">
          <dt className="spec-label">
            <svg
              className="spec-icon"
              xmlns="http://www.w3.org/2000/svg"
              width="15"
              height="15"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
            >
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
              <circle cx="9" cy="7" r="4" />
              <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
              <path d="M16 3.13a4 4 0 0 1 0 7.75" />
            </svg>
            Population
          </dt>
          <dd className="spec-value">{populationText}</dd>
        </div>

        <div className="spec-item">
          <dt className="spec-label">
            <svg
              className="spec-icon"
              xmlns="http://www.w3.org/2000/svg"
              width="15"
              height="15"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
            >
              <rect x="2" y="5" width="20" height="14" rx="2" />
              <line x1="2" y1="10" x2="22" y2="10" />
            </svg>
            Currency
          </dt>
          <dd className="spec-value">{country.currency || "Unknown"}</dd>
        </div>

        <div className="spec-item">
          <dt className="spec-label">
            <svg
              className="spec-icon"
              xmlns="http://www.w3.org/2000/svg"
              width="15"
              height="15"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              aria-hidden="true"
            >
              <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" />
            </svg>
            Calling code
          </dt>
          <dd className="spec-value">+{country.callingCode || "Unknown"}</dd>
        </div>
      </dl>
    </article>
  );
}

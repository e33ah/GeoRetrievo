import { useEffect, useRef, type KeyboardEvent } from "react";
import type { Country } from "../types/Country";
import {
  formatCurrencyName,
  formatLanguageName,
  getFlagEmoji,
  getRegionClass,
} from "../utils/formatters";

interface CountryDetailsModalProps {
  country: Country;
  onClose: () => void;
}

export function CountryDetailsModal({ country, onClose }: CountryDetailsModalProps) {
  const closeButtonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    const previouslyFocused = document.activeElement as HTMLElement | null;
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    closeButtonRef.current?.focus();

    return () => {
      document.body.style.overflow = previousOverflow;
      previouslyFocused?.focus();
    };
  }, [onClose]);

  const handleKeyDown = (event: KeyboardEvent<HTMLElement>) => {
    if (event.key === "Escape") {
      onClose();
      return;
    }

    if (event.key !== "Tab") return;

    const focusableElements = event.currentTarget.querySelectorAll<HTMLElement>(
      'button:not([disabled]), a[href], input:not([disabled]), [tabindex]:not([tabindex="-1"])',
    );
    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    if (event.shiftKey && document.activeElement === firstElement) {
      event.preventDefault();
      lastElement?.focus();
    } else if (!event.shiftKey && document.activeElement === lastElement) {
      event.preventDefault();
      firstElement?.focus();
    }
  };

  return (
    <div
      className="country-modal-backdrop"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
    >
      <section
        className="country-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="country-modal-title"
        onKeyDown={handleKeyDown}
      >
        <button
          ref={closeButtonRef}
          type="button"
          className="country-modal-close"
          onClick={onClose}
          aria-label="Close country details"
        >
          X
        </button>

        <header className="country-modal-header">
          {country.flag ? (
            <img
              src={country.flag}
              alt={`${country.name} flag`}
              className="country-modal-flag"
            />
          ) : (
            <span className="country-modal-flag-emoji" aria-hidden="true">
              {getFlagEmoji(country.code)}
            </span>
          )}
          <div>
            <p className="country-modal-kicker">Country profile</p>
            <h2 id="country-modal-title">{country.name}</h2>
            <span className={`country-region-badge ${getRegionClass(country.region)}`}>
              {country.region || "Unknown"}
            </span>
          </div>
        </header>

        <dl className="country-modal-details">
          <div>
            <dt>Capital</dt>
            <dd>{country.capital || "Unknown"}</dd>
          </div>
          <div>
            <dt>Population</dt>
            <dd>{country.population.toLocaleString()}</dd>
          </div>
          <div>
            <dt>Country code</dt>
            <dd>{country.code}</dd>
          </div>
          <div>
            <dt>Currency</dt>
            <dd>
              {country.currency
                ? `${country.currencySymbol ? `${country.currencySymbol} ` : ""}${formatCurrencyName(country.currency)} (${country.currency})`
                : "Unknown"}
            </dd>
          </div>
          <div>
            <dt>Calling code</dt>
            <dd>{country.callingCode ? `+${country.callingCode}` : "Unknown"}</dd>
          </div>
          <div>
            <dt>Languages</dt>
            <dd>
              {country.languages.length
                ? country.languages.map(formatLanguageName).join(", ")
                : "Unknown"}
            </dd>
          </div>
        </dl>
      </section>
    </div>
  );
}

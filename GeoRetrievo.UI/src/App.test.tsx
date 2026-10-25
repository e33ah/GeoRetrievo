import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { useCountries } from "./hooks/useCountries";
import type { Country } from "./types/Country";
import App from "./App";

vi.mock("./hooks/useCountries", () => ({
  useCountries: vi.fn(),
}));

const countries: Country[] = [
  {
    name: "Canada",
    code: "CA",
    capital: "Ottawa",
    region: "North America",
    population: 39000000,
    currency: "CAD",
    currencyCode: "CAD",
    currencySymbol: "CA$",
    languages: ["en", "fr"],
    callingCode: "1",
    flag: "",
  },
  {
    name: "Brazil",
    code: "BR",
    capital: "Brasilia",
    region: "South America",
    population: 203000000,
    currency: "BRL",
    currencyCode: "BRL",
    currencySymbol: "R$",
    languages: ["pt"],
    callingCode: "55",
    flag: "",
  },
  {
    name: "France",
    code: "FR",
    capital: "Paris",
    region: "Europe",
    population: 68000000,
    currency: "EUR",
    currencyCode: "EUR",
    currencySymbol: "€",
    languages: ["fr"],
    callingCode: "33",
    flag: "",
  },
  {
    name: "Australia",
    code: "AU",
    capital: "Canberra",
    region: "Oceania",
    population: 27000000,
    currency: "AUD",
    currencyCode: "AUD",
    currencySymbol: "A$",
    languages: ["en"],
    callingCode: "61",
    flag: "",
  },
];

describe("App", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  beforeEach(() => {
    vi.mocked(useCountries).mockReturnValue({
      countries,
      loading: false,
      error: "",
      retry: vi.fn(),
    });
  });

  it("filters countries by region and search, resets filters, sorts results, and opens details", async () => {
    const user = userEvent.setup();
    render(<App />);

    await user.click(screen.getByRole("button", { name: "South America" }));
    expect(screen.getByRole("button", { name: "View details for Brazil" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "View details for Canada" })).not.toBeInTheDocument();
    expect(
      screen.getByText((_, element) => element?.classList.contains("result-count") ?? false),
    ).toHaveTextContent("Showing 1 of 4 countries");

    await user.type(screen.getByRole("searchbox", { name: "Search countries" }), "brazil");
    expect(screen.getByRole("button", { name: "View details for Brazil" })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Reset filters" }));
    expect(screen.getByRole("searchbox", { name: "Search countries" })).toHaveValue("");

    await user.selectOptions(screen.getByRole("combobox", { name: "Sort countries by" }), "pop-desc");
    const countryCards = screen.getAllByRole("button", { name: /^View details for/ });
    expect(countryCards.map((card) => card.getAttribute("aria-label"))).toEqual([
      "View details for Brazil",
      "View details for France",
      "View details for Canada",
      "View details for Australia",
    ]);

    await user.click(screen.getByRole("button", { name: "View details for Canada" }));
    expect(screen.getByRole("dialog", { name: "Canada" })).toBeInTheDocument();
    expect(within(screen.getByRole("dialog")).getByText("Ottawa")).toBeInTheDocument();
  });

  it("scrolls smoothly to the top when requested", async () => {
    const user = userEvent.setup();
    const scrollTo = vi.fn();
    vi.stubGlobal("scrollTo", scrollTo);
    vi.stubGlobal("matchMedia", vi.fn().mockReturnValue({ matches: false }));
    render(<App />);

    await user.click(screen.getByRole("button", { name: "Back to top" }));

    expect(scrollTo).toHaveBeenCalledWith({ top: 0, behavior: "smooth" });
  });

  it("uses instant scrolling when reduced motion is preferred", async () => {
    const user = userEvent.setup();
    const scrollTo = vi.fn();
    vi.stubGlobal("scrollTo", scrollTo);
    vi.stubGlobal("matchMedia", vi.fn().mockReturnValue({ matches: true }));
    render(<App />);

    await user.click(screen.getByRole("button", { name: "Back to top" }));

    expect(scrollTo).toHaveBeenCalledWith({ top: 0, behavior: "instant" });
  });
});

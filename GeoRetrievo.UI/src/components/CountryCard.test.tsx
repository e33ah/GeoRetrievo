import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { CountryCard } from "./CountryCard";

const country = {
  name: "Canada",
  code: "CA",
  capital: "Ottawa",
  region: "North America",
  population: 35540419,
  currency: "CAD",
  currencyCode: "CAD",
  currencySymbol: "CA$",
  languages: ["English", "French"],
  callingCode: "1",
  flag: "https://flagcdn.com/w320/ca.png",
};

describe("CountryCard", () => {
  it("opens country details when clicked", async () => {
    const user = userEvent.setup();
    const onOpenDetails = vi.fn();
    render(<CountryCard country={country} onOpenDetails={onOpenDetails} />);

    await user.click(screen.getByRole("button", { name: "View details for Canada" }));

    expect(onOpenDetails).toHaveBeenCalledWith(country);
  });

  it("opens country details with the keyboard", async () => {
    const user = userEvent.setup();
    const onOpenDetails = vi.fn();
    render(<CountryCard country={country} onOpenDetails={onOpenDetails} />);
    const card = screen.getByRole("button", { name: "View details for Canada" });

    card.focus();
    await user.keyboard("{Enter}");

    expect(onOpenDetails).toHaveBeenCalledWith(country);
  });

  it("opens country details with Space", async () => {
    const user = userEvent.setup();
    const onOpenDetails = vi.fn();
    render(<CountryCard country={country} onOpenDetails={onOpenDetails} />);

    screen.getByRole("button", { name: "View details for Canada" }).focus();
    await user.keyboard(" ");

    expect(onOpenDetails).toHaveBeenCalledWith(country);
  });

  it("shows the currency name, symbol, and code", () => {
    render(<CountryCard country={country} onOpenDetails={vi.fn()} />);

    expect(screen.getByText("Canadian Dollar (CAD)")).toBeInTheDocument();
  });
});

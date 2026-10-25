import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { describe, expect, it, vi } from "vitest";
import type { Country } from "../types/Country";
import { CountryDetailsModal } from "./CountryDetailsModal";

const country: Country = {
  name: "Canada",
  code: "CA",
  capital: "Ottawa",
  region: "North America",
  population: 35540419,
  currency: "CAD",
  currencyCode: "CAD",
  currencySymbol: "CA$",
  languages: ["en", "fr"],
  callingCode: "1",
  flag: "https://flagcdn.com/w320/ca.png",
};

function StatefulModal() {
  const [open, setOpen] = useState(false);
  return (
    <>
      <button type="button" onClick={() => setOpen(true)}>
        Open details
      </button>
      {open && <CountryDetailsModal country={country} onClose={() => setOpen(false)} />}
    </>
  );
}

describe("CountryDetailsModal", () => {
  it("shows country details in an accessible dialog", () => {
    render(<CountryDetailsModal country={country} onClose={vi.fn()} />);

    expect(screen.getByRole("dialog", { name: "Canada" })).toBeInTheDocument();
    expect(screen.getByText("Ottawa")).toBeInTheDocument();
    expect(screen.getByText("35,540,419")).toBeInTheDocument();
    expect(screen.getByText("CA$ Canadian Dollar (CAD)")).toBeInTheDocument();
    expect(screen.getByText("English, French")).toBeInTheDocument();
  });

  it("closes on Escape and restores focus to the opener", async () => {
    const user = userEvent.setup();
    render(<StatefulModal />);
    const opener = screen.getByRole("button", { name: "Open details" });
    await user.click(opener);

    await user.keyboard("{Escape}");

    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    expect(opener).toHaveFocus();
  });

  it("closes when the backdrop is clicked", async () => {
    const user = userEvent.setup();
    render(<StatefulModal />);

    await user.click(screen.getByRole("button", { name: "Open details" }));
    await user.click(screen.getByRole("dialog").parentElement!);

    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("does not close when its content is clicked", async () => {
    const user = userEvent.setup();
    render(<StatefulModal />);

    await user.click(screen.getByRole("button", { name: "Open details" }));
    await user.click(screen.getByRole("heading", { name: "Canada" }));

    expect(screen.getByRole("dialog", { name: "Canada" })).toBeInTheDocument();
  });

  it("keeps keyboard focus inside the modal", async () => {
    const user = userEvent.setup();
    render(<CountryDetailsModal country={country} onClose={vi.fn()} />);
    const closeButton = screen.getByRole("button", { name: "Close country details" });

    expect(closeButton).toHaveFocus();
    await user.tab();
    expect(closeButton).toHaveFocus();
    await user.tab({ shift: true });
    expect(closeButton).toHaveFocus();
  });

  it("locks page scrolling while open and restores it when closed", async () => {
    const user = userEvent.setup();
    document.body.style.overflow = "auto";
    render(<StatefulModal />);

    await user.click(screen.getByRole("button", { name: "Open details" }));
    expect(document.body.style.overflow).toBe("hidden");

    await user.keyboard("{Escape}");
    expect(document.body.style.overflow).toBe("auto");
  });
});

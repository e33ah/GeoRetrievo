import { describe, expect, it } from "vitest";
import { formatCurrencyName, formatLanguageName } from "./formatters";

describe("formatLanguageName", () => {
  it.each([
    ["en", "English"],
    ["fr", "French"],
    ["es", "Spanish"],
  ])("formats %s as %s", (code, expected) => {
    expect(formatLanguageName(code)).toBe(expected);
  });

  describe("formatCurrencyName", () => {
    it.each([
      ["USD", "US Dollar"],
      ["EUR", "Euro"],
      ["CAD", "Canadian Dollar"],
    ])("formats %s as %s", (code, expected) => {
      expect(formatCurrencyName(code)).toBe(expected);
    });

    it("falls back to the original code when it is invalid", () => {
      expect(formatCurrencyName("not-a-currency")).toBe("not-a-currency");
    });
  });

  it("falls back to the original code when it is invalid", () => {
    expect(formatLanguageName("not-a-language-code")).toBe("not-a-language-code");
  });
});

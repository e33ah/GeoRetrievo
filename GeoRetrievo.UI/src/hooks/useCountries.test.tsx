import { renderHook, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { useCountries } from "./useCountries";

describe("useCountries logging", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it("logs safe diagnostic context while preserving the user-facing error", async () => {
    vi.stubGlobal("fetch", vi.fn().mockRejectedValue(new TypeError("Failed to fetch")));
    const consoleError = vi.spyOn(console, "error").mockImplementation(() => {});
    const consoleDebug = vi.spyOn(console, "debug").mockImplementation(() => {});

    const { result } = renderHook(() => useCountries());

    await waitFor(() => {
      expect(result.current.error).toContain("Could not connect to the GeoRetrievo API");
    });

    expect(consoleError).toHaveBeenCalledWith(
      "[GeoRetrievo]",
      expect.objectContaining({
        level: "error",
        event: "countries.fetch.failed",
        context: { errorName: "TypeError" },
      }),
    );
    expect(consoleError.mock.calls[0][1]).not.toHaveProperty("message");
    expect(consoleDebug).toHaveBeenCalledWith(
      "[GeoRetrievo]",
      expect.objectContaining({
        level: "debug",
        event: "countries.fetch.started",
        context: { attempt: 1 },
      }),
    );
    expect(consoleDebug).toHaveBeenCalledWith(
      "[GeoRetrievo]",
      expect.objectContaining({
        level: "debug",
        event: "countries.fetch.failed",
        context: { attempt: 1, errorName: "TypeError" },
      }),
    );
  });

  it("logs the number of countries loaded on success", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify([{ name: "Japan", code: "JP" }]), { status: 200 }),
      ),
    );
    const consoleDebug = vi.spyOn(console, "debug").mockImplementation(() => {});

    renderHook(() => useCountries());

    await waitFor(() => {
      expect(consoleDebug).toHaveBeenCalledWith(
        "[GeoRetrievo]",
        expect.objectContaining({
          level: "debug",
          event: "countries.fetch.succeeded",
          context: { attempt: 1, countryCount: 1 },
        }),
      );
    });
  });

  it("includes the HTTP status for unsuccessful API responses", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(null, { status: 503 })));
    const consoleError = vi.spyOn(console, "error").mockImplementation(() => {});

    const { result } = renderHook(() => useCountries());

    await waitFor(() => {
      expect(result.current.error).toContain("Could not connect to the GeoRetrievo API");
    });

    expect(consoleError).toHaveBeenCalledWith(
      "[GeoRetrievo]",
      expect.objectContaining({
        event: "countries.fetch.failed",
        context: { errorName: "ApiResponseError", httpStatus: 503 },
      }),
    );
  });
});

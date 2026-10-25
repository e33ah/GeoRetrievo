import { act, renderHook, waitFor } from "@testing-library/react";
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

  it("exposes loading and successfully loaded countries", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify([{ name: "Japan", code: "JP" }]), { status: 200 }),
      ),
    );

    const { result } = renderHook(() => useCountries());

    expect(result.current.loading).toBe(true);
    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(result.current.error).toBe("");
    expect(result.current.countries).toEqual(
      expect.arrayContaining([expect.objectContaining({ name: "Japan", code: "JP" })]),
    );
  });

  it("retries after a failed request and clears the error on success", async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 503 }))
      .mockResolvedValueOnce(
        new Response(JSON.stringify([{ name: "Japan", code: "JP" }]), { status: 200 }),
      );
    vi.stubGlobal("fetch", fetchMock);

    const { result } = renderHook(() => useCountries());
    await waitFor(() => expect(result.current.error).not.toBe(""));

    act(() => result.current.retry());

    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(result.current.error).toBe("");
    expect(result.current.countries).toEqual(
      expect.arrayContaining([expect.objectContaining({ name: "Japan", code: "JP" })]),
    );
  });

  it("aborts an in-flight request when retrying", async () => {
    let resolveFirstResponse: ((response: Response) => void) | undefined;
    const firstRequest = new Promise<Response>((resolve) => {
      resolveFirstResponse = resolve;
    });
    const fetchMock = vi
      .fn()
      .mockReturnValueOnce(firstRequest)
      .mockResolvedValueOnce(
        new Response(JSON.stringify([{ name: "Japan", code: "JP" }]), { status: 200 }),
      );
    vi.stubGlobal("fetch", fetchMock);

    const { result } = renderHook(() => useCountries());
    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));
    const firstSignal = fetchMock.mock.calls[0][1]?.signal as AbortSignal;

    act(() => result.current.retry());

    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(firstSignal.aborted).toBe(true);

    await act(async () => {
      resolveFirstResponse?.(
        new Response(JSON.stringify([{ name: "Stale response", code: "ST" }]), { status: 200 }),
      );
      await firstRequest;
    });

    expect(result.current.countries).toEqual(
      expect.arrayContaining([expect.objectContaining({ name: "Japan", code: "JP" })]),
    );
    expect(result.current.countries).not.toEqual(
      expect.arrayContaining([expect.objectContaining({ name: "Stale response" })]),
    );
  });

  it("ignores an error from an aborted request after a successful retry", async () => {
    let rejectFirstRequest: ((error: Error) => void) | undefined;
    const firstRequest = new Promise<Response>((_, reject) => {
      rejectFirstRequest = reject;
    });
    const fetchMock = vi
      .fn()
      .mockReturnValueOnce(firstRequest)
      .mockResolvedValueOnce(
        new Response(JSON.stringify([{ name: "Japan", code: "JP" }]), { status: 200 }),
      );
    vi.stubGlobal("fetch", fetchMock);
    const consoleError = vi.spyOn(console, "error").mockImplementation(() => {});

    const { result } = renderHook(() => useCountries());
    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1));
    act(() => result.current.retry());
    await waitFor(() => expect(result.current.loading).toBe(false));

    await act(async () => {
      rejectFirstRequest?.(new TypeError("late network failure"));
      await firstRequest.catch(() => undefined);
    });

    expect(result.current.error).toBe("");
    expect(result.current.countries).toEqual(
      expect.arrayContaining([expect.objectContaining({ name: "Japan", code: "JP" })]),
    );
    expect(consoleError).not.toHaveBeenCalled();
  });
});

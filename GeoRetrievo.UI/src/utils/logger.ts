export type LogContext = Readonly<Record<string, string | number | boolean>>;

export function logDebug(event: string, context: LogContext = {}): void {
  if (!import.meta.env.DEV) return;

  console.debug("[GeoRetrievo]", {
    timestamp: new Date().toISOString(),
    level: "debug",
    event,
    context,
  });
}

export function logError(event: string, context: LogContext = {}): void {
  console.error("[GeoRetrievo]", {
    timestamp: new Date().toISOString(),
    level: "error",
    event,
    context,
  });
}

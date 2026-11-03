/** Reading values out of the tools' own logs, whose shapes vary between versions. Nothing here trusts a field's type. */

export type LogRecord = Readonly<Record<string, unknown>>;

export function record(value: unknown): LogRecord | null {
  return value !== null && typeof value === "object" && !Array.isArray(value) ? Object.fromEntries(Object.entries(value)) : null;
}

export const list = (value: unknown): readonly unknown[] => (Array.isArray(value) ? value : []);
export const count = (value: unknown): number => (typeof value === "number" && Number.isFinite(value) ? value : 0);
export const string = (value: unknown): string | null => (typeof value === "string" ? value : null);

/** Text from a string or a list of text parts, such as message content. */
export function text(value: unknown): string {
  if (typeof value === "string") return value;
  return list(value).map((part) => (typeof part === "string" ? part : text(record(part)?.["text"] ?? ""))).join("");
}

/** One record per line. Unparsable lines are skipped, since a stopped tool can leave a partial line. */
export function logLines(content: string): LogRecord[] {
  return content.split("\n").flatMap((line) => {
    if (line.trim() === "") return [];
    try {
      const parsed = record(JSON.parse(line));
      return parsed === null ? [] : [parsed];
    } catch { return []; }
  });
}

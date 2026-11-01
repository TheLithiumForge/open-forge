export function isJsonObject(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

export function isNonnegativeSafeInteger(value: unknown): value is number {
  return typeof value === 'number' && Number.isSafeInteger(value) && value >= 0;
}

export function hasExactKeys(value: Record<string, unknown>, keys: readonly string[]): boolean {
  return (
    Object.keys(value).length === keys.length && keys.every((key) => Object.hasOwn(value, key))
  );
}

export function readJson(text: string, maxBytes: number): unknown {
  if (new TextEncoder().encode(text).byteLength > maxBytes) return null;
  try {
    const value: unknown = JSON.parse(text);
    return value;
  } catch {
    return null;
  }
}

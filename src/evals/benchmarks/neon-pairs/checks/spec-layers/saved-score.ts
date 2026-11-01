import { object } from "./requirements.ts";

export function replaceSavedScore(value: unknown, from: number, to: number): { readonly value: unknown; readonly replacements: number } {
  if (value === from) return { value: to, replacements: 1 };
  if (value === String(from)) return { value: String(to), replacements: 1 };
  if (!Array.isArray(value) && !object(value)) return { value, replacements: 0 };
  let replacements = 0;
  const changed = (item: unknown): unknown => {
    const result = replaceSavedScore(item, from, to);
    replacements += result.replacements;
    return result.value;
  };
  const result = Array.isArray(value) ? value.map(changed) : Object.fromEntries(Object.entries(value).map(([key, item]) => [key, changed(item)]));
  return { value: result, replacements };
}

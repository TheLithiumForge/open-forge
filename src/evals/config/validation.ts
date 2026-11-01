import { Tool } from "./choices.ts";
import type { ModelChoice } from "./config.types.ts";

export function object(value: unknown): Record<string, unknown> {
  if (typeof value !== "object" || value === null || Array.isArray(value)) throw new Error("Expected a configuration object.");
  return Object.fromEntries(Object.entries(value));
}
export function text(value: unknown): string {
  if (typeof value !== "string" || !value.trim()) throw new Error("Expected a nonempty string.");
  return value;
}
export function strings(value: unknown): string[] {
  if (!Array.isArray(value) || value.length === 0) throw new Error("Expected a nonempty list.");
  return value.map(text);
}
export function positive(value: unknown): number {
  if (typeof value !== "number" || !Number.isFinite(value) || value <= 0) throw new Error("Expected a positive number.");
  return value;
}
export function integer(value: unknown): number {
  const result = positive(value);
  if (!Number.isSafeInteger(result)) throw new Error("Expected a positive integer.");
  return result;
}
export function modelChoice(value: unknown): ModelChoice {
  const choice = object(value);
  const tool = choice["tool"];
  if (tool !== Tool.claude && tool !== Tool.codex && tool !== Tool.mock) throw new Error("Unknown model provider.");
  return { tool, model: choice["model"] === null ? null : text(choice["model"]), effort: choice["effort"] === null ? null : text(choice["effort"]) };
}

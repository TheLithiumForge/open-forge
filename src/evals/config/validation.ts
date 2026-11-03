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
  const optionalText = (item: unknown): string | null => item === null ? null : text(item);
  const base = { tool, model: optionalText(choice["model"]), effort: optionalText(choice["effort"]) };
  if (choice["team"] === undefined) return base;
  // Codex is the only tool whose subagents the harness can switch and count so far.
  if (tool !== Tool.codex) throw new Error("Only Codex runs a team of subagents for now.");
  const team = object(choice["team"]);
  return { ...base, team: { model: optionalText(team["model"]), effort: optionalText(team["effort"]) } };
}

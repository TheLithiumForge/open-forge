import { Rules } from "./rules.ts";

export interface Box { readonly x: number; readonly y: number; readonly width: number; readonly height: number }
export function near(actual: number | null, expected: number, tolerance = 0.6): boolean { return actual !== null && Number.isFinite(actual) && Math.abs(actual - expected) <= tolerance; }
export function pixels(text: string): number | null {
  const match = /^(-?\d+(?:\.\d+)?)px$/.exec(text.trim());
  return match ? Number(match[1]) : null;
}
export function rgb(text: string): string | null {
  if (/^#[\da-f]{6}$/i.test(text)) return text.toUpperCase();
  const match = /^rgba?\(\s*(\d+)[,\s]+(\d+)[,\s]+(\d+)(?:\s*[,/]\s*([\d.]+))?\s*\)$/i.exec(text.trim());
  if (!match || (match[4] !== undefined && Number(match[4]) !== 1)) return null;
  const values = match.slice(1, 4).map(Number);
  return values.every((value) => value >= 0 && value <= 255) ? `#${values.map((value) => value.toString(16).padStart(2, "0")).join("").toUpperCase()}` : null;
}
export function fullyVisible(card: Box, clip: Box, extra = Rules.outline + Rules.outlineOffset): boolean {
  return card.x - extra >= clip.x - 0.6 && card.y - extra >= clip.y - 0.6 && card.x + card.width + extra <= clip.x + clip.width + 0.6 && card.y + card.height + extra <= clip.y + clip.height + 0.6;
}
export function overlaps(a: Box, b: Box): boolean {
  return Math.min(a.x + a.width, b.x + b.width) > Math.max(a.x, b.x) + 0.6 && Math.min(a.y + a.height, b.y + b.height) > Math.max(a.y, b.y) + 0.6;
}
export function elapsedIn(text: string): number | null {
  const clock = /(?:time|elapsed)\s*:?\s*(?:(\d+):)?(\d+):(\d{2})(?!\d)/i.exec(text);
  if (clock) return (Number(clock[1] ?? 0) * 3600 + Number(clock[2]) * 60 + Number(clock[3])) * 1000;
  const seconds = /(?:time|elapsed)\s*:?\s*(\d+(?:\.\d+)?)\s*(?:s|seconds?)\b/i.exec(text);
  return seconds ? Number(seconds[1]) * 1000 : null;
}
export function cardName(snapshot: string): string | null {
  const match = /^\s*- button ("(?:[^"\\]|\\.)*")/m.exec(snapshot);
  if (!match?.[1]) return null;
  try { const value: unknown = JSON.parse(match[1]); return typeof value === "string" ? value : null; } catch { return null; }
}
export function faceIn(name: string): string | null {
  const shape = Rules.shapes.join("|"), marker = Rules.markers.join("|"), color = Rules.colors.join("|");
  return new RegExp(`\\b(?:${color}) (?:solid|outline) (?:${shape}), marker (?:${marker})(?=,|\\s+(?:revealed|matched|face)|$)`, "i").exec(name)?.[0].toLowerCase() ?? null;
}
export function transitionMs(duration: string): number[] | null {
  const values = duration.split(",").map((item) => /^(\d+(?:\.\d+)?)(ms|s)$/.exec(item.trim()));
  if (values.some((item) => !item)) return null;
  return values.map((item) => Number(item?.[1]) * (item?.[2] === "s" ? 1000 : 1));
}

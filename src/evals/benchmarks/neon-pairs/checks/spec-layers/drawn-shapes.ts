import { near, rgb } from "./view-values.ts";

export interface Point { readonly x: number; readonly y: number }
export interface Contour { readonly points: readonly Point[]; readonly color: string }
const Tolerance = 0.13;
const NeutralSpread = 45;

function unit(points: readonly Point[]): Point[] {
  if (points.length < 3 || points.some((point) => !Number.isFinite(point.x) || !Number.isFinite(point.y))) return [];
  const left = Math.min(...points.map((point) => point.x)), top = Math.min(...points.map((point) => point.y));
  const width = Math.max(...points.map((point) => point.x)) - left, height = Math.max(...points.map((point) => point.y)) - top;
  return width > 0 && height > 0 ? points.map((point) => ({ x: (point.x - left) / width, y: (point.y - top) / height })) : [];
}
export function diamond(points: readonly Point[]): boolean {
  const normalized = unit(points);
  return normalized.length >= 4 && normalized.every((point) => Math.abs(Math.abs(point.x - 0.5) + Math.abs(point.y - 0.5) - 0.5) <= Tolerance)
    && [[0.5, 0], [1, 0.5], [0.5, 1], [0, 0.5]].every(([x = 0, y = 0]) => normalized.some((point) => Math.hypot(point.x - x, point.y - y) <= Tolerance));
}
export function checkMark(points: readonly Point[]): boolean {
  let normalized = unit(points);
  if (normalized.length < 3) return false;
  if ((normalized[0]?.x ?? 0) > (normalized.at(-1)?.x ?? 0)) normalized = normalized.toReversed();
  const start = normalized[0], end = normalized.at(-1);
  const bottom = normalized.reduce((a, b) => a.y > b.y ? a : b);
  if (!start || !end || start.x > Tolerance || end.x < 1 - Tolerance || bottom.x < 0.2 || bottom.x > 0.6 || start.y < 0.2 || start.y > 0.8 || end.y > Tolerance) return false;
  return normalized.every((point) => {
    const expected = point.x <= bottom.x ? start.y + (1 - start.y) * point.x / bottom.x : 1 - (point.x - bottom.x) / (1 - bottom.x);
    return Math.abs(point.y - expected) <= Tolerance;
  });
}

export function sameBacks(cards: readonly (readonly Contour[])[]): boolean {
  const profiles = cards.map((contours) => {
    const contour = contours.find((entry) => diamond(entry.points));
    if (!contour) return null;
    const color = rgb(contour.color);
    if (!color) return null;
    const channels = [1, 3, 5].map((offset) => parseInt(color.slice(offset, offset + 2), 16));
    if (Math.max(...channels) - Math.min(...channels) > NeutralSpread) return null;
    return { color, width: Math.max(...contour.points.map((point) => point.x)) - Math.min(...contour.points.map((point) => point.x)), height: Math.max(...contour.points.map((point) => point.y)) - Math.min(...contour.points.map((point) => point.y)) };
  });
  const first = profiles[0];
  return first !== undefined && first !== null && profiles.every((profile, index) => profile !== null && profile.color === first.color && near(profile.width, first.width) && near(profile.height, first.height) && cards[index]?.length === cards[0]?.length);
}

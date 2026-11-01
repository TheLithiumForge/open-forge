export const Rules = {
  sizes: [4, 6, 8, 10, 12, 14, 16], mismatchMs: 800, seedMaximum: 2147483646, modulus: 2147483647,
  multiplier: 16807, formatVersion: 1, boardPrefix: "np1", snapshotBytes: 65536, replayBytes: 1048576, actionsMaximum: 10000,
  elapsedMaximumMs: 359999999, movesMaximum: 999999,
  palette: ["#63E7FF", "#FF71D6", "#8BEA7D", "#FFE082", "#FFB064", "#B49AFF", "#FF8296", "#D9F3FF"],
  colors: ["cyan", "magenta", "green", "yellow", "orange", "violet", "rose", "ice"],
  shapes: ["circle", "square", "triangle", "diamond", "star", "hexagon", "plus", "chevron"],
  markers: ["top", "top-right", "right", "bottom-right", "bottom", "bottom-left", "left", "top-left"],
  spacing: [0, 4, 6, 8, 12, 16, 24, 32], outline: 3, outlineOffset: 2, cardMinimum: 44, cardMaximum: 88,
} as const;
export const SelectionKind = { closed: "closed", one: "one", mismatch: "mismatch" } as const;
export type Selection = { readonly kind: typeof SelectionKind.closed }
  | { readonly kind: typeof SelectionKind.one; readonly index: number }
  | { readonly kind: typeof SelectionKind.mismatch; readonly first: number; readonly second: number; readonly remainingMs: number };
export interface Snapshot { readonly version: number; readonly boardCode: string; readonly matched: readonly number[]; readonly selection: Selection; readonly moves: number; readonly elapsedMs: number; readonly started: boolean }
export interface BoardCode { readonly size: number; readonly seed: number }
export interface Replay { readonly version: number; readonly boardCode: string; readonly actions: readonly { readonly atMs: number; readonly index: number }[]; readonly endMs: number }

export function faceName(id: number): string {
  if (!Number.isInteger(id) || id < 0 || id >= 128) throw new Error("Face ID must be between zero and 127.");
  const marker = Math.floor(id / 8) % 8;
  return `${Rules.colors[marker]} ${id < 64 ? "solid" : "outline"} ${Rules.shapes[id % 8]}, marker ${Rules.markers[marker]}`;
}

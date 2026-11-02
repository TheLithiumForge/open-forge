import { PNG } from "pngjs";
import { rememberImage, sameBackImage } from "./turn-image.ts";

/**
 * A card's look, from the middle of the card: which colors it has, which doesn't depend on exact
 * position, and where its light and dark parts are, which shows the shape.
 */
export interface FaceSignature {
  readonly colors: readonly number[];
  readonly shape: readonly number[];
}

export const NoFace: FaceSignature = { colors: [], shape: [] };

const GridSize = 24;
const LevelsPerChannel = 4;
const ColorBins = LevelsPerChannel ** 3;
// Two renders of one face can sit a pixel apart, which moves thin lines into the next cell.
const NeighborReach = 1;
// Card edges carry focus rings, glows and borders that change with state, not with the face.
const EdgeInset = 0.2;

export const FaceThresholds = {
  // Two looks whose color shares differ by more than this, out of 2, have different colors or fills.
  colors: 0.03,
  // A cell whose brightness differs by more than this, out of 255, is part of a different shape.
  changedCell: 40,
  // Two looks with more than this share of cells unlike anything nearby have different shapes.
  shapeShare: 0.07,
  // Two shots this close on average mean the card stopped moving.
  stable: 4,
} as const;

/** A part of a screenshot, in screenshot pixels. */
export interface Region {
  readonly x: number;
  readonly y: number;
  readonly width: number;
  readonly height: number;
}

export function decodePng(png: Buffer): PNG {
  return PNG.sync.read(png);
}

export function signatureOf(png: Buffer): FaceSignature {
  const image = PNG.sync.read(png);
  return regionSignature(image, { x: 0, y: 0, width: image.width, height: image.height });
}

export function regionSignature(image: PNG, whole: Region): FaceSignature {
  const left = Math.max(0, Math.round(whole.x + whole.width * EdgeInset));
  const top = Math.max(0, Math.round(whole.y + whole.height * EdgeInset));
  const width = Math.max(1, Math.min(image.width - left, Math.round(whole.width * (1 - 2 * EdgeInset))));
  const height = Math.max(1, Math.min(image.height - top, Math.round(whole.height * (1 - 2 * EdgeInset))));
  const colors = new Array<number>(ColorBins).fill(0);
  const sums = new Array<number>(GridSize * GridSize).fill(0);
  const counts = new Array<number>(GridSize * GridSize).fill(0);
  for (let y = 0; y < height; y += 1) {
    for (let x = 0; x < width; x += 1) {
      const offset = ((top + y) * image.width + left + x) * 4;
      const [red = 0, green = 0, blue = 0] = [image.data[offset], image.data[offset + 1], image.data[offset + 2]];
      const level = (value: number): number => Math.min(LevelsPerChannel - 1, Math.floor((value * LevelsPerChannel) / 256));
      const bin = (level(red) * LevelsPerChannel + level(green)) * LevelsPerChannel + level(blue);
      colors[bin] = (colors[bin] ?? 0) + 1;
      const cell = Math.floor((y * GridSize) / height) * GridSize + Math.floor((x * GridSize) / width);
      sums[cell] = (sums[cell] ?? 0) + Math.max(red, green, blue);
      counts[cell] = (counts[cell] ?? 0) + 1;
    }
  }
  return rememberImage({
    colors: colors.map((count) => count / (width * height)),
    shape: sums.map((sum, cell) => sum / Math.max(1, counts[cell] ?? 1)),
  }, image, whole);
}

export function colorDistance(a: FaceSignature, b: FaceSignature): number {
  let total = 0;
  for (let bin = 0; bin < ColorBins; bin += 1) total += Math.abs((a.colors[bin] ?? 0) - (b.colors[bin] ?? 0));
  return total;
}

/** The share of A's cells with no close match among B's cells at the same spot or one cell away. */
function unmatchedShare(a: FaceSignature, b: FaceSignature): number {
  let unmatched = 0;
  for (let y = 0; y < GridSize; y += 1) {
    for (let x = 0; x < GridSize; x += 1) {
      const value = a.shape[y * GridSize + x] ?? 0;
      let closest = Number.POSITIVE_INFINITY;
      for (let dy = -NeighborReach; dy <= NeighborReach; dy += 1) {
        for (let dx = -NeighborReach; dx <= NeighborReach; dx += 1) {
          const ny = y + dy;
          const nx = x + dx;
          if (ny < 0 || nx < 0 || ny >= GridSize || nx >= GridSize) continue;
          closest = Math.min(closest, Math.abs(value - (b.shape[ny * GridSize + nx] ?? 0)));
          if (closest <= FaceThresholds.changedCell) break;
        }
        if (closest <= FaceThresholds.changedCell) break;
      }
      if (closest > FaceThresholds.changedCell) unmatched += 1;
    }
  }
  return unmatched / (GridSize * GridSize);
}

/** How much of either shape has nothing like it nearby in the other, so half-pixel offsets don't count. */
export function shapeShare(a: FaceSignature, b: FaceSignature): number {
  return Math.max(unmatchedShare(a, b), unmatchedShare(b, a));
}

/** The average brightness change, without shifting. Used to tell when a card stopped moving. */
export function meanDistance(a: FaceSignature, b: FaceSignature): number {
  const cells = Math.max(a.shape.length, b.shape.length, 1);
  let total = 0;
  for (let cell = 0; cell < cells; cell += 1) total += Math.abs((a.shape[cell] ?? 0) - (b.shape[cell] ?? 0));
  return total / cells;
}

/** How far apart two looks are, where 1 is the edge of "the same face" on both color and shape. */
export function faceDistance(a: FaceSignature, b: FaceSignature): number {
  return Math.max(colorDistance(a, b) / FaceThresholds.colors, shapeShare(a, b) / FaceThresholds.shapeShare);
}

// A face down card can carry a hover or focus tint, so "still its back" allows more than "the same face".
const BackThresholds = { colors: 0.1, shapeShare: 0.07 } as const;
const backStates = new WeakMap<FaceSignature, FaceSignature[]>();

export function hasBackSignature(look: FaceSignature, back: FaceSignature): boolean {
  return colorDistance(look, back) <= BackThresholds.colors && shapeShare(look, back) <= BackThresholds.shapeShare;
}

function matchesBack(look: FaceSignature, back: FaceSignature): boolean {
  return hasBackSignature(look, back) && sameBackImage(look, back) !== false;
}

/** Whether a card shows one of its back images captured before activation. */
export function looksLikeBack(look: FaceSignature, back: FaceSignature): boolean {
  return matchesBack(look, back) || (backStates.get(back) ?? []).some((state) => matchesBack(look, state));
}

export function rememberBackState(back: FaceSignature, state: FaceSignature): void {
  if (!looksLikeBack(state, back)) backStates.set(back, [...(backStates.get(back) ?? []), state]);
}

export function sameFace(a: FaceSignature, b: FaceSignature): boolean {
  return colorDistance(a, b) <= FaceThresholds.colors && shapeShare(a, b) <= FaceThresholds.shapeShare;
}

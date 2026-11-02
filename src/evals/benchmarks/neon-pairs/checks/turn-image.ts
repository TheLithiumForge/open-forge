import { PNG } from "pngjs";
import type { FaceSignature, Region } from "./face-signature.ts";

interface Capture { readonly image: PNG; readonly region: Region }
const captures = new WeakMap<FaceSignature, Capture>();
const Inset = 0.1;
const FaceInset = 0.2;
const ChannelNoise = 8;
const ChangedShare = 0.001;
const ChannelRange = 255;
const PixelReach = 1;

function pixel(capture: Capture, x: number, y: number, channel: number): number {
  return capture.image.data[((Math.round(capture.region.y) + y) * capture.image.width + Math.round(capture.region.x) + x) * 4 + channel] ?? 0;
}

function outsideNeighbors(a: Capture, b: Capture, x: number, y: number, shifts: readonly number[]): boolean {
  // A 3D flip can round a back's edges differently. New strokes still exceed the nearby back colors.
  for (let channel = 0; channel < 3; channel += 1) {
    let low = ChannelRange, high = 0;
    for (let dy = -PixelReach; dy <= PixelReach; dy += 1) {
      for (let dx = -PixelReach; dx <= PixelReach; dx += 1) {
        const value = pixel(b, x + dx, y + dy, channel);
        low = Math.min(low, value);
        high = Math.max(high, value);
      }
    }
    const value = pixel(a, x, y, channel) - (shifts[channel] ?? 0);
    if (value < low - ChannelNoise || value > high + ChannelNoise) return true;
  }
  return false;
}

function medianDifference(values: readonly number[]): number {
  const counts = new Uint32Array(ChannelRange * 2 + 1);
  for (const value of values) counts[value + ChannelRange] = (counts[value + ChannelRange] ?? 0) + 1;
  let total = 0;
  for (const [index, count] of counts.entries()) {
    total += count;
    if (total > Math.floor(values.length / 2)) return index - ChannelRange;
  }
  return 0;
}

export function capturedPng(signature: FaceSignature): Buffer | null {
  const capture = captures.get(signature);
  if (!capture) return null;
  const { image, region } = capture;
  const x = Math.max(0, Math.round(region.x)), y = Math.max(0, Math.round(region.y));
  const crop = new PNG({ width: Math.min(image.width - x, Math.round(region.width)), height: Math.min(image.height - y, Math.round(region.height)) });
  PNG.bitblt(image, crop, x, y, crop.width, crop.height, 0, 0);
  return PNG.sync.write(crop);
}

export function rememberImage(signature: FaceSignature, image: PNG, region: Region): FaceSignature {
  captures.set(signature, { image, region });
  return signature;
}

/** Compares each card with its back before coarse sampling can lose a thin face. */
export function sameBackImage(look: FaceSignature, back: FaceSignature): boolean | null {
  const a = captures.get(look), b = captures.get(back);
  if (!a || !b) return null;
  const width = Math.round(Math.min(a.region.width, b.region.width));
  const height = Math.round(Math.min(a.region.height, b.region.height));
  const differences: number[][] = [[], [], []];
  const low = [ChannelRange, ChannelRange, ChannelRange];
  const high = [-ChannelRange, -ChannelRange, -ChannelRange];
  for (let y = Math.ceil(height * Inset); y < height * (1 - Inset); y += 1) {
    for (let x = Math.ceil(width * Inset); x < width * (1 - Inset); x += 1) {
      for (const [channel, values] of differences.entries()) {
        const value = pixel(a, x, y, channel) - pixel(b, x, y, channel);
        values.push(value);
        low[channel] = Math.min(low[channel] ?? ChannelRange, value);
        high[channel] = Math.max(high[channel] ?? -ChannelRange, value);
      }
    }
  }
  // If every difference fits within the noise allowance, removing the median cannot expose a stroke.
  if (low.every((value, channel) => (high[channel] ?? value) - value <= ChannelNoise)) return true;
  // A uniform focus tint isn't a face. Thin lines still differ after removing that tint.
  const shifts = differences.map(medianDifference);
  const reverseShifts = shifts.map((shift) => -shift);
  const pixels = differences[0]?.length ?? 0;
  const limit = Math.max(3, pixels * ChangedShare);
  const columns = Math.ceil(width * (1 - Inset)) - Math.ceil(width * Inset);
  let changed = 0;
  for (let index = 0; index < pixels; index += 1) {
    const x = Math.ceil(width * Inset) + index % columns;
    const y = Math.ceil(height * Inset) + Math.floor(index / columns);
    const inFace = x >= width * FaceInset && x < width * (1 - FaceInset) && y >= height * FaceInset && y < height * (1 - FaceInset);
    const neighbors = [index - columns, index + columns];
    if (index % columns > 0) neighbors.push(index - 1);
    if (index % columns + 1 < columns) neighbors.push(index + 1);
    // Edge glows change gradually. Keep every pixel change in the face, including soft dim strokes.
    if (differences.some((values, channel) => {
      const value = values[index] ?? 0;
      return Math.abs(value - (shifts[channel] ?? 0)) > ChannelNoise && (inFace || neighbors.some((neighbor) => neighbor >= 0 && neighbor < pixels && Math.abs(value - (values[neighbor] ?? 0)) > ChannelNoise));
    }) && (outsideNeighbors(a, b, x, y, shifts) || outsideNeighbors(b, a, x, y, reverseShifts))) {
      changed += 1;
      if (changed >= limit) return false;
    }
  }
  return true;
}

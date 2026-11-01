import type { PNG } from "pngjs";
import type { FaceSignature, Region } from "./face-signature.ts";

interface Capture { readonly image: PNG; readonly region: Region }
const captures = new WeakMap<FaceSignature, Capture>();
const Inset = 0.1;
const ChannelNoise = 8;
const ChangedShare = 0.001;

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
  for (let y = Math.ceil(height * Inset); y < height * (1 - Inset); y += 1) {
    for (let x = Math.ceil(width * Inset); x < width * (1 - Inset); x += 1) {
      const offset = (capture: Capture): number => ((Math.round(capture.region.y) + y) * capture.image.width + Math.round(capture.region.x) + x) * 4;
      for (const [channel, values] of differences.entries()) values.push((a.image.data[offset(a) + channel] ?? 0) - (b.image.data[offset(b) + channel] ?? 0));
    }
  }
  // A uniform focus tint isn't a face. Thin lines still differ after removing that tint.
  const shifts = differences.map((values) => [...values].sort((x, y) => x - y)[Math.floor(values.length / 2)] ?? 0);
  const pixels = differences[0]?.length ?? 0;
  let changed = 0;
  for (let index = 0; index < pixels; index += 1) {
    if (differences.some((values, channel) => Math.abs((values[index] ?? 0) - (shifts[channel] ?? 0)) > ChannelNoise)) changed += 1;
  }
  return changed < Math.max(3, pixels * ChangedShare);
}

import { mkdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { PNG } from "pngjs";

export const FaceSheetFile = "face-sheet.png";
const TileSize = 64;
const Columns = 16;
const MaximumFrames = 256;

export function faceSheet(frames: readonly Buffer[]): Buffer {
  if (!frames.length || frames.length > MaximumFrames) throw new Error("Face sheet needs between one and 256 card frames.");
  const sheet = new PNG({ width: Columns * TileSize, height: Math.ceil(frames.length / Columns) * TileSize });
  frames.forEach((frame, index) => {
    const source = PNG.sync.read(frame);
    for (let y = 0; y < TileSize; y += 1) {
      for (let x = 0; x < TileSize; x += 1) {
        const from = (Math.floor(y * source.height / TileSize) * source.width + Math.floor(x * source.width / TileSize)) * 4;
        const to = (((Math.floor(index / Columns) * TileSize + y) * sheet.width) + index % Columns * TileSize + x) * 4;
        for (let channel = 0; channel < 4; channel += 1) sheet.data[to + channel] = source.data[from + channel] ?? 0;
      }
    }
  });
  return PNG.sync.write(sheet);
}

export async function saveFaceSheet(folder: string, frames: readonly Buffer[]): Promise<string> {
  await mkdir(folder, { recursive: true });
  await writeFile(join(folder, FaceSheetFile), faceSheet(frames));
  return FaceSheetFile;
}

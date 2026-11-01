import { access, mkdir } from "node:fs/promises";
import { join } from "node:path";
import type { Browser } from "playwright";
import { clicking } from "./activation.ts";
import { waitForFace } from "./board.ts";
import { setGridSize } from "./grid-size.ts";
import { Device, openGame, readBoard } from "./open-game.ts";
import { NoFace } from "./face-signature.ts";
import { FaceSheetFile } from "./face-sheet.ts";

export const ScreenshotCases = [
  { name: "desktop-4x4.png", device: Device.desktop, size: 4, turn: true },
  { name: "desktop-16x16.png", device: Device.desktop, size: 16, turn: true },
  { name: "phone-4x4.png", device: Device.phone, size: 4, turn: true },
  { name: "phone-16x16.png", device: Device.phone, size: 16, turn: false },
] as const;
export const CompletionShot = "desktop-win.png";
export const EvidenceShotNames: readonly string[] = [FaceSheetFile, CompletionShot, ...ScreenshotCases.map((shot) => shot.name)];

export async function takeShots(browser: Browser, url: string, folder: string): Promise<string[]> {
  await mkdir(folder, { recursive: true });
  const taken: string[] = [];
  try { await access(join(folder, CompletionShot)); taken.push(CompletionShot); } catch { /* A failed game has no completion image. */ }
  for (const shot of ScreenshotCases) {
    const game = await openGame(browser, url, shot.device);
    try {
      if (!(await setGridSize(game.page, shot.size))) continue;
      if (shot.turn) {
        const board = await readBoard(game.page);
        await clicking(game.page)(0);
        await waitForFace(game.page, 0, board.backs[0] ?? NoFace);
        await clicking(game.page)(1);
        await waitForFace(game.page, 1, board.backs[1] ?? NoFace);
      }
      await game.page.screenshot({ path: join(folder, shot.name), fullPage: true });
      taken.push(shot.name);
    } catch {
      // Missing images stay missing evidence. They never turn a failed check into a pass.
    } finally { await game.context.close(); }
  }
  return taken;
}

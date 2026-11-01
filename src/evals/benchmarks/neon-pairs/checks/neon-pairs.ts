import { mkdir } from "node:fs/promises";
import { join } from "node:path";
import { pathToFileURL } from "node:url";
import { chromium, type Browser } from "playwright";
import type { SuiteCheckInput, SuiteCheckOutput } from "../../suite-checks.types.ts";
import { clicking } from "./activation.ts";
import { waitForFace, Waits } from "./board.ts";
import { CheckArea, CheckId, result, retriedOnce } from "./check-ids.ts";
import { deliveryChecks, stackCheck } from "./checks-delivery.ts";
import { keyboardCheck, phoneCheck } from "./checks-devices.ts";
import { largeBoardChecks } from "./checks-large.ts";
import { playChecks } from "./checks-play.ts";
import { scoreChecks } from "./checks-score.ts";
import { setGridSize } from "./grid-size.ts";
import { Device, openGame, readBoard } from "./open-game.ts";
import { NoFace } from "./face-signature.ts";

const Shots = [
  { name: "desktop-4x4.png", device: Device.desktop, size: 4, turn: true },
  { name: "desktop-16x16.png", device: Device.desktop, size: 16, turn: true },
  { name: "phone-4x4.png", device: Device.phone, size: 4, turn: true },
  { name: "phone-16x16.png", device: Device.phone, size: 16, turn: false },
] as const;

/** Screenshots for the people judging: the same sizes and states for every run, two cards face up. */
async function takeShots(browser: Browser, url: string, folder: string): Promise<string[]> {
  await mkdir(folder, { recursive: true });
  const taken: string[] = [];
  for (const shot of Shots) {
    const game = await openGame(browser, url, shot.device);
    try {
      if (!(await setGridSize(game.page, shot.size)) && shot.size !== 4) continue;
      if (shot.turn) {
        const board = await readBoard(game.page);
        await clicking(game.page)(0);
        await waitForFace(game.page, 0, board.backs[0] ?? NoFace);
      }
      await game.page.waitForTimeout(Waits.poll * 3);
      await game.page.screenshot({ path: join(folder, shot.name), fullPage: true });
      taken.push(shot.name);
    } catch {
      // A missing screenshot shows as missing in the gallery. The checks report the failure.
    } finally {
      await game.context.close();
    }
  }
  return taken;
}

export async function runNeonPairsChecks(input: SuiteCheckInput): Promise<SuiteCheckOutput> {
  const stack = stackCheck(input.project, input.build.typecheck);
  if (input.html === null) {
    return { checks: [stack, result(CheckId.singleFile, CheckArea.delivery, false, "The build failed, so there's no index.html to open.")], performance: null, shots: [] };
  }
  const url = pathToFileURL(input.html).href;
  const browser = await chromium.launch();
  try {
    const first = await openGame(browser, url, Device.desktop);
    await first.context.close();
    const large = await largeBoardChecks(browser, url, first.firstRenderMs);
    const checks = [
      stack,
      ...(await deliveryChecks(browser, url, true)),
      ...(await retriedOnce(() => playChecks(browser, url))),
      ...(await retriedOnce(() => scoreChecks(browser, url))),
      ...(await retriedOnce(() => phoneCheck(browser, url))),
      ...(await retriedOnce(() => keyboardCheck(browser, url))),
      ...large.checks,
    ];
    return { checks, performance: large.performance, shots: await takeShots(browser, url, input.shotsFolder) };
  } finally {
    await browser.close();
  }
}

import type { Browser, Page } from "playwright";
import type { CheckResult } from "../../../src/results/results.types.ts";
import { clicking, type Activate } from "./activation.ts";
import { Waits } from "./board.ts";
import { CheckArea, CheckId, guarded, result } from "./check-ids.ts";
import { setGridSize } from "./grid-size.ts";
import { Device, openGame, readBoard } from "./open-game.ts";
import { bestIn, movesIn, newGameButton, pageText } from "./page-text.ts";
import { playGame } from "./solver.ts";

const PlaySize = 4;
const OtherSize = 6;
// Clearly worse games waste this many extra turns on known mismatches, so any sensible score ranks them lower.
const Misses = { worse: 10, worst: 14 } as const;

interface Played {
  readonly best: string | null;
  readonly moves: number | null;
  readonly won: boolean;
}

async function play(page: Page, activate: Activate, extraMisses: number): Promise<Played> {
  await (await newGameButton(page))?.click();
  await page.waitForTimeout(Waits.poll * 3);
  const game = await playGame(page, activate, (await readBoard(page)).backs, { extraMisses });
  await page.waitForTimeout(Waits.settle);
  const text = await pageText(page);
  return { best: bestIn(text), moves: movesIn(text), won: game.won };
}

async function unfinished(page: Page, activate: Activate): Promise<string | null> {
  await (await newGameButton(page))?.click();
  await page.waitForTimeout(Waits.poll * 3);
  await activate(0);
  await activate(1);
  await page.waitForTimeout(Waits.settle);
  await (await newGameButton(page))?.click();
  await page.waitForTimeout(Waits.poll * 3);
  return bestIn(await pageText(page));
}

/** The best score and the move counter, over a run of clearly worse and better games. */
export function scoreChecks(browser: Browser, url: string): Promise<CheckResult[]> {
  return guarded(CheckId.best, CheckArea.best, async () => {
    const game = await openGame(browser, url, Device.desktop);
    try {
      const { page } = game;
      if ((await readBoard(page)).count !== PlaySize * PlaySize) await setGridSize(page, PlaySize);
      const activate = clicking(page);
      const start = bestIn(await pageText(page));
      const afterUnfinished = await unfinished(page, activate);
      const worse = await play(page, activate, Misses.worse);
      const better = await play(page, activate, 0);
      const worst = await play(page, activate, Misses.worst);
      await page.reload();
      await page.waitForTimeout(Waits.settle);
      if ((await readBoard(page)).count !== PlaySize * PlaySize) await setGridSize(page, PlaySize);
      const reloaded = bestIn(await pageText(page));
      const otherSize = (await setGridSize(page, OtherSize)) ? bestIn(await pageText(page)) : null;
      const steps = [
        afterUnfinished === start,
        worse.won && worse.best !== null && worse.best !== start,
        better.won && better.best !== null && better.best !== worse.best,
        worst.won && worst.best === better.best,
        reloaded === better.best,
        otherSize !== better.best,
      ];
      const detail = `Best went ${start ?? "none"} → ${afterUnfinished ?? "none"} (unfinished) → ${worse.best ?? "none"} (worse) → ${better.best ?? "none"} (better) → ${worst.best ?? "none"} (worst) → ${reloaded ?? "none"} (reload), and showed ${otherSize ?? "none"} for ${OtherSize}x${OtherSize}.`;
      const movesShown = worse.moves !== null && better.moves !== null && worse.moves > better.moves;
      return [
        result(CheckId.best, CheckArea.best, steps.every(Boolean), detail),
        result(CheckId.movesCompared, CheckArea.moves, movesShown, `A game with ${Misses.worse} extra misses showed ${worse.moves ?? "no"} moves, a careful one ${better.moves ?? "no"}.`),
      ];
    } finally {
      await game.context.close();
    }
  });
}

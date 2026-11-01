import type { Browser } from "playwright";
import type { CheckResult } from "../../../history/results.types.ts";
import { tapping, typing } from "./activation.ts";
import { CheckArea, CheckId, guarded, result } from "./check-ids.ts";
import { setGridSize } from "./grid-size.ts";
import { Device, openGame, readBoard } from "./open-game.ts";
import { playGame } from "./solver.ts";

const PlaySize = 4;

/** A whole 4x4 game by tapping at a phone size. */
export function phoneCheck(browser: Browser, url: string): Promise<CheckResult[]> {
  return guarded(CheckId.phone, CheckArea.phone, async () => {
    const game = await openGame(browser, url, Device.phone);
    try {
      if ((await readBoard(game.page)).count !== PlaySize * PlaySize) await setGridSize(game.page, PlaySize);
      const outcome = await playGame(game.page, tapping(game.page), (await readBoard(game.page)).backs, { extraMisses: 0 });
      const overflow = await game.page.evaluate(() => document.documentElement.scrollWidth > window.innerWidth + 1);
      const detail = `${outcome.won ? "Finished a game by tapping" : `Couldn't finish by tapping: ${(outcome.error ?? "").replace(/\.$/, "")}`}${overflow ? ", and the page scrolls sideways." : "."}`;
      return [result(CheckId.phone, CheckArea.phone, outcome.won && !overflow, detail)];
    } finally {
      await game.context.close();
    }
  });
}

/** A whole 4x4 game with the keyboard alone, which also counts any sound the game starts. */
export function keyboardCheck(browser: Browser, url: string): Promise<CheckResult[]> {
  return guarded(CheckId.keyboard, CheckArea.keyboard, async () => {
    const game = await openGame(browser, url, Device.desktop);
    try {
      if ((await readBoard(game.page)).count !== PlaySize * PlaySize) await setGridSize(game.page, PlaySize);
      await game.page.mouse.click(1, 1);
      const outcome = await playGame(game.page, typing(game.page), (await readBoard(game.page)).backs, { extraMisses: 0 });
      const sounds = await game.page.evaluate(() => window.__evalSounds ?? 0);
      return [
        result(CheckId.keyboard, CheckArea.keyboard, outcome.won, outcome.won ? "Finished a game with the keyboard alone." : `Couldn't finish with the keyboard: ${(outcome.error ?? "").replace(/\.$/, "")}`),
        result(CheckId.noSound, CheckArea.intent, sounds === 0, sounds === 0 ? "No sound started during a game." : `The game started ${sounds} sounds.`),
      ];
    } finally {
      await game.context.close();
    }
  });
}

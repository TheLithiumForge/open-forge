import type { Browser, Page } from "playwright";
import { mkdir } from "node:fs/promises";
import { join } from "node:path";
import type { CheckOutcome } from "../../../grading/grading.types.ts";
import { clicking, type Activate } from "./activation.ts";
import { waitForBacks, waitForFace, Waits } from "./board.ts";
import { CheckArea, CheckId, guarded, result, withPageErrors } from "./check-ids.ts";
import { CheckStatus } from "../../../history/statuses.ts";
import { NoFace, sameFace } from "./face-signature.ts";
import { setGridSize } from "./grid-size.ts";
import { Device, openGame, readBoard, type Board } from "./open-game.ts";
import { movesIn, newGameButton, pageText, showsWin } from "./page-text.ts";
import { playGame } from "./solver.ts";
import { CompletionShot } from "./screenshots.ts";

const PlaySize = 4;

async function freshGame(page: Page): Promise<Board> {
  await (await newGameButton(page))?.click();
  await page.waitForTimeout(Waits.poll * 3);
  return readBoard(page);
}

/** Turns two cards. Returns true when they show different faces, so the turn is a mismatch. */
async function mismatch(page: Page, activate: Activate, board: Board, a: number, b: number): Promise<boolean> {
  await activate(a);
  const faceA = await waitForFace(page, a, board.backs[a] ?? NoFace);
  await activate(b);
  const faceB = await waitForFace(page, b, board.backs[b] ?? NoFace);
  return faceA !== null && faceB !== null && !sameFace(faceA, faceB);
}

async function newGameDuringDelay(page: Page, activate: Activate): Promise<CheckOutcome> {
  const board = await freshGame(page);
  const different = await mismatch(page, activate, board, 0, 1);
  await (await newGameButton(page))?.click();
  const allBacks = await waitForBacks(page, board.backs.map((_, index) => index), board.backs, Waits.hide);
  const moves = movesIn(await pageText(page));
  const passed = allBacks && moves === 0;
  return result(CheckId.newGame, CheckArea.newGame, passed, `${different ? "During the flip-back delay" : "After a turn"}, New Game left ${allBacks ? "every card face down" : "cards face up"} and moves at ${moves ?? "no counter found"}.`);
}

async function sameCardTwice(page: Page, activate: Activate): Promise<CheckOutcome> {
  const board = await freshGame(page);
  const before = movesIn(await pageText(page));
  await activate(0);
  const first = await waitForFace(page, 0, board.backs[0] ?? NoFace);
  await activate(0);
  await page.waitForTimeout(Waits.settle);
  const afterDouble = movesIn(await pageText(page));
  await activate(1);
  const second = await waitForFace(page, 1, board.backs[1] ?? NoFace);
  if (first === null || second === null) return result(CheckId.sameCardTwice, CheckArea.input, false, "A card didn't turn over.");
  if (afterDouble !== before) return result(CheckId.sameCardTwice, CheckArea.input, false, "Activating the same card twice counted a move.");
  if (sameFace(first, second)) return result(CheckId.sameCardTwice, CheckArea.input, true, "Repeated activation kept the move count unchanged before the next matching card.");
  // The game can keep the card selected or let the second click put it back. Either way it must end face down.
  const down = await waitForBacks(page, [0], board.backs);
  return result(CheckId.sameCardTwice, CheckArea.input, down, down ? "Clicking a card twice didn't pair it with itself." : "The card clicked twice stayed face up, as if it had been paired with itself.");
}

/** Turns pairs of cards until two differ, so the next click lands in the flip-back delay. */
async function findMismatch(page: Page, activate: Activate, board: Board): Promise<readonly [number, number] | null> {
  for (let a = 0; a + 1 < board.count; a += 2) {
    if (await mismatch(page, activate, board, a, a + 1)) return [a, a + 1];
    // A matched pair, or two faces that only look alike: let the game settle, so no click lands in its delay.
    await waitForBacks(page, [a, a + 1], board.backs, Waits.hide);
  }
  return null;
}

async function clicksDuringDelay(page: Page, activate: Activate): Promise<CheckOutcome> {
  const board = await freshGame(page);
  const pair = await findMismatch(page, activate, board);
  if (pair === null) return { id: CheckId.clicksDuringDelay, area: CheckArea.input, status: CheckStatus.error, evidence: "The checker found no mismatch to exercise the delay." };
  const extra = pair[1] + 1;
  await activate(extra);
  const firstTwoDown = await waitForBacks(page, pair, board.backs);
  // The extra click might have opened a new turn. Finishing that turn leaves a normal board.
  const extraFace = await waitForFace(page, extra, board.backs[extra] ?? NoFace);
  if (extraFace !== null && extra + 1 < board.count) {
    await activate(extra + 1);
    const next = await waitForFace(page, extra + 1, board.backs[extra + 1] ?? NoFace);
    if (next !== null && !sameFace(extraFace, next)) await waitForBacks(page, [extra, extra + 1], board.backs);
  }
  const rest = await playGame(page, activate, board.backs, { extraMisses: 0 });
  const detail = `A click during the flip-back delay ${firstTwoDown ? "didn't keep the turn's cards up" : "kept the turn's cards face up"}, and the game ${rest.won ? "could still be finished" : `couldn't be finished: ${rest.error ?? ""}`}`;
  return result(CheckId.clicksDuringDelay, CheckArea.input, firstTwoDown && rest.won, detail);
}

async function fullGame(page: Page, activate: Activate, shotsFolder?: string): Promise<CheckOutcome[]> {
  const board = await freshGame(page);
  const before = await pageText(page);
  let beforeLastPair = before;
  const game = await playGame(page, activate, board.backs, { extraMisses: 0, beforeLastPair: async () => { beforeLastPair = await pageText(page); } });
  await page.waitForTimeout(Waits.settle);
  const after = await pageText(page);
  const movesBefore = movesIn(before) ?? 0;
  const movesAfter = movesIn(after);
  const won = game.won && showsWin(after, beforeLastPair);
  if (won && shotsFolder) {
    await mkdir(shotsFolder, { recursive: true });
    await page.screenshot({ path: join(shotsFolder, CompletionShot), fullPage: true });
  }
  return [
    result(CheckId.matching, CheckArea.matching, game.won, game.won ? `Every pair matched in ${game.turns} turns.` : (game.error ?? "The game didn't finish.")),
    result(CheckId.win, CheckArea.win, won, won ? "The game showed that it was won." : "No win message appeared after the last pair."),
    result(CheckId.moves, CheckArea.moves, movesAfter !== null && movesAfter > movesBefore, `Moves went from ${movesBefore} to ${movesAfter ?? "no counter found"}.`),
  ];
}

/** Matching, the win, moves, New Game and odd clicks, on one desktop board played with the mouse. */
export function playChecks(browser: Browser, url: string, shotsFolder?: string): Promise<CheckOutcome[]> {
  return guarded(CheckId.matching, CheckArea.matching, async () => {
    const game = await openGame(browser, url, Device.desktop);
    try {
      if ((await readBoard(game.page)).count !== PlaySize * PlaySize) await setGridSize(game.page, PlaySize);
      const activate = clicking(game.page);
      const results = [...(await fullGame(game.page, activate, shotsFolder))];
      results.push(...(await guarded(CheckId.newGame, CheckArea.newGame, async () => [await newGameDuringDelay(game.page, activate)])));
      results.push(...(await guarded(CheckId.sameCardTwice, CheckArea.input, async () => [await sameCardTwice(game.page, activate)])));
      results.push(...(await guarded(CheckId.clicksDuringDelay, CheckArea.input, async () => [await clicksDuringDelay(game.page, activate)])));
      const sounds = await game.page.evaluate(() => window.__evalSounds ?? 0);
      results.push(result(CheckId.noSound, CheckArea.intent, sounds === 0, sounds === 0 ? "No sound started during visible play." : `The game started ${sounds} sounds.`));
      return withPageErrors(results, game.errors);
    } finally {
      await game.context.close();
    }
  });
}

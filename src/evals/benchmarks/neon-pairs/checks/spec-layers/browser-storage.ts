import type { Page } from "playwright";
import { markVisibleCards as markCards } from "./browser-cards.ts";
import { Evidence } from "./outcomes.ts";
import { body, changeSize, click, metric, names, reset, solve, step, visibleDeck } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { faceIn } from "./view-values.ts";
import { hasStatus, StatusClause } from "./published-text.ts";

export async function preferences(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence);
  put("R08.1", await markCards(page) === 16, "A fresh HTTP origin starts with sixteen cards.");
  await changeSize(page, 6); const beforeReload = await visibleDeck(page);
  await page.reload(); await markCards(page);
  put("R08.2 R08.3", await markCards(page) === 36, "A size selected on this origin survives reload.");
  const after = await names(page);
  const reloadClean = after.every((name) => !faceIn(name)) && metric(await body(page), "moves") === 0;
  const afterReload = await visibleDeck(page);
  put("R08.4", reloadClean && JSON.stringify(beforeReload) !== JSON.stringify(afterReload), "Reload starts closed at zero moves with a different visibly learned deal.");
  await reset(page);
  await click(page, 0);
  const newFace = faceIn((await names(page))[0] ?? "");
  await reset(page); const restartClean = (await names(page)).every((name) => !faceIn(name)) && metric(await body(page), "moves") === 0 && metric(await body(page), "pairs") === 0 && metric(await body(page), "time") === 0; await click(page, 0);
  put("R09.2 C03.2", faceIn((await names(page))[0] ?? "") === newFace, "Restart repeats the first visible face of this deal.");
  const deck = await visibleDeckAfterReset(page);
  await reset(page, true);
  const newClean = await markCards(page) === 36 && metric(await body(page), "moves") === 0 && metric(await body(page), "pairs") === 0 && metric(await body(page), "time") === 0 && (await names(page)).every((name) => !faceIn(name));
  const other = await visibleDeck(page);
  put("R09.1 C02.2", newClean && JSON.stringify(deck) !== JSON.stringify(other), "New Game keeps the size and clears progress with a different visibly discovered deal.");
  await changeSize(page, 4);
  put("R09.3", await markCards(page) === 16, "Changing size replaces the board with that size.");
  const clean = (await names(page)).every((name) => /closed/i.test(name)) && metric(await body(page), "moves") === 0 && metric(await body(page), "time") === 0 && metric(await body(page), "pairs") === 0;
  put("R09.4", clean && restartClean && newClean && deck.length === other.length, "Restart, New Game and size change clear selection, matches, moves and elapsed time.");
}
async function visibleDeckAfterReset(page: Page): Promise<string[]> { await reset(page); return visibleDeck(page); }

export async function best(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence);
  const displayed = [hasStatus(await body(page), "Best", "none")];
  put(StatusClause.emptyBest, displayed[0] === true, "A fresh origin displays Best: none.");
  const deck = await visibleDeck(page); await reset(page);
  const unfinishedHasNoBest = metric(await body(page), "best") === null;
  const wrong = deck.findIndex((face) => face !== deck[0]);
  await click(page, 0); await click(page, wrong); await step(page, 800);
  await solve(page, deck);
  const initialBest = metric(await body(page), "best");
  displayed.push(hasStatus(await body(page), "Best", 9));
  put("R10.1", unfinishedHasNoBest && initialBest === 9, "Unfinished exploration creates no best. A completed nine-move game records nine.");
  await reset(page); await solve(page, deck);
  const bestMoves = metric(await body(page), "best");
  displayed.push(hasStatus(await body(page), "Best", 8));
  await reset(page); const kept = metric(await body(page), "best");
  displayed.push(hasStatus(await body(page), "Best", 8));
  await solve(page, deck);
  const equal = metric(await body(page), "best");
  await reset(page);
  await click(page, 0); await click(page, wrong); await step(page, 800); await solve(page, deck);
  put("R10.2 R10.4", initialBest === 9 && bestMoves === 8 && equal === 8 && metric(await body(page), "best") === 8, "Eight moves replaces nine. Equal and nine-move finishes then retain eight.");
  await reset(page, true);
  put("R10.5", kept === 8 && metric(await body(page), "best") === 8, "Restart and New Game keep the saved best.");
  await page.reload(); await markCards(page);
  displayed.push(hasStatus(await body(page), "Best", 8));
  put("R10.3", metric(await body(page), "best") === 8, "The best survives reload on the same HTTP origin.");
  await changeSize(page, 6);
  displayed.push(hasStatus(await body(page), "Best", "none"));
  await changeSize(page, 4); displayed.push(hasStatus(await body(page), "Best", 8));
  put(StatusClause.best, displayed.every(Boolean), "Best: N follows the current size before and after completion, restart and reload. An unplayed size shows Best: none.");
}

export async function stale(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence);
  let safe = true;
  for (const action of [async () => reset(page), async () => reset(page, true), async () => changeSize(page, 6), async () => { await page.reload(); await markCards(page); }]) {
    const deck = await visibleDeckAfterReset(page); await reset(page);
    const other = deck.findIndex((face) => face !== deck[0]);
    await click(page, 0); await click(page, other); await step(page, 300); await action(); await click(page, 0);
    const before = await names(page); await step(page, 800);
    safe &&= JSON.stringify(await names(page)) === JSON.stringify(before) && metric(await body(page), "moves") === 0;
    await reset(page);
  }
  put("R12.1 R12.2", safe, "After pending mismatches, Restart, New Game, a size change and leaving the page invalidate old callbacks.");
}

export async function storageFailure(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence);
  const deck = await visibleDeck(page); await reset(page); await solve(page, deck);
  put("R11.4 T09.2", metric(await body(page), "pairs") === 8, "With Storage reads and writes throwing SecurityError, a full visible-play game still finishes.");
}
export async function invalidPreferences(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence);
  await changeSize(page, 6);
  const keys = await page.evaluate(() => Object.keys(localStorage));
  for (const malformed of ["{", JSON.stringify({ size: 5, bestMoves: { 4: -1 } }), "null", "[]"]) {
    await page.evaluate(({ keys, malformed }) => { for (const key of keys) localStorage.setItem(key, malformed); }, { keys, malformed });
    await page.reload(); await markCards(page);
    if (await markCards(page) !== 16) { put("R11.3", false, "Invalid saved data did not fall back to 4x4."); return; }
  }
  put("R11.3", keys.length > 0 && (await names(page)).every((name) => !faceIn(name)), "Malformed JSON, unsupported sizes, negative scores, null and arrays fall back safely on a fresh HTTP origin.");
}

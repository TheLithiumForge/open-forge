import type { Page } from "playwright";
import { CheckStatus } from "../../../../history/statuses.ts";
import { CapClause } from "./cap-policy.ts";
import { exported, importText } from "./browser-formats.ts";
import { body, click, Labels, metric, names, reset, solve, step, visibleDeck } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { Evidence } from "./outcomes.ts";
import { Rules, SelectionKind } from "./rules.ts";
import { replaceSavedScore } from "./saved-score.ts";
import { cappedProgress } from "./vectors.ts";
import { elapsedIn, faceIn } from "./view-values.ts";

const DisplayQuantumMs = 1000;
const DisplayedMaximumMs = Math.floor(Rules.elapsedMaximumMs / DisplayQuantumMs) * DisplayQuantumMs;

export async function visibleTimeCap(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), deck = await visibleDeck(page);
  await reset(page); await click(page, 0);
  await page.clock.fastForward(Rules.elapsedMaximumMs + DisplayQuantumMs);
  const atLimit = elapsedIn(await body(page));
  const partner = deck.findIndex((face, index) => index !== 0 && face === deck[0]);
  await click(page, partner);
  const firstMatch = metric(await body(page), "pairs") === 1;
  const a = deck.findIndex((_, index) => index !== 0 && index !== partner);
  const b = deck.findIndex((face, index) => index !== 0 && index !== partner && index !== a && face !== deck[a]);
  await click(page, a); await click(page, b); await step(page, Rules.mismatchMs);
  const closed = await names(page);
  await solve(page, deck);
  const text = await body(page);
  put(CapClause.elapsedPlay, firstMatch && !faceIn(closed[a] ?? "") && !faceIn(closed[b] ?? "") && metric(text, "pairs") === deck.length / 2,
    "After a controlled jump beyond the displayed time limit, a match, an 800ms miss and the remaining game still finish through visible play.");
  const after = elapsedIn(text);
  put(CapClause.elapsedDisplay, atLimit !== null && after !== null && after >= atLimit && after - atLimit <= 1000,
    "The visible clock stays at any implemented cap, or advances by the continued active interval. Layer 2 doesn't require a numeric cap. Layer 3 checks the exact stored limit separately.");
}

export async function snapshotCaps(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), vector = cappedProgress();
  const totalCards = vector.state.matched.length + 2 * (vector.remaining.length + 1);
  await importText(page, JSON.stringify(vector.state));
  await page.getByRole("button", { name: Labels.resume, exact: true }).click();
  await click(page, vector.first.second); await step(page, 1);
  const reached = await exported(page), [a, b] = vector.remaining;
  if (!a || !b) throw new Error("Missing cap continuation pairs.");
  await click(page, a.first); await click(page, b.first);
  const missed = await exported(page);
  await step(page, Rules.mismatchMs - 1); const pending = await exported(page);
  await step(page, 1); const closed = await exported(page);
  for (const pair of vector.remaining) { await click(page, pair.first); await click(page, pair.second); }
  const completed = await exported(page), text = await body(page);
  put(CapClause.movesLimit, reached?.moves === Rules.movesMaximum && missed?.moves === Rules.movesMaximum && completed?.moves === Rules.movesMaximum,
    "A valid imported game reaches 999999 on its next match. Another miss and two matches keep that move count.");
  put(CapClause.movesPlay, missed?.selection.kind === SelectionKind.mismatch && pending?.selection.kind === SelectionKind.mismatch && pending.selection.remainingMs === 1 && closed?.selection.kind === SelectionKind.closed,
    "At the move cap, another turn reveals a mismatch and closes it at exactly 800 active milliseconds.");
  put(CapClause.movesMatch, reached?.matched.length === vector.state.matched.length + 2 && completed?.matched.length === vector.state.matched.length + 6,
    "Matches keep adding whole pairs after reaching the move cap.");
  put(CapClause.movesWin, completed?.matched.length === totalCards && metric(text, "pairs") === totalCards / 2 && /(?:win|won|every pair|complete)/i.test(text),
    "The capped game completes all eight pairs and shows a win.");
  put(CapClause.elapsedLimit, [reached, missed, pending, closed, completed].every((state) => state?.elapsedMs === Rules.elapsedMaximumMs),
    "Starting one millisecond below the cap reaches exactly 359999999ms and stays there across mismatch closing, matches, victory and valid version 1 exports.", { imported: vector.state, reached, missed, pending, closed, completed });
  evidence.merge(CapClause.elapsedPlay, closed?.selection.kind === SelectionKind.closed && completed?.matched.length === totalCards ? CheckStatus.pass : CheckStatus.fail, "Snapshot-backed play continues after the exact elapsed cap.");
  evidence.merge(CapClause.elapsedDisplay, elapsedIn(text) === DisplayedMaximumMs ? CheckStatus.pass : CheckStatus.fail, "The completed imported game displays its capped elapsed time.");
  await savedBestCap(page, evidence);
}

async function savedBestCap(page: Page, evidence: Evidence): Promise<void> {
  const best = metric(await body(page), "best");
  await page.reload();
  const persisted = metric(await body(page), "best");
  const stored = await page.evaluate(() => Object.entries(localStorage));
  let replacements = 0;
  for (const [key, text] of stored) {
    let value: unknown;
    try { value = JSON.parse(text); } catch { continue; }
    const changed = replaceSavedScore(value, Rules.movesMaximum, Rules.movesMaximum + 1);
    replacements += changed.replacements;
    if (changed.replacements) await page.evaluate(({ key, text }) => localStorage.setItem(key, text), { key, text: JSON.stringify(changed.value) });
  }
  if (best === Rules.movesMaximum && persisted === Rules.movesMaximum && replacements === 0) {
    evidence.record(CapClause.savedBest, CheckStatus.error, "The capped best persisted, but the checker could not identify its score in the saved encoding.");
    return;
  }
  await page.reload();
  evidence.test(CapClause.savedBest, best === Rules.movesMaximum && persisted === Rules.movesMaximum && replacements > 0 && metric(await body(page), "best") === null,
    "A genuine capped win saves a best of 999999 across same-origin reload. Raising only the discovered saved score to 1000000 makes the saved record invalid.");
}

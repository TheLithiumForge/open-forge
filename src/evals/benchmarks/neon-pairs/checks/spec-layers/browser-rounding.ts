import type { Page } from "playwright";
import { exported, loadBoard } from "./browser-formats.ts";
import { click, FractionalClockKey } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { Evidence } from "./outcomes.ts";
import { KnownCode } from "./vectors.ts";
import { SelectionKind } from "./rules.ts";

export const RoundingClause = { elapsed: "C08.6", remaining: "C08.7" } as const;
export async function snapshotRounding(page: Page, evidence: Evidence): Promise<void> {
  const fraction = (value: number) => page.evaluate(({ key, value }) => { Reflect.set(window, key, value); }, { key: FractionalClockKey, value });
  await fraction(0); await loadBoard(page, KnownCode); await click(page, 0);
  await fraction(1.25); const elapsed = await exported(page);
  await click(page, 1); await fraction(1.5); const pending = await exported(page);
  await fraction(801); const last = await exported(page);
  const put = reporter(evidence);
  const observed = { offsetsMs: [1.25, 1.5, 801], elapsed, pending, last };
  put(RoundingClause.elapsed, elapsed?.elapsedMs === 1 && pending?.elapsedMs === 1 && last?.elapsedMs === 801, "Exports at offsets 1.25, 1.5 and 801ms must floor elapsed time to 1, 1 and 801ms.", observed);
  put(RoundingClause.remaining, pending?.selection.kind === SelectionKind.mismatch && pending.selection.remainingMs === 800 && last?.selection.kind === SelectionKind.mismatch && last.selection.remainingMs === 1, "Exports at offsets 1.5 and 801ms must ceil remaining mismatch time to 800 and 1ms, with mismatch selection intact.", observed);
}

import type { Page } from "playwright";
import { exported, loadBoard } from "./browser-formats.ts";
import { click, FractionalClockKey } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { Evidence } from "./outcomes.ts";
import { KnownCode } from "./vectors.ts";
import { SelectionKind } from "./rules.ts";

export const RoundingClause = { elapsed: "C08.6", remaining: "C08.7" } as const;
export const ExportToleranceMs = 1;
export function wholeMilliseconds(actual: unknown, expected: number, minimum = 0): boolean {
  return typeof actual === "number" && Number.isInteger(actual) && actual >= minimum && Math.abs(actual - expected) <= ExportToleranceMs;
}
export async function snapshotRounding(page: Page, evidence: Evidence): Promise<void> {
  const fraction = (value: number) => page.evaluate(({ key, value }) => { Reflect.set(window, key, value); }, { key: FractionalClockKey, value });
  await fraction(0); await loadBoard(page, KnownCode); await click(page, 0);
  await fraction(1.25); const elapsed = await exported(page);
  await click(page, 1); await fraction(1.5); const pending = await exported(page);
  await fraction(801); const last = await exported(page);
  const put = reporter(evidence);
  const observed = { offsetsMs: [1.25, 1.5, 801], elapsed, pending, last };
  put(RoundingClause.elapsed, wholeMilliseconds(elapsed?.elapsedMs, 1.25) && wholeMilliseconds(pending?.elapsedMs, 1.5) && wholeMilliseconds(last?.elapsedMs, 801), "Elapsed exports are whole milliseconds within 1ms of independent active times 1.25, 1.5 and 801ms.", observed);
  put(RoundingClause.remaining, pending?.selection.kind === SelectionKind.mismatch && wholeMilliseconds(pending.selection.remainingMs, 799.75, 1) && last?.selection.kind === SelectionKind.mismatch && wholeMilliseconds(last.selection.remainingMs, 0.25, 1), "Pending exports are whole milliseconds within 1ms of independent remaining times 799.75 and 0.25ms, with at least 1ms and mismatch selection intact.", observed);
}

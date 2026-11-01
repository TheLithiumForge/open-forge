import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { markVisibleCards } from "./browser-cards.ts";
import { body, click, Labels, names, step } from "./browser-session.ts";
import { exported, importText, loadBoard, reproduction } from "./browser-formats.ts";
import { reporter } from "./browser-play.ts";
import { Evidence } from "./outcomes.ts";
import { freshSnapshot } from "./replay.ts";
import { snapshotsEqual } from "./snapshot.ts";
import { SelectionKind } from "./rules.ts";
import { KnownCode } from "./vectors.ts";
import { AnnouncementClause, LoadNotice } from "./published-text.ts";
import { LiveRegion, observeAnnouncements } from "./announcements.ts";

export const ImportClause = { completedStatus: "C09.5", resume: "C10.1", completed: "C10.7", pause: "D07.6", best: "C12.4", sessionBest: "A06.3", selectedSize: "C01.4" } as const;

export async function invalidLoads(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence); await loadBoard(page, KnownCode); await click(page, 0);
  const before = await exported(page);
  const saved = await page.evaluate(() => JSON.stringify(Object.entries(localStorage)));
  let boardError = true, boardsKept = true;
  for (const code of ["np1-5-0", "np2-4-1", "np1-04-1", "np1-4-01", "np1-4-0", "np1-4-2147483647", "np1-4-1x"]) {
    await loadBoard(page, code);
    boardError &&= (await body(page)).includes("Invalid board code.");
    const current = await exported(page);
    boardsKept &&= before !== null && current !== null && snapshotsEqual(before, current);
  }
  const boardDraft = await page.getByRole("textbox", { name: Labels.code, exact: true }).inputValue();
  const afterBoard = await exported(page);
  await importText(page, "{broken");
  const snapshotError = (await body(page)).includes("Invalid snapshot.");
  const snapshotDraft = await page.getByRole("textbox", { name: Labels.snapshot, exact: true }).inputValue();
  const afterSnapshot = await exported(page);
  const unchanged = boardsKept && before !== null && afterBoard !== null && afterSnapshot !== null && snapshotsEqual(before, afterBoard) && snapshotsEqual(before, afterSnapshot);
  put("R11.1 R11.2 C11.1 D10.9", unchanged && boardError && snapshotError, "Both invalid loads show their errors and keep every exported game field.");
  put("C11.2", boardDraft === "np1-4-1x" && snapshotDraft === "{broken", "Invalid drafts remain available for correction.");
  put("C11.3", boardError, "Invalid board code. is shown exactly."); put("C11.4", snapshotError, "Invalid snapshot. is shown exactly.");
  put("R08.6", await page.evaluate(() => JSON.stringify(Object.entries(localStorage))) === saved, "Both failed loads keep the saved preferences unchanged.");
  evidence.merge("C06.2", boardsKept && boardError ? "pass" : "fail", "Unsupported versions, sizes, seed limits, leading zeros and trailing characters are rejected before replacing nonzero progress.");
}

export async function importedProgress(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), base = freshSnapshot(KnownCode);
  const resume = page.getByRole("button", { name: Labels.resume, exact: true });
  const initiallyAbsent = await resume.count() === 0;
  const pending = { ...base, moves: 1, started: true, elapsedMs: 250, selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: 500 } };
  await importText(page, JSON.stringify(pending));
  const loadedText = await body(page), loaded = await exported(page);
  put("C09.2 C09.3", loaded !== null && snapshotsEqual(loaded, pending), "A complete valid snapshot replaces progress in a paused state.");
  put("C09.4", loadedText.includes("Loaded snapshot. Select Resume to continue."), "The exact imported-pause status is shown.");
  put(ImportClause.resume, initiallyAbsent && await resume.count() === 1, "Resume is absent in a normal game and appears for imported paused progress.");
  const before = await names(page); await click(page, 2); await card(page, 0).focus(); await page.keyboard.press("ArrowRight");
  put("C10.4 C10.5", JSON.stringify(await names(page)) === JSON.stringify(before) && await card(page, 1).evaluate((element) => element === document.activeElement), "Paused cards ignore activation while arrow focus still moves.");
  await step(page, 5000); const paused = await exported(page);
  put(`R04.5 R07.4 ${ImportClause.pause}`, paused !== null && snapshotsEqual(paused, pending), "5000 controlled milliseconds change neither imported clock nor remaining mismatch time.");
  await resume.click();
  put("C10.6 C13.4", await card(page, 1).evaluate((element) => element === document.activeElement), "Resume moves focus to the remembered card before disappearing.");
  await step(page, 499); const remaining = await exported(page); await step(page, 1); const closed = await exported(page);
  put("C10.2 C10.3", await resume.count() === 0 && remaining?.selection.kind === SelectionKind.mismatch && closed?.selection.kind === SelectionKind.closed && closed.elapsedMs === pending.elapsedMs + 500, "Resume continues the exact remaining interval and active clock.");
  const completed = { ...base, moves: 8, started: true, elapsedMs: 4000, matched: Array.from({ length: 16 }, (_, index) => index) };
  const saved = await page.evaluate(() => JSON.stringify(Object.entries(localStorage)));
  await importText(page, JSON.stringify(completed));
  put("C12.1 C12.2 A06.2", /(?:won|win|complete|every pair)/i.test(await body(page)) && await page.evaluate(() => document.getAnimations().every((animation) => animation.playState !== "running")), "Completed import displays a static completion without celebration.");
  put("C12.3", await page.evaluate(() => JSON.stringify(Object.entries(localStorage))) === saved, "Completed import does not write a best score.");
  const completedUnpaused = await resume.count() === 0 && (await body(page)).includes(LoadNotice.completed);
  await step(page, 1000); const still = await exported(page);
  put(`${ImportClause.completedStatus} ${ImportClause.completed}`, completedUnpaused && still !== null && snapshotsEqual(still, completed), "A completed import shows Loaded completed snapshot. without Resume and keeps its fixed time.");
  evidence.merge(ImportClause.resume, completedUnpaused ? "pass" : "fail", "Completed imports show no Resume button.");
  evidence.merge(ImportClause.pause, completedUnpaused && still !== null && snapshotsEqual(still, completed) ? "pass" : "fail", "Completed imported progress stays completed without a pause.");
  const unfinished = { ...completed, moves: 7, matched: completed.matched.filter((index) => index !== 0 && index !== 11) };
  await importText(page, JSON.stringify(unfinished)); await resume.click(); await click(page, 0); await click(page, 11);
  const recordedBest = /Best:\s*8\b/.test(await body(page));
  await page.reload(); await markVisibleCards(page);
  put(`${ImportClause.best} ${ImportClause.sessionBest}`, recordedBest && /Best:\s*8\b/.test(await body(page)), "A genuine later win from unfinished imported progress records eight moves as best and keeps it after reload.");
  put("C07.3", evidence.outcomes.find((item) => item.clause === "D10.1")?.status === "pass", "The snapshot byte-limit vectors exercise the same public textarea parser.");
}

export async function loadedFocusAndSize(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), focused: boolean[] = [], first: boolean[] = [];
  await loadBoard(page, "np1-6-2"); const boardSelected = await page.getByRole("combobox", { name: Labels.size }).inputValue(); await page.reload();
  const boardSaved = await page.getByRole("combobox", { name: Labels.size }).inputValue();
  await importText(page, JSON.stringify(freshSnapshot("np1-8-3"))); const snapshotSelected = await page.getByRole("combobox", { name: Labels.size }).inputValue(); await page.reload();
  const snapshotSaved = await page.getByRole("combobox", { name: Labels.size }).inputValue();
  put("R08.5", boardSaved === "6" && snapshotSaved === "8", "Successful board and snapshot loads save their size across same-origin reload.");
  put(ImportClause.selectedSize, boardSelected === "6" && snapshotSelected === "8", "Both successful load controls select the loaded size immediately.");
  await loadBoard(page, KnownCode);
  for (const label of [Labels.loadBoard, Labels.export, Labels.loadSnapshot]) {
    await card(page, 7).focus(); await reproduction(page);
    if (label === Labels.loadSnapshot) await page.getByRole("textbox", { name: Labels.snapshot, exact: true }).fill(JSON.stringify(freshSnapshot(KnownCode)));
    await page.getByRole("button", { name: label, exact: true }).click();
    await markVisibleCards(page);
    focused.push(await page.getByRole("button", { name: label, exact: true }).evaluate((element) => element === document.activeElement));
    if (label !== Labels.export) first.push(await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements[0]?.getAttribute("tabindex") === "0" && elements.slice(1).every((element) => element.getAttribute("tabindex") === "-1")));
  }
  put("C13.2", focused.every(Boolean), "Board load, snapshot load and export retain button focus.");
  put("I12.2", first.every(Boolean), "Successful loads reset the remembered board stop to the first card.");
}

export async function loadAnnouncements(page: Page, evidence: Evidence): Promise<void> {
  const entries = await observeAnnouncements(page), region = page.locator(LiveRegion);
  const completed = { ...freshSnapshot(KnownCode), started: true, moves: 8, matched: Array.from({ length: 16 }, (_, index) => index) };
  const actions: readonly [string, () => Promise<void>][] = [
    [LoadNotice.unfinished, () => importText(page, JSON.stringify(freshSnapshot(KnownCode)))],
    [LoadNotice.completed, () => importText(page, JSON.stringify(completed))],
    [LoadNotice.boardError, () => loadBoard(page, "invalid")],
    [LoadNotice.snapshotError, () => importText(page, "{")],
  ];
  const results: boolean[] = [];
  for (const [notice, action] of actions) for (let repeat = 0; repeat < 2; repeat++) {
    const before = (await entries()).length; await action();
    results.push((await entries()).length === before + 1 && await region.innerText() === notice && await page.locator("details").getByText(notice, { exact: true }).isVisible());
  }
  reporter(evidence)(AnnouncementClause.loads, results.every(Boolean), "Unfinished, completed and rejected loads each update the same live region once, including repeated statuses, and show their status beside reproduction controls.");
}

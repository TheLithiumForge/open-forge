import type { Page } from "playwright";
import { card } from "../board.ts";
import { markVisibleCards as markCards } from "./browser-cards.ts";
import { expectedDeck, parseBoardCode } from "./board-code.ts";
import { body, changeSize, click, Labels, names, reset, step } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { Evidence } from "./outcomes.ts";
import { freshSnapshot } from "./replay.ts";
import { decodeSnapshot, snapshotsEqual } from "./snapshot.ts";
import { faceName, Rules, SelectionKind, type Snapshot } from "./rules.ts";
import { KnownCode, snapshotVectors } from "./vectors.ts";
import { cardName, faceIn } from "./view-values.ts";

export async function reproduction(page: Page): Promise<void> {
  const summary = page.locator("summary").filter({ hasText: new RegExp(`^${Labels.reproduce}$`) });
  if (!(await summary.evaluate((element) => element.parentElement?.hasAttribute("open")))) await summary.click();
}
export async function loadBoard(page: Page, code: string): Promise<void> {
  await reproduction(page); await page.getByRole("textbox", { name: Labels.code, exact: true }).fill(code);
  await page.getByRole("button", { name: Labels.loadBoard, exact: true }).click(); await markCards(page);
}
export async function exported(page: Page): Promise<Snapshot | null> {
  await reproduction(page); await page.getByRole("button", { name: Labels.export, exact: true }).click();
  return decodeSnapshot(await page.getByRole("textbox", { name: Labels.snapshot, exact: true }).inputValue());
}
export async function importText(page: Page, text: string): Promise<void> {
  await reproduction(page);
  const field = page.getByRole("textbox", { name: Labels.snapshot, exact: true });
  await field.evaluate((element, value) => {
    const setter = Object.getOwnPropertyDescriptor(HTMLTextAreaElement.prototype, "value")?.set;
    if (!setter) throw new Error("Native textarea setter missing.");
    setter.call(element, value); element.dispatchEvent(new Event("input", { bubbles: true }));
  }, text);
  if (await field.inputValue() !== text) throw new Error("The snapshot draft was not delivered unchanged.");
  await page.getByRole("button", { name: Labels.loadSnapshot, exact: true }).click(); await markCards(page);
}

export async function snapshotCases(page: Page, evidence: Evidence): Promise<void> {
  const results = new Map<string, { pass: boolean; details: string[] }>();
  for (const vector of snapshotVectors()) {
    await importText(page, JSON.stringify({ ...freshSnapshot(KnownCode), started: true, moves: 4, elapsedMs: 2000, selection: { kind: SelectionKind.one, index: 0 } }));
    const before = await exported(page), expected = decodeSnapshot(vector.text);
    await importText(page, vector.text);
    const rejected = (await body(page)).includes("Invalid snapshot.");
    const actual = await exported(page);
    const pass = actual !== null && (expected ? !rejected && snapshotsEqual(actual, expected) : rejected && before !== null && snapshotsEqual(actual, before));
    for (const id of vector.clauses) {
      const prior = results.get(id) ?? { pass: true, details: [] };
      prior.pass &&= pass; prior.details.push(`${vector.name}: ${pass ? "pass" : "fail"}`); results.set(id, prior);
    }
  }
  for (const [id, result] of results) evidence.test(id, result.pass, `Documented snapshot controls. ${result.details.join(". ")}`);
}

export async function knownDeals(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), runs: string[] = [], valid: boolean[] = [];
  for (const size of Rules.sizes) for (const seed of [1, 2, Rules.seedMaximum]) {
    const code = `np1-${size}-${seed}`, deck = expectedDeck(code);
    await loadBoard(page, code);
    let pass = await markCards(page) === deck.length;
    for (let index = 0; index < deck.length; index += 2) {
      await click(page, index); await click(page, index + 1);
      for (const position of [index, index + 1]) pass &&= faceIn(cardName(await card(page, position).ariaSnapshot()) ?? "") === faceName(deck[position] ?? -1);
      await step(page, Rules.mismatchMs);
    }
    valid.push(pass); runs.push(`${code}=${pass ? "pass" : "fail"}`);
  }
  put("R01.4 A08.3", valid.every(Boolean), `Compared row-order positions to independent literal and generated decks after visible reveal. ${runs.join(", ")}.`);
}

export async function boardControls(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), summary = page.locator("summary").filter({ hasText: new RegExp(`^${Labels.reproduce}$`) });
  put("C04.1 C04.2 C04.3", await summary.count() === 1 && await summary.evaluate((element) => element.parentElement?.tagName === "DETAILS" && !element.parentElement.hasAttribute("open")), "Reproduction controls start in a collapsed native details with the exact summary.");
  await reproduction(page);
  const code = page.getByRole("textbox", { name: Labels.code, exact: true }), snapshot = page.getByRole("textbox", { name: Labels.snapshot, exact: true });
  put("C05.1 C05.2", await code.evaluate((element) => element instanceof HTMLInputElement && element.maxLength === 32), "Native Board code input has maxLength 32.");
  put("C07.1 C07.2", await snapshot.evaluate((element) => element instanceof HTMLTextAreaElement && element.maxLength === Rules.snapshotBytes), "Snapshot JSON is a native textarea with the stated character limit.");
  for (const [id, label] of [["C06.1", Labels.loadBoard], ["C08.1", Labels.export], ["C09.1", Labels.loadSnapshot]] as const) put(id, await page.getByRole("button", { name: label, exact: true }).count() === 1, `Native button named ${label}.`);
  await loadBoard(page, KnownCode); const initial = await exported(page);
  put("C06.2", initial !== null && snapshotsEqual(initial, freshSnapshot(KnownCode)), "A valid code replaces the game with its hidden starting state.");
  await loadBoard(page, ` ${KnownCode} `);
  const trim = await exported(page);
  put("D01.4", trim !== null && snapshotsEqual(trim, freshSnapshot(KnownCode)), "Whitespace is either trimmed to the known deal or rejected without changing that deal.");
  await code.fill("edited draft"); await click(page, 0);
  put("C05.4", await code.inputValue() === "edited draft", "A card action keeps the edited board-code draft.");
  await reset(page); const restart = await code.inputValue(); await reset(page, true); const fresh = await code.inputValue(); await changeSize(page, 6); const changed = await code.inputValue();
  put("C05.3 R09.5 C02.3", restart === KnownCode && Boolean(parseBoardCode(fresh)) && fresh !== restart && changed !== fresh && parseBoardCode(changed)?.size === 6, "Restart shows the same code. New Game and size change show fresh valid codes.");
  put("R09.6 C03.3", restart === KnownCode, "Restart retains the exact board code.");
  put("C01.3", await page.getByRole("combobox", { name: Labels.size }).locator("option").evaluateAll((elements) => elements.every((element) => element instanceof HTMLOptionElement && element.value === String(Number(element.textContent?.match(/\d+/)?.[0])))), "Grid option values are decimal side lengths.");
  const progress: Snapshot = { ...freshSnapshot(KnownCode), moves: 3, elapsedMs: 1200, started: true, selection: { kind: SelectionKind.one, index: 0 } };
  await importText(page, JSON.stringify(progress));
  const beforeNames = JSON.stringify(await names(page));
  const before = await exported(page); const text = await snapshot.inputValue();
  put("C08.2", before !== null && text.includes("\n") && text.includes('"version": 1'), "Export writes readable version 1 JSON.");
  await snapshot.focus(); await snapshot.press("ControlOrMeta+A");
  put("C08.3 C08.5", await snapshot.evaluate((element) => element instanceof HTMLTextAreaElement && !element.disabled && element.selectionStart === 0 && element.selectionEnd === element.value.length), "Native keyboard selection covers the entire exported JSON with the Clipboard API unavailable.");
  const again = await exported(page);
  put("C08.4", before !== null && again !== null && snapshotsEqual(before, progress) && snapshotsEqual(before, again) && JSON.stringify(await names(page)) === beforeNames, "Export leaves nonzero progress, revealed cards and the paused active clock unchanged.");
  put("C14.2", await page.getByRole("button", { name: /replay/i }).count() === 0 && await page.getByRole("textbox", { name: /replay/i }).count() === 0, "The shipped controls contain no replay UI.");
}

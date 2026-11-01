import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { observeAnnouncements } from "./announcements.ts";
import { markVisibleCards } from "./browser-cards.ts";
import { body, changeSize, Labels, metric, names, step } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { Evidence } from "./outcomes.ts";
import { Announcement } from "./published-text.ts";
import { cardName, faceIn } from "./view-values.ts";

export const TreeClause = { journey: "I17.1", controls: "I17.2", states: "I17.3", win: "I17.4", restart: "I17.5", versions: "I17.6" } as const;
export function toolRecord(text: string): { readonly passed: boolean; readonly detail: string } {
  const browser = /\b(?:Chromium|Chrome|Firefox|WebKit|Edge|Safari|Brave)\s+v?(\d+(?:\.\d+)*)/i.exec(text)?.[0] ?? /\bbrowser(?: version)?\s*:\s*[^\n]+?\b\d+(?:\.\d+)*/i.exec(text)?.[0];
  const tool = /\b(?:Playwright|Puppeteer|Selenium|WebDriver(?:IO)?|Cypress|DevTools|CDP|axe(?:-core)?)\s+v?(\d+(?:\.\d+)*)/i.exec(text)?.[0] ?? /\b(?:tool|tool version|tree tool)\s*:\s*[^\n]+?\b\d+(?:\.\d+)*/i.exec(text)?.[0];
  const unused = /\bscreen reader\s*:\s*(?:not run|not used|none)\b|\bno (?:actual )?screen reader (?:was )?(?:run|used)\b/i.exec(text)?.[0];
  const used = /\bscreen reader(?: used)?\s*:\s*(?:used[\s,:]+)?(?:NVDA|JAWS|VoiceOver|Narrator|Orca|TalkBack|ChromeVox)\s+v?(\d+(?:\.\d+)*)/i.exec(text)?.[0] ?? /\bscreen reader\s*:\s*used[\s,:]+[a-z][\w .-]+?\s+v?\d+(?:\.\d+)*/i.exec(text)?.[0];
  return { passed: Boolean(browser && tool && (unused || used)), detail: [browser, tool, unused ?? used].filter(Boolean).join(". ") || "No browser, tool and screen-reader-use record in the project README." };
}

async function position(page: Page, index: number): Promise<void> {
  await page.keyboard.press("Control+Home");
  for (let row = 0; row < Math.floor(index / 4); row++) await page.keyboard.press("ArrowDown");
  for (let column = 0; column < index % 4; column++) await page.keyboard.press("ArrowRight");
}

export async function keyboardTree(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), fresh = page.getByRole("button", { name: Labels.fresh, exact: true });
  await changeSize(page, 4); await fresh.focus(); await page.keyboard.press("Enter"); await markVisibleCards(page);
  const tree = () => page.locator("body").ariaSnapshot();
  const initial = await tree();
  put(TreeClause.controls, await page.getByRole("combobox", { name: Labels.size, exact: true }).count() === 1 && await fresh.count() === 1 && initial.includes(Labels.size) && initial.includes(Labels.fresh), `Named controls in the accessibility tree:\n${initial}`);
  const closed = await names(page);
  let entered = false;
  for (let stop = 0; stop < 32; stop++) {
    await page.keyboard.press("Tab");
    entered = await page.evaluate((attribute) => document.activeElement?.hasAttribute(attribute) === true, CardAttribute);
    if (entered) break;
  }
  const deck: string[] = [], reveals: string[] = [];
  for (let index = 0; index < 16; index++) {
    await position(page, index); await page.keyboard.press(index % 2 ? "Space" : "Enter");
    const snapshot = await card(page, index).ariaSnapshot(), name = cardName(snapshot) ?? "";
    deck.push(faceIn(name) ?? ""); reveals.push(snapshot);
    if (index % 2) await step(page, 800);
  }
  // Restart keeps the deal learned entirely through keyboard reveals.
  await page.getByRole("button", { name: Labels.restart, exact: true }).focus(); await page.keyboard.press("Enter"); await markVisibleCards(page);
  await page.keyboard.press("Tab");
  const groups = new Map<string, number[]>();
  deck.forEach((face, index) => groups.set(face, [...(groups.get(face) ?? []), index]));
  const messages = await observeAnnouncements(page), results: boolean[] = [];
  let pairs = 0;
  for (const positions of groups.values()) {
    if (positions.length !== 2) { results.push(false); continue; }
    for (const index of positions) { await position(page, index); await page.keyboard.press("Enter"); }
    pairs++;
    const entries = await messages();
    results.push(entries.length === pairs && entries.at(-1) === (pairs === 8 ? Announcement.win(8, 0) : Announcement.match(pairs)));
  }
  const wonTree = await tree(), completed = metric(await body(page), "pairs") === 8 && metric(await body(page), "moves") === 8;
  const exposed = closed.length === 16 && closed.every((name, index) => new RegExp(`Row ${Math.floor(index / 4) + 1}, column ${index % 4 + 1}, closed`, "i").test(name)) && deck.every(Boolean) && reveals.every((snapshot) => /revealed|matched/.test(snapshot)) && results.length === 8 && results.every(Boolean);
  put(TreeClause.states, exposed, `Closed positions and revealed states from the accessibility tree:\n${closed.join("\n")}\n${reveals.join("\n")}\nTurn payloads: ${JSON.stringify(await messages())}`);
  put(TreeClause.win, completed && wonTree.includes(Announcement.win(8, 0)), `Completion, moves and time in the accessibility tree:\n${wonTree}`);
  put(TreeClause.journey, entered && exposed && completed, `Keyboard-only discovery and completion. Initial tree:\n${initial}\nCompleted tree:\n${wonTree}`);
  await page.keyboard.press("Tab");
  const left = await page.evaluate((attribute) => !document.activeElement?.hasAttribute(attribute), CardAttribute);
  await fresh.focus(); await page.keyboard.press("Enter"); await markVisibleCards(page);
  put(TreeClause.restart, left && (await names(page)).every((name) => /closed/i.test(name)) && metric(await body(page), "moves") === 0, `Tab leaves the board and keyboard New Game starts closed:\n${await tree()}`);
}

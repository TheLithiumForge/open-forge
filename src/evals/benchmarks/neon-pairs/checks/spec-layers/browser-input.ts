import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { markVisibleCards as markCards } from "./browser-cards.ts";
import { Evidence } from "./outcomes.ts";
import { body, click, Labels, metric, names, reset, solve, step, visibleDeck } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { faceIn } from "./view-values.ts";
import { heldMenu } from "./browser-touch.ts";
import { Announcement, AnnouncementClause } from "./published-text.ts";
import { LiveRegion, observeAnnouncements } from "./announcements.ts";

export async function navigation(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence);
  const initialFocus = await page.evaluate(() => document.activeElement?.tagName);
  put("I11.1", initialFocus === "BODY" || initialFocus === "HTML", "Initial load keeps document focus instead of focusing a control or card.");
  const stops = async (): Promise<number[]> => page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.flatMap((element, index) => element.getAttribute("tabindex") === "0" ? [index] : []));
  put("I05.1 I05.3", JSON.stringify(await stops()) === "[0]", "Initially only the first card is a board tab stop.");
  const target = async (start: number, key: string, expected: number): Promise<boolean> => {
    await card(page, start).focus(); await page.keyboard.press(key);
    return card(page, expected).evaluate((element) => element === document.activeElement);
  };
  for (const [id, key, start, expected] of [
    ["I06.1", "ArrowLeft", 5, 4], ["I06.2", "ArrowRight", 5, 6], ["I06.3", "ArrowUp", 5, 1], ["I06.4", "ArrowDown", 5, 9],
    ["I06.5", "Home", 6, 4], ["I06.6", "End", 5, 7], ["I06.7", "Control+Home", 6, 0], ["I06.8", "Control+End", 5, 15],
  ] as const) put(id, await target(start, key, expected), `${key} from ${start} must focus ${expected}.`);
  const edges = await target(0, "ArrowLeft", 0) && await target(0, "ArrowUp", 0) && await target(3, "ArrowRight", 3) && await target(15, "ArrowDown", 15);
  put("I07.1", edges, "All four edges stop arrow movement without wrapping.");
  put("I06.9", (await names(page)).every((name) => !faceIn(name)) && metric(await body(page), "moves") === 0, "All navigation leaves every card closed at zero moves.");
  await card(page, 5).focus(); await page.keyboard.press("Tab");
  const leftForward = await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => !elements.some((element) => element === document.activeElement));
  await page.keyboard.press("Shift+Tab");
  put("I05.2", await card(page, 5).evaluate((element) => element === document.activeElement), "Shift+Tab reenters at the remembered card.");
  await page.keyboard.press("Shift+Tab");
  put("I08.1 I08.2", leftForward && await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => !elements.some((element) => element === document.activeElement)) && (await stops()).length === 1, "Tab and Shift+Tab leave the board with one remembered stop.");
  const focusedControls: boolean[] = [], resetStops: boolean[] = [];
  for (const label of [Labels.fresh, Labels.restart]) {
    await card(page, 7).focus(); await page.getByRole("button", { name: label, exact: true }).click(); await markCards(page);
    focusedControls.push(await page.getByRole("button", { name: label, exact: true }).evaluate((element) => element === document.activeElement));
    resetStops.push(JSON.stringify(await stops()) === "[0]");
  }
  put("C13.1 C13.3", focusedControls.every(Boolean), "New Game and Restart retain button focus.");
  put("C13.5 I12.1", resetStops.every(Boolean), "Both deal changes reset the remembered card to zero.");
}

export async function activation(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence);
  put("I01.1", await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.every((element) => element.tagName === "BUTTON")), "Every discovered card is a native button.");
  const results: string[] = [];
  for (const action of [async () => click(page, 0), async () => card(page, 0).tap(), async () => card(page, 0).press("Enter"), async () => card(page, 0).press("Space")]) {
    await reset(page); await action(); results.push(JSON.stringify(await names(page)));
  }
  put("I01.2", new Set(results).size === 1 && results[0]?.includes("revealed") === true, "Pointer, completed tap, Enter and Space produce the same revealed board.");
  let repeatSafe = true;
  for (const key of ["Enter", " "]) {
    await reset(page); await card(page, 0).focus();
    const allowed = await card(page, 0).evaluate((element, key) => element.dispatchEvent(new KeyboardEvent("keydown", { key, code: key === "Enter" ? key : "Space", repeat: true, bubbles: true, cancelable: true })), key);
    repeatSafe &&= !allowed && (await names(page)).every((name) => !faceIn(name));
  }
  put("I02.1", repeatSafe, "Repeated Enter and Space keydown events leave a closed board untouched.");
  await reset(page); const before = JSON.stringify(await names(page));
  const menuAllowed = await card(page, 0).evaluate((element) => element.dispatchEvent(new MouseEvent("contextmenu", { bubbles: true, cancelable: true, button: 2 })));
  await card(page, 0).click({ button: "right" });
  put("I03.2", menuAllowed && JSON.stringify(await names(page)) === before, "Context menu is not cancelled and a right click does not reveal a card.");
  const heldAllowed = await heldMenu(page);
  put("I03.1", menuAllowed && heldAllowed && JSON.stringify(await names(page)) === before, "Right-click and held native touch keep contextmenu uncancelled and don't activate a card. The operating-system menu itself isn't captured.");
  await card(page, 0).hover();
  put("I04.1", JSON.stringify(await names(page)) === before, "Hover changes no accessible game state.");
  await card(page, 0).dispatchEvent("pointerdown", { pointerId: 1, pointerType: "touch", isPrimary: true });
  await card(page, 0).dispatchEvent("pointercancel", { pointerId: 1, pointerType: "touch", isPrimary: true });
  put("I04.2", JSON.stringify(await names(page)) === before, "Cancelled touch pointer leaves game state unchanged.");
}

export async function controlNames(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), select = page.getByRole("combobox", { name: Labels.size, exact: true });
  put("C01.1", await select.evaluate((element) => element.tagName === "SELECT"), "Grid size is the name of a native select.");
  const options = await select.locator("option").allTextContents();
  put("C01.2", options.length === 7 && [4, 6, 8, 10, 12, 14, 16].every((size) => options.some((option) => new RegExp(`^${size}[x×]${size}$`).test(option.replaceAll(" ", "")))), `Size labels: ${options.join(", ")}.`);
  for (const [id, label] of [["C02.1", Labels.fresh], ["C03.1", Labels.restart]] as const) put(id, await page.getByRole("button", { name: label, exact: true }).count() === 1, `Native button named ${label}.`);
}

export async function announcements(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), region = page.locator(LiveRegion);
  put("I16.1", await region.count() === 1, "Exactly one polite atomic live region.");
  const deck = await visibleDeck(page); await reset(page);
  const entries = await observeAnnouncements(page);
  const count = async (): Promise<number> => (await entries()).length;
  const wrong = deck.findIndex((face) => face !== deck[0]);
  await click(page, 0); const first = await count(); await click(page, wrong); const one = await count();
  await step(page, 800); await card(page, 2).focus(); await page.keyboard.press("ArrowRight"); await step(page, 3000);
  put("I16.2 I16.5", first === 0 && one === 1 && await count() === one, "One result announcement after a turn, none for focus, clock or mismatch closing.");
  await click(page, 0); await click(page, wrong);
  put("I16.4", await count() === 2, "Two consecutive misses each cause their own live-region update.");
  put(AnnouncementClause.miss, (await entries()).length === 2 && (await entries()).every((text) => text === Announcement.miss), "Both misses use the published wording exactly.");
  await step(page, 800); await reset(page); const before = await count(); await solve(page, deck);
  const final = await region.innerText();
  put("I16.3", await count() - before === 8 && /(?:match|pair)/i.test(final) && /(?:win|won|complete|every pair|all pairs)/i.test(final), "Eight matches produce eight announcements. The last announces the match and victory together.");
  const matches = (await entries()).slice(before);
  put(AnnouncementClause.matches, matches.slice(0, -1).length === 7 && matches.slice(0, -1).every((text, index) => text === Announcement.match(index + 1)), "The first and later matches use the singular and plural published wording.");
  put(AnnouncementClause.win, final === Announcement.win(8, 0), "The final match announces eight moves and the stopped clock in one published message.");
}

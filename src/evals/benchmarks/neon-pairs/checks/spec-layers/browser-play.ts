import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { markVisibleCards as markCards } from "./browser-cards.ts";
import { Evidence } from "./outcomes.ts";
import { Rules } from "./rules.ts";
import { body, changeSize, click, hidden, metric, names, reset, solve, step, visibleDeck } from "./browser-session.ts";
import { faceIn } from "./view-values.ts";

export function reporter(evidence: Evidence) {
  return (ids: string, passed: boolean, detail: string): void => {
    for (const id of ids.split(" ")) evidence.test(id, passed, detail);
  };
}
export async function sizes(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), counts: number[] = [], paired: boolean[] = [], squares: boolean[] = [];
  for (const size of Rules.sizes) {
    await changeSize(page, size);
    counts.push(await markCards(page));
    const deck = await visibleDeck(page);
    paired.push(new Set(deck).size === size * size / 2 && deck.every((face) => deck.filter((other) => other === face).length === 2));
    squares.push(await page.locator(`[${CardAttribute}]`).evaluateAll((elements, side) => {
      const boxes = elements.map((element) => element.getBoundingClientRect());
      const xs = new Set(boxes.map((box) => Math.round(box.x))), ys = new Set(boxes.map((box) => Math.round(box.y)));
      return xs.size === side && ys.size === side && boxes.every((box) => Math.abs(box.width - box.height) < 1);
    }, size));
  }
  put("R01.1", counts.every((count, index) => count === (Rules.sizes[index] ?? 0) ** 2), `Visible card counts: ${counts.join(", ")}.`);
  put("R01.2", squares.every(Boolean), "Measured square cards in equal row and column counts at every supported side length.");
  put("R01.3", paired.every(Boolean), "Revealed every position through play and counted exactly two of each named face at every size.");
}

export async function turns(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), deck = await visibleDeck(page);
  await reset(page);
  await click(page, 0);
  const first = await names(page), afterFirst = await body(page);
  await click(page, 0);
  put("R02.1 R05.2", Boolean(faceIn(first[0] ?? "")) && metric(afterFirst, "moves") === 0, "First reveal shows a face with zero moves.");
  put("R02.2", JSON.stringify(await names(page)) === JSON.stringify(first) && metric(await body(page), "moves") === 0, "Repeated first activation keeps names and moves.");
  const partner = deck.findIndex((face, index) => index !== 0 && face === deck[0]);
  await click(page, partner);
  const match = await names(page), matchedText = await body(page);
  put("R03.1 R03.2", faceIn(match[partner] ?? "") === deck[0] && /matched/i.test(match[0] ?? ""), "Second reveal retains the matching faces and names their matched state.");
  put("R03.3 R06.1 R06.2", metric(matchedText, "pairs") === 1 && /1\s*\/\s*8/.test(matchedText), "Pair counter changes immediately to 1 / 8.");
  await click(page, 0);
  put("R02.3", JSON.stringify(await names(page)) === JSON.stringify(match) && metric(await body(page), "moves") === 1, "Activating a matched card keeps state.");
  const a = deck.findIndex((_, index) => index !== 0 && index !== partner);
  const b = deck.findIndex((face, index) => index !== a && index !== 0 && index !== partner && face !== deck[a]);
  await click(page, a); await click(page, b);
  const miss = await names(page);
  put("R05.1 R05.4", metric(await body(page), "moves") === 2, "One match and one mismatch cost two moves.");
  await click(page, deck.findIndex((_, index) => ![0, partner, a, b].includes(index)));
  put("R04.3 R05.3 I10.2", JSON.stringify(await names(page)) === JSON.stringify(miss) && metric(await body(page), "moves") === 2, "An activation during mismatch changes neither selection nor moves.");
  await card(page, a).focus(); await page.keyboard.press("ArrowRight");
  put("I10.1", await card(page, a).evaluate((element) => element !== document.activeElement), "Arrow focus moves during mismatch.");
  await step(page, 799);
  put("R04.1", faceIn((await names(page))[a] ?? "") !== null, "Mismatch stays revealed through 799 controlled active milliseconds.");
  await hidden(page, true); await step(page, 5000);
  put("R04.4", faceIn((await names(page))[a] ?? "") !== null, "Synthetic browser visibility hides the page for 5000 controlled milliseconds without closing the mismatch.");
  await hidden(page, false); await step(page, 1);
  const closed = await names(page);
  put("R04.2 R07.8", !faceIn(closed[a] ?? "") && !faceIn(closed[b] ?? ""), "Both mismatching cards close at the remaining active millisecond.");
  await card(page, 0).focus();
  put("I09.1 I09.2 I09.3", await card(page, 0).getAttribute("aria-disabled") === "true" && await card(page, 0).evaluate((element) => element === document.activeElement), "A matched card remains focusable with aria-disabled=true and ignored activation.");
  await reset(page); await solve(page, deck);
  const won = await body(page);
  put("R06.3 V18.1", metric(won, "pairs") === 8 && /(?:win|won|every pair|complete)/i.test(won), "All eight pairs produce a win summary.");
  const summary = await page.locator("body").evaluate((element) => Array.from(element.querySelectorAll("p,section,aside,[role=status]")).map((node) => node.textContent ?? "").filter((text) => /(?:win|won|every pair|complete)/i.test(text)).join(" "));
  put("R06.4", /\b8\b/.test(summary), "The completion summary must contain the final eight moves.");
  put("R06.5", /(?:\d+\s*s\b|\d+:\d{2}|\d+\s*ms\b)/i.test(summary), "The completion summary must include final elapsed time.");
}

export async function clock(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence);
  await step(page, 3000);
  const before = metric(await body(page), "time");
  await click(page, 0); await step(page, 2300);
  const active = metric(await body(page), "time");
  put("R07.1 R07.2", before === 0 && active !== null && active >= 2 && active <= 3, `Clock before reveal=${before}, after 2300 active milliseconds=${active}.`);
  await hidden(page, true); await step(page, 9000);
  put("R07.3", metric(await body(page), "time") === active, "Hidden controlled time does not advance the displayed clock.");
  await hidden(page, false); await page.clock.fastForward(5000);
  const skipped = metric(await body(page), "time");
  put("R07.6", skipped !== null && active !== null && skipped - active === 5, "Skipping render callbacks still accounts for 5000 active milliseconds.");
  await reset(page); const deck = await visibleDeck(page); await reset(page); await solve(page, deck);
  const stopped = metric(await body(page), "time"); await step(page, 5000);
  put("R07.5", metric(await body(page), "time") === stopped, "Clock stays fixed after completion.");
  put("R07.7", evidence.outcomes.filter((item) => item.clause.startsWith("R07.")).every((item) => item.status === "pass"), "Clock uses the same controlled active-time advances as the mismatch journey.");
}

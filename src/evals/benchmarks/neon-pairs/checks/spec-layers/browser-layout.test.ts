import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { test } from "node:test";
import { chromium, type Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { markVisibleCards } from "./browser-cards.ts";
import { ContentMaximum, cssGridBoard, FocusSamples, focusedOutline, focusSamples, GameTitle, LayoutAttribute, OutlineClauses } from "./browser-layout.ts";
import { scroll } from "./browser-access.ts";
import { layout, spacingOnScale, styles } from "./browser-view.ts";
import { readableLayout, zoom, ZoomClause, ZoomViewport } from "./browser-zoom.ts";
import { Labels } from "./browser-session.ts";
import { Evidence } from "./outcomes.ts";
import { Rules } from "./rules.ts";

async function fakeGame(page: Page): Promise<void> {
  await page.clock.install();
  await page.setContent(`<style>
    * { box-sizing: border-box } body { margin: 0; font: 16px/24px system-ui }
    p { margin: 0 } article { max-width: ${ContentMaximum}px; margin: 0 auto; padding: 16px }
    .title { font-size: 24px; line-height: 32px } .controls { display: flex; gap: 6px; flex-wrap: wrap }
    .viewport { max-height: min(70dvh, 768px); max-width: 100%; width: fit-content; padding: 6px; border: 1px solid; overflow: auto }
    .board { display: flex; flex-wrap: wrap; gap: 6px }
    .card { flex: 0 0 44px; height: 44px; padding: 0; border: 2px solid; border-radius: 8px; scroll-margin: 6px }
    .card:focus { outline: 3px solid white; outline-offset: 2px }
  </style><main hidden><h1>${GameTitle}</h1></main><article><section>
    <div class="title"><span>${GameTitle}</span></div>
    <div class="controls"><label>${Labels.size}<select><option value="4">4x4</option><option value="16">16x16</option></select></label><button>${Labels.fresh}</button><button>${Labels.restart}</button></div>
    <p id="status"></p><p id="result"></p><div class="viewport"><div class="board"></div></div>
    <input aria-label="Unstyled native control"><p id="secondary" style="font-size:14px;margin:0">Secondary text</p>
  </section></article>`);
  await page.evaluate(({ colors, sizeLabel }) => {
    const board = document.querySelector<HTMLElement>(".board"), select = document.querySelector("select");
    if (!board || !select) throw new Error("Missing fake controls.");
    select.setAttribute("aria-label", sizeLabel);
    let size = 4, moves = 0, first: number | null = null;
    const matched = new Set<number>();
    const label = (index: number, state: string): string => `Row ${Math.floor(index / size) + 1}, column ${index % size + 1}, ${state}`;
    const face = (index: number): string => `${colors[Math.floor(index / 2) % colors.length]} solid circle, marker top`;
    const status = (): void => {
      const status = document.querySelector("#status"), result = document.querySelector("#result");
      if (status) status.textContent = `Moves: ${moves} Pairs: ${matched.size / 2}/${size * size / 2} Time: 0:00 Best: none`;
      if (result) result.textContent = matched.size === size * size ? `You won in ${moves} moves. Time: 0:00` : "";
    };
    const start = (): void => {
      size = Number(select.value); moves = 0; first = null; matched.clear(); board.replaceChildren();
      board.style.width = `${size * 50 - 6}px`;
      for (let index = 0; index < size * size; index++) {
        const button = document.createElement("button"); button.className = "card"; button.tabIndex = index === 0 ? 0 : -1;
        button.setAttribute("aria-label", label(index, "closed"));
        button.onfocus = () => { for (const sibling of board.querySelectorAll("button")) sibling.tabIndex = sibling === button ? 0 : -1; button.scrollIntoView({ block: "nearest", inline: "nearest" }); };
        button.onclick = () => {
          if (matched.has(index) || first === index) return;
          button.setAttribute("aria-label", label(index, `${face(index)}, revealed`));
          if (first === null) first = index;
          else {
            moves++;
            if (face(first) === face(index)) for (const position of [first, index]) {
              matched.add(position); board.children[position]?.setAttribute("aria-label", label(position, `${face(position)}, matched`));
            }
            first = null;
          }
          status();
        };
        button.onkeydown = (event) => {
          const next = event.ctrlKey && event.key === "End" ? size * size - 1 : event.ctrlKey && event.key === "Home" ? 0 : event.key === "ArrowRight" ? index + 1 : event.key === "ArrowLeft" ? index - 1 : event.key === "ArrowDown" ? index + size : -1;
          if (next >= 0 && next < size * size) { event.preventDefault(); (board.children[next] as HTMLElement).focus(); }
        };
        board.append(button);
      }
      status();
    };
    select.onchange = start;
    for (const button of document.querySelectorAll<HTMLButtonElement>(".controls button")) button.onclick = start;
    start();
  }, { colors: Rules.colors, sizeLabel: Labels.size });
  await markVisibleCards(page);
}

test("layout discovery accepts named cards in flex containers without main or h1", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage(); await fakeGame(page);
    const measured = await styles(page);
    assert.equal(measured.grid["display"], "flex");
    assert.equal(measured.main["maxWidth"], `${ContentMaximum}px`);
    assert.equal(measured.title["fontSize"], "24px");
    assert.equal(await page.locator(`[${LayoutAttribute}="board"] > button`).count(), Rules.sizes[0] ** 2);
    const evidence = new Evidence(); await layout(page, evidence);
    assert.ok(evidence.outcomes.every((item) => item.status === "pass"), JSON.stringify(evidence.outcomes));
    await page.addStyleTag({ content: "body { display: grid }" });
    assert.equal(await cssGridBoard(page), false, "Grid elsewhere doesn't satisfy the board-only clause.");
    await page.addStyleTag({ content: ".board { display: grid; grid-template-columns: repeat(16, 44px) }" });
    assert.equal(await cssGridBoard(page), true);
    // Extra similar-sized unnamed controls can't replace the required named cards.
    await page.evaluate(() => { for (let index = 0; index < 300; index++) document.body.append(document.createElement("button")); });
    assert.equal(await markVisibleCards(page), Rules.sizes.at(-1)! ** 2);
  } finally { await browser.close(); }
});

test("spacing permits resets and keywords, excludes native internals and checks authored control spacing", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage(); await fakeGame(page);
    assert.equal(spacingOnScale(["0px", "auto", "normal", "-3px", ...Rules.spacing.map((value) => `${value}px`)]), true);
    assert.equal(spacingOnScale(["0.5px"]), false);
    assert.equal(spacingOnScale((await styles(page)).spacing), true);
    await page.locator("#secondary").evaluate((element) => (element as HTMLElement).style.margin = "");
    assert.equal(spacingOnScale((await styles(page)).spacing), true, "The authored p reset removes the browser margin.");
    await page.addStyleTag({ content: ".controls select { padding: 3px }" });
    assert.equal(spacingOnScale((await styles(page)).spacing), false);
    await page.addStyleTag({ content: ".controls select { padding: 0 } #secondary { margin: 14px }" });
    assert.equal(spacingOnScale((await styles(page)).spacing), false);
    await page.addStyleTag({ content: "#secondary { margin: 0 } .title span { margin-left: 3px }" });
    assert.equal(spacingOnScale((await styles(page)).spacing), false);
  } finally { await browser.close(); }
});

test("focus checks keep six-pixel scroll margins and detect clipped interior outlines", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    for (const width of [360, 1280]) {
      const page = await browser.newPage({ viewport: { width, height: 800 } }); await fakeGame(page);
      await page.getByRole("combobox", { name: Labels.size }).selectOption("16"); await markVisibleCards(page);
      assert.equal(await focusSamples(page), true, `All outline samples at ${width}.`);
      await page.addStyleTag({ content: ".card { scroll-margin: 0 }" });
      // Board-edge padding protects the corners, so the regression must visit the interior.
      for (const index of FocusSamples.slice(0, 2)) { await card(page, index).focus(); assert.equal(await focusedOutline(page, index), true); }
      assert.equal(await focusSamples(page), false, `Clipped interior outline at ${width}.`);
      await page.close();
    }
  } finally { await browser.close(); }
});

test("scroll clauses pass protected outlines and fail clipped mid-board outlines", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage({ hasTouch: true }); await fakeGame(page);
    const protectedBoard = new Evidence(); await scroll(page, protectedBoard);
    for (const clause of OutlineClauses) assert.equal(protectedBoard.outcomes.find((item) => item.clause === clause)?.status, "pass");
    await page.addStyleTag({ content: ".card { scroll-margin: 0 }" });
    const clipped = new Evidence(); await scroll(page, clipped);
    for (const clause of OutlineClauses) assert.equal(clipped.outcomes.find((item) => item.clause === clause)?.status, "fail");
  } finally { await browser.close(); }
});

test("zoom clauses execute at the equivalent viewport and reject broken layout, input and outlines", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const context = await browser.newContext({ viewport: ZoomViewport, deviceScaleFactor: ZoomViewport.deviceScaleFactor });
    const page = await context.newPage(); await fakeGame(page);
    const evidence = new Evidence(); await zoom(page, evidence);
    for (const clause of Object.values(ZoomClause)) assert.equal(evidence.outcomes.find((item) => item.clause === clause)?.status, "pass", JSON.stringify(evidence.outcomes));
    await page.addStyleTag({ content: "#status, #result { position: absolute; top: 0 }" });
    assert.equal(await readableLayout(page), false);
    await page.addStyleTag({ content: "article { min-width: 900px } .card { scroll-margin: 0 }" });
    await page.evaluate((attribute) => { document.addEventListener("click", (event) => { if ((event.target as Element).hasAttribute(attribute)) event.stopImmediatePropagation(); }, true); }, CardAttribute);
    const broken = new Evidence(); await zoom(page, broken);
    for (const clause of [ZoomClause.readable, ZoomClause.pageWidth, ZoomClause.play, ZoomClause.outline]) assert.equal(broken.outcomes.find((item) => item.clause === clause)?.status, "fail", clause);
    await context.close();
  } finally { await browser.close(); }
});

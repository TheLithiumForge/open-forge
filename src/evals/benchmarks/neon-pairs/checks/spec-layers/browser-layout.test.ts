import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { test } from "node:test";
import { chromium } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { markVisibleCards } from "./browser-cards.ts";
import { ContentMaximum, cssGridBoard, FocusSamples, focusedOutline, focusSamples, LayoutAttribute, OutlineClauses, ScrollMarginClause } from "./browser-layout.ts";
import { fakeGame } from "./browser-game.fixture.ts";
import { scroll } from "./browser-access.ts";
import { layout, spacingOnScale, styles } from "./browser-view.ts";
import { readableLayout, zoom, ZoomClause, ZoomViewport } from "./browser-zoom.ts";
import { Labels } from "./browser-session.ts";
import { Evidence } from "./outcomes.ts";
import { Rules } from "./rules.ts";


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
    for (const clause of [...OutlineClauses, ScrollMarginClause]) assert.equal(protectedBoard.outcomes.find((item) => item.clause === clause)?.status, "pass");
    await page.addStyleTag({ content: ".card { scroll-margin: 0 }" });
    const clipped = new Evidence(); await scroll(page, clipped);
    for (const clause of [...OutlineClauses, ScrollMarginClause]) assert.equal(clipped.outcomes.find((item) => item.clause === clause)?.status, "fail");
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

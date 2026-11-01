import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { test } from "node:test";
import { chromium } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { fakeGame } from "./browser-game.fixture.ts";
import { activation, BrowserKeyClause, BrowserKeys, navigation } from "./browser-input.ts";
import { ExportToleranceMs, wholeMilliseconds } from "./browser-rounding.ts";
import { changeSize, moveFocus, names } from "./browser-session.ts";
import { Evidence } from "./outcomes.ts";

test("whole-millisecond exports allow either nearby integer and keep pending time positive", () => {
  for (const expected of [1.25, 1.5, 799.75, 801]) {
    assert.equal(wholeMilliseconds(Math.floor(expected), expected), true);
    assert.equal(wholeMilliseconds(Math.ceil(expected), expected), true);
    assert.equal(wholeMilliseconds(Math.round(expected), expected), true);
    assert.equal(wholeMilliseconds(Math.ceil(expected) + ExportToleranceMs + 1, expected), false);
  }
  assert.equal(wholeMilliseconds(1, 0.25, 1), true);
  assert.equal(wholeMilliseconds(0, 0.25, 1), false);
  assert.equal(wholeMilliseconds(2, 0.25, 1), false);
  for (const value of [1.25, NaN, Infinity, -1, null, undefined, "1"]) assert.equal(wholeMilliseconds(value, 1), false);
});

test("navigation leaves browser keys alone and detects legacy Home or modified-arrow handlers", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    for (const fault of [null, BrowserKeys[0], BrowserKeys[3]]) {
      const page = await browser.newPage(); await fakeGame(page);
      if (fault) await page.evaluate(({ fault, attribute }) => document.addEventListener("keydown", (event) => {
        if (fault === "Home" ? event.key === "Home" : event.altKey && event.key === "ArrowLeft") {
          event.preventDefault(); document.querySelector<HTMLButtonElement>(`[${attribute}="0"]`)?.focus();
        }
      }), { fault, attribute: CardAttribute });
      const evidence = new Evidence(); await navigation(page, evidence);
      assert.equal(evidence.outcomes.find((outcome) => outcome.clause === BrowserKeyClause)?.status, fault ? "fail" : "pass", JSON.stringify(evidence.outcomes));
      if (!fault) assert.ok(evidence.outcomes.every((outcome) => outcome.status === "pass"), JSON.stringify(evidence.outcomes));
      await page.close();
    }
  } finally { await browser.close(); }
});

test("far-card setup needs only plain arrows and leaves every card closed", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage({ hasTouch: true }); await fakeGame(page);
    await changeSize(page, 16);
    await card(page, 0).focus(); await moveFocus(page, 255, 16);
    assert.equal(await card(page, 255).evaluate((element) => element === document.activeElement), true);
    await moveFocus(page, 0, 16);
    assert.equal(await card(page, 0).evaluate((element) => element === document.activeElement), true);
    assert.ok((await names(page)).every((name) => name.includes("closed")));
    await changeSize(page, 4);
    const activated = new Evidence(); await activation(page, activated);
    assert.ok(!activated.seen.has("I02.1"));
  } finally { await browser.close(); }
});

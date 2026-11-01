import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { mkdtemp, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { chromium } from "playwright";
import { session } from "./browser-session.ts";
import { reproductionFixture } from "./reproduction.fixture.ts";
import { removeTemporary } from "./temporary.ts";
import { snapshotRounding, RoundingClause } from "./browser-rounding.ts";
import { Evidence } from "./outcomes.ts";
import { BesideControlsPx, boxGap, ImportClause, importedProgress, loadedFocusAndSize, loadAnnouncements } from "./browser-import.ts";
import { BoardTrimClause, boardControls } from "./browser-formats.ts";
import { AnnouncementClause } from "./published-text.ts";

test("public exports accept whole milliseconds within tolerance and reject stale or zero pending times", async () => {
  const root = await mkdtemp(join(tmpdir(), "eval-rounding-"));
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const html = join(root, "index.html"); await writeFile(html, reproductionFixture());
    const open = await session(browser, html, { fractionalClock: true });
    try {
      const evidence = new Evidence(); await snapshotRounding(open.page, evidence);
      for (const clause of Object.values(RoundingClause)) assert.equal(evidence.outcomes.find((item) => item.clause === clause)?.status, "pass", JSON.stringify(evidence.outcomes));
      await open.page.evaluate(() => {
        const original = JSON.stringify;
        Object.defineProperty(JSON, "stringify", { configurable: true, value: (...args: unknown[]) => (Reflect.apply(original, JSON, args) as string).replace(/"elapsedMs": 1(?=,)/g, '"elapsedMs": 2').replace(/"remainingMs": 800(?=\s*})/g, '"remainingMs": 799') });
      });
      const alternate = new Evidence(); await snapshotRounding(open.page, alternate);
      for (const clause of Object.values(RoundingClause)) assert.equal(alternate.outcomes.find((item) => item.clause === clause)?.status, "pass", JSON.stringify(alternate.outcomes));
      await open.page.evaluate(() => {
        const original = JSON.stringify;
        Object.defineProperty(JSON, "stringify", { configurable: true, value: (...args: unknown[]) => (Reflect.apply(original, JSON, args) as string).replace(/"elapsedMs": 2(?=,)/g, '"elapsedMs": 3').replace(/"remainingMs": 1(?=\s*})/g, '"remainingMs": 0') });
      });
      const broken = new Evidence(); await snapshotRounding(open.page, broken);
      for (const clause of Object.values(RoundingClause)) assert.equal(broken.outcomes.find((item) => item.clause === clause)?.status, "fail", clause);
    } finally { await open.close(); }
  } finally { await browser.close(); await removeTemporary(root, "eval-rounding-"); }
});

test("completed loads have no Resume, unfinished wins record bests and load statuses announce once", async () => {
  const root = await mkdtemp(join(tmpdir(), "eval-loads-"));
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const html = join(root, "index.html"); await writeFile(html, reproductionFixture());
    const open = await session(browser, html);
    try {
      const evidence = new Evidence(); await importedProgress(open.page, evidence);
      for (const clause of Object.values(ImportClause).filter((clause) => clause !== ImportClause.selectedSize)) assert.equal(evidence.outcomes.find((item) => item.clause === clause)?.status, "pass", JSON.stringify(evidence.outcomes));
      await open.page.evaluate(() => document.querySelector("#load-snapshot")!.addEventListener("click", () => {
        const value = JSON.parse(document.querySelector("textarea")!.value);
        if (value.matched.length === 16) { const button = document.createElement("button"); button.id = "resume"; button.textContent = "Resume"; document.querySelector("details")!.append(button); }
      }));
      const pausedComplete = new Evidence(); await importedProgress(open.page, pausedComplete);
      for (const clause of [ImportClause.completedStatus, ImportClause.resume, ImportClause.completed, ImportClause.pause]) assert.equal(pausedComplete.outcomes.find((item) => item.clause === clause)?.status, "fail", clause);
      await open.page.evaluate(() => {
        localStorage.removeItem("best"); const set = Storage.prototype.setItem;
        Storage.prototype.setItem = function (key, value) { if (key !== "best") set.call(this, key, value); };
      });
      const unsaved = new Evidence(); await importedProgress(open.page, unsaved);
      for (const clause of [ImportClause.best, ImportClause.sessionBest]) assert.equal(unsaved.outcomes.find((item) => item.clause === clause)?.status, "fail", clause);
      const loads = new Evidence(); await loadAnnouncements(open.page, loads);
      assert.equal(loads.outcomes.find((item) => item.clause === AnnouncementClause.loads)?.status, "pass");
      await open.page.evaluate(() => { document.querySelector("details")!.after(document.querySelector("#notice")!); });
      const nextTo = new Evidence(); await loadAnnouncements(open.page, nextTo);
      assert.equal(nextTo.outcomes.find((item) => item.clause === AnnouncementClause.loads)?.status, "pass");
      await open.page.evaluate((space) => { const spacer = document.createElement("div"); spacer.style.height = `${space}px`; document.querySelector("details")!.after(spacer); }, BesideControlsPx * 4);
      const far = new Evidence(); await loadAnnouncements(open.page, far);
      assert.equal(far.outcomes.find((item) => item.clause === AnnouncementClause.loads)?.status, "fail");
      await open.page.evaluate(() => {
        const region = document.querySelector("#live")!;
        region.replaceChildren = () => {};
      });
      const silent = new Evidence(); await loadAnnouncements(open.page, silent);
      assert.equal(silent.outcomes.find((item) => item.clause === AnnouncementClause.loads)?.status, "fail");
    } finally { await open.close(); }
  } finally { await browser.close(); await removeTemporary(root, "eval-loads-"); }
});

test("loaded size selection changes immediately instead of only after reload", async () => {
  const root = await mkdtemp(join(tmpdir(), "eval-loaded-size-"));
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const html = join(root, "index.html"); await writeFile(html, reproductionFixture());
    const open = await session(browser, html);
    try {
      const valid = new Evidence(); await loadedFocusAndSize(open.page, valid);
      assert.equal(valid.outcomes.find((item) => item.clause === ImportClause.selectedSize)?.status, "pass");
      await open.page.context().addInitScript(() => document.addEventListener("click", (event) => { if ((event.target as Element).matches("#load-board,#load-snapshot")) queueMicrotask(() => { document.querySelector("select")!.value = "4"; }); }));
      await open.page.reload();
      const stale = new Evidence(); await loadedFocusAndSize(open.page, stale);
      assert.equal(stale.outcomes.find((item) => item.clause === ImportClause.selectedSize)?.status, "fail");
    } finally { await open.close(); }
  } finally { await browser.close(); await removeTemporary(root, "eval-loaded-size-"); }
});

test("the board control must trim whitespace and replace existing progress", async () => {
  const root = await mkdtemp(join(tmpdir(), "eval-trim-"));
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const html = join(root, "index.html"); await writeFile(html, reproductionFixture());
    const open = await session(browser, html);
    try {
      const valid = new Evidence(); await boardControls(open.page, valid);
      assert.equal(valid.outcomes.find((item) => item.clause === BoardTrimClause)?.status, "pass");
      await open.page.evaluate(() => {
        document.querySelector("#load-board")!.addEventListener("click", (event) => { const text = document.querySelector<HTMLInputElement>("#code")!.value; if (text !== text.trim()) event.stopImmediatePropagation(); }, true);
      });
      const rejected = new Evidence(); await boardControls(open.page, rejected);
      assert.equal(rejected.outcomes.find((item) => item.clause === BoardTrimClause)?.status, "fail");
    } finally { await open.close(); }
  } finally { await browser.close(); await removeTemporary(root, "eval-trim-"); }
});

test("box gaps are zero for touching boxes and grow with the empty space between them", () => {
  const panel = { x: 0, y: 0, width: 100, height: 50 };
  assert.equal(boxGap({ x: 20, y: 10, width: 10, height: 10 }, panel), 0);
  assert.equal(boxGap({ x: 0, y: 50, width: 100, height: 20 }, panel), 0);
  assert.equal(boxGap({ x: 0, y: 80, width: 100, height: 20 }, panel), 30);
  assert.equal(boxGap({ x: 103, y: 54, width: 10, height: 10 }, panel), 5);
});

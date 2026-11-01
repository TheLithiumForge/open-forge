import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { test } from "node:test";
import { chromium } from "playwright";
import { Announcement, AnnouncementClause, clockText, hasStatus, StatusClause } from "./published-text.ts";
import { keyboardTree, toolRecord, TreeClause } from "./browser-tree.ts";
import { borderTransition, ColorToken, facePaint, facePalette, TargetClause } from "./browser-targets.ts";
import { Rules, faceName } from "./rules.ts";
import { fakeGame } from "./browser-game.fixture.ts";
import { publishedClock, turns } from "./browser-play.ts";
import { announcements } from "./browser-input.ts";
import { Evidence } from "./outcomes.ts";
import { compareReplay } from "./commands.ts";
import { freshSnapshot } from "./replay.ts";
import { KnownCode } from "./vectors.ts";
import { appearance } from "./browser-view.ts";
import { visibleTimeCap } from "./browser-caps.ts";
import { CapClause } from "./cap-policy.ts";

test("published status grammar keeps labels, fractions, floor rounding and continuing minutes", () => {
  for (const [label, value] of [["Moves", 8], ["Pairs", "1/8"], ["Time", "60:01"], ["Best", "none"]] as const) assert.equal(hasStatus(`${label}:\n${value}`, label, value), true);
  for (const text of ["Moves taken: 8", "moves: 8", "Moves 8", "Best Moves: 8"]) assert.equal(hasStatus(text, "Moves", 8), false);
  assert.equal(hasStatus("Pairs: 1 of 8", "Pairs", "1/8"), false);
  assert.equal(hasStatus("Time: 1:00:01", "Time", "60:01"), false);
  assert.equal(clockText(1999), "0:01"); assert.equal(clockText(3601999), "60:01");
});

test("tree tool records require browser, tool and screen-reader-use evidence", () => {
  const versions = "Chromium 156.0.8078.4. Playwright 1.64.1.";
  assert.equal(toolRecord(`${versions} Screen reader: not run.`).passed, true);
  assert.equal(toolRecord(`${versions} Screen reader: NVDA 2026.3.`).passed, true);
  assert.equal(toolRecord("Safari 26.1. Selenium 4.37. Screen reader: JAWS 2026.").passed, true);
  assert.equal(toolRecord("Browser: Example Browser 3.2\nTool: Custom Tree Capture 1.5\nScreen reader: used Custom Reader 2.0.").passed, true);
  for (const text of [versions, "Screen reader: not run.", `${versions} Screen reader: used.`, `${versions} Install NVDA 2026.3 later.`, "Chromium 156.0.8078.4. Screen reader: not run."]) assert.equal(toolRecord(text).passed, false);
});

test("every face uses marker palette colors and the named fill and stroke targets", () => {
  for (let id = 0; id < 128; id++) {
    const color = Rules.palette[Math.floor(id / 8) % 8]!, shape = { fill: id < 64 ? color : "none", stroke: id < 64 ? "none" : color }, marker = { fill: color, stroke: ColorToken.face };
    assert.equal(facePalette(faceName(id), [color]), true);
    assert.equal(facePalette(faceName(id), ["#000000"]), false);
    assert.equal(facePaint(id, shape, marker), true);
    assert.equal(facePaint(id, { ...shape, [id < 64 ? "fill" : "stroke"]: "#000000" }, marker), false);
    assert.equal(facePaint(id, shape, { ...marker, fill: "#000000" }), false);
    assert.equal(facePaint(id, shape, { ...marker, stroke: color }), false);
  }
  assert.equal(borderTransition("border-color", "120ms"), true);
  assert.equal(borderTransition("border-color, opacity", "120ms"), false);
  assert.equal(borderTransition("border-color", "100ms"), false);
});

test("replay output requires snapshot-only success and stderr-only nonzero rejection", () => {
  const valid = { name: "valid", clauses: [], text: JSON.stringify({ version: 1, boardCode: KnownCode, actions: [], endMs: 0 }) };
  const result = { code: 0, timedOut: false, stdout: JSON.stringify(freshSnapshot(KnownCode)), stderr: "" };
  assert.equal(compareReplay(valid, result), true);
  assert.equal(compareReplay(valid, { ...result, stdout: `npm banner\n${result.stdout}` }), false);
  assert.equal(compareReplay(valid, { ...result, stderr: "diagnostic" }), false);
  const invalid = { ...valid, text: "{" };
  const rejected = { ...result, code: 1, stdout: "", stderr: "Invalid replay." };
  assert.equal(compareReplay(invalid, rejected), true);
  for (const change of [{ stdout: "bad" }, { stdout: " " }, { stderr: "" }, { code: null }, { code: 0 }]) assert.equal(compareReplay(invalid, { ...rejected, ...change }), false);
});

test("the keyboard tree journey finishes, records states and catches trapped Tab", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage(); await fakeGame(page);
    const evidence = new Evidence(); await keyboardTree(page, evidence);
    for (const clause of Object.values(TreeClause).filter((clause) => clause !== TreeClause.versions)) assert.equal(evidence.outcomes.find((item) => item.clause === clause)?.status, "pass", JSON.stringify(evidence.outcomes));
    assert.match(evidence.outcomes.find((item) => item.clause === TreeClause.states)?.evidence ?? "", /button "Row 1, column 1/);
    await page.evaluate(() => document.addEventListener("keydown", (event) => { if (event.key === "Tab" && (event.target as Element).matches(".card")) event.preventDefault(); }));
    const broken = new Evidence(); await keyboardTree(page, broken);
    assert.equal(broken.outcomes.find((item) => item.clause === TreeClause.restart)?.status, "fail");
  } finally { await browser.close(); }
});

test("published clock and turn announcements pass exact wording and reject altered labels", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage(); await fakeGame(page);
    const clock = new Evidence(); await publishedClock(page, clock);
    assert.equal(clock.outcomes.find((item) => item.clause === StatusClause.clock)?.status, "pass");
    const speech = new Evidence(); await announcements(page, speech);
    for (const clause of [AnnouncementClause.matches, AnnouncementClause.miss, AnnouncementClause.win]) assert.equal(speech.outcomes.find((item) => item.clause === clause)?.status, "pass", clause);
    assert.equal(Announcement.match(1), "Pair found. 1 pair matched.");
    await page.evaluate(() => {
      const status = document.querySelector("#status")!;
      new MutationObserver(() => { if (status.textContent?.includes("Time:")) status.textContent = status.textContent.replace("Time:", "Elapsed:"); }).observe(status, { childList: true });
    });
    const altered = new Evidence(); await publishedClock(page, altered);
    assert.equal(altered.outcomes.find((item) => item.clause === StatusClause.clock)?.status, "fail");
  } finally { await browser.close(); }
});

test("move and pair status clauses check the published formats throughout play", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage(); await fakeGame(page);
    const valid = new Evidence(); await turns(page, valid);
    for (const clause of [StatusClause.moves, StatusClause.pairs]) assert.equal(valid.outcomes.find((item) => item.clause === clause)?.status, "pass", clause);
    await page.evaluate(() => {
      const status = document.querySelector("#status")!;
      new MutationObserver(() => { if (status.textContent?.includes("Moves:")) status.textContent = status.textContent.replace("Moves:", "Moves taken:").replace(/Pairs: (\d+)\/(\d+)/, "Pairs: $1 of $2"); }).observe(status, { childList: true });
    });
    const altered = new Evidence(); await turns(page, altered);
    for (const clause of [StatusClause.moves, StatusClause.pairs]) assert.equal(altered.outcomes.find((item) => item.clause === clause)?.status, "fail", clause);
  } finally { await browser.close(); }
});

test("Layer 2 continued-play checks permit a clock without a numeric cap", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage(); await fakeGame(page);
    const evidence = new Evidence(); await visibleTimeCap(page, evidence);
    for (const clause of [CapClause.elapsedPlay, CapClause.elapsedDisplay]) assert.equal(evidence.outcomes.find((item) => item.clause === clause)?.status, "pass", clause);
  } finally { await browser.close(); }
});

test("named visual targets reject decoy palette colors and a wrong hover border", async () => {
  const browser = await chromium.launch(existsSync(chromium.executablePath()) ? {} : { channel: "chrome" });
  try {
    const page = await browser.newPage(); await fakeGame(page);
    await page.evaluate(() => {
      const help = document.createElement("p"); help.id = "help"; help.textContent = "Arrow keys move board focus."; document.body.append(help);
      document.querySelector(".viewport")!.setAttribute("aria-describedby", help.id);
      const label = document.createElement("dt"); label.textContent = "Moves:"; document.body.append(label);
    });
    await page.addStyleTag({ content: `.viewport, button:not(.card), select, input, textarea { background:${ColorToken.panel} } #help, dt { color:${ColorToken.secondary}; font-size:14px } .card { transition:border-color 120ms ease } .card:hover { border-color:${ColorToken.text} }` });
    const valid = new Evidence(); await appearance(page, valid);
    const clauses = [TargetClause.panel, TargetClause.secondaryColor, TargetClause.secondarySize, TargetClause.hover, TargetClause.transition];
    for (const clause of clauses) assert.equal(valid.outcomes.find((item) => item.clause === clause)?.status, "pass", clause);
    await page.addStyleTag({ content: `select { background:#123456 } #help { color:${ColorToken.text}; font-size:16px } .card:hover { border-color:#FF0000 } .card { transition:border-color 120ms, opacity 120ms }` });
    const broken = new Evidence(); await appearance(page, broken);
    for (const clause of clauses) assert.equal(broken.outcomes.find((item) => item.clause === clause)?.status, "fail", clause);
  } finally { await browser.close(); }
});

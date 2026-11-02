import assert from "node:assert/strict";
import { test } from "node:test";
import { chromium } from "playwright";
import { PNG } from "pngjs";
import { boardFitsSize } from "./grid-size.ts";
import { markCards } from "./board.ts";
import { CheckId } from "./check-ids.ts";
import { playChecks } from "./checks-play.ts";
import { looksLikeBack, signatureOf } from "./face-signature.ts";
import { Finding, FixtureSize, matchedStyleGame, rowBoard } from "./regression-fixtures.ts";
import { CheckStatus } from "../../../history/statuses.ts";

test("closed 3D backs allow subpixel edges while new dim strokes still turn a card", () => {
  const dot = (center: number): PNG => {
    const image = new PNG({ width: 120, height: 120 });
    for (let y = 0; y < image.height; y += 1) for (let x = 0; x < image.width; x += 1) {
      const light = Math.round(64 * Math.max(0, Math.min(1, 11 - Math.hypot(x - center, y - 60))));
      image.data.set([16 + light, 16, 24 + light, 255], (y * image.width + x) * 4);
    }
    return image;
  };
  const back = signatureOf(PNG.sync.write(dot(59.5)));
  const closed = dot(60);
  assert.equal(looksLikeBack(signatureOf(PNG.sync.write(closed)), back), true);
  for (let y = 30; y < 90; y += 1) closed.data.set([48, 48, 56, 255], (y * closed.width + 40) * 4);
  assert.equal(looksLikeBack(signatureOf(PNG.sync.write(closed)), back), false);
});

test(Finding.rows, { skip: !process.env["PLAYWRIGHT_BROWSERS_PATH"] }, async () => {
  const browser = await chromium.launch();
  try {
    const page = await browser.newPage();
    await page.setContent(rowBoard(FixtureSize.large));
    assert.equal(await markCards(page), FixtureSize.large ** 2);
    assert.equal(await boardFitsSize(page, FixtureSize.large), true);
  } finally { await browser.close(); }
});

test(Finding.matched, { skip: !process.env["PLAYWRIGHT_BROWSERS_PATH"] }, async () => {
  const browser = await chromium.launch();
  try {
    const checks = await playChecks(browser, `data:text/html,${encodeURIComponent(matchedStyleGame())}`);
    for (const id of [CheckId.matching, CheckId.sameCardTwice, CheckId.clicksDuringDelay, CheckId.win]) {
      const check = checks.find((check) => check.id === id);
      assert.equal(check?.status, CheckStatus.pass, `${id}: ${check?.evidence}`);
    }
  } finally { await browser.close(); }
});

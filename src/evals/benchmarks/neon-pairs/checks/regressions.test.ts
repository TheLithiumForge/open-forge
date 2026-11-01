import assert from "node:assert/strict";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";
import { test } from "node:test";
import { chromium } from "playwright";
import { PNG } from "pngjs";
import { clicking, typing } from "./activation.ts";
import { backsOf, boardLooks, card, cardSignature, markCards, waitForBacks, waitForFace } from "./board.ts";
import { CheckId, GameFailure } from "./check-ids.ts";
import { deliveryChecks } from "./checks-delivery.ts";
import { colorDistance, looksLikeBack, shapeShare, signatureOf } from "./face-signature.ts";
import { boardFitsSize, setGridSize } from "./grid-size.ts";
import { CheckStatus } from "../../../history/statuses.ts";
import { dismissModal } from "./modal.ts";
import { readBoard } from "./open-game.ts";
import { bestIn, identifiesGame, movesIn, newGameButton, pageText, showsWin } from "./page-text.ts";
import { bestPage, Finding, FixtureSize, FixtureText, focusGlowGame, modalPage, namedPage, oddBoard, repeatGlowGame, thinGame, winPage } from "./regression-fixtures.ts";
import { playGame } from "./solver.ts";

const OldBackThresholds = { colors: 0.1, shape: 0.07 } as const;
const oldBack = (look: ReturnType<typeof signatureOf>, back: ReturnType<typeof signatureOf>): boolean => colorDistance(look, back) <= OldBackThresholds.colors && shapeShare(look, back) <= OldBackThresholds.shape;
const oldName = (text: string): boolean => /\bNeon Pairs\b/i.test(text);
const oldWin = (text: string): boolean => /\b(you win|you won|winner|won|victory|complete|completed|congrat\w*|solved|well done|cleared|all pairs)\b/i.test(text);

test(`${Finding.name}: stylized letters and the document title identify the game`, () => {
  assert.equal(oldName(FixtureText.logo), false);
  assert.equal(identifiesGame("", FixtureText.logo), true);
  assert.equal(identifiesGame(FixtureText.title, "Find a match"), true);
  assert.equal(identifiesGame("Other game", "Find a match"), false);
});

test(`${Finding.best}: units and an empty best keep score text distinct`, () => {
  for (const value of ["05moves", "05 moves", "01:12seconds", "05"]) assert.equal(bestIn(`${FixtureText.best}\n${value}`), value.startsWith("01") ? "01:12" : "05");
  assert.equal(bestIn(`${FixtureText.best}\n${FixtureText.noBest}`), null);
  assert.equal(bestIn(`${FixtureText.best}\n16 cards\nMoves: 99`), null);
  assert.equal(bestIn("A personal best for every grid\nMoves: 99"), null);
});

test(`${Finding.win}: a finishing message must appear after the last pair`, () => {
  for (const text of [FixtureText.win, FixtureText.alternateWin]) {
    assert.equal(oldWin(text), false);
    assert.equal(showsWin(text, "Moves: 12"), true);
    assert.equal(showsWin(text, text), false);
  }
  assert.equal(showsWin("Moves: 14", "Moves: 12"), false);
  assert.equal(showsWin("Match! 7 of 8 pairs found.", "Match! 6 of 8 pairs found."), false);
  assert.equal(showsWin("Moves: 14. Match all pairs to win.", "Moves: 12. Match all pairs to win."), false);
});

test(`${Finding.turned}: full pixels catch a dim outline that the old signature loses`, () => {
  const image = new PNG({ width: 120, height: 120 });
  for (let offset = 0; offset < image.data.length; offset += 4) image.data.set([16, 16, 24, 255], offset);
  const back = signatureOf(PNG.sync.write(image));
  for (let y = 30; y < 90; y += 1) image.data.set([48, 48, 56, 255], (y * image.width + 40) * 4);
  const face = signatureOf(PNG.sync.write(image));
  assert.equal(oldBack(face, back), true);
  assert.equal(looksLikeBack(face, back), false);
  assert.equal(looksLikeBack(back, back), true);
});

test(`${Finding.focus}: gradual light changes keep thin strokes distinct`, () => {
  const image = new PNG({ width: 120, height: 120 });
  for (let offset = 0; offset < image.data.length; offset += 4) image.data.set([16, 16, 24, 255], offset);
  const back = signatureOf(PNG.sync.write(image));
  for (let y = 0; y < image.height; y += 1) {
    for (let x = 0; x < image.width; x += 1) {
      const light = Math.round(20 * (1 - x / image.width));
      image.data.set([16 + light, 16 + light, 24 + light, 255], (y * image.width + x) * 4);
    }
  }
  assert.equal(looksLikeBack(signatureOf(PNG.sync.write(image)), back), true);
  for (let y = 30; y < 90; y += 1) image.data.set([48, 48, 56, 255], (y * image.width + 40) * 4);
  assert.equal(looksLikeBack(signatureOf(PNG.sync.write(image)), back), false);
  for (let offset = 0; offset < image.data.length; offset += 4) image.data.set([16, 16, 24, 255], offset);
  for (let y = 30; y < 90; y += 1) {
    for (const [column, light] of [6, 12, 6].entries()) image.data.set([16 + light, 16 + light, 24 + light, 255], (y * image.width + 40 + column) * 4);
  }
  assert.equal(looksLikeBack(signatureOf(PNG.sync.write(image)), back), false);
});

test(Finding.focus, { skip: !process.env["PLAYWRIGHT_BROWSERS_PATH"] }, async (t) => {
  const browser = await chromium.launch();
  const page = await browser.newPage();
  try {
    for (const activate of [clicking, typing]) await t.test(activate.name, async () => {
      await page.goto(`data:text/html,${encodeURIComponent(focusGlowGame())}`);
      const board = await readBoard(page);
      const back = board.backs[1]; assert.ok(back);
      const play = activate(page);
      await play(0);
      assert.notEqual(await waitForFace(page, 0, board.backs[0] ?? back), null);
      await play(1);
      assert.notEqual(await waitForFace(page, 1, back), null);
      assert.equal(await waitForBacks(page, [0, 1], board.backs), true);
      assert.equal(looksLikeBack(await cardSignature(page, 1), back), true);
      assert.equal(await card(page, 1).evaluate((element) => element === document.activeElement), true);
      await card(page, 0).hover();
      assert.equal(looksLikeBack(await cardSignature(page, 0), board.backs[0] ?? back), true);
      const game = await playGame(page, play, board.backs, { extraMisses: 0 });
      assert.equal(game.won, true, game.error ?? "");
      assert.equal(showsWin(await pageText(page)), true);
      await (await newGameButton(page))?.click();
      assert.equal(await waitForBacks(page, board.backs.map((_, index) => index), board.backs), true);
      await page.goto(`data:text/html,${encodeURIComponent(focusGlowGame(true))}`);
      const broken = await readBoard(page);
      const brokenBack = broken.backs[0]; assert.ok(brokenBack);
      await card(page, 0).hover();
      await activate(page)(0);
      assert.equal(await waitForFace(page, 0, brokenBack), null);
      assert.equal(await card(page, 0).evaluate((element) => element === document.activeElement), true);
    });
  } finally { await browser.close(); }
});

test(Finding.repeated, { skip: !process.env["PLAYWRIGHT_BROWSERS_PATH"] }, async (t) => {
  const browser = await chromium.launch();
  const page = await browser.newPage();
  const blur = async (keyboard: boolean): Promise<void> => {
    if (keyboard) await page.keyboard.press("Tab");
    else await page.mouse.click(1, 1);
  };
  try {
    for (const activate of [clicking, typing]) await t.test(activate.name, async () => {
      await page.goto(`data:text/html,${encodeURIComponent(repeatGlowGame())}`);
      const board = await readBoard(page);
      const back = board.backs[0]; assert.ok(back);
      const play = activate(page);
      await play(0);
      assert.notEqual(await waitForFace(page, 0, back), null);
      await blur(activate === typing);
      // An unfocused face can resemble its back, but the checker already revealed it.
      assert.equal(looksLikeBack(await cardSignature(page, 0), back), true);
      await play(0);
      assert.notEqual(await waitForFace(page, 0, back), null);
      assert.equal(movesIn(await pageText(page)), 0);
      assert.equal(looksLikeBack((await boardLooks(page))[0] ?? back, back), false);
      await play(2);
      assert.notEqual(await waitForFace(page, 2, board.backs[2] ?? back), null);
      const moves = movesIn(await pageText(page));
      await blur(activate === typing);
      await play(0);
      assert.notEqual(await waitForFace(page, 0, back), null);
      assert.equal(movesIn(await pageText(page)), moves);
      assert.equal(looksLikeBack((await boardLooks(page))[0] ?? back, back), false);
      const game = await playGame(page, play, board.backs, { extraMisses: 0 });
      assert.equal(game.won, true, game.error ?? "");
      assert.equal(showsWin(await pageText(page)), true);
      await (await newGameButton(page))?.click();
      const fresh = await readBoard(page);
      await play(0);
      assert.notEqual(await waitForFace(page, 0, fresh.backs[0] ?? back), null);
      await page.goto(`data:text/html,${encodeURIComponent(repeatGlowGame(true))}`);
      const broken = await readBoard(page);
      await activate(page)(0);
      assert.equal(await waitForFace(page, 0, broken.backs[0] ?? back), null);
    });
  } finally { await browser.close(); }
});

test("small browser fixtures exercise each finding and its broken counterpart", { skip: !process.env["PLAYWRIGHT_BROWSERS_PATH"] }, async (t) => {
  const root = await mkdtemp(fileURLToPath(new URL("../../../.l1-fixtures-", import.meta.url)));
  const browser = await chromium.launch();
  const page = await browser.newPage();
  let file = 0;
  const urlFor = async (html: string): Promise<string> => {
    const path = join(root, `${file++}.html`);
    await writeFile(path, html);
    return pathToFileURL(path).href;
  };
  try {
    await t.test(CheckId.singleFile, async () => {
      for (const [broken, titleOnly] of [[false, false], [false, true], [true, false]] as const) {
        const checks = await deliveryChecks(browser, await urlFor(namedPage(broken, titleOnly)), true);
        assert.equal(checks.find((check) => check.id === CheckId.singleFile)?.status, broken ? CheckStatus.fail : CheckStatus.pass);
      }
    });
    await t.test(Finding.turned, async () => {
      for (const activate of [clicking, typing]) {
        await page.goto(await urlFor(thinGame()));
        const board = await readBoard(page);
        let beforeLastPair = "";
        const game = await playGame(page, activate(page), board.backs, { extraMisses: 0, beforeLastPair: async () => { beforeLastPair = await pageText(page); } });
        assert.equal(game.won, true, game.error ?? "");
        assert.equal(showsWin(await pageText(page), beforeLastPair), true);
        await (await newGameButton(page))?.click();
        assert.equal(await waitForBacks(page, [0, 1], board.backs), true);
      }
      await page.goto(await urlFor(thinGame(true)));
      await markCards(page);
      const backs = await backsOf(page);
      const back = backs[0]; assert.ok(back);
      await card(page, 0).hover();
      await clicking(page)(0);
      assert.equal(await waitForFace(page, 0, back), null);
    });
    await t.test(CheckId.best, async () => {
      await page.goto(await urlFor(bestPage("05")));
      const text = await pageText(page);
      assert.equal(bestIn(text), "05");
      assert.equal(/^[^\p{L}]{1,12}$/u.test(text.split("\n")[1] ?? ""), false);
      await page.goto(await urlFor(bestPage("no score")));
      assert.equal(bestIn(await pageText(page)), null);
    });
    await t.test(CheckId.win, async () => {
      for (const phrase of [FixtureText.win, FixtureText.alternateWin]) {
        await page.goto(await urlFor(winPage(false, false, phrase)));
        const before = await pageText(page);
        await page.goto(await urlFor(winPage(true, false, phrase)));
        assert.equal(showsWin(await pageText(page), before), true);
        await page.goto(await urlFor(winPage(true, true, phrase)));
        assert.equal(showsWin(await pageText(page), before), false);
      }
    });
    await t.test(Finding.modal, async () => {
      for (const kind of ["escape", "close", "again", "fallback"] as const) {
        await page.goto(await urlFor(modalPage(kind)));
        assert.equal(await page.locator("dialog:modal").count(), 1);
        await dismissModal(page);
        assert.equal(await page.locator("dialog:modal").count(), 0);
        await page.getByRole("button", { name: "Next action" }).click({ timeout: 2000 });
      }
      await page.goto(await urlFor(thinGame(false, true)));
      await markCards(page);
      for (let index = 0; index < FixtureSize.small ** 2; index += 1) await clicking(page)(index);
      assert.equal(await page.locator("dialog:modal").count(), 1);
      assert.equal(await setGridSize(page, FixtureSize.large), true);
      await page.goto(await urlFor(modalPage("stuck")));
      await assert.rejects(dismissModal(page), GameFailure);
    });
    await t.test(CheckId.sizesFinishable, async () => {
      for (let size = FixtureSize.small; size <= FixtureSize.large; size += 1) {
        await page.goto(await urlFor(oddBoard(size)));
        const count = await markCards(page);
        assert.equal(count, size ** 2 - size % 2);
        if (size % 2) assert.notEqual(count, size ** 2);
        assert.equal(await boardFitsSize(page, size), true);
      }
      for (const [missing, columns] of [[0, FixtureSize.odd], [3, FixtureSize.odd], [1, FixtureSize.small]]) {
        await page.goto(await urlFor(oddBoard(FixtureSize.odd, missing, columns)));
        assert.equal(await boardFitsSize(page, FixtureSize.odd), false);
      }
      await page.goto(await urlFor(oddBoard(FixtureSize.small, 2)));
      assert.equal(await boardFitsSize(page, FixtureSize.small), false);
    });
  } finally {
    await browser.close();
    await rm(root, { recursive: true, force: true });
  }
});

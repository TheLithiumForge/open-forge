import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { Evidence } from "./outcomes.ts";
import { changeSize, click, moveFocus, names, reset, solve, visibleDeck } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { faceIn, VisualTiming } from "./view-values.ts";
import { styles } from "./browser-view.ts";
import { touchBoardEnd } from "./browser-touch.ts";
import { focusSamples, OutlineClauses, ScrollMarginClause } from "./browser-layout.ts";
export { zoom } from "./browser-zoom.ts";

export const RevealedNameClauses = ["I15.1", "I15.2"] as const;

export async function accessibleNames(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), closed = await names(page);
  const positions = (name: string, index: number): boolean => new RegExp(`row\\s*${Math.floor(index / 4) + 1}\\b`, "i").test(name) && new RegExp(`col(?:umn)?\\s*${index % 4 + 1}\\b`, "i").test(name);
  put("I14.1", closed.every((name, index) => positions(name, index) && /closed/i.test(name)), "All closed accessible names give their one-based row and column.");
  const secret = await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.some((element) => /(?:face[-_ ]?id|data-face|marker (?:top|left|right|bottom)|cyan solid|outline chevron)/i.test(element.outerHTML)));
  put("I14.2 I14.3 V14.2", !secret, "Closed cards carry no face identifiers or named face SVG. Only the common back is mounted.", { secret, closed });
  const deck = await visibleDeck(page); await reset(page); await click(page, 0);
  const shown = (await names(page))[0] ?? "";
  const partner = deck.findIndex((face, index) => index !== 0 && face === deck[0]); await click(page, partner);
  const matched = (await names(page))[0] ?? "";
  put(RevealedNameClauses.join(" "), positions(shown, 0) && positions(matched, 0) && /revealed/i.test(shown) && /matched/i.test(matched) && faceIn(shown) === deck[0] && faceIn(matched) === deck[0], "Revealed and matched accessible names keep position, specified face name and state.", { shown, matched, expectedFace: deck[0] });
}

export async function scroll(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), results: boolean[] = [], reachable: boolean[] = [], counts: number[] = [], minimum: boolean[] = [], fits: boolean[] = [], touch: boolean[] = [];
  const margins: boolean[] = [];
  for (const width of [360, 1280]) {
    await page.setViewportSize({ width, height: 800 }); await changeSize(page, 16);
    counts.push((await names(page)).length); const measured = await styles(page);
    margins.push(await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.length === 256 && elements.every((element) => ["scrollMarginTop", "scrollMarginRight", "scrollMarginBottom", "scrollMarginLeft"].every((property) => Number.parseFloat(Reflect.get(getComputedStyle(element), property) as string) >= 6))));
    minimum.push(measured.cardBox.width >= 44 && measured.cardBox.height >= 44);
    fits.push(measured.pageWidth <= width);
    await card(page, 0).focus(); await moveFocus(page, 255, 16);
    reachable.push(await card(page, 255).evaluate((element) => element === document.activeElement));
    results.push(await focusSamples(page));
    await moveFocus(page, 0, 16);
    await card(page, 0).scrollIntoViewIfNeeded();
    touch.push(await touchBoardEnd(page));
    for (const index of [0, 255]) {
      await card(page, index).focus();
      reachable.push(await card(page, index).evaluate((element) => document.activeElement === element));
      await card(page, index).tap({ force: true });
    }
  }
  put(OutlineClauses.join(" "), results.every(Boolean), "Corner and mid-board focused cards keep their 5px outline allowance inside every clipping ancestor after focus scrolling.");
  put(ScrollMarginClause, margins.every(Boolean) && results.every(Boolean), "All 256 cards have at least 6px scroll margin on every side and keep interior outlines visible at both viewport widths.");
  put("V19.1", counts.every((count) => count === 256), `Large card counts at phone and desktop widths: ${counts.join(", ")}.`);
  put("V19.2", fits.every(Boolean) && await page.getByRole("button", { name: "New Game", exact: true }).isVisible(), "Both viewport widths fit the page and keep New Game reachable.");
  put("V19.3", minimum.every(Boolean), "All sampled large-board cards remain at least 44px square.");
  put("V19.4", reachable.every(Boolean) && touch.every(Boolean), "Native touch scroll gestures move the board on each overflowing axis. Plain arrows reach the last card and first and last cards accept completed taps at both widths.");
}

export async function motion(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), deck = await visibleDeck(page); await reset(page); await solve(page, deck);
  const animations = await page.evaluate(() => {
    // Flush style before reading animations after the final React render.
    document.body.getBoundingClientRect();
    return document.getAnimations().filter((animation) => !(animation instanceof CSSTransition)).map((animation) => ({ duration: animation.effect?.getTiming().duration, iterations: animation.effect?.getTiming().iterations }));
  });
  put("V18.2", animations.some((animation) => animation.duration === VisualTiming.winMs && animation.iterations === 1), "Win pulse runs once for 600ms.");
  // The controlled game clock doesn't advance Chromium's CSS animation timeline.
  await page.waitForTimeout(VisualTiming.winMs + VisualTiming.settleMs);
  const staticAfter = await page.evaluate(() => document.getAnimations().filter((animation) => animation.playState === "running").length === 0);
  await page.emulateMedia({ reducedMotion: "reduce" }); await reset(page); await solve(page, deck);
  put("V18.3", await page.evaluate(() => { document.body.getBoundingClientRect(); return document.getAnimations().every((animation) => animation.playState !== "running"); }), "Reduced motion shows completion without an active animation.");
  put("V18.4", staticAfter && await page.locator('canvas,[class*="confetti"],blink').count() === 0 && animations.every((animation) => animation.iterations === 1), "No continuing flash, confetti canvas or repeating celebration remains.");
}


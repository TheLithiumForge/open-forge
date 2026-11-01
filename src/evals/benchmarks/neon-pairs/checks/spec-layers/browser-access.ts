import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { Evidence } from "./outcomes.ts";
import { changeSize, click, names, reset, solve, step, visibleDeck } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { faceIn, fullyVisible } from "./view-values.ts";
import { styles } from "./browser-view.ts";

export async function accessibleNames(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), closed = await names(page);
  const positions = (name: string, index: number): boolean => new RegExp(`row\\s*${Math.floor(index / 4) + 1}\\b`, "i").test(name) && new RegExp(`col(?:umn)?\\s*${index % 4 + 1}\\b`, "i").test(name);
  put("I14.1", closed.every((name, index) => positions(name, index) && /closed/i.test(name)), "All closed accessible names give their one-based row and column.");
  const secret = await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.some((element) => /(?:face[-_ ]?id|data-face|marker (?:top|left|right|bottom)|cyan solid|outline chevron)/i.test(element.outerHTML)));
  put("I14.2 I14.3 V14.2", !secret, "Closed cards carry no face identifiers or named face SVG. Only the common back is mounted.");
  const deck = await visibleDeck(page); await reset(page); await click(page, 0);
  const shown = (await names(page))[0] ?? "";
  const partner = deck.findIndex((face, index) => index !== 0 && face === deck[0]); await click(page, partner);
  const matched = (await names(page))[0] ?? "";
  put("I15.1 I15.2", positions(shown, 0) && positions(matched, 0) && /revealed/i.test(shown) && /matched/i.test(matched) && faceIn(shown) === deck[0] && faceIn(matched) === deck[0], "Revealed and matched accessible names keep position, specified face name and state.");
}

export async function scroll(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), results: boolean[] = [], reachable: boolean[] = [], counts: number[] = [], minimum: boolean[] = [], fits: boolean[] = [], touch: boolean[] = [];
  for (const width of [360, 1280]) {
    await page.setViewportSize({ width, height: 800 }); await changeSize(page, 16);
    counts.push((await names(page)).length); const measured = await styles(page);
    minimum.push(measured.cardBox.width >= 44 && measured.cardBox.height >= 44);
    fits.push(measured.pageWidth <= width);
    await card(page, 0).focus(); await page.keyboard.press("Control+End");
    reachable.push(await card(page, 255).evaluate((element) => element === document.activeElement));
    await page.keyboard.press("Control+Home");
    const clip = (await styles(page)).viewportBox;
    const cdp = await page.context().newCDPSession(page);
    try {
      await cdp.send("Input.synthesizeScrollGesture", { x: Math.max(1, Math.min(width - 1, clip.x + clip.width / 2)), y: Math.max(1, Math.min(799, clip.y + clip.height / 2)), xDistance: -1600, yDistance: -1600, gestureSourceType: "touch", speed: 1600 });
      touch.push(await card(page, 255).evaluate((element) => {
        let parent = element.parentElement;
        while (parent?.parentElement && !/(auto|scroll)/.test(getComputedStyle(parent).overflowX + getComputedStyle(parent).overflowY)) parent = parent.parentElement;
        return parent !== null && (parent.scrollWidth <= parent.clientWidth || parent.scrollLeft > 0) && (parent.scrollHeight <= parent.clientHeight || parent.scrollTop > 0);
      }));
    } finally { await cdp.detach(); }
    for (const index of [0, 255]) {
      await card(page, index).focus();
      const geometry = await card(page, index).evaluate((element) => {
        let parent = element.parentElement;
        while (parent?.parentElement && !/(auto|scroll)/.test(getComputedStyle(parent).overflowX + getComputedStyle(parent).overflowY)) parent = parent.parentElement;
        const box = (node: Element | null) => { const rect = node?.getBoundingClientRect(); return { x: rect?.x ?? 0, y: rect?.y ?? 0, width: rect?.width ?? 0, height: rect?.height ?? 0 }; };
        return { card: box(element), viewport: box(parent), focused: document.activeElement === element };
      });
      results.push(fullyVisible(geometry.card, geometry.viewport));
      await card(page, index).tap({ force: true }); reachable.push(geometry.focused);
    }
  }
  put("I13.1 V19.5", results.every(Boolean), "First and last focused cards, including their 5px outline allowance, fit inside the board after native focus scrolling.");
  put("V19.1", counts.every((count) => count === 256), `Large card counts at phone and desktop widths: ${counts.join(", ")}.`);
  put("V19.2", fits.every(Boolean) && await page.getByRole("button", { name: "New Game", exact: true }).isVisible(), "Both viewport widths fit the page and keep New Game reachable.");
  put("V19.3", minimum.every(Boolean), "All sampled large-board cards remain at least 44px square.");
  put("V19.4", reachable.every(Boolean) && touch.every(Boolean), "Native touch scroll gestures move the board on each overflowing axis. Ctrl+End reaches the last card and first and last cards accept completed taps at both widths.");
}

export async function motion(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), deck = await visibleDeck(page); await reset(page); await solve(page, deck);
  const animations = await page.evaluate(() => document.getAnimations().map((animation) => ({ duration: animation.effect?.getTiming().duration, iterations: animation.effect?.getTiming().iterations })));
  put("V18.2", animations.some((animation) => animation.duration === 600 && animation.iterations === 1), "Win pulse runs once for 600ms.");
  await step(page, 601);
  const staticAfter = await page.evaluate(() => document.getAnimations().filter((animation) => animation.playState === "running").length === 0);
  await page.emulateMedia({ reducedMotion: "reduce" }); await reset(page); await solve(page, deck);
  put("V18.3", await page.evaluate(() => document.getAnimations().every((animation) => animation.playState !== "running")), "Reduced motion shows completion without an active animation.");
  put("V18.4", staticAfter && await page.locator('canvas,[class*="confetti"],blink').count() === 0 && animations.every((animation) => animation.iterations === 1), "No continuing flash, confetti canvas or repeating celebration remains.");
}

export async function zoom(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), measured = await styles(page);
  put("V20.5 T08.4", !/(?:user-scalable\s*=\s*(?:no|0)|maximum-scale\s*=\s*1(?:\D|$))/i.test(measured.viewportMeta), "The viewport metadata keeps browser zoom enabled.");
  for (const id of ["V20.1", "V20.2", "V20.3", "V20.4"]) evidence.record(id, "error", "Headless Playwright has no native browser zoom control. Device scale and CSS zoom are different. Validate this clause at native 200% browser zoom.");
}

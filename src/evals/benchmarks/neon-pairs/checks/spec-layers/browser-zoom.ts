import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { focusSamples, focusedOutline } from "./browser-layout.ts";
import { reporter } from "./browser-play.ts";
import { body, changeSize, metric, names, reset, solve, visibleDeck } from "./browser-session.ts";
import { BehaviorFailure, Evidence } from "./outcomes.ts";
import { faceIn, overlaps } from "./view-values.ts";
import { styles } from "./browser-view.ts";

export const ZoomViewport = { width: 640, height: 400, deviceScaleFactor: 2 } as const;
export const ZoomClause = { readable: "V20.1", pageWidth: "V20.2", play: "V20.3", outline: "V20.4", enabled: "V20.5" } as const;

export async function readableLayout(page: Page): Promise<boolean> {
  const boxes = await page.locator("body").evaluate((body, attribute) => {
    const boxes: { x: number; y: number; width: number; height: number }[] = [];
    const visible = (element: Element): boolean => {
      const style = getComputedStyle(element);
      return style.visibility === "visible" && style.display !== "none" && style.opacity !== "0" && element.getClientRects().length > 0;
    };
    const walker = document.createTreeWalker(body, NodeFilter.SHOW_TEXT);
    while (walker.nextNode()) {
      const node = walker.currentNode, element = node.parentElement;
      if (!node.textContent?.trim() || !element || element.closest('button,input,select,textarea,svg,script,style,[aria-hidden="true"]') || !visible(element)) continue;
      const range = document.createRange(); range.selectNodeContents(node);
      for (const rect of range.getClientRects()) if (rect.width > 0 && rect.height > 0) boxes.push({ x: rect.x, y: rect.y, width: rect.width, height: rect.height });
    }
    for (const element of body.querySelectorAll(`button:not([${attribute}]),input,select,textarea`)) {
      if (!visible(element)) continue;
      const rect = element.getBoundingClientRect();
      boxes.push({ x: rect.x, y: rect.y, width: rect.width, height: rect.height });
    }
    return boxes;
  }, CardAttribute);
  return boxes.length > 0 && boxes.every((box, index) => boxes.slice(index + 1).every((other) => !overlaps(box, other)));
}

export async function zoom(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), readable: boolean[] = [], fits: boolean[] = [], playable: boolean[] = [], outlines: boolean[] = [];
  let completed = false;
  const configured = await page.evaluate(() => ({ width: innerWidth, height: innerHeight, deviceScaleFactor: devicePixelRatio }));
  if (configured.width !== ZoomViewport.width || configured.height !== ZoomViewport.height || configured.deviceScaleFactor !== ZoomViewport.deviceScaleFactor) throw new Error("Zoom journey requires the equivalent CSS viewport and device scale.");
  for (const size of [4, 16]) {
    await changeSize(page, size);
    readable.push(await readableLayout(page));
    fits.push((await styles(page)).pageWidth <= ZoomViewport.width);
    // Real pointer clicks scroll into view and check hit targets, unlike forced reveal helpers.
    await card(page, 0).click(); await card(page, 1).click();
    playable.push((await names(page)).slice(0, 2).every((name) => faceIn(name) !== null) && metric(await body(page), "moves") === 1);
    await reset(page);
    await card(page, 0).focus(); await page.keyboard.press("Enter"); await page.keyboard.press("ArrowRight"); await page.keyboard.press("Space");
    playable.push((await names(page)).slice(0, 2).every((name) => faceIn(name) !== null) && metric(await body(page), "moves") === 1);
    if (size === 16) {
      outlines.push(await focusSamples(page));
      await card(page, 0).focus(); await page.keyboard.press("Control+End");
      playable.push(await card(page, size * size - 1).evaluate((element) => element === document.activeElement));
    } else {
      outlines.push(await focusedOutline(page, 1));
      try {
        await reset(page); const deck = await visibleDeck(page); await reset(page); await solve(page, deck);
        completed = /(?:win|won|every pair|complete)/i.test(await body(page));
        playable.push(completed);
        readable.push(await readableLayout(page));
        fits.push((await styles(page)).pageWidth <= ZoomViewport.width);
      } catch (error) {
        if (!(error instanceof BehaviorFailure)) throw error;
        playable.push(false);
      }
    }
  }
  if (readable.every(Boolean) && !completed) evidence.record(ZoomClause.readable, "blocked", "The game didn't finish, so the win summary couldn't be measured at the equivalent zoom viewport.");
  else put(ZoomClause.readable, readable.every(Boolean), "Controls, status and completed-game text have separate text and control boxes at the equivalent zoom viewport.");
  put(ZoomClause.pageWidth, fits.every(Boolean), "The outer page fits 640 CSS pixels for both sizes and after victory.");
  put(ZoomClause.play, playable.every(Boolean), "Pointer turns, Enter and Space turns and large-board keyboard navigation work at device scale 2.");
  put(ZoomClause.outline, outlines.every(Boolean), "Focus scrolling keeps corner and interior card outlines inside every clipping ancestor.");
  const measured = await styles(page);
  put(`${ZoomClause.enabled} T08.4`, !/(?:user-scalable\s*=\s*(?:no|0)|maximum-scale\s*=\s*1(?:\D|$))/i.test(measured.viewportMeta), "The viewport metadata keeps browser zoom enabled.");
}

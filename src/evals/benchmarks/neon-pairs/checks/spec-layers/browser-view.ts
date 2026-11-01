import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { Evidence } from "./outcomes.ts";
import { changeSize, click, reset, solve, visibleDeck } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { Rules } from "./rules.ts";
import { cardName, faceIn, near, pixels, rgb, transitionMs, VisualTiming } from "./view-values.ts";
import { hoverChanges } from "./hover-style.ts";
import { checkMark, sameBacks } from "./drawn-shapes.ts";
import { drawings } from "./browser-drawing.ts";
import { signatureOf, type FaceSignature } from "../face-signature.ts";
import { renderedFaces } from "./rendered-faces.ts";
import { ContentMaximum, LayoutAttribute, markLayout } from "./browser-layout.ts";
import { borderTransition, ColorToken, facePalette, namedTargets, TargetClause } from "./browser-targets.ts";
import { Labels } from "./browser-session.ts";

export const FaceClause = { unique: "V10.1", closed: "V14.2", artwork: "V15.1", check: "V15.2", opacity: "V15.3" } as const;

export function spacingOnScale(values: readonly string[]): boolean {
  return values.every((value) => value === "normal" || value === "auto" || pixels(value) !== null && ((pixels(value) ?? 0) <= 0 || Rules.spacing.some((allowed) => allowed > 0 && near(pixels(value), allowed))));
}

export async function styles(page: Page) {
  await markLayout(page);
  return page.evaluate(({ attribute, layout }) => {
    const card = document.querySelector(`[${attribute}="0"]`);
    if (!card) throw new Error("No visible card to measure.");
    const grid = document.querySelector(`[${layout}="board"]`);
    let viewport = grid;
    while (viewport?.parentElement && !/(auto|scroll)/.test(getComputedStyle(viewport).overflowX + getComputedStyle(viewport).overflowY)) viewport = viewport.parentElement;
    const fields = ["backgroundColor", "color", "borderColor", "borderWidth", "borderRadius", "outlineColor", "outlineWidth", "outlineOffset", "opacity", "display", "gap", "padding", "maxWidth", "maxHeight", "overflowX", "overflowY", "fontFamily", "fontSize", "lineHeight", "transitionDuration", "transitionProperty"];
    const style = (element: Element | null): Record<string, string> => Object.fromEntries(fields.map((field) => [field, element ? Reflect.get(getComputedStyle(element), field) as string : ""]));
    const box = (element: Element | null) => { const rect = element?.getBoundingClientRect(); return { x: rect?.x ?? 0, y: rect?.y ?? 0, width: rect?.width ?? 0, height: rect?.height ?? 0 }; };
    const all = [document.documentElement, document.body, ...Array.from(document.querySelectorAll("body *"))];
    const content = document.querySelector(`[${layout}="content"]`), title = document.querySelector(`[${layout}="title"]`);
    const authored: CSSStyleRule[] = [];
    const collect = (rules: CSSRuleList): void => {
      for (const rule of rules) {
        if (rule instanceof CSSMediaRule && !matchMedia(rule.conditionText).matches || rule instanceof CSSSupportsRule && !CSS.supports(rule.conditionText)) continue;
        if (rule instanceof CSSStyleRule) authored.push(rule);
        if ("cssRules" in rule) collect(rule.cssRules as CSSRuleList);
      }
    };
    for (const sheet of [...document.styleSheets, ...document.adoptedStyleSheets]) {
      try { collect(sheet.cssRules); } catch { /* Offline pages can't read external stylesheets. */ }
    }
    return { body: style(document.body), main: style(content), card: style(card), grid: style(grid), viewport: style(viewport), title: style(title),
      cardBox: box(card), gridBox: box(grid), viewportBox: box(viewport), mainBox: box(content), width: innerWidth, height: innerHeight, pageWidth: document.documentElement.scrollWidth,
      colors: all.flatMap((element) => { const style = getComputedStyle(element); return [style.color, style.backgroundColor, style.borderColor]; }),
      secondary: all.some((element) => getComputedStyle(element).fontSize === "14px"),
      spacing: all.filter((element) => !(element instanceof SVGElement) && !element.closest("svg") && element.tagName !== "OPTION" && element.getClientRects().length > 0).flatMap((element) => {
        const style = getComputedStyle(element), typed = element.computedStyleMap();
        const control = element.matches("select,button,input,textarea,summary");
        const set = (name: string): boolean => element instanceof HTMLElement && Boolean(element.style.getPropertyValue(name)) || authored.some((rule) => {
          try { return Boolean(rule.style.getPropertyValue(name)) && element.matches(rule.selectorText); } catch { return false; }
        });
        // Typed margins keep auto centering, which resolved style reports as an arbitrary pixel gap.
        return ["margin-top", "margin-bottom", "margin-left", "margin-right", "padding-top", "padding-bottom", "padding-left", "padding-right", "row-gap", "column-gap"].filter((name) => !control || set(name)).map((name) => name.startsWith("margin") ? typed.get(name)?.toString() ?? style.getPropertyValue(name) : style.getPropertyValue(name));
      }),
      canvas: document.querySelectorAll("canvas").length, svg: card.querySelectorAll("svg").length, viewportMeta: document.querySelector('meta[name="viewport"]')?.getAttribute("content") ?? "",
    };
  }, { attribute: CardAttribute, layout: LayoutAttribute });
}

export async function layout(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), measured = await styles(page);
  put("V02.1", near(pixels(measured.main["maxWidth"] ?? ""), ContentMaximum) && near(measured.mainBox.x, (measured.width - measured.mainBox.width) / 2), "Measured content max-width and centered geometry.");
  put("V02.2", measured.main["padding"] === "16px", "Content padding is 16px.");
  put("V02.3", measured.pageWidth <= measured.width, "Outer page fits the viewport.");
  put("V03.1", near(pixels(measured.viewport["maxHeight"] ?? ""), Math.min(measured.height * 0.7, 768)), "Board height cap is the lesser of 70dvh and 768px.");
  put("V03.2", measured.viewport["padding"] === "6px", "Board viewport padding is 6px.");
  put("V03.3", ["auto", "scroll"].includes(measured.viewport["overflowX"] ?? "") && ["auto", "scroll"].includes(measured.viewport["overflowY"] ?? ""), "Board allows both scroll axes.");
  put("V04.1", near(measured.cardBox.width, measured.cardBox.height) && measured.cardBox.width >= Rules.cardMinimum && measured.cardBox.width <= Rules.cardMaximum, "Square card bounds measured in CSS pixels.");
  for (const [id, actual, expected] of [["V04.2", measured.grid["gap"], 6], ["V04.3", measured.card["borderWidth"], 2], ["V04.4", measured.card["borderRadius"], 8]] as const) put(id, near(pixels(actual ?? ""), expected), `${id}: computed value ${actual}.`);
  await page.setViewportSize({ width: 360, height: 800 });
  const phone = await styles(page);
  put("V05.1", phone.pageWidth <= phone.width && phone.gridBox.width <= phone.viewportBox.width, "Phone 4-column board fits its board viewport.");
  await changeSize(page, 16); const large = await styles(page);
  put("V05.2", near(large.cardBox.width, Rules.cardMinimum) && large.gridBox.width > large.viewportBox.width, "Phone 16-column board keeps 44px cards and internal horizontal overflow.");
}

export async function appearance(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), closed = await styles(page);
  put("V01.1", await page.locator("button,select,input,textarea").count() >= 19 && await page.locator('[role="button"]:not(button)').count() === 0, "Native HTML controls provide the interaction elements.");
  const palette = closed.colors.map(rgb);
  for (const [id, color] of [["V06.1", "#090B14"], ["V06.5", "#F4F6FF"]]) put(id ?? "", palette.includes(color ?? ""), `Computed element colors include ${color}.`);
  const summary = page.locator("summary").filter({ hasText: new RegExp(`^${Labels.reproduce}$`) });
  if (await summary.count() && !await summary.evaluate((element) => element.parentElement?.hasAttribute("open"))) await summary.click();
  const targets = await namedTargets(page);
  put(TargetClause.panel, targets.panels.length > 0 && targets.panels.every((color) => rgb(color) === ColorToken.panel), "Board scroll area, reproduction panel and visible non-card native controls use the Panel background.");
  put(TargetClause.secondaryColor, targets.secondary.length > 0 && targets.secondary.every((target) => rgb(target.color) === ColorToken.secondary), "Board help, status labels and supporting notes use the Secondary color.");
  put("V06.3", rgb(closed.card["backgroundColor"] ?? "") === "#1B2034", "Closed card background matches its token.");
  put("V06.7", rgb(closed.card["borderColor"] ?? "") === "#757C9A", "Closed card border matches its token.");
  put("V08.1 T08.3", /(?:system-ui|-apple-system|BlinkMacSystemFont)/i.test(closed.body["fontFamily"] ?? "") && /sans-serif/i.test(closed.body["fontFamily"] ?? ""), "Computed body font uses the system sans-serif stack.");
  put("V08.2", closed.body["fontSize"] === "16px" && closed.body["lineHeight"] === "24px", "Body text is 16px on 24px lines.");
  put("V08.3", closed.title["fontSize"] === "24px", "Title text is 24px.");
  put(TargetClause.secondarySize, targets.secondary.length > 0 && targets.secondary.every((target) => target.size === "14px"), "Board help, status labels and supporting notes are 14px.");
  put("V09.1", spacingOnScale(closed.spacing), "Positive authored spacing uses the scale. Zero, auto and normal pass. Reset margins pass and native control internals are excluded.");
  put("V09.2", closed.card["borderWidth"] === "2px" && closed.card["borderRadius"] === "8px" && closed.body["fontSize"] === "16px", "Border, radius and font metrics keep their separate stated values.");
  await click(page, 0); const open = await styles(page);
  put("V01.3", open.svg > 0 && open.canvas === 0, "Revealed card artwork uses inline SVG.");
  put("V06.4", rgb(open.card["backgroundColor"] ?? "") === "#111625", "Revealed card background matches its token.");
  await reset(page); await card(page, 0).focus(); await page.keyboard.press("ArrowRight"); await page.keyboard.press("ArrowLeft");
  const focused = await styles(page);
  put("V06.8 V16.1", focused.card["outlineWidth"] === "3px" && focused.card["outlineOffset"] === "2px" && rgb(focused.card["outlineColor"] ?? "") === "#FFFFFF", "Keyboard focus has a 3px white outline offset by 2px.");
  const computed = () => card(page, 0).evaluate((element) => { const style = getComputedStyle(element); return Object.fromEntries(Array.from(style).map((name) => [name, style.getPropertyValue(name)])); });
  await page.mouse.move(0, 0); const beforeHover = await computed();
  await card(page, 0).hover(); await page.waitForTimeout(VisualTiming.transitionMs + VisualTiming.settleMs);
  const afterHover = await computed(), changed = hoverChanges(beforeHover, afterHover);
  put(TargetClause.hover, changed.length === 0 && ["top", "right", "bottom", "left"].every((side) => rgb(afterHover[`border-${side}-color`] ?? "") === ColorToken.text), `Hover changes only border colors to Text. Other changes: ${changed.join(", ") || "none"}.`);
  put(TargetClause.transition, borderTransition(focused.card["transitionProperty"] ?? "", focused.card["transitionDuration"] ?? ""), "Only border-color properties have 120ms transitions.");
  await page.emulateMedia({ reducedMotion: "reduce" }); const reduced = await styles(page);
  put("V17.2", (transitionMs(reduced.card["transitionDuration"] ?? "") ?? [NaN]).every((ms) => ms === 0), "Reduced motion removes card transitions.");
}

export async function faces(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence); await changeSize(page, 16);
  const closed = await drawings(page);
  put("V14.1", sameBacks(closed), "Each hidden card draws the same neutral diamond contour, including CSS borders and SVG geometry.");
  const colors: string[] = [], captured: FaceSignature[] = [], paletteAtMarker: boolean[] = [];
  const deck = await visibleDeck(page, async (positions) => {
    for (const position of positions) {
      const paints = await card(page, position).locator("svg *").evaluateAll((elements) => elements.flatMap((element) => [getComputedStyle(element).fill, getComputedStyle(element).stroke]));
      colors.push(...paints);
      paletteAtMarker.push(facePalette(await card(page, position).getAttribute("aria-label") ?? cardName(await card(page, position).ariaSnapshot()) ?? "", paints));
      captured[position] = signatureOf(await card(page, position).screenshot({ animations: "disabled" }));
    }
  }), unique = new Set(deck);
  const distinct = renderedFaces(captured);
  evidence.inherited.set(distinct.id, distinct);
  put(FaceClause.unique, unique.size === 128, "The 16x16 board must use 128 distinct named identities.", { unique: unique.size, deck });
  put("V10.3 V10.4 V10.5 V13.1 V13.2", [...unique].every((name) => faceIn(name) === name) && Rules.shapes.every((shape) => deck.some((name) => name.includes(` ${shape},`))) && Rules.markers.every((marker) => deck.some((name) => name.endsWith(`marker ${marker}`))) && ["solid", "outline"].every((fill) => deck.some((name) => name.includes(` ${fill} `))), "Visible names span all eight shapes, markers and both fills in the specified lowercase form.");
  put("V07.1", Rules.palette.every((color) => colors.map(rgb).includes(color)), "Revealed SVG artwork uses each specified palette entry.");
  put(TargetClause.palette, paletteAtMarker.length === 256 && paletteAtMarker.every(Boolean), "Each revealed face name and rendered palette color agree with its marker index, without requiring numeric face IDs in Layer 2.");
  await reset(page); await solve(page, deck); await page.waitForTimeout(VisualTiming.transitionMs + VisualTiming.settleMs);
  const matched = await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.filter((element) => element.getAttribute("aria-disabled") === "true").map((element) => ({ svg: element.querySelectorAll("svg").length, opacity: getComputedStyle(element).opacity, color: getComputedStyle(element).borderColor, text: element.textContent ?? "", html: element.innerHTML })));
  put("V15.1", matched.length > 0 && matched.every((entry) => entry.svg > 0), "Revealed matched cards retain inline SVG artwork.");
  const marks = await drawings(page);
  put("V15.2", matched.length === deck.length && matched.every((entry) => rgb(entry.color) === Rules.palette[2]) && marks.every((contours) => contours.some((contour) => rgb(contour.color) === Rules.palette[2] && checkMark(contour.points))), "Every matched card has a green border and a drawn green check contour.");
  put("V15.3", matched.length > 0 && matched.every((entry) => entry.opacity === "1"), "Matched cards keep full opacity.");
  await card(page, 0).hover(); await page.waitForTimeout(VisualTiming.transitionMs + VisualTiming.settleMs);
  evidence.merge(TargetClause.hover, rgb(await card(page, 0).evaluate((element) => getComputedStyle(element).borderColor)) === ColorToken.green ? "pass" : "fail", "A matched card keeps the Green border while hovered.");
}

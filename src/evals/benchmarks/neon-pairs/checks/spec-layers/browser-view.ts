import type { Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { Evidence } from "./outcomes.ts";
import { changeSize, click, reset, step, visibleDeck } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { Rules } from "./rules.ts";
import { faceIn, near, pixels, rgb, transitionMs } from "./view-values.ts";

export async function styles(page: Page) {
  return page.evaluate((attribute) => {
    const card = document.querySelector(`[${attribute}="0"]`);
    if (!card) throw new Error("No visible card to measure.");
    let grid = card.parentElement;
    while (grid?.parentElement && getComputedStyle(grid).display !== "grid") grid = grid.parentElement;
    let viewport = grid;
    while (viewport?.parentElement && !/(auto|scroll)/.test(getComputedStyle(viewport).overflowX + getComputedStyle(viewport).overflowY)) viewport = viewport.parentElement;
    const fields = ["backgroundColor", "color", "borderColor", "borderWidth", "borderRadius", "outlineColor", "outlineWidth", "outlineOffset", "opacity", "display", "gap", "padding", "maxWidth", "maxHeight", "overflowX", "overflowY", "fontFamily", "fontSize", "lineHeight", "transitionDuration", "transitionProperty"];
    const style = (element: Element | null): Record<string, string> => Object.fromEntries(fields.map((field) => [field, element ? Reflect.get(getComputedStyle(element), field) as string : ""]));
    const box = (element: Element | null) => { const rect = element?.getBoundingClientRect(); return { x: rect?.x ?? 0, y: rect?.y ?? 0, width: rect?.width ?? 0, height: rect?.height ?? 0 }; };
    const all = [document.documentElement, document.body, ...Array.from(document.querySelectorAll("body *"))];
    return { body: style(document.body), main: style(document.querySelector("main")), card: style(card), grid: style(grid), viewport: style(viewport), title: style(document.querySelector("h1")),
      cardBox: box(card), gridBox: box(grid), viewportBox: box(viewport), mainBox: box(document.querySelector("main")), width: innerWidth, height: innerHeight, pageWidth: document.documentElement.scrollWidth,
      colors: all.flatMap((element) => { const style = getComputedStyle(element); return [style.color, style.backgroundColor, style.borderColor]; }),
      secondary: all.some((element) => getComputedStyle(element).fontSize === "14px"),
      spacing: all.filter((element) => !(element instanceof SVGElement) && !element.closest("svg") && !["OPTION", "SELECT", "BUTTON", "INPUT", "TEXTAREA", "SUMMARY"].includes(element.tagName)).flatMap((element) => { const style = getComputedStyle(element); return [style.marginTop, style.marginBottom, style.marginLeft, style.marginRight, style.paddingTop, style.paddingBottom, style.paddingLeft, style.paddingRight, style.rowGap, style.columnGap]; }),
      canvas: document.querySelectorAll("canvas").length, svg: card.querySelectorAll("svg").length, viewportMeta: document.querySelector('meta[name="viewport"]')?.getAttribute("content") ?? "",
    };
  }, CardAttribute);
}

export async function layout(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence), measured = await styles(page);
  put("V02.1", near(pixels(measured.main["maxWidth"] ?? ""), 1120) && near(measured.mainBox.x, (measured.width - measured.mainBox.width) / 2), "Measured main max-width and centered geometry.");
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
  for (const [id, color] of [["V06.1", "#090B14"], ["V06.2", "#141827"], ["V06.5", "#F4F6FF"], ["V06.6", "#B8BDD2"]]) put(id ?? "", palette.includes(color ?? ""), `Computed element colors include ${color}.`);
  put("V06.3", rgb(closed.card["backgroundColor"] ?? "") === "#1B2034", "Closed card background matches its token.");
  put("V06.7", rgb(closed.card["borderColor"] ?? "") === "#757C9A", "Closed card border matches its token.");
  put("V08.1 T08.3", /(?:system-ui|-apple-system|BlinkMacSystemFont)/i.test(closed.body["fontFamily"] ?? "") && /sans-serif/i.test(closed.body["fontFamily"] ?? ""), "Computed body font uses the system sans-serif stack.");
  put("V08.2", closed.body["fontSize"] === "16px" && closed.body["lineHeight"] === "24px", "Body text is 16px on 24px lines.");
  put("V08.3", closed.title["fontSize"] === "24px", "Title text is 24px.");
  put("V08.4", closed.secondary, "Secondary text includes the specified 14px size.");
  put("V09.1", closed.spacing.every((value) => value === "normal" || value === "auto" || Rules.spacing.some((allowed) => near(pixels(value), allowed))), "Computed layout spacing uses the scale, with native controls excluded.");
  put("V09.2", closed.card["borderWidth"] === "2px" && closed.card["borderRadius"] === "8px" && closed.body["fontSize"] === "16px", "Border, radius and font metrics keep their separate stated values.");
  await click(page, 0); const open = await styles(page);
  put("V01.3", open.svg > 0 && open.canvas === 0, "Revealed card artwork uses inline SVG.");
  put("V06.4", rgb(open.card["backgroundColor"] ?? "") === "#111625", "Revealed card background matches its token.");
  await reset(page); await card(page, 0).focus(); await page.keyboard.press("ArrowRight"); await page.keyboard.press("ArrowLeft");
  const focused = await styles(page);
  put("V06.8 V16.1", focused.card["outlineWidth"] === "3px" && focused.card["outlineOffset"] === "2px" && rgb(focused.card["outlineColor"] ?? "") === "#FFFFFF", "Keyboard focus has a 3px white outline offset by 2px.");
  await card(page, 0).hover(); await step(page, 120); const hovered = await styles(page);
  put("V16.2", ["backgroundColor", "opacity", "borderWidth", "borderRadius"].every((field) => hovered.card[field] === focused.card[field]), "Hover keeps fill, opacity, border width and radius.");
  put("V17.1", (transitionMs(focused.card["transitionDuration"] ?? "") ?? [NaN]).every((ms) => ms === 120) && /border/.test(focused.card["transitionProperty"] ?? "") && /opacity/.test(focused.card["transitionProperty"] ?? ""), "Only stated border and opacity properties have 120ms transitions.");
  await page.emulateMedia({ reducedMotion: "reduce" }); const reduced = await styles(page);
  put("V17.2", (transitionMs(reduced.card["transitionDuration"] ?? "") ?? [NaN]).every((ms) => ms === 0), "Reduced motion removes card transitions.");
}

export async function faces(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence); await changeSize(page, 16);
  const closed = await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.map((element) => element.innerHTML.replace(/\s+/g, " ")));
  put("V14.1", new Set(closed).size === 1 && /(?:polygon|path|diamond|◇|◊|♦|♢)/i.test(closed[0] ?? ""), "All hidden cards have the same neutral diamond back.");
  const deck = await visibleDeck(page), unique = new Set(deck);
  put("V10.1", unique.size === 128, `Visible play found ${unique.size} named identities.`);
  put("V10.3 V10.4 V10.5 V13.1 V13.2", [...unique].every((name) => faceIn(name) === name) && Rules.shapes.every((shape) => deck.some((name) => name.includes(` ${shape},`))) && Rules.markers.every((marker) => deck.some((name) => name.endsWith(`marker ${marker}`))) && ["solid", "outline"].every((fill) => deck.some((name) => name.includes(` ${fill} `))), "Visible names span all eight shapes, markers and both fills in the specified lowercase form.");
  const colors = await page.locator(`[${CardAttribute}] svg *`).evaluateAll((elements) => elements.flatMap((element) => [getComputedStyle(element).fill, getComputedStyle(element).stroke]));
  put("V07.1", Rules.palette.every((color) => colors.map(rgb).includes(color)), "Revealed SVG artwork uses each specified palette entry.");
  const matched = await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.filter((element) => element.getAttribute("aria-disabled") === "true").map((element) => ({ svg: element.querySelectorAll("svg").length, opacity: getComputedStyle(element).opacity, color: getComputedStyle(element).borderColor, text: element.textContent ?? "", html: element.innerHTML })));
  put("V15.1", matched.length > 0 && matched.every((entry) => entry.svg > 0), "Revealed matched cards retain inline SVG artwork.");
  put("V15.2", matched.length > 0 && matched.every((entry) => rgb(entry.color) === Rules.palette[2] && /(?:check|✓|✔|polyline)/i.test(entry.html + entry.text)), "Every matched card has a green border and a check mark.");
  put("V15.3", matched.length > 0 && matched.every((entry) => entry.opacity === "1"), "Matched cards keep full opacity.");
}

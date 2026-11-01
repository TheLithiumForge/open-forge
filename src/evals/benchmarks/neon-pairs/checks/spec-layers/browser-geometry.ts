import type { Page } from "playwright";
import { card } from "../board.ts";
import { loadBoard } from "./browser-formats.ts";
import { click, step } from "./browser-session.ts";
import { reporter } from "./browser-play.ts";
import { Evidence } from "./outcomes.ts";
import { expectedDeck } from "./board-code.ts";
import { Rules } from "./rules.ts";
import { near, rgb } from "./view-values.ts";

export async function svgGeometry(page: Page, evidence: Evidence): Promise<void> {
  const put = reporter(evidence); await loadBoard(page, "np1-16-1");
  const deck = expectedDeck("np1-16-1");
  const found: { viewBox: string; shapes: { box: { x: number; y: number; width: number; height: number }; tag: string; radius: number; stroke: string; width: number; fill: string }[] }[] = [];
  for (let index = 0; index < deck.length; index += 2) {
    await click(page, index); await click(page, index + 1);
    for (const position of [index, index + 1]) found.push(await card(page, position).locator("svg").first().evaluate((svg) => ({
      viewBox: svg.getAttribute("viewBox") ?? "", shapes: Array.from(svg.querySelectorAll("circle,rect,path,polygon,polyline")).flatMap((element) => {
        if (!(element instanceof SVGGraphicsElement)) return [];
        const box = element.getBBox(), style = getComputedStyle(element);
        return [{ box: { x: box.x, y: box.y, width: box.width, height: box.height }, tag: element.tagName, radius: Number(element.getAttribute("r")), stroke: style.stroke, width: Number.parseFloat(style.strokeWidth), fill: style.fill }];
      }),
    })));
    await step(page, Rules.mismatchMs);
  }
  const main = found.map((svg) => svg.shapes.find((shape) => shape.box.width >= 40 && shape.box.height >= 40));
  const marker = found.map((svg) => svg.shapes.filter((shape) => shape.tag === "circle" && shape.radius === 5).at(-1));
  put("V12.1", found.every((svg) => svg.viewBox.replaceAll(",", " ").trim().replace(/\s+/g, " ") === "0 0 100 100"), "Every revealed face uses the specified viewBox.");
  put("V12.2", main.every((shape) => shape && shape.box.x >= 24.9 && shape.box.y >= 24.9 && shape.box.x + shape.box.width <= 75.1 && shape.box.y + shape.box.height <= 75.1 && near(shape.box.x + shape.box.width / 2, 50) && near(shape.box.y + shape.box.height / 2, 50)), "Main shape bounding boxes are centered within 25..75 on both axes.");
  put("V12.3", marker.every((shape, index) => shape && near(Math.hypot(shape.box.x + 5 - 50, shape.box.y + 5 - 50), 40) && found[index]?.shapes.at(-1) === shape), "Radius-5 markers are drawn last and centered forty units from the face center.");
  put("V12.4", marker.every((shape) => shape && shape.width === 3 && rgb(shape.stroke) === "#111625"), "Markers use a 3-unit Face-background stroke.");
  put("V12.5", marker.every((shape) => !shape || near(shape.box.x + 5, 50) || near(shape.box.y + 5, 50) || near(Math.abs(shape.box.x + 5 - 50), 28.284, 0.01) && near(Math.abs(shape.box.y + 5 - 50), 28.284, 0.01)), "Diagonal marker offsets use the normalized 28.284-unit components.");
  put("V12.6", main.every((shape, index) => (deck[index] ?? 0) < 64 || shape?.width === 5 && shape.fill === "none"), "Outlined main shapes use a 5-unit stroke and no fill.");
  put("V01.2", await card(page, 0).evaluate((element) => { let node = element.parentElement; while (node && getComputedStyle(node).display !== "grid") node = node.parentElement; return element.tagName === "BUTTON" && node !== null; }), "DOM card buttons sit in a CSS Grid.");
  put("V01.4", await page.locator("canvas,[data-slot]").count() === 0, "No canvas or shadcn data-slot components occur in the rendered game.");
}

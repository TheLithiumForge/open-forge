import type { Page } from "playwright";
import { CardAttribute } from "../board.ts";
import type { Contour } from "./drawn-shapes.ts";

export async function drawings(page: Page): Promise<Contour[][]> {
  return page.locator(`[${CardAttribute}]`).evaluateAll((cards) => cards.map((card) => {
    const contours: { points: { x: number; y: number }[]; color: string }[] = [];
    for (const element of Array.from(card.querySelectorAll("*"))) {
      const style = getComputedStyle(element), bounds = element.getBoundingClientRect();
      if (bounds.width === 0 || bounds.height === 0 || style.visibility !== "visible" || Number(style.opacity) === 0) continue;
      if (element instanceof SVGGeometryElement) {
        const color = style.stroke !== "none" && parseFloat(style.strokeWidth) > 0 ? style.stroke : style.fill;
        const matrix = element.getScreenCTM();
        if (!matrix || color === "none" || color === "rgba(0, 0, 0, 0)") continue;
        const length = element.getTotalLength();
        contours.push({ color, points: Array.from({ length: 65 }, (_, index) => {
          const point = element.getPointAtLength(length * index / 64).matrixTransform(matrix);
          return { x: point.x, y: point.y };
        }) });
      } else if (style.transform !== "none" && parseFloat(style.borderRadius) === 0 && parseFloat(style.borderTopWidth) > 0 && parseFloat(style.borderRightWidth) > 0 && parseFloat(style.borderBottomWidth) > 0 && parseFloat(style.borderLeftWidth) > 0) {
        const matrix = new DOMMatrix(style.transform), width = element.clientWidth, height = element.clientHeight;
        contours.push({ color: style.borderTopColor, points: [[0, 0], [width, 0], [width, height], [0, height], [0, 0]].map(([x = 0, y = 0]) => {
          const point = new DOMPoint(x, y).matrixTransform(matrix); return { x: point.x, y: point.y };
        }) });
      }
    }
    return contours;
  }));
}

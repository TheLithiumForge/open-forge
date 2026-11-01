import type { Page } from "playwright";
import { CardAttribute } from "../board.ts";
import { LayoutAttribute } from "./browser-layout.ts";
import { Labels } from "./browser-session.ts";
import { Rules } from "./rules.ts";
import { faceIn, rgb, transitionMs, VisualTiming } from "./view-values.ts";

export const TargetClause = { panel: "V06.2", secondaryColor: "V06.6", secondarySize: "V08.4", palette: "V10.9", paints: "V12.8", hover: "V16.2", transition: "V17.1" } as const;
export const ColorToken = { panel: "#141827", secondary: "#B8BDD2", text: "#F4F6FF", face: "#111625", green: Rules.palette[2] } as const;
export function facePalette(name: string, paints: readonly string[]): boolean {
  const face = faceIn(name), marker = face?.split(", marker ")[1];
  const index = Rules.markers.findIndex((value) => value === marker);
  return index >= 0 && face?.startsWith(`${Rules.colors[index]} `) === true && paints.map(rgb).includes(Rules.palette[index] ?? "");
}
export function facePaint(id: number, shape: { readonly fill: string; readonly stroke: string }, marker: { readonly fill: string; readonly stroke: string }): boolean {
  const color = Rules.palette[Math.floor(id / 8) % 8];
  return rgb(id < 64 ? shape.fill : shape.stroke) === color && (id < 64 || shape.fill === "none") && rgb(marker.fill) === color && rgb(marker.stroke) === ColorToken.face;
}
export function borderTransition(property: string, duration: string): boolean {
  const properties = property.split(",").map((value) => value.trim());
  return properties.length > 0 && properties.every((value) => /^border(?:-(?:top|right|bottom|left))?-color$/.test(value)) && (transitionMs(duration) ?? [NaN]).every((value) => value === VisualTiming.transitionMs);
}
export async function namedTargets(page: Page) {
  return page.evaluate(({ attribute, layout, reproduce }) => {
    const first = document.querySelector(`[${attribute}]`), board = document.querySelector(`[${layout}="board"]`);
    let viewport = board;
    while (viewport?.parentElement && !/(auto|scroll)/.test(getComputedStyle(viewport).overflowX + getComputedStyle(viewport).overflowY)) viewport = viewport.parentElement;
    const panels = [viewport, ...Array.from(document.querySelectorAll(`button:not([${attribute}]),select,input,textarea`)), ...Array.from(document.querySelectorAll("details")).filter((element) => element.querySelector("summary")?.textContent?.trim() === reproduce)];
    const secondary = new Set<Element>();
    for (let parent = first; parent; parent = parent.parentElement) for (const id of (parent.getAttribute("aria-describedby") ?? "").split(/\s+/)) {
      const described = document.getElementById(id); if (described) secondary.add(described);
    }
    const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
    while (walker.nextNode()) {
      const node = walker.currentNode;
      if (node.parentElement && /^(?:Moves|Pairs|Time|Best):$/.test(node.textContent?.trim() ?? "")) secondary.add(node.parentElement);
    }
    for (const element of document.querySelectorAll('[role="note"]')) secondary.add(element);
    const shown = (element: Element | null): element is Element => element !== null && element.getClientRects().length > 0 && getComputedStyle(element).visibility === "visible";
    return {
      panels: panels.filter(shown).map((element) => getComputedStyle(element).backgroundColor),
      secondary: [...secondary].filter(shown).map((element) => ({ color: getComputedStyle(element).color, size: getComputedStyle(element).fontSize })),
    };
  }, { attribute: CardAttribute, layout: LayoutAttribute, reproduce: Labels.reproduce });
}

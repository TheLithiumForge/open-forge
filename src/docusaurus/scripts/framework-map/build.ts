// Writes the static framework diagram used by the README, in a light and a dark
// version, from the same data as the site's interactive diagram.
// Run from src/docusaurus: npm run diagram
import { writeFileSync } from "node:fs";
import { childRouteNote, coreRoles, legendOrder, loadingLabels, loadingNotes, memoryStates, taskSteps } from "../../src/components/framework-map/framework-map-data.ts";
import type { Loading } from "../../src/components/framework-map/framework-map-data.ts";
import { box, FONT, MONO, pill, text, themes, wrap } from "./svg-parts.ts";
import type { Theme } from "./svg-parts.ts";

const WIDTH = 900;
const PANEL_TOP = 118;
const CORE = { x: 20, width: 520, cardWidth: 241, cardHeight: 112, gap: 10 };
const MEMORY = { x: 556, width: 324, stateHeight: 72, gap: 24 };
const PANEL_HEIGHT = 418;
const STEPS_TOP = PANEL_TOP + PANEL_HEIGHT + 24;
const LEGEND = { top: STEPS_TOP + 52, x: 150, noteX: 342, rowHeight: 26 };
const NOTE_TOP = LEGEND.top + legendOrder.length * LEGEND.rowHeight + 18;
const HEIGHT = NOTE_TOP + 44;
const OUTPUT = { light: "static/img/framework-map-light.svg", dark: "static/img/framework-map-dark.svg" } as const;

function badge(x: number, y: number, loading: Loading, theme: Theme): string {
  const label = loadingLabels[loading];
  if (loading === "startup") return pill(x, y, label, theme.line, theme.line, theme.onLine);
  if (loading === "listed") return pill(x, y, label, "none", theme.line, theme.line);
  if (loading === "checkpoints") return pill(x, y, label, "none", theme.memory, theme.memory, true);
  return pill(x, y, label, "none", theme.muted, theme.muted, true);
}

function entry(theme: Theme): string {
  const node = (x: number, width: number, title: string, note: string): string =>
    box(x, 20, width, 56, theme.panel, theme.line) +
    text(x + width / 2, 42, [title], { size: 14, fill: theme.text, weight: 700, family: MONO, anchor: "middle" }) +
    text(x + width / 2, 62, [note], { size: 12, fill: theme.muted, anchor: "middle" });
  const arrow = `<path d="M405 48 H447" stroke="${theme.line}" stroke-width="2"/><path d="M447 43 L455 48 L447 53 Z" fill="${theme.line}"/>`;
  const coreCenter = CORE.x + CORE.width / 2;
  const memoryCenter = MEMORY.x + MEMORY.width / 2;
  const fork = `<path d="M605 76 V98 M${coreCenter} ${PANEL_TOP} V98 H${memoryCenter} V${PANEL_TOP}" fill="none" stroke="${theme.line}" stroke-width="2"/>`;
  return node(155, 250, "AGENTS.md", "Tells the agent to read the loader") + arrow + node(455, 300, ".agents/loader.md", "The rules, and the routes to everything else") + fork;
}

function panelTitle(x: number, title: string, subtitle: string, theme: Theme): string {
  return `<text x="${x}" y="${PANEL_TOP + 30}" font-family="${FONT}"><tspan font-size="17" font-weight="700" fill="${theme.text}">${title}</tspan><tspan dx="8" font-size="11" font-weight="600" fill="${theme.muted}" letter-spacing="1">${subtitle}</tspan></text>`;
}

function core(theme: Theme): string {
  const cards = coreRoles.map((role, index) => {
    const x = CORE.x + 14 + (index % 2) * (CORE.cardWidth + CORE.gap);
    const y = PANEL_TOP + 46 + Math.floor(index / 2) * (CORE.cardHeight + CORE.gap);
    const roleLines = wrap(role.role, CORE.cardWidth - 24, 13);
    return (
      box(x, y, CORE.cardWidth, CORE.cardHeight, theme.panel, theme.border, 9, 1) +
      text(x + 12, y + 22, [role.name], { size: 15, fill: theme.line, weight: 700 }) +
      text(x + 12, y + 42, roleLines, { size: 13, fill: theme.text }) +
      text(x + 12, y + 42 + roleLines.length * 17, [role.example], { size: 11.5, fill: theme.muted }) +
      badge(x + 12, y + CORE.cardHeight - 26, role.loading, theme)
    );
  });
  return box(CORE.x, PANEL_TOP, CORE.width, PANEL_HEIGHT, theme.panel, theme.line, 14) + panelTitle(CORE.x + 14, "Core", "HOW TO WORK", theme) + cards.join("");
}

function memory(theme: Theme): string {
  const states = memoryStates.map((state, index) => {
    const x = MEMORY.x + 14;
    const y = PANEL_TOP + 46 + index * (MEMORY.stateHeight + MEMORY.gap);
    const width = MEMORY.width - 28;
    const transition = state.next ? text(x + width / 2, y + MEMORY.stateHeight + 15, [`↓ ${state.next}`], { size: 11.5, fill: theme.muted, anchor: "middle" }) : "";
    return (
      box(x, y, width, MEMORY.stateHeight, theme.panel, theme.border, 9, 1) +
      text(x + 12, y + 21, [state.name], { size: 14.5, fill: theme.memory, weight: 700 }) +
      text(x + 12, y + 40, wrap(`${state.tense}. ${state.holds}`, width - 24, 12), { size: 12, fill: theme.text }, 15) +
      badge(x + width - 12 - Math.ceil(loadingLabels[state.loading].length * 6.6 + 16), y + 8, state.loading, theme) +
      transition
    );
  });
  return box(MEMORY.x, PANEL_TOP, MEMORY.width, PANEL_HEIGHT, theme.panel, theme.memory, 14) + panelTitle(MEMORY.x + 14, "Memory", "WHAT TO REMEMBER", theme) + states.join("");
}

function steps(theme: Theme): string {
  const y = STEPS_TOP;
  const widths = taskSteps.map((step) => Math.ceil(step.length * 13 * 0.56) + 44);
  let x = (WIDTH - widths.reduce((sum, width) => sum + width, 0) - (taskSteps.length - 1) * 10) / 2;
  const pills = taskSteps.map((step, index) => {
    const width = widths[index] ?? 0;
    const markup =
      box(x, y, width, 28, theme.panel, theme.border, 14, 1) +
      `<circle cx="${x + 15}" cy="${y + 14}" r="10" fill="${theme.line}"/>` +
      text(x + 15, y + 18, [String(index + 1)], { size: 11, fill: theme.onLine, weight: 700, anchor: "middle" }) +
      text(x + 31, y + 18.5, [step], { size: 13, fill: theme.text });
    x += width + 10;
    return markup;
  });
  return pills.join("");
}

function legend(theme: Theme): string {
  const rows = legendOrder.map((loading, index) => {
    const y = LEGEND.top + index * LEGEND.rowHeight;
    return badge(LEGEND.x, y, loading, theme) + text(LEGEND.noteX, y + 13, [loadingNotes[loading]], { size: 12, fill: theme.muted });
  });
  const note = text(WIDTH / 2, NOTE_TOP, wrap(childRouteNote, WIDTH - 120, 12.5), { size: 12.5, fill: theme.text, weight: 600, anchor: "middle" }, 17);
  return rows.join("") + note;
}

function diagram(theme: Theme): string {
  const height = HEIGHT;
  const title = "How Open Forge fits together: AGENTS.md points to the loader, which routes to Core (how to work) and Memory (what to remember).";
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${WIDTH} ${height}" width="${WIDTH}" height="${height}" role="img" aria-label="${title}"><title>${title}</title>${box(0.5, 0.5, WIDTH - 1, height - 1, theme.canvas, theme.border, 16, 1)}${entry(theme)}${core(theme)}${memory(theme)}${steps(theme)}${legend(theme)}</svg>\n`;
}

for (const name of ["light", "dark"] as const) {
  writeFileSync(OUTPUT[name], diagram(themes[name]));
  console.log(`wrote ${OUTPUT[name]}`);
}

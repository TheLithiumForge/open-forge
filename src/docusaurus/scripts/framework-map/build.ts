// Writes the static framework diagram used by the README, in a light and a dark
// version, from the same data as the site's interactive diagram.
// Run from src/docusaurus: npm run diagram
import { writeFileSync } from "node:fs";
import {
  captions,
  coreRoles,
  diagramDescription,
  entryNodes,
  entrypointNote,
  linkedFiles,
  loadingGuide,
  loadingLabels,
  memoryLoading,
  memoryNote,
  memoryStates,
  pitch,
  startupFootprint,
  taskSteps,
} from "../../src/components/framework-map/framework-map-data.ts";
import type { FileRow, Loading } from "../../src/components/framework-map/framework-map-data.ts";
import { siteBaseUrl, siteUrl } from "../../src/config/site.ts";
import { box, escape, FONT, MONO, pill, text, themes, wrap } from "./svg-parts.ts";
import type { Theme } from "./svg-parts.ts";

const WIDTH = 900;
const CORE = { x: 20, width: 520, cardWidth: 241, gap: 10 };
const MEMORY = { x: 556, width: 324, gap: 24 };
const NODES = [
  { x: 110, width: 300 },
  { x: 470, width: 320 },
] as const;
const OUTPUT = { light: "static/img/framework-map-light.svg", dark: "static/img/framework-map-dark.svg" } as const;

// The two loading states: tinted with a solid outline, or unfilled with a dashed one.
function region(x: number, y: number, width: number, height: number, loading: Loading, theme: Theme, radius = 7): string {
  if (loading === "startup") return box(x, y, width, height, theme.chip, theme.line, radius, 1);
  return `<rect x="${x}" y="${y}" width="${width}" height="${height}" rx="${radius}" fill="none" stroke="${theme.muted}" stroke-width="1" stroke-dasharray="4 3"/>`;
}

function badge(x: number, y: number, loading: Loading, theme: Theme): string {
  const label = loadingLabels[loading];
  if (loading === "startup") return pill(x, y, label, theme.panel, theme.line, theme.line);
  return pill(x, y, label, "none", theme.muted, theme.muted, true);
}

interface Lines {
  readonly lines: readonly string[];
  readonly size: number;
  readonly lineHeight: number;
}

const block = (value: string, width: number, size: number): Lines => ({ lines: wrap(value, width, size), size, lineHeight: size + 4 });
const blockHeight = (lines: Lines): number => lines.lines.length * lines.lineHeight;

interface Card {
  readonly height: number;
  readonly draw: (x: number, y: number, height: number, theme: Theme) => string;
}

interface CardContent {
  readonly title: string;
  readonly titleFill: (theme: Theme) => string;
  readonly summary: string;
  readonly loading: Loading;
  readonly loadingNote?: string | undefined;
  readonly rows: readonly FileRow[];
  readonly lines: readonly string[];
}

// A card is a folder: its index (title, summary, badge) in the upper region,
// then a row for what the index links to, then examples.
function card(content: CardContent, width: number): Card {
  const inner = width - 12;
  const textWidth = inner - 18;
  const summary = block(content.summary, textWidth, 13);
  const note = content.loadingNote === undefined ? undefined : block(content.loadingNote, textWidth, 11.5);
  const indexHeight = 30 + blockHeight(summary) + 24 + (note ? blockHeight(note) + 2 : 0) + 6;
  const rows = content.rows.map((row) => ({ row, lines: block(row.text, inner - 16, 10.5) }));
  const rowsHeight = rows.reduce((sum, row) => sum + blockHeight(row.lines) + 8 + 4, 0);
  const lines = content.lines.map((line) => block(line, inner - 6, 11.5));
  const linesHeight = lines.reduce((sum, line) => sum + blockHeight(line) + 2, 0);
  const height = 6 + indexHeight + 6 + rowsHeight + linesHeight + 8;

  const draw = (x: number, y: number, cardHeight: number, theme: Theme): string => {
    let markup = box(x, y, width, cardHeight, theme.panel, theme.border, 9, 1);
    const ix = x + 6;
    const iy = y + 6;
    markup += region(ix, iy, inner, indexHeight, content.loading, theme);
    markup += text(ix + 9, iy + 21, [content.title], { size: 15, fill: content.titleFill(theme), weight: 700 });
    let cursor = iy + 38;
    markup += text(ix + 9, cursor, summary.lines, { size: 13, fill: theme.text }, summary.lineHeight);
    cursor += blockHeight(summary) - 8;
    markup += badge(ix + 9, cursor, content.loading, theme);
    cursor += 32;
    if (note) markup += text(ix + 9, cursor, note.lines, { size: 11.5, fill: theme.muted }, note.lineHeight);

    let rowY = iy + indexHeight + 6;
    for (const { row, lines: rowLines } of rows) {
      const rowHeight = blockHeight(rowLines) + 8;
      markup += region(ix, rowY, inner, rowHeight, row.loading, theme, 6);
      markup += text(ix + 8, rowY + 14.5, rowLines.lines, { size: 10.5, fill: theme.text, weight: 600 }, rowLines.lineHeight);
      rowY += rowHeight + 4;
    }
    let lineY = rowY + 12;
    for (const line of lines) {
      markup += text(ix + 3, lineY, line.lines, { size: 11.5, fill: theme.muted }, line.lineHeight);
      lineY += blockHeight(line) + 2;
    }
    return markup;
  };
  return { height, draw };
}

const coreCards = coreRoles.map((role) =>
  card(
    {
      title: role.name,
      titleFill: (theme) => theme.line,
      summary: role.role,
      loading: role.loading,
      rows: role.files ?? [linkedFiles],
      lines: [role.example, ...(role.contents === undefined ? [] : [role.contents])],
    },
    CORE.cardWidth,
  ),
);
const memoryCards = memoryStates.map((state) =>
  card(
    {
      title: `${state.name} · ${state.tense}`,
      titleFill: (theme) => theme.memory,
      summary: state.holds,
      loading: state.loading,
      loadingNote: state.loadingNote,
      rows: [linkedFiles],
      lines: state.contents === undefined ? [] : [state.contents],
    },
    MEMORY.width - 28,
  ),
);

// Vertical layout, top to bottom.
const PITCH = { top: 34, pitch: block(pitch, 760, 15), footprint: block(startupFootprint, 760, 12) };
const ENTRY_TOP = PITCH.top + blockHeight(PITCH.pitch) + blockHeight(PITCH.footprint) + 14;
const nodeNotes = NODES.map((node, index) => block(entryNodes[index]?.note ?? "", node.width - 24, 12));
const ENTRY_HEIGHT = 38 + Math.max(...nodeNotes.map(blockHeight));
const FORK_Y = ENTRY_TOP + ENTRY_HEIGHT + 18;
const coreCenter = CORE.x + CORE.width / 2;
const memoryCenter = MEMORY.x + MEMORY.width / 2;
const NOTE = block(entrypointNote, memoryCenter - coreCenter - 44, 12);
const PANEL_TOP = FORK_Y + 20 + blockHeight(NOTE) + 12;

// Memory's heading carries the badge for its own entrypoint.
const MEMORY_HEADING = 70;
const coreRows = [0, 2, 4].map((index) => Math.max(coreCards[index]?.height ?? 0, coreCards[index + 1]?.height ?? 0));
const coreHeight = 46 + coreRows.reduce((sum, row) => sum + row, 0) + (coreRows.length - 1) * CORE.gap + 14;
const memoryNoteLines = wrap(memoryNote, MEMORY.width - 28, 11.5);
const memoryHeight = MEMORY_HEADING + memoryCards.reduce((sum, item) => sum + item.height, 0) + (memoryCards.length - 1) * MEMORY.gap + 16 + memoryNoteLines.length * 15 + 8;
const PANEL_HEIGHT = Math.max(coreHeight, memoryHeight);
const STEPS_TOP = PANEL_TOP + PANEL_HEIGHT + 24;
const CAPTION = { top: STEPS_TOP + 58, width: WIDTH - 160 };
const GUIDE_HEIGHT = 24;
const captionBlocks = captions.map((caption, index) => ({ ...block(caption, CAPTION.width, index === 0 ? 12 : 11.5), first: index === 0 }));
const HEIGHT = CAPTION.top + GUIDE_HEIGHT + captionBlocks.reduce((sum, item) => sum + blockHeight(item) + 8, 0) + 8;

function header(theme: Theme): string {
  return (
    text(WIDTH / 2, PITCH.top, PITCH.pitch.lines, { size: 15, fill: theme.text, weight: 700, anchor: "middle" }, PITCH.pitch.lineHeight + 1) +
    text(WIDTH / 2, PITCH.top + blockHeight(PITCH.pitch) + 6, PITCH.footprint.lines, { size: 12, fill: theme.muted, anchor: "middle" }, PITCH.footprint.lineHeight)
  );
}

function entry(theme: Theme): string {
  const nodes = NODES.map((node, index) => {
    const file = entryNodes[index]?.file ?? "";
    const center = node.x + node.width / 2;
    const note = nodeNotes[index];
    return (
      region(node.x, ENTRY_TOP, node.width, ENTRY_HEIGHT, "startup", theme, 10) +
      text(center, ENTRY_TOP + 24, [file], { size: 14, fill: theme.text, weight: 700, family: MONO, anchor: "middle" }) +
      text(center, ENTRY_TOP + 43, note?.lines ?? [], { size: 12, fill: theme.muted, anchor: "middle" }, note?.lineHeight ?? 16)
    );
  });
  const arrowY = ENTRY_TOP + ENTRY_HEIGHT / 2;
  const arrow = `<path d="M${NODES[0].x + NODES[0].width} ${arrowY} H462" stroke="${theme.line}" stroke-width="2"/><path d="M462 ${arrowY - 5} L470 ${arrowY} L462 ${arrowY + 5} Z" fill="${theme.line}"/>`;
  const stem = NODES[1].x + NODES[1].width / 2;
  const fork = `<path d="M${stem} ${ENTRY_TOP + ENTRY_HEIGHT} V${FORK_Y} M${coreCenter} ${PANEL_TOP} V${FORK_Y} H${memoryCenter} V${PANEL_TOP}" fill="none" stroke="${theme.line}" stroke-width="2"/>`;
  const note = text((coreCenter + memoryCenter) / 2, FORK_Y + 24, NOTE.lines, { size: 12, fill: theme.text, anchor: "middle" }, NOTE.lineHeight);
  return nodes.join("") + arrow + fork + note;
}

function panelTitle(x: number, title: string, subtitle: string, theme: Theme): string {
  return `<text x="${x}" y="${PANEL_TOP + 30}" font-family="${FONT}"><tspan font-size="17" font-weight="700" fill="${theme.text}">${title}</tspan><tspan dx="8" font-size="11" font-weight="600" fill="${theme.muted}" letter-spacing="1">${subtitle}</tspan></text>`;
}

// Core rows share any height Memory needs beyond them, so the panels end together.
const coreStretch = (PANEL_HEIGHT - coreHeight) / coreRows.length;

function core(theme: Theme): string {
  let y = PANEL_TOP + 46;
  let markup = box(CORE.x, PANEL_TOP, CORE.width, PANEL_HEIGHT, theme.panel, theme.line, 14) + panelTitle(CORE.x + 14, "Core", "HOW TO WORK", theme);
  coreRows.forEach((baseHeight, row) => {
    const rowHeight = baseHeight + coreStretch;
    for (const column of [0, 1]) {
      const item = coreCards[row * 2 + column];
      if (item) markup += item.draw(CORE.x + 14 + column * (CORE.cardWidth + CORE.gap), y, rowHeight, theme);
    }
    y += rowHeight + CORE.gap;
  });
  return markup;
}

function memory(theme: Theme): string {
  const x = MEMORY.x + 14;
  const width = MEMORY.width - 28;
  let y = PANEL_TOP + MEMORY_HEADING;
  let markup =
    box(MEMORY.x, PANEL_TOP, MEMORY.width, PANEL_HEIGHT, theme.panel, theme.memory, 14) +
    panelTitle(x, "Memory", "WHAT TO REMEMBER", theme) +
    badge(x, PANEL_TOP + 40, memoryLoading, theme);
  memoryStates.forEach((state, index) => {
    const item = memoryCards[index];
    if (!item) return;
    markup += item.draw(x, y, item.height, theme);
    y += item.height;
    if (state.next) markup += text(x + width / 2, y + 16, [`↓ ${state.next}`], { size: 11.5, fill: theme.muted, anchor: "middle" });
    y += MEMORY.gap;
  });
  return markup + text(x, PANEL_TOP + PANEL_HEIGHT - 14 - (memoryNoteLines.length - 1) * 15, memoryNoteLines, { size: 11.5, fill: theme.muted }, 15);
}

// The first two steps repeat the two loading styles.
function steps(theme: Theme): string {
  const widths = taskSteps.map((step) => Math.ceil(step.text.length * 13 * 0.56) + 44);
  let x = (WIDTH - widths.reduce((sum, width) => sum + width, 0) - (taskSteps.length - 1) * 10) / 2;
  return taskSteps
    .map((step, index) => {
      const width = widths[index] ?? 0;
      const outline = step.loading ? region(x, STEPS_TOP, width, 28, step.loading, theme, 14) : box(x, STEPS_TOP, width, 28, theme.panel, theme.border, 14, 1);
      const markup =
        outline +
        `<circle cx="${x + 15}" cy="${STEPS_TOP + 14}" r="10" fill="${theme.line}"/>` +
        text(x + 15, STEPS_TOP + 18, [String(index + 1)], { size: 11, fill: theme.onLine, weight: 700, anchor: "middle" }) +
        text(x + 31, STEPS_TOP + 18.5, [step.text], { size: 13, fill: theme.text });
      x += width + 10;
      return markup;
    })
    .join("");
}

function caption(theme: Theme): string {
  const href = new URL(`${siteBaseUrl}${loadingGuide.href.slice(1)}`, siteUrl).href;
  const guide = `<a href="${escape(href)}" text-decoration="underline">${text(WIDTH / 2, CAPTION.top, [loadingGuide.label], { size: 12, fill: theme.line, anchor: "middle" })}</a>`;
  let y = CAPTION.top + GUIDE_HEIGHT;
  return (
    guide + captionBlocks
      .map((item) => {
        const style = item.first ? { size: item.size, fill: theme.text, anchor: "middle" as const } : { size: item.size, fill: theme.muted, anchor: "middle" as const };
        const markup = text(WIDTH / 2, y, item.lines, style, item.lineHeight);
        y += blockHeight(item) + 8;
        return markup;
      })
      .join("")
  );
}

function diagram(theme: Theme): string {
  const title = escape(diagramDescription);
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${WIDTH} ${HEIGHT}" width="${WIDTH}" height="${HEIGHT}" role="img" aria-label="${title}"><title>${title}</title>${box(0.5, 0.5, WIDTH - 1, HEIGHT - 1, theme.canvas, theme.border, 16, 1)}${header(theme)}${entry(theme)}${core(theme)}${memory(theme)}${steps(theme)}${caption(theme)}</svg>\n`;
}

for (const name of ["light", "dark"] as const) {
  writeFileSync(OUTPUT[name], diagram(themes[name]));
  console.log(`wrote ${OUTPUT[name]}`);
}

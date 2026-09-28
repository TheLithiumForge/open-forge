// Small SVG building blocks for the static framework diagram.

import { codeSpans } from "../../src/components/framework-map/code-spans.ts";

export interface Theme {
  readonly canvas: string;
  readonly panel: string;
  readonly text: string;
  readonly muted: string;
  readonly line: string;
  readonly memory: string;
  readonly border: string;
  readonly onLine: string;
  readonly chip: string;
}

export const themes: Readonly<Record<"light" | "dark", Theme>> = {
  light: { canvas: "#fff6f0", panel: "#ffffff", text: "#1c1e21", muted: "#5b6168", line: "#d9480f", memory: "#a8650a", border: "#f0cdb8", onLine: "#ffffff", chip: "#fbe3d6" },
  dark: { canvas: "#141110", panel: "#1b1614", text: "#ece8e4", muted: "#a59c95", line: "#ff6b1f", memory: "#ffb347", border: "#4a2e20", onLine: "#140a04", chip: "#3a2418" },
};

export const FONT = "system-ui, -apple-system, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif";
export const MONO = "ui-monospace, 'Cascadia Code', Menlo, Consolas, monospace";

// Average glyph width relative to font size, kept generous so wrapped lines fit.
const GLYPH_WIDTH = 0.53;

export const escape = (text: string): string => text.replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");

// Keeps a code span on one line while wrapping.
const NO_BREAK = " ";

// Wraps text to a width. A span in backticks stays on one line.
export function wrap(text: string, width: number, size: number): string[] {
  const perLine = Math.floor(width / (size * GLYPH_WIDTH));
  const kept = codeSpans(text)
    .map((span) => (span.code ? "`" + span.text.replaceAll(" ", NO_BREAK) + "`" : span.text))
    .join("");
  const lines: string[] = [];
  let line = "";
  for (const word of kept.split(" ")) {
    const next = line ? `${line} ${word}` : word;
    if (next.length > perLine && line) {
      lines.push(line);
      line = word;
    } else {
      line = next;
    }
  }
  return (line ? [...lines, line] : lines).map((wrapped) => wrapped.replaceAll(NO_BREAK, " "));
}

export interface TextStyle {
  readonly size: number;
  readonly fill: string;
  readonly weight?: number;
  readonly family?: string;
  readonly anchor?: "start" | "middle" | "end";
}

export function text(x: number, y: number, lines: readonly string[], style: TextStyle, lineHeight = style.size * 1.3): string {
  const attributes = [
    `font-size="${style.size}"`,
    `fill="${style.fill}"`,
    `font-family="${style.family ?? FONT}"`,
    style.weight ? `font-weight="${style.weight}"` : "",
    style.anchor ? `text-anchor="${style.anchor}"` : "",
  ].join(" ");
  const spans = lines.map((line, index) => `<tspan x="${x}" dy="${index === 0 ? 0 : lineHeight}">${inline(line)}</tspan>`).join("");
  return `<text x="${x}" y="${y}" ${attributes}>${spans}</text>`;
}

// Renders one line, with backtick spans in the monospace font.
function inline(line: string): string {
  return codeSpans(line)
    .map((span) => (span.code ? `<tspan font-family="${MONO}">${escape(span.text)}</tspan>` : escape(span.text)))
    .join("");
}

export function box(x: number, y: number, width: number, height: number, fill: string, stroke: string, radius = 10, strokeWidth = 1.5): string {
  return `<rect x="${x}" y="${y}" width="${width}" height="${height}" rx="${radius}" fill="${fill}" stroke="${stroke}" stroke-width="${strokeWidth}"/>`;
}

export const pillWidth = (label: string): number => Math.ceil(label.length * 11 * 0.57) + 16;

export function pill(x: number, y: number, label: string, fill: string, stroke: string, color: string, dashed = false): string {
  const size = 11;
  const width = pillWidth(label);
  const dash = dashed ? ` stroke-dasharray="3 2"` : "";
  return `<rect x="${x}" y="${y}" width="${width}" height="18" rx="9" fill="${fill}" stroke="${stroke}"${dash}/>${text(x + width / 2, y + 13, [label], { size, fill: color, weight: 600, anchor: "middle" })}`;
}

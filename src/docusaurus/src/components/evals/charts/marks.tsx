import type { ReactNode } from "react";

/** Colors and type for every chart, from the site's own variables so both color modes follow the theme. */
export const ChartStyle = {
  openForge: "var(--eval-open-forge)",
  other: "var(--eval-other)",
  grid: "var(--eval-grid)",
  axis: "var(--eval-axis)",
  text: "var(--ifm-font-color-base)",
  hover: "var(--eval-hover)",
  surface: "var(--ifm-background-color)",
  fontSize: 12,
} as const;

export const markFill = (openForge: boolean) => openForge ? ChartStyle.openForge : ChartStyle.other;

/** A thin bar with a 4px rounded end away from zero and a square end on the zero line. */
export function DeltaBar({ x, y, width, height, positive, openForge }: {
  readonly x: number;
  readonly y: number;
  readonly width: number;
  readonly height: number;
  readonly positive: boolean;
  readonly openForge: boolean;
}) {
  const left = Math.min(x, x + width), size = Math.abs(width), right = left + size, bottom = y + height;
  const radius = Math.min(4, size / 2, height / 2);
  const path = positive
    ? `M${left},${y}H${right - radius}Q${right},${y} ${right},${y + radius}V${bottom - radius}Q${right},${bottom} ${right - radius},${bottom}H${left}Z`
    : `M${right},${y}H${left + radius}Q${left},${y} ${left},${y + radius}V${bottom - radius}Q${left},${bottom} ${left + radius},${bottom}H${right}Z`;
  return <path d={path} fill={markFill(openForge)} />;
}

/** A tooltip card: the value first, then what it belongs to. */
export function TooltipCard({ title, lines }: { readonly title: string; readonly lines: readonly ReactNode[] }) {
  return <div className="eval-tooltip">
    {lines.map((line, index) => <div key={index} className={index === 0 ? "eval-tooltip-value" : undefined}>{line}</div>)}
    <div className="eval-muted">{title}</div>
  </div>;
}

/** Narrows a Recharts payload entry, which the library types loosely, to an object with a setup id. */
export function setupOf(value: unknown): string | null {
  return typeof value === "object" && value !== null && "setup" in value && typeof value.setup === "string" ? value.setup : null;
}

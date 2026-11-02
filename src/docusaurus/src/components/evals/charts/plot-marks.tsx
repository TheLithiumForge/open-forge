import { StartingPoints, type StartingPointId } from "../data/cells.ts";
import { ChartStyle, markFill } from "./marks.tsx";

/** What a point's color shows: Open Forge or not, or the starting point. */
export const ColorBy = { openForge: "open-forge", startingPoint: "starting-point" } as const;
export type ColorBy = (typeof ColorBy)[keyof typeof ColorBy];

const Shape = { circle: "circle", square: "square", triangle: "triangle", diamond: "diamond" } as const;
type Shape = (typeof Shape)[keyof typeof Shape];

/** Four starting points pass the colour checks only with a shape each, so color and shape both name the starting point. */
const PointStyles: Readonly<Record<StartingPointId, { readonly color: string; readonly shape: Shape }>> = {
  idea: { color: "var(--eval-point-idea)", shape: Shape.circle },
  brief: { color: "var(--eval-point-brief)", shape: Shape.square },
  "full-spec": { color: "var(--eval-point-full-spec)", shape: Shape.triangle },
  "existing-app": { color: "var(--eval-point-existing-app)", shape: Shape.diamond },
};

export interface MarkStyle {
  readonly color: string;
  readonly shape: Shape;
}

export function markStyle(colorBy: ColorBy, openForge: boolean, startingPoint: StartingPointId | null): MarkStyle {
  return colorBy === ColorBy.startingPoint && startingPoint !== null ? PointStyles[startingPoint] : { color: markFill(openForge), shape: Shape.circle };
}

/** A point mark with a ring in the surface color, so overlapping marks stay apart. */
export function PlotMark({ cx, cy, r, style }: { readonly cx: number; readonly cy: number; readonly r: number; readonly style: MarkStyle }) {
  const ring = { fill: style.color, fillOpacity: 0.85, stroke: ChartStyle.surface, strokeWidth: 1.5 };
  if (style.shape === Shape.square) return <rect x={cx - r * 0.9} y={cy - r * 0.9} width={r * 1.8} height={r * 1.8} rx={1.5} {...ring} />;
  if (style.shape === Shape.triangle) return <path d={`M${cx},${cy - r * 1.1}L${cx + r},${cy + r * 0.8}L${cx - r},${cy + r * 0.8}Z`} {...ring} />;
  if (style.shape === Shape.diamond) return <path d={`M${cx},${cy - r * 1.15}L${cx + r * 1.15},${cy}L${cx},${cy + r * 1.15}L${cx - r * 1.15},${cy}Z`} {...ring} />;
  return <circle cx={cx} cy={cy} r={r} {...ring} />;
}

const LegendSize = 14;
const icon = (style: MarkStyle) => <svg width={LegendSize} height={LegendSize} viewBox={`0 0 ${LegendSize} ${LegendSize}`} aria-hidden="true">
  <PlotMark cx={LegendSize / 2} cy={LegendSize / 2} r={5} style={style} />
</svg>;

/** The legend for the chosen color channel. */
export function PlotLegend({ colorBy, startingPoints }: { readonly colorBy: ColorBy; readonly startingPoints: readonly StartingPointId[] }) {
  const entries = colorBy === ColorBy.startingPoint
    ? StartingPoints.filter((point) => startingPoints.includes(point.id)).map((point) => ({ key: point.id, title: point.title, style: PointStyles[point.id] }))
    : [{ key: "open-forge", title: "Open Forge setups", style: markStyle(colorBy, true, null) }, { key: "other", title: "Other setups", style: markStyle(colorBy, false, null) }];
  return <ul className="eval-legend" aria-label="Point colors">
    {entries.map((entry) => <li key={entry.key}>{icon(entry.style)} {entry.title}</li>)}
  </ul>;
}

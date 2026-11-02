import type { ReactNode } from "react";
import BrowserOnly from "@docusaurus/BrowserOnly";

/** Room for the axis labels under a plot, so the figure never needs its own scrollbar. */
export const AxisBand = 36;
/** The height of one setup's row on a chart. */
export const RowHeight = 34;

/**
 * A chart with its title, a one-line reading guide and its data as a table. The plot draws in the browser only, since
 * Recharts measures its container. The table is the accessible version and holds every value the plot shows.
 */
export function ChartFigure({ title, description, controls, legend, height, plot, data, wide = false, dataOpen = false }: {
  readonly title: ReactNode;
  readonly description: ReactNode;
  /** Choices that change what the chart plots, between the caption and the plot. */
  readonly controls?: ReactNode;
  /** A legend for charts whose colors mean something other than Open Forge or not. */
  readonly legend?: ReactNode;
  readonly height: number;
  readonly plot: () => ReactNode;
  readonly data: ReactNode;
  /** Spans the whole chart grid. */
  readonly wide?: boolean;
  /** Shows the data table without a click. */
  readonly dataOpen?: boolean;
}) {
  const space = <div className="eval-chart-plot" style={{ height }} aria-hidden="true" />;
  return <figure className={wide ? "eval-chart eval-chart-wide" : "eval-chart"}>
    <figcaption>
      <h3>{title}</h3>
      <p className="eval-muted">{description}</p>
    </figcaption>
    {controls}
    {legend}
    <BrowserOnly fallback={space}>{() => <div className="eval-chart-plot" style={{ height }} aria-hidden="true">{plot()}</div>}</BrowserOnly>
    <details className="eval-chart-data" open={dataOpen}>
      <summary>Chart data</summary>
      <div className="eval-scroll">{data}</div>
    </details>
  </figure>;
}

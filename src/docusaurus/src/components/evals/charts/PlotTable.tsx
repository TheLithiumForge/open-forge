import type { Channels, PlotPoint } from "../data/plot-points.ts";
import { plotText, type PlotMeasure } from "../data/plot.ts";
import { Term } from "../info/Term.tsx";

function Heading({ measure }: { readonly measure: PlotMeasure }) {
  return <th scope="col">{measure.definition === null ? measure.title : <Term id={measure.definition}>{measure.title}</Term>}</th>;
}

/** The rows the chart plots, with the chosen measures as columns. */
export function PlotTable({ points, channels }: { readonly points: readonly PlotPoint[]; readonly channels: Channels }) {
  return <table>
    <thead><tr><th scope="col">Point</th><Heading measure={channels.x} /><Heading measure={channels.y} />{channels.size === null ? null : <Heading measure={channels.size} />}</tr></thead>
    <tbody>{points.map((point) => <tr key={point.key}>
      <th scope="row"><Term id={point.setup}>{point.name}</Term>{point.label === point.name ? null : point.label.slice(point.name.length)}</th>
      <td>{plotText(channels.x, point.raw.x)}</td><td>{plotText(channels.y, point.raw.y)}</td>
      {channels.size === null ? null : <td>{plotText(channels.size, point.raw.size)}</td>}
    </tr>)}</tbody>
  </table>;
}

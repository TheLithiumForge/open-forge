import { useId } from "react";
import { PlotMeasures, SizeMeasures, type PlotMeasure } from "../data/plot.ts";
import { ColorBy } from "./plot-marks.tsx";

/** What the chart plots on each channel. Every select offers the measures from the shared list. */
export interface PlotChoice {
  readonly x: string;
  readonly y: string;
  /** Empty when every point has the same size. */
  readonly size: string;
  readonly colorBy: ColorBy;
  /** Every run instead of one point per setup, when the filters keep one model and one starting point. */
  readonly runs: boolean;
}

export const NoSize = "";

function Select({ label, value, options, onChange }: {
  readonly label: string;
  readonly value: string;
  readonly options: readonly (readonly [string, string])[];
  readonly onChange: (value: string) => void;
}) {
  const id = useId();
  return <div className="eval-filter">
    <label htmlFor={id}>{label}</label>
    <select id={id} value={value} onChange={(event) => onChange(event.target.value)}>
      {options.map(([key, title]) => <option key={key} value={key}>{title}</option>)}
    </select>
  </div>;
}

const choices = (measures: readonly PlotMeasure[]) => measures.map((measure) => [measure.key, measure.title] as const);

export function PlotControls({ choice, onChange, canShowRuns }: {
  readonly choice: PlotChoice;
  readonly onChange: (choice: PlotChoice) => void;
  readonly canShowRuns: boolean;
}) {
  return <div className="eval-plot-controls" role="group" aria-label="Chart measures">
    <Select label="X axis" value={choice.x} options={choices(PlotMeasures)} onChange={(x) => onChange({ ...choice, x })} />
    <Select label="Y axis" value={choice.y} options={choices(PlotMeasures)} onChange={(y) => onChange({ ...choice, y })} />
    <Select label="Bubble size" value={choice.size} options={[[NoSize, "Same size"], ...choices(SizeMeasures)]} onChange={(size) => onChange({ ...choice, size })} />
    <Select label="Color" value={choice.colorBy} options={[[ColorBy.openForge, "Open Forge or not"], [ColorBy.startingPoint, "Starting point"]]}
      onChange={(value) => onChange({ ...choice, colorBy: value === ColorBy.startingPoint ? ColorBy.startingPoint : ColorBy.openForge })} />
    {canShowRuns ? <Select label="Points" value={choice.runs ? "runs" : "setups"} options={[["setups", "One per setup"], ["runs", "Every run"]]}
      onChange={(value) => onChange({ ...choice, runs: value === "runs" })} /> : null}
  </div>;
}

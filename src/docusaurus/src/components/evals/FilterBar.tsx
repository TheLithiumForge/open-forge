import { useId } from "react";
import { All, type FilterOption, type Filters } from "./data/aggregate.ts";
import { StartingPoints, type StartingPointId } from "./data/cells.ts";

function Select({ label, value, allLabel, options, onChange }: {
  readonly label: string;
  readonly value: string;
  /** Null when this view needs one value, so it offers no averaged choice. */
  readonly allLabel: string | null;
  readonly options: readonly FilterOption[];
  readonly onChange: (value: string) => void;
}) {
  const id = useId();
  return <div className="eval-filter">
    <label htmlFor={id}>{label}</label>
    <select id={id} value={value} onChange={(event) => onChange(event.target.value)}>
      {allLabel === null ? null : <option value={All}>{allLabel}</option>}
      {options.map((option) => <option key={option.key} value={option.key}>{option.label}</option>)}
    </select>
  </div>;
}

/** The filters every view below follows. "All" averages over every model, judge or starting point. */
export function FilterBar({ filters, onChange, models, judges, startingPoints, averages, workModes }: {
  readonly filters: Filters;
  readonly onChange: (filters: Filters) => void;
  readonly models: readonly FilterOption[];
  readonly judges: readonly FilterOption[];
  readonly startingPoints: readonly StartingPointId[];
  /** Whether "All" is a choice. History shows one line of results at a time, so it needs one value each. */
  readonly averages: boolean;
  /** How the work was handed over. History follows one line of results, so it leaves this menu out. */
  readonly workModes?: readonly FilterOption[];
}) {
  const points = StartingPoints.filter((point) => startingPoints.includes(point.id)).map((point) => ({ key: point.id, label: point.title }));
  const startingPoint = (value: string) => StartingPoints.find((point) => point.id === value)?.id ?? All;
  return <div className="eval-filters" role="group" aria-label="Filters">
    <Select label="Model" value={filters.model} allLabel={averages ? "All models" : null} options={models} onChange={(model) => onChange({ ...filters, model })} />
    <Select label="Judge" value={filters.judge} allLabel={averages ? "All judges" : null} options={judges} onChange={(judge) => onChange({ ...filters, judge })} />
    <Select label="Starting point" value={filters.startingPoint} allLabel={averages ? "All starting points" : null} options={points}
      onChange={(value) => onChange({ ...filters, startingPoint: startingPoint(value) })} />
    {workModes === undefined ? null : <Select label="Work mode" value={filters.workMode} allLabel={averages ? "All work modes" : null} options={workModes}
      onChange={(workMode) => onChange({ ...filters, workMode })} />}
  </div>;
}

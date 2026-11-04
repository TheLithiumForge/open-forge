import { createColumnHelper, type ColumnDef, type FilterFn } from "@tanstack/react-table";
import type { Batch } from "../records/records.types.ts";
import type { Comparison } from "../data/aggregate.ts";
import { caseDefinition, caseLabel, searchText, startingPointTitle } from "../data/cell-labels.ts";
import { NoValue } from "../data/format.ts";
import { CellMeasures, formatChange, measureChange } from "../data/measures.ts";
import { setupName, setupRank } from "../data/names.ts";
import { Term } from "../info/Term.tsx";

/** Every column sorts on a string or a number. A missing value sorts last. */
export type SortValue = string | number | undefined;
type Column = ColumnDef<Comparison, SortValue>;

export const ColumnIds = { setup: "setup", startingPoint: "startingPoint", model: "model", judge: "judge", runs: "runs" } as const;

const helper = createColumnHelper<Comparison>();
const known = (value: number | null) => value ?? undefined;

/** Keeps rows whose value equals the chosen one. An empty choice keeps every row. */
export const equalsChoice: FilterFn<Comparison> = (row, columnId, value: unknown) => value === undefined || value === "" || row.getValue(columnId) === value;
/** Matches the search words anywhere in a row's names, case and batch. */
export const matchesSearch: FilterFn<Comparison> = (row, _columnId, value: unknown) =>
  String(value ?? "").toLowerCase().split(/\s+/).every((word) => searchText(row.original.cell).includes(word));

/** The starting point, with the case when the starting point has several. */
export function startingPointText({ cell }: Comparison): string {
  const title = startingPointTitle(cell.startingPoint), label = caseLabel(cell);
  return label === title ? title : `${title}, ${label}`;
}

const plural = (count: number, one: string, many: string) => `${count} ${count === 1 ? one : many}`;

/** The table's columns: who and what first, then each measure with its change from the control. */
export function resultColumns(batches: ReadonlyMap<string, Batch>, open: (item: Comparison) => void): Column[] {
  const measureColumns = CellMeasures.filter((spec) => spec.inTable).map((spec): Column => helper.group({
    id: spec.id,
    header: () => <><Term id={spec.definition}>{spec.title}</Term>{spec.sameModelOnly ? <span className="eval-muted">, same model only</span> : null}</>,
    columns: [
      helper.accessor((item): SortValue => known(spec.value(item.cell).mean), { id: `${spec.id}-value`, header: "Value", sortUndefined: "last",
        cell: (info) => {
          const { cell } = info.row.original, value = spec.value(cell);
          // A mean from only some of a cell's runs says how many, since the Runs column counts them all.
          return <>{spec.format(value.mean)}{value.mean !== null && value.values.length < cell.runs ? <div className="eval-muted">{value.values.length} of {cell.runs} runs</div> : null}</>;
        } }),
      helper.accessor((item): SortValue => item.cell.setup === item.cell.control ? undefined : known(measureChange(spec, item)), { id: `${spec.id}-change`, header: "Change", sortUndefined: "last",
        cell: (info) => info.row.original.cell.setup === info.row.original.cell.control ? <span className="eval-muted">Control</span> : formatChange(spec, measureChange(spec, info.row.original)) }),
    ],
  }));
  return [
    helper.accessor((item): SortValue => item.cell.setup, { id: ColumnIds.setup, header: "Scenario", filterFn: equalsChoice,
      sortingFn: (a, b) => setupRank(a.original.cell.setup) - setupRank(b.original.cell.setup),
      cell: (info) => <Term id={info.row.original.cell.setup} batch={batches.get(info.row.original.cell.batchId)}>{setupName(info.row.original.cell.setup)}</Term> }),
    helper.accessor((item): SortValue => startingPointText(item), { id: ColumnIds.startingPoint, header: "Starting point", filterFn: equalsChoice,
      cell: (info) => {
        const { cell } = info.row.original, batch = batches.get(cell.batchId), definition = caseDefinition(cell);
        return <><Term id={cell.startingPoint} batch={batch} />{definition === cell.startingPoint ? null : <div><Term id={definition} batch={batch} /></div>}</>;
      } }),
    helper.accessor((item): SortValue => item.cell.modelLabel, { id: ColumnIds.model, header: "Model",
      cell: (info) => <>{info.row.original.cell.modelLabel}{info.row.original.cell.agentVersion === null ? null : <div className="eval-muted">{info.row.original.cell.agentVersion}</div>}</> }),
    helper.accessor((item): SortValue => item.cell.judgeLabel ?? undefined, { id: ColumnIds.judge, header: "Judge", sortUndefined: "last", cell: (info) => info.row.original.cell.judgeLabel ?? NoValue }),
    helper.accessor((item): SortValue => item.cell.runs, { id: ColumnIds.runs, header: "Runs",
      cell: (info) => {
        const { cell } = info.row.original;
        return <>
          <button type="button" className="eval-link-button" onClick={(event) => { event.stopPropagation(); open(info.row.original); }}>{plural(cell.runs, "run", "runs")}</button>
          {cell.recheck === 0 ? null : <div className="eval-muted">{cell.recheck} {cell.recheck === 1 ? "waits" : "wait"} on a recheck</div>}
          {cell.runs - cell.checked - cell.recheck <= 0 ? null : <div className="eval-muted">{cell.runs - cell.checked - cell.recheck} {cell.runs - cell.checked - cell.recheck === 1 ? "waits" : "wait"} on checks</div>}
        </>;
      } }),
    ...measureColumns,
  ];
}

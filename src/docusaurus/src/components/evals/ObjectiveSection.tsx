import { useMemo, type ReactNode } from "react";
import type { Batch } from "./records/records.types.ts";
import type { Comparison } from "./data/aggregate.ts";
import { compact, NoValue, signed } from "./data/format.ts";
import { setupName } from "./data/names.ts";
import { objectiveRows, ToolKinds, type ObjectiveRow } from "./data/objective.ts";
import { taskRows, type TaskRow } from "./data/work-views.ts";
import { ContextClassChart, tokenDelta } from "./charts/ContextClassChart.tsx";
import { Term } from "./info/Term.tsx";

/** How many file types the table shows, largest first. The rest add up in the last column. */
const ShownFileTypes = 6;
const decimal = (value: number | null | undefined) => value == null ? NoValue : value.toFixed(value < 10 ? 1 : 0);
const countDelta = (value: number | null | undefined, control: number | null | undefined) => value == null || control == null ? "" : ` (${signed(value - control, 1)})`;

function Table({ label, head, children }: { readonly label: string; readonly head: readonly string[]; readonly children: ReactNode }) {
  return <div className="eval-scroll" tabIndex={0} role="region" aria-label={label}>
    <table className="eval-table">
      <thead><tr>{head.map((title) => <th key={title} scope="col">{title}</th>)}</tr></thead>
      <tbody>{children}</tbody>
    </table>
  </div>;
}

const SetupCell = ({ row }: { readonly row: Pick<ObjectiveRow, "setup" | "name"> }) => <th scope="row"><Term id={row.setup}>{row.name}</Term></th>;

function FileTypes({ rows }: { readonly rows: readonly ObjectiveRow[] }) {
  const totals = new Map<string, number>();
  for (const row of rows) for (const [type, value] of Object.entries(row.measures.fileTypes)) totals.set(type, (totals.get(type) ?? 0) + value);
  const types = [...totals].sort((a, b) => b[1] - a[1]).slice(0, ShownFileTypes).map(([type]) => type);
  const rest = (row: ObjectiveRow) => Object.entries(row.measures.fileTypes).filter(([type]) => !types.includes(type)).reduce((sum, [, value]) => sum + value, 0);
  return <><h3>By file type</h3>
    <p>Mean input tokens per run from file content and search results, by file extension. Markdown is mostly spec documents and Open Forge files, and the rest is mostly code and tests.</p>
    <Table label="Input tokens by file type" head={["Scenario", ...types.map((type) => `.${type}`), "Other"]}>
      {rows.map((row) => <tr key={row.setup}><SetupCell row={row} />
        {types.map((type) => <td key={type}>{compact(row.measures.fileTypes[type] ?? 0)}{tokenDelta(row.measures.fileTypes[type] ?? 0, row.control === null ? null : row.control.fileTypes[type] ?? 0)}</td>)}
        <td>{compact(rest(row))}</td></tr>)}
    </Table></>;
}

function ToolCalls({ rows }: { readonly rows: readonly ObjectiveRow[] }) {
  return <><h3>Tool calls by kind</h3>
    <p>Mean calls per run from the agent's own events, with the change from the control in brackets. A compound command counts once, as its most significant part: an edit, then a test or build run, a read, a search. Steps before the first edit show how long the agent read and planned.</p>
    <Table label="Tool calls by kind" head={["Scenario", ...ToolKinds.map((kind) => kind.title), "Failed", "Steps before the first edit", "Peak context"]}>
      {rows.map((row) => <tr key={row.setup}><SetupCell row={row} />
        {ToolKinds.map((kind) => <td key={kind.key}>{decimal(row.measures.calls[kind.key])}{countDelta(row.measures.calls[kind.key], row.control?.calls[kind.key])}</td>)}
        <td>{decimal(row.measures.failed)}</td>
        <td>{decimal(row.measures.stepsBeforeEdit)}{countDelta(row.measures.stepsBeforeEdit, row.control?.stepsBeforeEdit)}</td>
        <td>{compact(row.measures.peakContext)}</td></tr>)}
    </Table></>;
}

function Tasks({ rows }: { readonly rows: readonly TaskRow[] }) {
  const setups = [...new Set(rows.flatMap((row) => [...row.tokens.keys()]))];
  return <><h3>Tokens per task</h3>
    <p>Mean <Term id="tokens">tokens</Term> per run for each task, exact. A Layer 4 case is one task, and earlier starting points count their whole build as one.</p>
    <Table label="Tokens per task" head={["Task", ...setups.map(setupName)]}>
      {rows.map((row) => <tr key={row.caseId}><th scope="row">{row.caseId}</th>{setups.map((setup) => <td key={setup}>{compact(row.tokens.get(setup) ?? null)}</td>)}</tr>)}
    </Table></>;
}

/** The objective measures beside the control: tokens by class and file type, tool calls by kind and tokens per task. */
export function ObjectiveSection({ items, batches }: { readonly items: readonly Comparison[]; readonly batches: ReadonlyMap<string, Batch> }) {
  const rows = useMemo(() => objectiveRows(items, batches), [items, batches]);
  const tasks = useMemo(() => taskRows(items), [items]);
  const rough = rows.reduce((sum, row) => sum + row.measures.rough, 0);
  return <section className="eval-section" aria-labelledby="eval-objective">
    <h2 id="eval-objective">Where the tokens went</h2>
    <p>These measures come from each run's own logs, with no judge. Token totals are exact. How they split across classes and file types is an estimate, and every run's split adds up to its exact total.</p>
    {rows.length === 0 ? <p>No run in these filters has an analysis yet. Run <code>npm run evals -- analyze --missing</code> to add them from the saved logs.</p> : <>
      {rough > 0 && <p className="eval-muted">{rough} of these runs were saved before Codex's session log was kept, so their split is a rougher estimate from their events alone.</p>}
      <div className="eval-charts"><ContextClassChart rows={rows} /></div>
      <FileTypes rows={rows} />
      <ToolCalls rows={rows} />
    </>}
    {tasks.length > 0 && <Tasks rows={tasks} />}
  </section>;
}

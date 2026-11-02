import { useId, useMemo, useState } from "react";
import { flexRender, getCoreRowModel, getFilteredRowModel, getSortedRowModel, useReactTable, type ColumnFiltersState, type Header, type SortingState } from "@tanstack/react-table";
import type { Batch } from "../records/records.types.ts";
import type { Comparison } from "../data/aggregate.ts";
import { StartingPoints } from "../data/cells.ts";
import { resultsCsv } from "../data/csv.ts";
import { setupName, setupRank } from "../data/names.ts";
import { CsvType, save } from "../download.ts";
import { CellDialog } from "./CellDialog.tsx";
import { ColumnIds, matchesSearch, resultColumns, startingPointText } from "./columns.tsx";

const CsvName = "open-forge-eval-results.csv";
const SortMarks = { asc: "↑", desc: "↓" } as const;
const pointRank = (item: Comparison) => StartingPoints.findIndex((point) => point.id === item.cell.startingPoint);
const readingOrder = (a: Comparison, b: Comparison) => pointRank(a) - pointRank(b) || a.cell.caseId.localeCompare(b.cell.caseId)
  || a.cell.modelLabel.localeCompare(b.cell.modelLabel) || setupRank(a.cell.setup) - setupRank(b.cell.setup) || (a.cell.judgeLabel ?? "").localeCompare(b.cell.judgeLabel ?? "");

/**
 * A header cell. Measures group a value and a change column under one heading, so a column without a group spans both
 * header rows instead of leaving an empty cell above it.
 */
function HeaderCell({ header, rows }: { readonly header: Header<Comparison, unknown>; readonly rows: number }) {
  const sorted = header.column.getIsSorted();
  const content = flexRender(header.column.columnDef.header, header.getContext());
  const sortable = header.column.getCanSort() && header.column.columns.length === 0;
  return <th scope="col" colSpan={header.colSpan} rowSpan={header.isPlaceholder ? rows : undefined}
    aria-sort={sorted === "asc" ? "ascending" : sorted === "desc" ? "descending" : undefined}>
    {sortable ? <button type="button" className="eval-sort" onClick={header.column.getToggleSortingHandler()}>{content}<span aria-hidden="true">{sorted === false ? "" : SortMarks[sorted]}</span></button> : content}
  </th>;
}

function Choice({ label, value, values, onChange }: { readonly label: string; readonly value: string; readonly values: readonly (readonly [string, string])[]; readonly onChange: (value: string) => void }) {
  const id = useId();
  return <div className="eval-filter"><label htmlFor={id}>{label}</label>
    <select id={id} value={value} onChange={(event) => onChange(event.target.value)}>
      <option value="">All</option>{values.map(([key, text]) => <option key={key} value={key}>{text}</option>)}
    </select></div>;
}

/** One row per starting point, case, setup, model and judge, with sorting, search, column filters and a CSV of the shown rows. */
export function ResultsTable({ items, batches }: { readonly items: readonly Comparison[]; readonly batches: ReadonlyMap<string, Batch> }) {
  const [sorting, setSorting] = useState<SortingState>([]);
  const [search, setSearch] = useState("");
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
  const [opened, setOpened] = useState<Comparison | null>(null);
  // Rows without a finished run have nothing to show yet. Data status lists their batches.
  const data = useMemo(() => items.filter((item) => item.cell.runs > 0).sort(readingOrder), [items]);
  const columns = useMemo(() => resultColumns(batches, setOpened), [batches]);
  const table = useReactTable({ data, columns, state: { sorting, globalFilter: search, columnFilters }, onSortingChange: setSorting, onGlobalFilterChange: setSearch,
    onColumnFiltersChange: setColumnFilters, globalFilterFn: matchesSearch, getCoreRowModel: getCoreRowModel(), getSortedRowModel: getSortedRowModel(), getFilteredRowModel: getFilteredRowModel() });
  const searchId = useId();
  const rows = table.getRowModel().rows;
  const filterValue = (id: string) => String(table.getColumn(id)?.getFilterValue() ?? "");
  const setFilter = (id: string) => (value: string) => table.getColumn(id)?.setFilterValue(value === "" ? undefined : value);
  const setups = [...new Set(data.map((item) => item.cell.setup))].sort((a, b) => setupRank(a) - setupRank(b)).map((setup) => [setup, setupName(setup)] as const);
  const points = [...new Set(data.map(startingPointText))].map((text) => [text, text] as const);
  return <section className="eval-section" aria-labelledby="eval-table">
    <h2 id="eval-table">All results</h2>
    <p>One row for each starting point, case, setup, model and judge with a finished run, from the newest batch of each case and model. Values are means over the row's runs. Select a row to see its runs.</p>
    <div className="eval-table-tools">
      <div className="eval-filter eval-search"><label htmlFor={searchId}>Search</label>
        <input id={searchId} type="search" value={search} placeholder="Setup, model, judge or batch" onChange={(event) => setSearch(event.target.value)} /></div>
      <Choice label="Starting point" value={filterValue(ColumnIds.startingPoint)} values={points} onChange={setFilter(ColumnIds.startingPoint)} />
      <Choice label="Setup" value={filterValue(ColumnIds.setup)} values={setups} onChange={setFilter(ColumnIds.setup)} />
      <button type="button" className="button button--sm button--secondary" onClick={() => save(CsvName, resultsCsv(rows.map((row) => row.original)), CsvType)}>Download CSV</button>
    </div>
    <p className="eval-muted" role="status">{rows.length} of {data.length} rows. The CSV holds the rows shown, in this order.</p>
    <div className="eval-scroll" tabIndex={0} role="region" aria-label="All results">
      <table className="eval-table eval-results">
        <thead>{table.getHeaderGroups().map((group, index, groups) => <tr key={group.id}>
          {group.headers.filter((header) => index === 0 || header.column.depth > 0).map((header) => <HeaderCell key={header.id} header={header} rows={groups.length} />)}
        </tr>)}</thead>
        <tbody>{rows.map((row) => <tr key={row.id} className="eval-row" onClick={() => setOpened(row.original)}>
          {row.getVisibleCells().map((cell) => <td key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</td>)}
        </tr>)}</tbody>
      </table>
    </div>
    <CellDialog item={opened} batch={opened === null ? undefined : batches.get(opened.cell.batchId)} onClose={() => setOpened(null)} />
  </section>;
}

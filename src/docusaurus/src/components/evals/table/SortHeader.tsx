import { flexRender, type Header, type Table } from "@tanstack/react-table";
import type {} from "./column-meta.types.ts";

const SortMarks = { asc: "↑", desc: "↓", none: "↕" } as const;

/**
 * A header cell with a sort button. A header that holds a definition label gets a small sort button beside it, since
 * one button can't hold another. A table may group columns under one heading, so a column without a group spans both
 * header rows instead of leaving an empty cell above it.
 */
function HeaderCell<T>({ header, rows }: { readonly header: Header<T, unknown>; readonly rows: number }) {
  const sorted = header.column.getIsSorted();
  const content = flexRender(header.column.columnDef.header, header.getContext());
  const sortable = header.column.getCanSort() && header.column.columns.length === 0;
  const label = header.column.columnDef.meta?.label;
  const mark = <span aria-hidden="true">{sorted === false ? (label === undefined ? "" : SortMarks.none) : SortMarks[sorted]}</span>;
  const toggle = header.column.getToggleSortingHandler();
  return <th scope="col" colSpan={header.colSpan} rowSpan={header.isPlaceholder ? rows : undefined}
    aria-sort={sorted === "asc" ? "ascending" : sorted === "desc" ? "descending" : undefined}>
    {!sortable ? content : label === undefined
      ? <button type="button" className="eval-sort" onClick={toggle}>{content}{mark}</button>
      : <span className="eval-sort-head">{content}<button type="button" className="eval-sort" aria-label={`Sort by ${label}`} onClick={toggle}>{mark}</button></span>}
  </th>;
}

/** A table's header rows, with every leaf column sortable. */
export function SortHead<T>({ table }: { readonly table: Table<T> }) {
  return <thead>{table.getHeaderGroups().map((group, index, groups) => <tr key={group.id}>
    {group.headers.filter((header) => index === 0 || header.column.depth > 0).map((header) => <HeaderCell key={header.id} header={header} rows={groups.length} />)}
  </tr>)}</thead>;
}

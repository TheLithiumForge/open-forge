import { useState } from "react";
import { createColumnHelper, flexRender, getCoreRowModel, getSortedRowModel, useReactTable, type ColumnDef, type SortingState } from "@tanstack/react-table";
import { NoValue, signed } from "../data/format.ts";
import { setupRank } from "../data/names.ts";
import { TokenStep, TokenTrend, type TradeoffRow } from "../data/tradeoff.ts";
import { Term } from "../info/Term.tsx";
import { SortHead } from "./SortHeader.tsx";

type SortValue = string | number | undefined;
const helper = createColumnHelper<TradeoffRow>();
const known = (value: number | null) => value ?? undefined;
const points = (value: number | null) => value === null ? NoValue : `${signed(value, 1)} points`;
const TrendText: Readonly<Record<TokenTrend, string>> = { [TokenTrend.more]: "more", [TokenTrend.fewer]: "fewer", [TokenTrend.same]: "about the same" };

function tokens(row: TradeoffRow): string {
  if (row.tokenChange === null || row.tokenTrend === null) return NoValue;
  return row.tokenTrend === TokenTrend.same ? "About the same" : `${Math.abs(Math.round(row.tokenChange))}% ${TrendText[row.tokenTrend]}`;
}

/** Points per 10% more tokens, or why there is no price: the setup used fewer tokens or about the same. */
function perStep(row: TradeoffRow, value: number | null): string {
  if (row.tokenTrend === TokenTrend.fewer) return "Fewer tokens";
  if (row.tokenTrend === TokenTrend.same) return "About the same tokens";
  return points(value);
}

const perStepHeader = (id: string, title: string) => () => <><Term id={id}>{title}</Term> per {TokenStep}% more <Term id="tokens">tokens</Term></>;

const Columns: ColumnDef<TradeoffRow, SortValue>[] = [
  helper.accessor((row): SortValue => row.setup, { id: "setup", header: "Scenario", sortingFn: (a, b) => setupRank(a.original.setup) - setupRank(b.original.setup),
    cell: (info) => <Term id={info.row.original.setup}>{info.row.original.name}</Term> }),
  helper.accessor((row): SortValue => known(row.tokenChange), { id: "tokens", header: () => <Term id="tokens" />, meta: { label: "tokens" }, sortUndefined: "last", cell: (info) => tokens(info.row.original) }),
  helper.accessor((row): SortValue => known(row.coverageChange), { id: "coverage", header: () => <Term id="check-coverage" />, meta: { label: "check coverage" }, sortUndefined: "last", cell: (info) => points(info.row.original.coverageChange) }),
  helper.accessor((row): SortValue => known(row.judgeChange), { id: "judge", header: () => <Term id="judge-score" />, meta: { label: "judge score" }, sortUndefined: "last", cell: (info) => points(info.row.original.judgeChange) }),
  helper.accessor((row): SortValue => known(row.coveragePerStep), { id: "coverage-per-step", header: perStepHeader("check-coverage", "Coverage"), meta: { label: "coverage per 10% more tokens" }, sortUndefined: "last",
    cell: (info) => perStep(info.row.original, info.row.original.coveragePerStep) }),
  helper.accessor((row): SortValue => known(row.judgePerStep), { id: "judge-per-step", header: perStepHeader("judge-score", "Judge score"), meta: { label: "judge score per 10% more tokens" }, sortUndefined: "last",
    cell: (info) => perStep(info.row.original, info.row.original.judgePerStep) }),
  helper.accessor((row): SortValue => row.groups, { id: "groups", header: "Cases and models" }),
  helper.accessor((row): SortValue => row.runs, { id: "runs", header: "Runs" }),
];

/** What each setup's extra tokens buy against its control, with the same averages as the cards. */
export function TradeoffTable({ rows }: { readonly rows: readonly TradeoffRow[] }) {
  const [sorting, setSorting] = useState<SortingState>([]);
  const table = useReactTable({ data: [...rows], columns: Columns, state: { sorting }, onSortingChange: setSorting, getCoreRowModel: getCoreRowModel(), getSortedRowModel: getSortedRowModel() });
  return <section className="eval-section" aria-labelledby="eval-tradeoff">
    <h2 id="eval-tradeoff">What the extra tokens buy</h2>
    <p>Each row sets a setup against its control, averaged the same way as the cards. The two columns per {TokenStep}% more tokens divide each quality change by the token change. For example, 20 points for 30% more tokens reads as +6.7 points. A setup that used fewer tokens shows that instead.</p>
    {rows.length === 0 ? <p>No setup has a comparison with the control in these filters.</p> : <div className="eval-scroll" tabIndex={0} role="region" aria-label="What the extra tokens buy">
      <table className="eval-table">
        <SortHead table={table} />
        <tbody>{table.getRowModel().rows.map((row) => <tr key={row.id}>
          {row.getVisibleCells().map((cell) => <td key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</td>)}
        </tr>)}</tbody>
      </table>
    </div>}
  </section>;
}

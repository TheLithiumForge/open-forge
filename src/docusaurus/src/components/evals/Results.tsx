import { useMemo, useState } from "react";
import type { Batch } from "./records/records.types.ts";
import { All, comparisons, filtered, options, setupSummaries, type Filters } from "./data/aggregate.ts";
import { cells } from "./data/cells.ts";
import { preliminaryReasons } from "./data/status.ts";
import { controlSetups } from "./data/chart-data.ts";
import { tradeoffRows } from "./data/tradeoff.ts";
import { FilterBar } from "./FilterBar.tsx";
import { Headline } from "./Headline.tsx";
import { ChartsSection } from "./charts/ChartsSection.tsx";
import { ResultsTable } from "./table/ResultsTable.tsx";
import { TradeoffTable } from "./table/TradeoffTable.tsx";
import { DataStatus } from "./DataStatus.tsx";

const Averages: Filters = { model: All, judge: All, startingPoint: All };

/** The Results page: the headline, charts, table and data status, all following one set of filters. */
export function Results({ batches }: { readonly batches: readonly Batch[] }) {
  const [filters, setFilters] = useState<Filters>(Averages);
  const all = useMemo(() => comparisons(cells(batches)), [batches]);
  const allCells = useMemo(() => all.map((item) => item.cell), [all]);
  const { models, judges } = useMemo(() => options(allCells), [allCells]);
  const startingPoints = useMemo(() => [...new Set(allCells.map((cell) => cell.startingPoint))], [allCells]);
  const shown = useMemo(() => filtered(all, filters), [all, filters]);
  const shownCells = useMemo(() => shown.map((item) => item.cell), [shown]);
  const summaries = useMemo(() => setupSummaries(shown), [shown]);
  const reasons = preliminaryReasons(batches, shownCells);
  const tradeoffs = useMemo(() => tradeoffRows(summaries, controlSetups(shownCells)), [summaries, shownCells]);
  const byId = useMemo(() => new Map(batches.map((batch) => [batch.experiment.id, batch])), [batches]);
  return <>
    <FilterBar filters={filters} onChange={setFilters} models={models} judges={judges} startingPoints={startingPoints} averages />
    <Headline summaries={summaries} reasons={reasons} />
    <TradeoffTable rows={tradeoffs} />
    <ChartsSection summaries={summaries} items={shown} filters={filters} />
    <ResultsTable items={shown} batches={byId} />
    <DataStatus batches={batches} all={allCells} shown={shownCells} reasons={reasons} />
  </>;
}

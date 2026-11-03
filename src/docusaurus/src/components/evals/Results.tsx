import { useMemo, useState } from "react";
import type { Batch } from "./records/records.types.ts";
import { All, comparisons, filtered, options, setupSummaries, type Filters } from "./data/aggregate.ts";
import { cells, newestCells } from "./data/cells.ts";
import { preliminaryReasons } from "./data/status.ts";
import { controlSetups } from "./data/chart-data.ts";
import { tradeoffRows } from "./data/tradeoff.ts";
import { FilterBar } from "./FilterBar.tsx";
import { Headline } from "./Headline.tsx";
import { ChartsSection } from "./charts/ChartsSection.tsx";
import { ObjectiveSection } from "./ObjectiveSection.tsx";
import { MilestoneSection, PacketSection } from "./WorkViews.tsx";
import { ResultsTable } from "./table/ResultsTable.tsx";
import { TradeoffTable } from "./table/TradeoffTable.tsx";
import { DataStatus } from "./DataStatus.tsx";

const Averages: Filters = { model: All, judge: All, startingPoint: All, workMode: All };

/**
 * The Results page: the headline, charts, objective measures, table and data status, all following one set of filters.
 * Chained milestones start from earlier results, so the averages leave them to the milestone view.
 */
export function Results({ batches }: { readonly batches: readonly Batch[] }) {
  const [filters, setFilters] = useState<Filters>(Averages);
  const all = useMemo(() => newestCells(comparisons(cells(batches))), [batches]);
  const allCells = useMemo(() => all.map((item) => item.cell), [all]);
  const { models, judges, workModes } = useMemo(() => options(allCells), [allCells]);
  const startingPoints = useMemo(() => [...new Set(allCells.map((cell) => cell.startingPoint))], [allCells]);
  const shown = useMemo(() => filtered(all, filters), [all, filters]);
  const standalone = useMemo(() => shown.filter((item) => item.cell.chain === null), [shown]);
  const shownCells = useMemo(() => shown.map((item) => item.cell), [shown]);
  const standaloneCells = useMemo(() => standalone.map((item) => item.cell), [standalone]);
  const summaries = useMemo(() => setupSummaries(standalone), [standalone]);
  const reasons = preliminaryReasons(batches, shownCells);
  const tradeoffs = useMemo(() => tradeoffRows(summaries, controlSetups(standaloneCells)), [summaries, standaloneCells]);
  const byId = useMemo(() => new Map(batches.map((batch) => [batch.experiment.id, batch])), [batches]);
  return <>
    <FilterBar filters={filters} onChange={setFilters} models={models} judges={judges} startingPoints={startingPoints} averages workModes={workModes} />
    <Headline summaries={summaries} reasons={reasons} />
    <TradeoffTable rows={tradeoffs} />
    <ChartsSection summaries={summaries} items={standalone} filters={filters} />
    <ObjectiveSection items={standalone} batches={byId} />
    <MilestoneSection items={shown} />
    <PacketSection items={shown} batches={byId} />
    <ResultsTable items={shown} batches={byId} />
    <DataStatus batches={batches} all={allCells} shown={shownCells} reasons={reasons} />
  </>;
}

import { useMemo, useState } from "react";
import type { Batch } from "./records/records.types.ts";
import { All, filterOptions, type Filters } from "./data/aggregate.ts";
import { StartingPoints } from "./data/cells.ts";
import { summaries, type Summary } from "./data/summaries.ts";
import { FilterBar } from "./FilterBar.tsx";
import { HistoryGroup } from "./HistoryGroup.tsx";
import { ReferenceHistory } from "./ReferenceHistory.tsx";

/** One model, judge and starting point at a time, since history lines don't average. Each filter starts at its first value. */
export function History({ batches }: { readonly batches: readonly Batch[] }) {
  // Only batches with a known starting point have a history line.
  const rows = useMemo(() => batches.flatMap(summaries).filter((row) => StartingPoints.some((point) => point.layer === row.layer)), [batches]);
  const models = useMemo(() => filterOptions(rows.map((row) => ({ key: row.agentKey, label: row.modelLabel, runs: row.runs }))), [rows]);
  const [chosen, choose] = useState<Filters>({ model: All, judge: All, startingPoint: All });
  // Each unset filter takes the first value that still has history under the filters before it.
  const model = chosen.model === All ? models[0]?.key ?? All : chosen.model;
  const ofModel = rows.filter((row) => row.agentKey === model);
  const judges = filterOptions(rows.map((row) => ({ key: row.judgeKey, label: row.judgeLabel, runs: row.runs })));
  const judge = chosen.judge === All ? ofModel[0]?.judgeKey ?? judges[0]?.key ?? All : chosen.judge;
  const points = StartingPoints.filter((point) => rows.some((row) => row.layer === point.layer));
  const startingPoint = chosen.startingPoint === All ? StartingPoints.find((point) => ofModel.some((row) => row.judgeKey === judge && row.layer === point.layer))?.id ?? points[0]?.id ?? All : chosen.startingPoint;
  const filters: Filters = { model, judge, startingPoint };
  const layer = StartingPoints.find((point) => point.id === filters.startingPoint)?.layer ?? null;
  const groups = new Map<string, Summary[]>();
  for (const row of ofModel.filter((item) => item.judgeKey === filters.judge && item.layer === layer)) {
    const key = JSON.stringify([row.scope, row.setup]);
    groups.set(key, [...groups.get(key) ?? [], row]);
  }
  return <>
    <FilterBar filters={filters} onChange={choose} models={models} judges={judges} startingPoints={points.map((point) => point.id)} averages={false} />
    <section className="eval-section" aria-labelledby="eval-history">
      <h2 id="eval-history">CLI history</h2>
      <p>Each line keeps the starting point, case, model, judge, grading and tools the same, so only the CLI version changes. Changed grading versions get separate charts.</p>
      {groups.size === 0 ? <p>No history matches these filters.</p> : null}
      {[...groups].map(([key, group]) => <HistoryGroup key={key} group={group} />)}
    </section>
    <ReferenceHistory batches={batches.filter((batch) => (batch.experiment.config.layer ?? null) === layer)} />
  </>;
}

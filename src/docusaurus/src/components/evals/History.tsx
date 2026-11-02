import { useMemo, useState } from "react";
import type { Batch } from "./records/records.types.ts";
import { summaries, type Summary } from "./data/summaries.ts";
import { pairKey, pairOptions } from "./data/views.ts";
import { PairSelector } from "./PairSelector.tsx";
import { HistoryGroup } from "./HistoryGroup.tsx";
import { ReferenceHistory } from "./ReferenceHistory.tsx";
export function History({ batches, layer }: {
  readonly batches: readonly Batch[];
  readonly layer: number | null;
}) {
  const selected = useMemo(() => batches.filter((batch) => (batch.experiment.config.layer ?? null) === layer), [batches, layer]);
  const rows = useMemo(() => selected.flatMap(summaries), [selected]);
  const pairs = pairOptions(rows);
  const [chosenPair, choosePair] = useState("");
  const pair = pairs.find((item) => item.key === chosenPair) ?? pairs[0];
  const groups = new Map<string, Summary[]>();
  for (const row of rows.filter((item) => pairKey(item) === pair?.key)) {
    const key = JSON.stringify([row.scope, row.setup]);
    groups.set(key, [...groups.get(key) ?? [], row]);
  }
  return <>
    <h2>CLI history</h2>
    <p>Each line fixes the layer, benchmark, implementer, judge, grading and tools. CLI versions change within that group. Changed grading versions get separate charts.</p>
    {pair === undefined ? null : <PairSelector pairs={pairs} value={pair.key} onChange={choosePair} />}
    {groups.size === 0 ? <p>No model history is published for this layer.</p> : null}
    {[...groups].map(([key, group]) => <HistoryGroup key={key} group={group} />)}
    <ReferenceHistory batches={selected} />
  </>;
}

import { useState } from "react";
import type { Batch } from "../records/records.types.ts";
import type { Comparison } from "../data/aggregate.ts";
import { setupName } from "../data/names.ts";
import { Attempt } from "../Attempt.tsx";
import { Dialog } from "../Dialog.tsx";
import { RunList } from "./RunList.tsx";
import { startingPointText } from "./columns.tsx";

/** One table row's runs. Each run opens its saved files, checks and judge answers in the same dialog. */
export function CellDialog({ item, batch, onClose }: {
  readonly item: Comparison | null;
  readonly batch: Batch | undefined;
  readonly onClose: () => void;
}) {
  const [runId, setRunId] = useState<string | null>(null);
  const run = runId === null ? undefined : batch?.runs.find((entry) => entry.id === runId);
  const close = () => { setRunId(null); onClose(); };
  const title = item === null ? "" : `${setupName(item.cell.setup)}, ${startingPointText(item)}`;
  const subtitle = item === null ? undefined : `${item.cell.modelLabel}, ${item.cell.judgeLabel === null ? "not judged yet" : `judged by ${item.cell.judgeLabel}`}.`;
  return <Dialog open={item !== null} onClose={close} title={title} subtitle={subtitle}>
    {item === null ? null : batch === undefined ? <p>This batch's records aren't loaded.</p>
      : run === undefined ? <RunList cell={item.cell} batch={batch} open={setRunId} />
      : <>
        <button type="button" className="button button--sm button--secondary" onClick={() => setRunId(null)}>Back to all runs</button>
        <Attempt batch={batch} run={run} />
      </>}
  </Dialog>;
}

import type { Assessment, Batch, Choice, Judge, Run } from "../records/records.types.ts";
import { latestRun } from "./analysis.ts";
import { choiceKey, isReference } from "./observations.ts";
import { modelLabel } from "./names.ts";

/** How far one batch has come. A replaced infrastructure failure or a run waiting to be redone isn't counted. */
export interface BatchProgress {
  readonly planned: number;
  /** Runs whose agent finished, while their grading may still run. */
  readonly implemented: number;
  /** Implemented runs whose latest checks finished without a grader error. */
  readonly checked: number;
  readonly judges: readonly { readonly judge: string; readonly judged: number }[];
}

const Waiting = "limited";

function judged(run: Run, choice: Choice, assessments: readonly Assessment[]): boolean {
  const latest = assessments.filter((item): item is Judge => item.kind === "judge" && item.runId === run.id && choiceKey(item.requestedModel) === choiceKey(choice)).at(-1);
  return latest !== undefined && latest.error === null && latest.answers.length > 0;
}

export function batchProgress(batch: Batch): BatchProgress {
  const replaced = new Set(batch.runs.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  const counted = batch.runs.filter((run) => !isReference(run) && run.pipelineStatus !== Waiting && !(run.failureKind === "infrastructure" && replaced.has(run.id)));
  const implemented = counted.filter((run) => run.agentSeconds !== null);
  const checked = implemented.filter((run) => {
    const coverage = latestRun(run, batch.assessments).result?.coverage;
    return coverage !== undefined && coverage.errors === 0 && coverage.applicable > 0;
  });
  const choices = new Map<string, Choice>();
  for (const item of batch.assessments) if (item.kind === "judge" && item.requestedModel !== null) choices.set(choiceKey(item.requestedModel), item.requestedModel);
  const { agents, setups, runsPerSetup } = batch.experiment.config;
  return {
    planned: batch.experiment.reference === undefined ? agents.length * setups.length * runsPerSetup : 0,
    implemented: implemented.length, checked: checked.length,
    judges: [...choices.values()].map((choice) => ({ judge: modelLabel(choice), judged: implemented.filter((run) => judged(run, choice, batch.assessments)).length })),
  };
}

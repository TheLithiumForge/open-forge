import type { ModelChoice } from "../config/config.types.ts";
import { assessRun } from "./assessments.ts";
import type { RunView } from "./records.types.ts";
import { RecordSchemaVersion, RunStatus } from "./statuses.ts";

export function plannedRun(id: string, batchId: string, setup: string, agent: ModelChoice, repeat: number): RunView {
  return assessRun({
    schemaVersion: RecordSchemaVersion, id, batchId, setup, agent, repeat, pipelineStatus: RunStatus.pending,
    functionalResult: null, failureKind: null, replacesRunId: null, replacementReason: null, artifactHash: null,
    requestedModel: agent.model, reportedModel: null, error: null, startedAt: null, finishedAt: null,
    agentSeconds: null, finalMessage: null, questions: [], telemetry: null, build: null, performance: null, shots: [],
  }, []);
}

export function replacementRun(run: RunView, id: string, reason: string): RunView {
  // A chained run's replacement starts from the same parent.
  return { ...plannedRun(id, run.batchId, run.setup, run.agent, run.repeat), ...(run.parentRunId == null ? {} : { parentRunId: run.parentRunId }), replacesRunId: run.id, replacementReason: reason };
}

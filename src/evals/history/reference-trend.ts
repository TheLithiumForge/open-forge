import type { ModelChoice } from "../config/config.types.ts";
import { coverage } from "../grading/counting.ts";
import { fingerprint } from "./fingerprints.ts";
import type { AssessmentRecord, AssessmentVersions, ChecksAssessment } from "./records.types.ts";
import type { BatchRecord, RunRecord } from "./results.types.ts";
import { ReferenceImplementer, type ReferenceVersion } from "./reference.ts";
import { AssessmentKind } from "./statuses.ts";
import { JudgeAreas, yesShare } from "./summary.ts";
import { listBatches, readAssessments, readRuns, type HistoryStore } from "./store.ts";

export interface ReferencePoint {
  readonly batchId: string;
  readonly runId: string;
  readonly assessmentId: string;
  readonly at: string | null;
  readonly commit: string;
  readonly coverage: number | null;
  readonly rules: number | null;
  readonly code: number | null;
  readonly intent: number | null;
  readonly scores: Readonly<Record<string, number | null>>;
  readonly error: string | null;
}
export interface ReferenceGrading extends AssessmentVersions {
  readonly layer: number | null;
  readonly referenceHash: string;
  readonly specHash: string;
  readonly specVersion: number;
  readonly model: ModelChoice | null;
  readonly reportedModel: string | null;
  readonly ballots: number | null;
}
export interface ReferenceTrend {
  readonly key: string;
  readonly grading: ReferenceGrading;
  readonly points: readonly ReferencePoint[];
}
export interface ReferenceHistory {
  readonly batch: BatchRecord;
  readonly runs: readonly RunRecord[];
  readonly assessments: readonly AssessmentRecord[];
}

export function referenceTrends(history: readonly ReferenceHistory[]): ReferenceTrend[] {
  const groups = new Map<string, { key: string; grading: ReferenceGrading; points: ReferencePoint[] }>();
  for (const { batch, runs, assessments } of history) {
    if (batch.reference === undefined) continue;
    for (const run of runs.filter((item) => item.implementer === ReferenceImplementer)) {
      const reference: ReferenceVersion = run.reference ?? batch.reference;
      const checks = assessments.filter((item): item is ChecksAssessment => item.kind === AssessmentKind.checks && item.runId === run.id);
      for (const assessment of assessments) {
        if (assessment.kind === AssessmentKind.pick || assessment.runId !== run.id) continue;
        const judged = assessment.kind === AssessmentKind.judge ? assessment : null;
        const checked = assessment.kind === AssessmentKind.checks ? assessment : checks.filter((item) => item.versions.checker === assessment.versions.checker
          && item.versions.rubric === assessment.versions.rubric && item.versions.spec === assessment.versions.spec
          && (item.createdAt ?? "") <= (assessment.createdAt ?? "")).at(-1);
        const grading: ReferenceGrading = { ...assessment.versions, layer: batch.config.layer ?? null,
          referenceHash: reference.contentHash, specHash: assessment.versions.spec ?? reference.specHash, specVersion: assessment.versions.specVersion ?? reference.specVersion,
          model: judged?.requestedModel ?? null, reportedModel: judged?.reportedModel ?? null, ballots: judged?.ballots ?? null };
        const key = fingerprint(grading);
        const group = groups.get(key) ?? { key, grading, points: [] };
        const answers = judged?.error === null ? judged.answers : [];
        group.points.push({ batchId: batch.id, runId: run.id, assessmentId: assessment.id, at: assessment.createdAt,
          commit: reference.commit, coverage: checked === undefined || checked.error !== null ? null : coverage(checked.outcomes, checked.selection.applicable).share,
          rules: yesShare(answers, JudgeAreas.rules), code: yesShare(answers, JudgeAreas.code), intent: yesShare(answers, JudgeAreas.intent),
          scores: Object.fromEntries([...new Set(judged?.answers.map((answer) => answer.area) ?? [])].map((area) => [area, yesShare(answers, area)])),
          error: assessment.error ?? checked?.error ?? null });
        groups.set(key, group);
      }
    }
  }
  return [...groups.values()].map((group) => ({ ...group, points: group.points.sort((a, b) => (a.at ?? "").localeCompare(b.at ?? "")) }));
}

export async function referenceHistory(store: Pick<HistoryStore, "listBatches" | "readRuns" | "readAssessments"> = { listBatches, readRuns, readAssessments }): Promise<ReferenceTrend[]> {
  const batches = (await store.listBatches()).filter((batch) => batch.reference !== undefined);
  const history = await Promise.all(batches.map(async (batch) => ({ batch, runs: await store.readRuns(batch), assessments: await store.readAssessments(batch.id) })));
  return referenceTrends(history);
}

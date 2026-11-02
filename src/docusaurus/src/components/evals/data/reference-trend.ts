import type { Batch, Checks } from "../records/records.types.ts";
import { ballotShare, latestRun } from "./analysis.ts";
import { checkKey, isReference } from "./observations.ts";
export interface ReferencePoint {
  readonly id: string;
  readonly at: string | null;
  readonly cliVersion: string | null;
  readonly commit: string;
  readonly coverage: number | null;
  readonly rules: number | null;
  readonly code: number | null;
  readonly wishes: number | null;
  readonly error: string | null;
}
export interface ReferenceTrend {
  readonly key: string;
  readonly layer: number | null;
  readonly points: readonly ReferencePoint[];
}
export function referenceTrends(batches: readonly Batch[]): ReferenceTrend[] {
  const groups = new Map<string, {
    key: string;
    layer: number | null;
    points: ReferencePoint[];
  }>();
  for (const batch of batches)
    for (const run of batch.runs.filter(isReference)) {
      const reference = run.reference ?? batch.experiment.reference;
      if (reference === undefined)
        continue;
      const checks = batch.assessments.filter((item): item is Checks => item.kind === "checks" && item.runId === run.id);
      for (const assessment of batch.assessments) {
        if (assessment.kind === "pick" || assessment.runId !== run.id)
          continue;
        const checked = assessment.kind === "checks" ? assessment : checks.filter((item) => checkKey(item) === checkKey(assessment)
          && (item.createdAt ?? "") <= (assessment.createdAt ?? "")).at(-1);
        const judge = assessment.kind === "judge" ? assessment : null;
        const layer = batch.experiment.config.layer ?? null;
        // Same grouping as the local UI: source, spec, grading and judge identity.
        const key = JSON.stringify([layer, reference.contentHash, assessment.versions.spec ?? reference.specHash,
          assessment.versions.specVersion ?? reference.specVersion, assessment.versions, judge?.requestedModel,
          judge?.reportedModel, judge?.ballots]);
        const group = groups.get(key) ?? { key, layer, points: [] };
        const view = latestRun({ ...run, functionalResult: null }, [
          ...checked === undefined ? [] : [checked], ...judge === null ? [] : [judge],
        ]);
        group.points.push({
          id: assessment.id, at: assessment.createdAt, cliVersion: batch.experiment.cliVersion,
          commit: reference.commit, coverage: view.result?.coverage.share ?? null, rules: ballotShare(view, "rules"),
          code: ballotShare(view, "code"), wishes: ballotShare(view, "intent"), error: assessment.error ?? checked?.error ?? null
        });
        groups.set(key, group);
      }
    }
  return [...groups.values()].map((group) => ({ ...group, points: group.points.sort((a, b) => (a.at ?? "").localeCompare(b.at ?? "")) }));
}

import assert from "node:assert/strict";
import { AllTargets } from "../targets.ts";
import { BuildWorkflow, newestBuilds, type BuildRun, type BuildArtifact } from "../ci/build-runs.ts";
import { validateBuildRun } from "./release-selection.ts";

export interface RecordedBuild {
  run: BuildRun;
  artifacts: readonly BuildArtifact[];
}

export function validateBuildArtifacts(artifacts: readonly BuildArtifact[]): void {
  const missing = AllTargets.filter((rid) => !artifacts.some((artifact) => artifact.name === `${BuildWorkflow.artifactPrefix}${rid}` && !artifact.expired));
  assert.ok(missing.length === 0, `Missing or expired package artifacts: ${missing.join(", ")}.`);
}

export function buildRejection(run: BuildRun, repository: string, sha: string): string | undefined {
  try {
    validateBuildRun(run, repository, sha);
    return undefined;
  } catch (error) {
    return error instanceof Error ? error.message : String(error);
  }
}

export function chooseReusableBuild(records: readonly RecordedBuild[], repository: string, sha: string): { run?: BuildRun; reason: string } {
  const rejected: string[] = [];
  for (const run of newestBuilds(records.map((record) => record.run))) {
    let reason = buildRejection(run, repository, sha);
    if (!reason) {
      const artifacts = records.find((record) => record.run.id === run.id)?.artifacts ?? [];
      try {
        validateBuildArtifacts(artifacts);
      } catch (error) {
        reason = error instanceof Error ? error.message : String(error);
      }
    }
    if (!reason) return { run, reason: `Reusing Build ${run.id} for ${sha}: most recent successful exact-commit build.yml run with all six unexpired package artifacts.` };
    rejected.push(`Build ${run.id}: ${reason}`);
  }
  return { reason: `No reusable Build qualified for ${sha}. ${rejected.length ? rejected.join("\n") : "No build.yml runs were found."}` };
}

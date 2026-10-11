import assert from "node:assert/strict";
import { buildRuns, buildRun, buildArtifacts } from "../ci/build-api.ts";
import type { GithubConnection } from "../ci/github-api.ts";
import { validateBuildRun } from "./release-selection.ts";
import { buildRejection, chooseReusableBuild, validateBuildArtifacts, type RecordedBuild } from "./reusable-build.ts";

export async function selectBuild(connection: GithubConnection, sha: string, suppliedId: string): Promise<string> {
  if (suppliedId) {
    assert.match(suppliedId, /^\d+$/u, "The build run ID must be numeric.");
    const run = await buildRun(connection, suppliedId);
    validateBuildRun(run, connection.repository, sha);
    validateBuildArtifacts(await buildArtifacts(connection, run.id));
    process.stdout.write(`Reusing supplied Build ${run.id} for ${sha}: validation passed and all six package artifacts are present and unexpired.\n`);
    return suppliedId;
  }
  const records: RecordedBuild[] = [];
  for (const run of await buildRuns(connection, sha)) {
    const artifacts = buildRejection(run, connection.repository, sha) ? [] : await buildArtifacts(connection, run.id);
    records.push({ run, artifacts });
  }
  const choice = chooseReusableBuild(records, connection.repository, sha);
  process.stdout.write(`${choice.reason}\n`);
  return choice.run ? String(choice.run.id) : "";
}

import assert from "node:assert/strict";
import { test } from "node:test";
import { chooseReusableBuild, validateBuildArtifacts, buildRejection } from "../reusable-build.ts";
import {
  BuildId,
  BuildRepository,
  BuildSha,
  BuildTime,
  FailedBuild,
  OtherBuildSha,
  OtherRepository,
  OtherWorkflow,
  PackageArtifacts,
  recordedBuild,
} from "../../__tests__/build-fixtures.ts";

test("release selection explains when no Build runs exist", () => {
  const choice = chooseReusableBuild([], BuildRepository, BuildSha);
  assert.equal(choice.run, undefined);
  assert.match(choice.reason, /No build.yml runs were found/u);
});

for (const [scenario, run] of [
  ["failed", recordedBuild({ conclusion: FailedBuild })],
  ["another commit", recordedBuild({ head_sha: OtherBuildSha })],
  ["another repository", recordedBuild({ repository: { full_name: OtherRepository } })],
  ["another workflow", recordedBuild({ path: OtherWorkflow })],
] as const) {
  test(`release selection rejects a Build from ${scenario}`, () => {
    assert.ok(buildRejection(run, BuildRepository, BuildSha));
    assert.equal(chooseReusableBuild([{ run, artifacts: PackageArtifacts }], BuildRepository, BuildSha).run, undefined);
  });
}

for (const [scenario, artifacts] of [
  ["missing", PackageArtifacts.slice(1)],
  ["expired", PackageArtifacts.map((artifact, index) => ({ ...artifact, expired: index === 0 }))],
] as const) {
  test(`release selection rejects ${scenario} platform artifacts`, () => {
    assert.throws(() => validateBuildArtifacts(artifacts), /Missing or expired/u);
    const choice = chooseReusableBuild([{ run: recordedBuild(), artifacts }], BuildRepository, BuildSha);
    assert.equal(choice.run, undefined);
    assert.match(choice.reason, /Missing or expired/u);
  });
}

test("release selection chooses the newest qualifying run independent of API page order", () => {
  validateBuildArtifacts(PackageArtifacts);
  assert.equal(buildRejection(recordedBuild(), BuildRepository, BuildSha), undefined);
  const newest = recordedBuild({ id: BuildId.newer, created_at: BuildTime.newer });
  const records = [
    { run: recordedBuild(), artifacts: PackageArtifacts },
    { run: newest, artifacts: PackageArtifacts },
  ];
  assert.equal(chooseReusableBuild(records, BuildRepository, BuildSha).run?.id, newest.id);
  assert.equal(chooseReusableBuild([...records].reverse(), BuildRepository, BuildSha).run?.id, newest.id);
  records[1] = { run: newest, artifacts: [] };
  assert.equal(chooseReusableBuild(records, BuildRepository, BuildSha).run?.id, BuildId.older);
});

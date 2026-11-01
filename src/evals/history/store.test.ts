import assert from "node:assert/strict";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { ResultFiles } from "../paths.ts";
import { recheckRun } from "../grading/recheck.ts";
import { rejudgeRun } from "../grading/rejudge.ts";
import { artifactHash, HistoryFiles } from "./artifacts.ts";
import { checksAssessment } from "./assessments.ts";
import { readJson } from "./json-files.ts";
import { sha256 } from "./fingerprints.ts";
import { replacementRun } from "./run-records.ts";
import { AssessmentKind, CheckStatus, FailureKind, PreferenceWinner, RunStatus } from "./statuses.ts";
import { FixtureCheckId, FixtureVersions, temporaryHistory, v2Fixture } from "./test-fixtures.ts";

test("recheck and rejudge append independently and readers can choose an earlier grade", async () => {
  const { store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    await store.saveBatch(batch);
    await store.saveRun(run);
    const initial = checksAssessment(run, run.checks, FixtureVersions);
    await store.appendAssessment(initial);
    const beforeRun = await readFile(join(store.runFolder(batch.id, run.id), ResultFiles.run));
    const beforeBatch = await readFile(join(store.batchFolder(batch.id), HistoryFiles.experiment));
    const checkPath = join(store.batchFolder(batch.id), HistoryFiles.assessments, `${AssessmentKind.checks}.jsonl`);
    const beforeChecks = await readFile(checkPath, "utf8");
    const saved = await store.readRun(batch.id, run.id);
    await Promise.all([
      recheckRun(saved, FixtureVersions, { append: (record) => store.appendAssessment(record), evaluate: async (_, assessmentId) => {
        const shots = join(store.assessmentEvidenceFolder(batch.id, assessmentId), ResultFiles.shots);
        await mkdir(shots, { recursive: true });
        await writeFile(join(shots, "proof.png"), "fake screenshot");
        return { checks: [{ id: FixtureCheckId, area: "play", passed: null, detail: "blocked" }], performance: null, shots: [`${assessmentId}/proof.png`] };
      } }),
      rejudgeRun(saved, FixtureVersions, { append: (record) => store.appendAssessment(record), evaluate: async () => [{
        id: "legible", area: "code", question: "legible", yes: 1, no: 0, unsure: 0, evidence: ["source"],
      }] }),
    ]);
    const latest = await store.readRun(batch.id, run.id);
    assert.equal(latest.checkOutcomes[0]?.status, CheckStatus.blocked);
    assert.equal(latest.functionalResult?.coverage.share, 0);
    assert.equal(latest.judge[0]?.yes, 1);
    assert.equal(latest.pipelineStatus, RunStatus.done);
    const evidence = (await store.readArtifacts(batch)).find((file) => file.assessmentId === latest.checksAssessmentId);
    assert.equal(evidence?.path, `${ResultFiles.shots}/proof.png`);
    assert.equal((await store.readRun(batch.id, run.id, { checks: initial.id })).functionalResult?.coverage.share, 1);
    assert.equal((await store.readAssessments(batch.id)).length, 3);
    assert.equal((await readFile(checkPath, "utf8")).startsWith(beforeChecks), true);
    assert.deepEqual(await readFile(join(store.runFolder(batch.id, run.id), ResultFiles.run)), beforeRun);
    assert.deepEqual(await readFile(join(store.batchFolder(batch.id), HistoryFiles.experiment)), beforeBatch);
    await assert.rejects(store.appendAssessment(initial));
    await assert.rejects(store.readRun(batch.id, run.id, { judge: initial.id }));
  } finally { await cleanup(); }
});

test("a failed recheck stays as an error assessment and a judge error leaves checks alone", async () => {
  const { store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    await store.saveBatch(batch);
    await store.saveRun(run);
    await store.appendAssessment(checksAssessment(run, run.checks, FixtureVersions));
    const saved = await store.readRun(batch.id, run.id);
    const fail = async (): Promise<never> => { throw new Error("grader unavailable"); };
    const judge = await rejudgeRun(saved, FixtureVersions, { append: (record) => store.appendAssessment(record), evaluate: fail });
    assert.notEqual(judge.error, null);
    assert.equal((await store.readRun(batch.id, run.id)).functionalResult?.coverage.share, 1);
    const checked = await recheckRun(saved, FixtureVersions, { append: (record) => store.appendAssessment(record), evaluate: fail });
    assert.equal(checked.outcomes[0]?.status, CheckStatus.error);
    assert.equal((await store.readRun(batch.id, run.id)).functionalResult?.coverage.errors, 1);
    assert.equal((await store.readAssessments(batch.id)).length, 3);
  } finally { await cleanup(); }
});

test("manifests hash saved files and picks record both sides and reveal state", async () => {
  const { store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    const replacement = replacementRun(run, `${run.id}-next`, "infrastructure retry");
    await store.saveBatch({ ...batch, runs: [run.id, replacement.id] });
    await store.saveRun({ ...run, status: RunStatus.failed, failureKind: FailureKind.infrastructure });
    const game = "saved game";
    await writeFile(join(store.runFolder(batch.id, run.id), ResultFiles.game), game);
    await store.saveRun(run);
    await store.saveRun(replacement);
    const artifacts = await store.readArtifacts(await store.readBatch(batch.id));
    assert.equal(artifacts[0]?.path, ResultFiles.game);
    assert.equal(artifacts[0]?.bytes, Buffer.byteLength(game));
    assert.equal(artifacts[0]?.sha256, sha256(game));
    const saved = await store.readRun(batch.id, run.id);
    assert.equal(saved.artifactHash, artifactHash(artifacts));
    await store.addPreference(batch.id, { left: run.id, right: replacement.id, winner: PreferenceWinner.tie, at: batch.createdAt });
    const pick = (await store.readAssessments(batch.id)).find((record) => record.kind === AssessmentKind.pick);
    assert.equal(pick?.left.artifactHash, saved.artifactHash);
    assert.equal(pick?.right.side, PreferenceWinner.right);
    assert.equal(pick?.revealed, true);
    await assert.rejects(store.addPreference(batch.id, { left: run.id, right: run.id, winner: PreferenceWinner.tie, at: batch.createdAt }));
    assert.equal((await store.readRun(batch.id, replacement.id)).replacesRunId, run.id);
    const raw = await readJson<Record<string, unknown>>(join(store.runFolder(batch.id, run.id), ResultFiles.run));
    assert.equal("checks" in raw, false);
    assert.equal("judge" in raw, false);
  } finally { await cleanup(); }
});

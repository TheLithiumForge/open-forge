import assert from "node:assert/strict";
import { writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { recordPick } from "./picks.ts";
import { temporaryHistory, v2Fixture } from "../../history/test-fixtures.ts";
import { PreferenceWinner } from "../../history/statuses.ts";
import { ResultFiles } from "../../paths.ts";

test("blind and revealed picks append both hashes and the side shown to the person", async () => {
  const { store, cleanup } = await temporaryHistory();
  try {
    const { batch, run } = v2Fixture();
    const other = { ...run, id: `${run.id}-other` };
    await store.saveBatch({ ...batch, revealed: false, runs: [run.id, other.id] });
    for (const saved of [run, other]) {
      await store.saveRun(saved);
      await writeFile(join(store.runFolder(batch.id, saved.id), ResultFiles.game), "authored fixture");
      await store.saveRun(saved);
    }
    const preference = { left: run.id, right: other.id, winner: PreferenceWinner.left, at: batch.createdAt };
    assert.equal(await recordPick(batch.id, preference, store), true);
    await store.saveBatch({ ...await store.readBatch(batch.id), revealed: true });
    assert.equal(await recordPick(batch.id, { ...preference, left: other.id, right: run.id, winner: PreferenceWinner.tie }, store), true);
    const picks = (await store.readAssessments(batch.id)).filter((item) => item.kind === "pick");
    assert.deepEqual(picks.map((pick) => pick.revealed), [false, true]);
    assert.equal(picks[0]?.left.artifactHash, (await store.readRun(batch.id, run.id)).artifactHash);
    assert.equal(picks[1]?.right.runId, run.id);
    assert.equal(picks[1]?.right.side, PreferenceWinner.right);
    assert.equal(await recordPick(batch.id, { ...preference, right: run.id }, store), false);
  } finally { await cleanup(); }
});

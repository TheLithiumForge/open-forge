import assert from "node:assert/strict";
import { test } from "node:test";
import { runCli, type CliServices } from "../cli.ts";
import { Command, Flag } from "./commands.ts";
import { batchMatrixSummary } from "./summary.ts";
import { MatrixAgents, MatrixJudges } from "./matrix.ts";
import { temporaryHistory, v2Fixture } from "../history/test-fixtures.ts";

test("summary reads a saved batch and prints all planned pairs without model services", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const { batch, run } = v2Fixture();
  await history.store.saveBatch({ ...batch, config: { ...batch.config, layer: 2, agents: MatrixAgents, judge: MatrixJudges[0], judges: MatrixJudges } });
  await history.store.saveRun({ ...run, agent: MatrixAgents[0] });
  const output: string[] = [], noEffect = async () => { throw new Error("An unrelated command ran."); };
  const services: CliServices = { print: (text) => output.push(text), doctor: noEffect, images: noEffect, smoke: noEffect, qualify: noEffect, plan: noEffect,
    suite: noEffect, run: noEffect, ui: noEffect, launch: noEffect, export: noEffect, validateCase: noEffect, summary: (id) => batchMatrixSummary(id, history.store) };
  assert.equal(await runCli([Command.summary, Flag.batch, batch.id], services), 0);
  const value = JSON.parse(output[0] ?? "null");
  assert.equal(value.layer, 2);
  assert.equal(value.pairs.length, MatrixAgents.length * MatrixJudges.length);
  assert.ok(value.pairs.every((pair: object) => !("runs" in pair)));
  assert.equal(value.implementationAttempts, 1);
  await assert.rejects(runCli([Command.summary], services));
  await assert.rejects(runCli([Command.summary, Flag.batch, batch.id, Flag.all], services));
});

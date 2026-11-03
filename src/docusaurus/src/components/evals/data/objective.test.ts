import assert from "node:assert/strict";
import { test } from "node:test";
import { previewBatch } from "../../../../scripts/evals/preview-fixture.ts";
import type { Batch } from "../records/records.types.ts";
import { comparisons } from "./aggregate.ts";
import { cells, newestCells, OneAgent } from "./cells.ts";
import { latestAnalyses, objectiveRows } from "./objective.ts";
import { milestoneRows, OrchestratedSuffix, orchestrationRows } from "./work-views.ts";

const Chain = "fixture-chain";
const byId = (batches: readonly Batch[]) => new Map(batches.map((batch) => [batch.experiment.id, batch]));
const specOf = (batch: Batch, runId: string) => latestAnalyses(batch).get(runId)?.parts.filter((part) => part.class === "spec").reduce((sum, part) => sum + part.input, 0) ?? 0;

/** The preview batch as a later milestone of a chain, under its own id. */
function chained(batch: Batch): Batch {
  const id = `${batch.experiment.id}-chained`;
  return { ...batch, experiment: { ...batch.experiment, id, createdAt: "2026-02-01T00:00:00.000Z", chain: { id: Chain, milestone: 2, parentBatchId: batch.experiment.id } },
    runs: batch.runs.map((run) => ({ ...run, batchId: id })), setups: batch.setups.map((setup) => ({ ...setup, experimentId: id })),
    assessments: batch.assessments.map((item) => ({ ...item, experimentId: id })), analyses: (batch.analyses ?? []).map((item) => ({ ...item, experimentId: id })) };
}

test("each setup's objective measures average its runs' analyses and sit beside the control's", () => {
  const batch = previewBatch(1);
  const rows = objectiveRows(comparisons(cells([batch])), byId([batch]));
  const control = batch.experiment.config.setups[0], compared = batch.experiment.config.setups[1];
  const runsOf = (setup: string | undefined) => batch.runs.filter((run) => run.setup === setup).map((run) => specOf(batch, run.id));
  const mean = (values: readonly number[]) => values.reduce((sum, value) => sum + value, 0) / values.length;
  const controlRow = rows.find((row) => row.setup === control), comparedRow = rows.find((row) => row.setup === compared);
  assert.equal(controlRow?.control, null);
  assert.equal(controlRow?.measures.classes["spec"], mean(runsOf(control)));
  assert.equal(comparedRow?.measures.classes["spec"], mean(runsOf(compared)));
  assert.equal(comparedRow?.control?.classes["spec"], mean(runsOf(control)));
  assert.equal(comparedRow?.measures.rough, 0);
});

test("a chained milestone keeps its own cells beside the standalone runs of the same task and meets them as its twin", () => {
  const standalone = previewBatch(1), milestone = chained(standalone);
  const items = newestCells(comparisons(cells([standalone, milestone])));
  assert.ok(items.some((item) => item.cell.chain === null) && items.some((item) => item.cell.chain?.id === Chain));
  const rows = milestoneRows(items);
  assert.ok(rows.length > 0);
  assert.ok(rows.every((row) => row.chain === Chain && row.milestone === 2 && row.tokens === row.standaloneTokens));
});

test("an orchestrated setup meets its one-agent twin in the same batch with the orchestrator's share of input", () => {
  const base = previewBatch(1), twin = base.experiment.config.setups[0] ?? "", orchestrated = `${twin}${OrchestratedSuffix}`;
  const moved = new Set(base.runs.filter((run) => run.setup === twin && run.repeat === 1).map((run) => run.id));
  const tokens = { newInput: 100, cachedInput: 0, cacheWrite: 0, output: 0, reasoning: null };
  const thread = (id: string, role: "main" | "subagent", newInput: number) => ({ id, parentId: null, role, requests: 1, tokens: { ...tokens, newInput }, peakContext: null, toolCalls: 0 });
  const batch: Batch = { ...base, experiment: { ...base.experiment, config: { ...base.experiment.config, setups: [...base.experiment.config.setups, orchestrated] } },
    setups: [...base.setups, { ...base.setups[0], schemaVersion: 2, id: orchestrated, experimentId: base.experiment.id, label: orchestrated, commands: [], contentHash: null, files: [], workMode: "orchestrated" }],
    runs: base.runs.map((run) => moved.has(run.id) ? { ...run, setup: orchestrated } : run),
    analyses: (base.analyses ?? []).map((item) => moved.has(item.runId) ? { ...item, threads: [thread("o", "main", 100), thread("s", "subagent", 300)] } : item) };
  const rows = orchestrationRows(comparisons(cells([batch])), byId([batch]));
  assert.equal(rows.length, batch.experiment.config.agents.length);
  assert.ok(rows.every((row) => row.orchestratorShare === 0.25 && row.subagents === 1 && row.oneAgentTokens !== undefined));
  assert.ok(cells([batch]).some((cell) => cell.workMode === OneAgent && cell.setup === twin));
});

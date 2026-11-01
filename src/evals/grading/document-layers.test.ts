import assert from "node:assert/strict";
import { test } from "node:test";
import { loadSuite } from "../benchmarks/suite.ts";
import { runLayer2Checks, runLayer3Checks } from "../benchmarks/neon-pairs/checks/spec-layers/container.ts";
import { LayerId } from "../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";
import { DocumentCase } from "../materialize/delivery.ts";
import { documentGradingInputs, documentLayer, layerChecks, layerQuestions } from "./document-layers.ts";
import { judgePrompt, judgeQuestions } from "./judge/judge.ts";

test("document layers select their existing container checks and frozen judge questions", async () => {
  for (const [id, layer, checks] of [[DocumentCase.two, LayerId.two, runLayer2Checks], [DocumentCase.three, LayerId.three, runLayer3Checks]] as const) {
    assert.equal(documentLayer(id), layer); assert.equal(layerChecks(id), checks);
    const suite = await loadSuite(id), questions = await judgeQuestions(suite);
    assert.deepEqual(questions, await layerQuestions(id)); assert.ok(questions.length > 0);
    const versions = await documentGradingInputs(id);
    assert.deepEqual(versions.map((file) => file.path), [`layers/${layer}.json`, `rubrics/${layer}.json`]);
    assert.ok(versions.every((file) => /^[a-f\d]{64}$/.test(file.sha256)));
    const prompt = judgePrompt(suite, questions, { sources: [], images: [] });
    assert.ok(suite.documents?.every((document) => prompt.includes(document)));
  }
});

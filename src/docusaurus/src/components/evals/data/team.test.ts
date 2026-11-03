import assert from "node:assert/strict";
import { test } from "node:test";
import { modelLabel } from "./names.ts";
import { choiceKey } from "./observations.ts";

test("a team is its own implementer on the site: its key and label differ from the solo agent's, and a solo key keeps its old form", () => {
  const solo = { tool: "codex", model: "gpt-6.1-sol", effort: "high" };
  const sol = { ...solo, team: { model: "gpt-6.1-sol", effort: "high" } }, luna = { ...solo, team: { model: "gpt-6-luna", effort: "high" } };
  assert.equal(new Set([solo, sol, luna].map(choiceKey)).size, 3);
  assert.equal(choiceKey(solo), JSON.stringify([solo.tool, solo.model, solo.effort]));
  assert.equal(new Set([solo, sol, luna].map((choice) => modelLabel(choice))).size, 3);
  assert.ok(modelLabel(luna).startsWith(modelLabel(solo)));
});

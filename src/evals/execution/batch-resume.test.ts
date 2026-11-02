import assert from "node:assert/strict";
import { test } from "node:test";
import { RunStatus } from "../history/statuses.ts";
import { gradedOnResume, replacedOnResume } from "./batch.ts";

test("resume replaces any first attempt and a replacement only when a usage limit stopped it, and never a run twice", () => {
  for (const status of [RunStatus.agent, RunStatus.judging, RunStatus.limited]) assert.equal(replacedOnResume({ status, replacesRunId: null }, false), true);
  assert.equal(replacedOnResume({ status: RunStatus.limited, replacesRunId: "first" }, false), true);
  for (const status of [RunStatus.agent, RunStatus.building, RunStatus.failed]) assert.equal(replacedOnResume({ status, replacesRunId: "first" }, false), false);
  for (const status of [RunStatus.agent, RunStatus.limited]) assert.equal(replacedOnResume({ status, replacesRunId: null }, true), false);
});

test("resume grades saved files only when the agent finished and grading stopped", () => {
  for (const status of [RunStatus.checking, RunStatus.judging]) assert.equal(gradedOnResume({ status, agentSeconds: 60 }, true), true);
  assert.equal(gradedOnResume({ status: RunStatus.checking, agentSeconds: 60 }, false), false);
  assert.equal(gradedOnResume({ status: RunStatus.checking, agentSeconds: null }, true), false);
  for (const status of [RunStatus.agent, RunStatus.building, RunStatus.limited, RunStatus.pending]) assert.equal(gradedOnResume({ status, agentSeconds: 60 }, true), false);
});

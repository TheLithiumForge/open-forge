import assert from "node:assert/strict";
import { test } from "node:test";
import { RunStatus } from "../history/statuses.ts";
import { replacedOnResume } from "./batch.ts";

test("resume replaces any first attempt and a replacement only when a usage limit stopped it", () => {
  for (const status of [RunStatus.agent, RunStatus.judging, RunStatus.limited]) assert.equal(replacedOnResume({ status, replacesRunId: null }), true);
  assert.equal(replacedOnResume({ status: RunStatus.limited, replacesRunId: "first" }), true);
  for (const status of [RunStatus.agent, RunStatus.building, RunStatus.failed]) assert.equal(replacedOnResume({ status, replacesRunId: "first" }), false);
});

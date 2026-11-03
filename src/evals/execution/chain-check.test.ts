import assert from "node:assert/strict";
import { test } from "node:test";
import { changedChecks } from "./chain-check.ts";

test("a chain check lists only regression and guard checks whose result moved from the parent's, never the new task's own", () => {
  const parent = [{ id: "regression-l2-best", status: "pass" }, { id: "regression-l2-focus", status: "pass" }, { id: "repair-B1-01", status: "pass" }];
  const child = [
    { id: "regression-l2-best", status: "pass", area: "regression" }, { id: "regression-l2-focus", status: "fail", area: "regression" },
    { id: "repair-B2-01", status: "fail", area: "repair" }, { id: "brownfield-diff-scope", status: "pass", area: "brownfield" },
  ];
  assert.deepEqual(changedChecks(parent, child), [{ id: "regression-l2-focus", parent: "pass", child: "fail" }, { id: "brownfield-diff-scope", parent: null, child: "pass" }]);
});

import assert from "node:assert/strict";
import { test } from "node:test";
import { CheckStatus } from "../../../history/statuses.ts";
import { loadLayer } from "./layer.ts";
import { parseCheckReport } from "./report.ts";

test("container reports need each declared ID, a valid status and evidence", async () => {
  const layer = await loadLayer();
  const seed = { checks: layer.checks.map((check) => ({ ...check, status: CheckStatus.pass, evidence: "Checked" })), shots: [], performance: null };
  assert.equal(parseCheckReport(seed, layer).checks.length, layer.checks.length);
  const first = seed.checks[0];
  assert.ok(first);
  for (const changes of [{ status: "unknown" }, { evidence: "" }, { id: "undeclared" }]) {
    assert.throws(() => parseCheckReport({ ...seed, checks: [{ ...first, ...changes }, ...seed.checks.slice(1)] }, layer));
  }
  assert.throws(() => parseCheckReport({ ...seed, checks: [first, first, ...seed.checks.slice(2)] }, layer));
  assert.throws(() => parseCheckReport({ ...seed, shots: ["../outside.png"] }, layer));
  assert.throws(() => parseCheckReport({ ...seed, performance: { firstRenderMs: -1, largeBoardTapP95Ms: null } }, layer));
});

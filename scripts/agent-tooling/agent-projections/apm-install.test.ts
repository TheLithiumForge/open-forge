import assert from "node:assert/strict";
import { test } from "node:test";
import { describeStep, installSteps } from "./apm-install.ts";

test("APM installation runs the frozen install before the model policy patch", () => {
  const steps = installSteps("node");
  assert.deepEqual(
    steps.map((step) => step.executable),
    ["apm", "node"],
  );
  assert.equal(describeStep(steps[0] ?? assert.fail("missing install step")), "Install locked APM packages: apm install --frozen --target codex");
  assert.match(steps[1]?.args[0] ?? "", /patch-codex-agent-models\.ts$/u);
});

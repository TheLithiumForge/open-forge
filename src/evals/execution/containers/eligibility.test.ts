import assert from "node:assert/strict";
import { test } from "node:test";
import { Isolation } from "../../config/choices.ts";
import { executionLabel, LocalExecutionLabel } from "./eligibility.ts";
import { AuthVariables } from "../../config/auth.ts";
import { dockerEnvironment } from "./docker.ts";

test("every local execution label excludes scored results", () => {
  assert.deepEqual(executionLabel(Isolation.local), LocalExecutionLabel);
  assert.equal(LocalExecutionLabel.fitForScoredResults, false);
});

test("Docker client environment keeps model credentials out unless selected by its caller", () => {
  const env = dockerEnvironment();
  for (const name of Object.values(AuthVariables)) assert.equal(name in env, false);
});

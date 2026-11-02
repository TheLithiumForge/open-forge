import assert from "node:assert/strict";
import { test } from "node:test";
import { validationReport } from "./validation-inputs.ts";

test("validation reports written beside grader sources aren't grader inputs", () => {
  for (const path of ["L1.validation.json", "spec-layers/L3.validation.json"]) assert.equal(validationReport(path), true);
  for (const path of ["L1.json", "L1.requirements.json", "validation.ts", "checks/validate.ts"]) assert.equal(validationReport(path), false);
});

import assert from "node:assert/strict";
import { test } from "node:test";
import { mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { validationReport } from "./validation-inputs.ts";
import { protectedHashes } from "./protected.ts";

test("validation reports written beside grader sources aren't grader inputs", () => {
  for (const path of ["L1.validation.json", "spec-layers/L3.validation.json", "cases/l4-f01/checks/validation.json"]) assert.equal(validationReport(path), true);
  for (const path of ["L1.json", "L1.requirements.json", "validation.ts", "checks/validate.ts", "checks/invalidation.json"]) assert.equal(validationReport(path), false);
});

test("a validation report written during a check leaves the protected hashes unchanged", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "eval-protected-")), project = await mkdtemp(join(tmpdir(), "eval-protected-project-"));
  t.after(async () => { await rm(root, { recursive: true, force: true }); await rm(project, { recursive: true, force: true }); });
  await mkdir(join(root, "layers"));
  await writeFile(join(root, "layers", "L1.json"), "{}");
  const before = await protectedHashes([root], project);
  await writeFile(join(root, "layers", "L1.validation.json"), "{}");
  assert.deepEqual(await protectedHashes([root], project), before);
  await writeFile(join(root, "layers", "L1.json"), "{\"changed\":true}");
  assert.notDeepEqual(await protectedHashes([root], project), before);
});

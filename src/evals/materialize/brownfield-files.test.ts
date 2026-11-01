import assert from "node:assert/strict";
import { test } from "node:test";
import { startingReadme } from "./brownfield-files.ts";
import { AcceptanceLine, SetupFiles } from "./setup.ts";

test("brownfield document pointers use the supplied docs and code only keeps the build instructions", () => {
  const build = "Run npm test to check the project.";
  const readme = [{ path: SetupFiles.readme, bytes: Buffer.from(`# Project\n\nRead [the rules](../specs/rules.md).\n\n${build}\n`) }];
  const docs = startingReadme(readme, AcceptanceLine);
  assert.ok(docs?.includes("](docs/rules.md)"));
  assert.ok(docs?.includes(AcceptanceLine));
  const code = startingReadme(readme, null);
  assert.ok(code?.includes(build));
  assert.ok(!code?.includes("../specs/"));
  assert.ok(!code?.includes(AcceptanceLine));
});

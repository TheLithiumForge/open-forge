import assert from "node:assert/strict";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { sha256 } from "../../materialize/files.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { protectedOutcome } from "./protected.ts";

const Pack = "docs-pack", Given = "A project document long enough to count.", Secret = "A grader's private file long enough to count.";

test("a case's document pack belongs to the project, and its other files stay private", async (t) => {
  const project = await mkdtemp(join(tmpdir(), "protected-"));
  t.after(() => rm(project, { recursive: true, force: true }));
  const item = { documents: Pack, specDelta: null, inputs: [{ path: `${Pack}/given.md`, sha256: sha256(Given) }, { path: "rules.json", sha256: sha256(Secret) }] };
  await writeFile(join(project, "given.md"), Given);
  assert.equal((await protectedOutcome(item, project, [], [])).status, CheckStatus.pass);
  await writeFile(join(project, "copied.md"), Secret);
  assert.equal((await protectedOutcome(item, project, [], [])).status, CheckStatus.fail);
});

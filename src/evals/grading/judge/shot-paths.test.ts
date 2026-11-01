import assert from "node:assert/strict";
import { join } from "node:path";
import { test } from "node:test";
import { CompletionShot } from "../../benchmarks/neon-pairs/checks/screenshots.ts";
import { ResultFiles } from "../../paths.ts";
import { judgeImagePaths } from "./shot-paths.ts";

test("judge images resolve both original and reassessment evidence without accepting escaped paths", () => {
  const run = { batchId: "batch", id: "run", shots: [CompletionShot, `assessment/${CompletionShot}`] };
  const stores = { run: () => "original", assessment: () => "rechecked" };
  assert.deepEqual(judgeImagePaths(run, stores), [join(stores.run(), ResultFiles.shots, CompletionShot), join(stores.assessment(), ResultFiles.shots, CompletionShot)]);
  for (const shot of [`../${CompletionShot}`, `assessment/../${CompletionShot}`, "unknown.png"]) assert.throws(() => judgeImagePaths({ ...run, shots: [shot] }, stores));
});

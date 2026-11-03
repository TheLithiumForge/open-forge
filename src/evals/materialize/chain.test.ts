import assert from "node:assert/strict";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { chainedFiles, chainIgnored, SeedConflict, writeChainedStart } from "./chain.ts";
import { IgnoreFile } from "./ignore.ts";

const Ignores = [{ path: IgnoreFile, content: "dist/\ntest-results/\n.agents/\n" }];
const Original = "export const best = (previous: number, moves: number) => Math.min(previous, moves);\n";
const Seed = [
  "diff --git a/src/best.ts b/src/best.ts", "--- a/src/best.ts", "+++ b/src/best.ts", "@@ -1 +1 @@",
  `-${Original.trimEnd()}`, "+export const best = (_previous: number, moves: number) => moves;", "",
].join("\n");
const file = (path: string, text: string) => ({ path, bytes: Buffer.from(text) });

test("a chained start leaves build output behind but carries the setup's context files", () => {
  const ignored = chainIgnored(Ignores);
  assert.equal(ignored("dist", true), true);
  assert.equal(ignored("test-results", true), true);
  assert.equal(ignored("src", true), false);
  assert.equal(ignored(".agents", true), false, "Context files carry over even when an ignore rule names them.");
});

test("the next defect applies to the parent's files, and a parent that changed its lines gives a seed conflict", () => {
  const planted = chainedFiles([file("src/best.ts", Original), file("README.md", "Neon Pairs\n")], Seed);
  assert.match(planted.find((item) => item.path === "src/best.ts")?.bytes.toString() ?? "", /=> moves;/);
  assert.equal(planted.find((item) => item.path === "README.md")?.bytes.toString(), "Neon Pairs\n");
  assert.throws(() => chainedFiles([file("src/best.ts", "export const best = () => 0;\n")], Seed), SeedConflict);
});

test("writing a chained start copies the kept files under the project folder and lists their hashes", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "chain-test-"));
  t.after(() => rm(root, { recursive: true, force: true }));
  const parent = join(root, "parent");
  for (const [path, text] of [["src/best.ts", Original], [".agents/memory/note.md", "kept\n"], ["dist/index.html", "built\n"], ["node_modules/x/index.js", "x\n"]] as const) {
    await mkdir(dirname(join(parent, path)), { recursive: true });
    await writeFile(join(parent, path), text);
  }
  const start = await writeChainedStart(parent, Ignores, Seed, "neon-pairs");
  t.after(() => rm(dirname(start.folder), { recursive: true, force: true }));
  assert.deepEqual(start.files.map((item) => item.path), [".agents/memory/note.md", "src/best.ts"]);
  assert.match(await readFile(join(start.folder, "src/best.ts"), "utf8"), /=> moves;/);
});

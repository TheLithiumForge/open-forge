import assert from "node:assert/strict";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { temporaryHistory } from "../history/test-fixtures.ts";
import { fingerprint, sha256 } from "../history/fingerprints.ts";
import { SpecVersion } from "../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";
import { copyReference, ReferenceSourceCommands, type SourceCommand } from "./reference-source.ts";

const Commit = "a".repeat(40), Spec = sha256("fixture-spec"), Source = "export const game = 1;\n";
const Tracked = ["src/game.ts", "node_modules/output.ts", "dist/index.html", "ignored-output.txt"];

test("reference copies tracked plain files and hashes their bytes while excluding ignored output", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const source = join(history.root, "source");
  await mkdir(join(source, "src"), { recursive: true });
  await writeFile(join(source, Tracked[0] ?? ""), Source);
  await writeFile(join(source, "untracked.txt"), "not included");
  const calls: (readonly string[])[] = [];
  const command: SourceCommand = async (args, cwd) => {
    assert.equal(cwd, source); calls.push(args);
    if (args === ReferenceSourceCommands.commit) return Commit;
    return args === ReferenceSourceCommands.tracked ? `${Tracked.join("\0")}\0` : `${Tracked.at(-1)}\0`;
  };
  const target = join(history.root, "copied");
  const reference = await copyReference(source, target, Spec, command);
  assert.equal(await readFile(join(target, Tracked[0] ?? ""), "utf8"), Source);
  assert.deepEqual(reference.files, [{ path: Tracked[0], bytes: Buffer.byteLength(Source), sha256: sha256(Source) }]);
  assert.equal(reference.contentHash, fingerprint(reference.files));
  assert.equal(reference.commit, Commit); assert.equal(reference.specHash, Spec); assert.equal(reference.specVersion, SpecVersion);
  assert.deepEqual(calls, [ReferenceSourceCommands.commit, ReferenceSourceCommands.ignored, ReferenceSourceCommands.tracked]);
  for (const file of [...Tracked.slice(1), "untracked.txt"]) await assert.rejects(readFile(join(target, file)));
  await writeFile(join(source, Tracked[0] ?? ""), `${Source}\n`);
  const changed = await copyReference(source, join(history.root, "changed"), Spec, command);
  assert.notEqual(changed.contentHash, reference.contentHash);
});

test("reference rejects unsafe tracked paths and missing commit data", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  await assert.rejects(copyReference(history.root, join(history.root, "target"), Spec, async () => ""));
  await assert.rejects(copyReference(history.root, join(history.root, "target"), Spec,
    async (args) => args === ReferenceSourceCommands.commit ? Commit : args === ReferenceSourceCommands.ignored ? "" : "../outside.ts\0"));
});

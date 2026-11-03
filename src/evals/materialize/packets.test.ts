import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { EvalPaths } from "../paths.ts";
import { BrownfieldFiles, readBrownfieldCase } from "./brownfield-case.ts";
import { packetFiles, packetFolders, readPacket } from "./packets.ts";

test("every packet's files match what its tasks generate, so run npm run packets --prefix src/evals after changing a task", async () => {
  const folders = await packetFolders();
  assert.ok(folders.length > 0);
  for (const folder of folders) {
    for (const [path, content] of Object.entries(await packetFiles(await readPacket(folder)))) assert.equal(await readFile(join(folder, path), "utf8"), content, `${folder} ${path}`);
  }
});

test("a packet holds every task's planted bug and checks, and its request lists each symptom once", async () => {
  for (const folder of await packetFolders()) {
    const packet = await readPacket(folder);
    const loaded = await readBrownfieldCase(folder, packet.id);
    const tasks = await Promise.all(packet.tasks.map((task) => readBrownfieldCase(join(EvalPaths.suites, task.id), task.id)));
    const seed = await readFile(join(folder, BrownfieldFiles.seed), "utf8");
    for (const task of tasks) assert.ok(seed.includes(await readFile(join(task.root, task.seedPatch ?? ""), "utf8")), task.id);
    const ids = (file: unknown): string[] => (file as { checks: { id: string }[] }).checks.map((check) => check.id);
    const own = ids(JSON.parse(await readFile(join(folder, loaded.checks, "checks.json"), "utf8")));
    const theirs = (await Promise.all(tasks.map(async (task) => ids(JSON.parse(await readFile(join(task.root, task.checks, "checks.json"), "utf8")))))).flat();
    assert.deepEqual(own, theirs);
    for (const task of packet.tasks) assert.equal(loaded.request.split(task.symptom).length, 2, task.id);
  }
});

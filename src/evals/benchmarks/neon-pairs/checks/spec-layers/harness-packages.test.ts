import assert from "node:assert/strict";
import { mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { packageClosure } from "./harness-packages.ts";

async function install(modules: string, name: string, dependencies: Record<string, string> = {}): Promise<void> {
  await mkdir(join(modules, name), { recursive: true });
  await writeFile(join(modules, name, "package.json"), JSON.stringify({ name, dependencies }));
}

test("a package brings its dependencies and theirs, each once", async (t) => {
  const modules = await mkdtemp(join(tmpdir(), "package-closure-"));
  t.after(() => rm(modules, { recursive: true, force: true }));
  await install(modules, "checker", { helper: "1", "@scope/alias": "npm:other@1" });
  await install(modules, "helper", { leaf: "1" });
  await install(modules, "@scope/alias", { leaf: "1" });
  await install(modules, "leaf");
  await install(modules, "unrelated");
  assert.deepEqual(await packageClosure(modules, ["checker"]), ["@scope/alias", "checker", "helper", "leaf"]);
});

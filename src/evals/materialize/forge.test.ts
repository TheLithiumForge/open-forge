import assert from "node:assert/strict";
import { access } from "node:fs/promises";
import { test } from "node:test";
import { createForgeRunner, ForgeFlags, ForgeStatus, FrameworkVersion, installFramework, type ForgeRunner } from "./forge.ts";

const success = { code: 0, stdout: "", stderr: "" };

test("the CLI runner resolves the installed launcher and forwards arguments through Node", async () => {
  const runner = createForgeRunner(async (command, args, cwd) => {
    assert.equal(command, process.execPath);
    assert.equal(cwd, process.cwd());
    const launcher = args[0];
    assert.ok(launcher);
    await access(launcher);
    assert.deepEqual(args.slice(1), [ForgeFlags.version]);
    return success;
  });
  assert.deepEqual(await runner([ForgeFlags.version], process.cwd()), success);
});

test("wrong versions, invalid JSON and incomplete CLI results stop installation", async () => {
  const wrongVersion: ForgeRunner = async () => ({ ...success, stdout: `${FrameworkVersion}-other` });
  await assert.rejects(installFramework(process.cwd(), wrongVersion));
  for (const result of [{ ...success, stdout: "invalid JSON" }, { ...success, stdout: JSON.stringify({ status: "blocked" }) }, { ...success, code: 1, stdout: JSON.stringify({ status: ForgeStatus.completed }) }]) {
    const runner: ForgeRunner = async (args) => args[0] === ForgeFlags.version ? { ...success, stdout: FrameworkVersion } : result;
    await assert.rejects(installFramework(process.cwd(), runner));
  }
});

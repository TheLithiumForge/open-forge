import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { test } from "node:test";
import { crc32 } from "node:zlib";
import { AllTargets } from "../../targets.ts";
import { hostWheels } from "../wheel.ts";
import { pythonExecutable, pythonPlatformPackage, PythonMainPackage, PythonAliasPackage } from "../package-model.ts";
import { PythonPackage, wheelInfo, wheelName, wheelTag } from "../wheel-model.ts";
import { readWheel, WheelFixture } from "./wheel-fixtures.ts";
import { fileSnapshot } from "./file-snapshot.ts";
import { ZipFormat } from "../zip-format.ts";

for (const runtime of AllTargets)
  test(`platform wheel for ${runtime} contains only the native payload and wheel metadata`, (context) => {
    const fixture = WheelFixture;
    const name = pythonPlatformPackage(runtime);
    const path = wheelName(fixture.pythonVersion, name, runtime);
    const wheels = hostWheels(fixture.version, runtime, fixture.native, fixture.license);
    const archive = wheels.get(path) ?? assert.fail();
    assert.deepEqual(hostWheels(fixture.version, runtime, fixture.native, fixture.license).get(path), archive);
    const files = verifyWheel(archive, name);
    const executable = files.find((entry) => entry.path === pythonExecutable(runtime)) ?? assert.fail();
    assert.deepEqual(executable.bytes, fixture.native);
    assert.equal(executable.mode, ZipFormat.executableMode);
    const info = wheelInfo(fixture.pythonVersion, name);
    assert.deepEqual(
      files.filter((entry) => !entry.path.startsWith(info + "/")).map((entry) => entry.path),
      [pythonExecutable(runtime)],
    );
    assert.equal(
      files.some((entry) => entry.path.endsWith(PythonPackage.entryPoints)),
      false,
    );
    fileSnapshot(context, runtime, { name: path, tag: wheelTag(runtime), files: projection(files) });
  });

for (const name of [PythonMainPackage, PythonAliasPackage])
  test(`universal wheel ${name} has the shared console script and no native payload`, (context) => {
    const fixture = WheelFixture;
    const path = wheelName(fixture.pythonVersion, name);
    const archive = hostWheels(fixture.version, AllTargets[0] ?? assert.fail(), fixture.native, fixture.license).get(path) ?? assert.fail();
    const files = verifyWheel(archive, name);
    assert.equal(
      files.some((entry) => entry.mode === ZipFormat.executableMode || entry.bytes.equals(fixture.native)),
      false,
    );
    const info = wheelInfo(fixture.pythonVersion, name);
    assert.equal(files.filter((entry) => !entry.path.startsWith(info + "/")).length, name === PythonAliasPackage ? 0 : 1);
    fileSnapshot(context, name, { name: path, tag: wheelTag(), files: projection(files) });
  });

function verifyWheel(archive: Buffer, name: string) {
  const files = readWheel(archive);
  const info = wheelInfo(WheelFixture.pythonVersion, name);
  const file = (path: string) => files.find((entry) => entry.path === path) ?? assert.fail(path);
  assert.deepEqual(file(`${info}/${PythonPackage.license}`).bytes, WheelFixture.license);
  const recordPath = `${info}/${PythonPackage.record}`;
  const records = file(recordPath).bytes.toString().trimEnd().split("\n");
  assert.equal(records.length, files.length);
  for (const entry of files) {
    assert.equal(entry.checksum, crc32(entry.bytes));
    const record = entry.path === recordPath ? `${recordPath},,` : `${entry.path},sha256=${createHash("sha256").update(entry.bytes).digest("base64url")},${entry.bytes.length}`;
    assert.ok(records.includes(record));
  }
  return files;
}

function projection(files: ReturnType<typeof readWheel>) {
  return files.map((entry) => ({
    path: entry.path,
    mode: entry.mode,
    ...(entry.mode === ZipFormat.executableMode ? { sha256: createHash("sha256").update(entry.bytes).digest("hex") } : { text: entry.bytes.toString().split("\n") }),
  }));
}

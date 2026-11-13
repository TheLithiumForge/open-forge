import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { test } from "node:test";
import { crc32 } from "node:zlib";
import { AllTargets } from "../../targets.ts";
import { hostWheels } from "../wheel.ts";
import { pythonExecutable, pythonModule, pythonPlatformPackage, PythonMainPackage, PythonAliasPackage } from "../package-model.ts";
import { PythonConsole, PythonPackage, WheelField, wheelInfo, wheelName } from "../wheel-model.ts";
import { ExpectedWheelTag, readWheel, WheelFixture } from "./wheel-fixtures.ts";
import { ZipFormat } from "../zip-format.ts";
import { metadata } from "./wheel-metadata.ts";
import { platformMarker } from "../environment-markers.ts";
import { pythonEntryPoint } from "../entry-point.ts";
import { AliasPackageName } from "../../package-model.ts";

for (const runtime of AllTargets)
  test(`platform wheel for ${runtime} contains only the native payload and wheel metadata`, () => {
    const fixture = WheelFixture;
    const name = pythonPlatformPackage(runtime);
    const path = wheelName(fixture.pythonVersion, name, runtime);
    const wheels = hostWheels(fixture.version, runtime, fixture.native, fixture.license);
    const archive = wheels.get(path) ?? assert.fail();
    assert.deepEqual(hostWheels(fixture.version, runtime, fixture.native, fixture.license).get(path), archive);
    const files = verifyWheel(archive, name, pythonExecutable(runtime));
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
    const { fields, wheel } = metadata(files, name);
    assert.equal(fields.has(WheelField.requires), false);
    assert.deepEqual(wheel.get(WheelField.pure), [String(false)]);
    assert.deepEqual(wheel.get(WheelField.tag), ExpectedWheelTag.platforms[runtime]);
  });

for (const name of [PythonMainPackage, PythonAliasPackage])
  test(`universal wheel ${name} has the shared console script and no native payload`, () => {
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
    const { fields, wheel, file } = metadata(files, name);
    assert.deepEqual(
      fields.get(WheelField.requires),
      name === PythonAliasPackage
        ? [`${PythonMainPackage}==${fixture.pythonVersion}`]
        : AllTargets.map((runtime) => `${pythonPlatformPackage(runtime)}==${fixture.pythonVersion}; ${platformMarker(runtime)}`),
    );
    assert.deepEqual(wheel.get(WheelField.pure), [String(true)]);
    assert.deepEqual(wheel.get(WheelField.tag), [ExpectedWheelTag.universal]);
    const lines = file(PythonPackage.entryPoints).bytes.toString().trim().split("\n");
    assert.deepEqual(lines, [`[${PythonConsole.section}]`, `${AliasPackageName} = ${pythonModule(PythonMainPackage)}:${PythonConsole.callable}`]);
    if (name === PythonMainPackage) assert.deepEqual(files.find((entry) => entry.path === `${pythonModule(name)}/${PythonPackage.init}`)?.bytes, Buffer.from(pythonEntryPoint()));
  });

function verifyWheel(archive: Buffer, name: string, executable?: string) {
  const files = readWheel(archive);
  const info = wheelInfo(WheelFixture.pythonVersion, name);
  const file = (path: string) => files.find((entry) => entry.path === path) ?? assert.fail(path);
  assert.deepEqual(file(`${info}/${PythonPackage.license}`).bytes, WheelFixture.license);
  const recordPath = `${info}/${PythonPackage.record}`;
  const records = file(recordPath).bytes.toString().trimEnd().split("\n");
  assert.equal(records.length, files.length);
  for (const entry of files) {
    assert.equal(entry.mode, entry.path === executable ? ZipFormat.executableMode : ZipFormat.fileMode);
    assert.equal(entry.checksum, crc32(entry.bytes));
    const record = entry.path === recordPath ? `${recordPath},,` : `${entry.path},sha256=${createHash("sha256").update(entry.bytes).digest("base64url")},${entry.bytes.length}`;
    assert.ok(records.includes(record));
  }
  return files;
}

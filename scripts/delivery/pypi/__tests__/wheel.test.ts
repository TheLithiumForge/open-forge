import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { test } from "node:test";
import { crc32 } from "node:zlib";
import { PlatformPackages } from "../../package-model.ts";
import { AllTargets } from "../../targets.ts";
import { createWheel } from "../wheel.ts";
import { pythonEntryPoint } from "../entry-point.ts";
import { PythonPackage, wheelInfo, wheelName, wheelTag } from "../wheel-model.ts";
import { ExpectedTags, readWheel, WheelFixture } from "./wheel-fixtures.ts";

for (const runtime of AllTargets)
  test(`wheel for ${runtime} has the right tag, native bytes, entry point and complete RECORD`, () => {
    const fixture = WheelFixture;
    const archive = createWheel(fixture.version, runtime, fixture.native, fixture.license);
    assert.deepEqual(createWheel(fixture.version, runtime, fixture.native, fixture.license), archive);
    assert.equal(wheelTag(runtime), ExpectedTags[runtime]);
    assert.equal(wheelName(fixture.pythonVersion, runtime), `${PythonPackage.module}-${fixture.pythonVersion}-${ExpectedTags[runtime]}${PythonPackage.suffix}`);
    const info = wheelInfo(fixture.pythonVersion);
    const files = readWheel(archive);
    const file = (path: string) => files.find((entry) => entry.path === path) ?? assert.fail(path);
    const executable = file(`${PythonPackage.module}/bin/${PlatformPackages[runtime].nativeFileName}`);
    assert.deepEqual(executable.bytes, fixture.native);
    assert.equal(executable.mode, 0o100755);
    assert.deepEqual(file(`${info}/${PythonPackage.license}`).bytes, fixture.license);
    assert.equal(file(`${PythonPackage.module}/${PythonPackage.init}`).bytes.toString(), pythonEntryPoint(runtime));
    assert.equal(file(`${info}/${PythonPackage.entryPoints}`).bytes.toString(), `[console_scripts]\n${PythonPackage.name} = ${PythonPackage.module}:main\n`);
    const metadata = file(`${info}/${PythonPackage.metadata}`).bytes.toString().split("\n");
    assert.ok(metadata.includes(`Version: ${fixture.pythonVersion}`));
    assert.ok(metadata.includes(`Requires-Python: ${PythonPackage.minimum}`));
    assert.ok(metadata.includes(`License-File: ${PythonPackage.licenseFile}`));
    assert.deepEqual(file(`${info}/licenses/${PythonPackage.licenseFile}`).bytes, fixture.license);
    assert.ok(file(`${info}/${PythonPackage.wheel}`).bytes.toString().split("\n").includes(`Tag: ${ExpectedTags[runtime]}`));
    const recordPath = `${info}/${PythonPackage.record}`;
    const records = file(recordPath).bytes.toString().trimEnd().split("\n");
    assert.equal(records.length, files.length);
    assert.equal(files.length, 7);
    for (const entry of files) {
      assert.equal(entry.checksum, crc32(entry.bytes));
      if (entry.path === recordPath) assert.ok(records.includes(`${recordPath},,`));
      else assert.ok(records.includes(`${entry.path},sha256=${createHash("sha256").update(entry.bytes).digest("base64url")},${entry.bytes.length}`));
    }
  });

import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { test } from "node:test";
import { AllTargets } from "../../targets.ts";
import { hostWheel, wheelEntries } from "../wheel.ts";
import { PythonPackage, PythonPackageName, WheelField, wheelInfo, wheelName, wheelScript } from "../wheel-model.ts";
import { ExpectedWheelTag, WheelFixture } from "./wheel-fixtures.ts";

function headers(bytes: Buffer): Map<string, string[]> {
  const result = new Map<string, string[]>();
  for (const line of bytes.toString().split("\n")) {
    const separator = line.indexOf(": ");
    if (separator < 0) continue;
    const name = line.slice(0, separator);
    result.set(name, [...(result.get(name) ?? []), line.slice(separator + 2)]);
  }
  return result;
}

for (const runtime of AllTargets)
  test(`wheel for ${runtime} installs only the native executable as a script`, () => {
    const fixture = WheelFixture;
    const entries = wheelEntries(fixture.version, runtime, fixture.native, fixture.license);
    const info = wheelInfo(fixture.pythonVersion);
    const file = (path: string) => entries.find((entry) => entry.path === path) ?? assert.fail(path);
    assert.deepEqual(
      entries.filter((entry) => !entry.path.startsWith(`${info}/`)).map((entry) => entry.path),
      [wheelScript(fixture.pythonVersion, runtime)],
    );
    assert.deepEqual(file(wheelScript(fixture.pythonVersion, runtime)).bytes, fixture.native);
    assert.deepEqual(
      entries.filter((entry) => entry.executable === true).map((entry) => entry.path),
      [wheelScript(fixture.pythonVersion, runtime)],
    );
    assert.deepEqual(file(`${info}/${PythonPackage.license}`).bytes, fixture.license);
    const fields = headers(file(`${info}/${PythonPackage.metadata}`).bytes);
    assert.deepEqual(fields.get(WheelField.name), [PythonPackageName]);
    assert.deepEqual(fields.get(WheelField.version), [fixture.pythonVersion]);
    assert.deepEqual(fields.get(WheelField.python), [PythonPackage.minimum]);
    assert.deepEqual(fields.get(WheelField.license), [PythonPackage.licenseFile]);
    const wheel = headers(file(`${info}/${PythonPackage.wheel}`).bytes);
    assert.deepEqual(wheel.get(WheelField.pure), [String(false)]);
    assert.deepEqual(wheel.get(WheelField.tag), [ExpectedWheelTag[runtime]]);
    const recordPath = `${info}/${PythonPackage.record}`;
    const records = file(recordPath).bytes.toString().trimEnd().split("\n");
    assert.deepEqual(
      records,
      entries.map((entry) =>
        entry.path === recordPath ? `${recordPath},,` : `${entry.path},sha256=${createHash("sha256").update(entry.bytes).digest("base64url")},${entry.bytes.length}`,
      ),
    );
    const archive = hostWheel(fixture.version, runtime, fixture.native, fixture.license);
    assert.equal(archive.name, wheelName(fixture.pythonVersion, runtime));
    assert.ok(archive.name.endsWith(`-${ExpectedWheelTag[runtime]}${PythonPackage.suffix}`));
    assert.deepEqual(hostWheel(fixture.version, runtime, fixture.native, fixture.license).bytes, archive.bytes);
  });

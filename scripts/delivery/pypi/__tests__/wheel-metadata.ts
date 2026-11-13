import assert from "node:assert/strict";
import { PythonPackage, WheelField, wheelInfo } from "../wheel-model.ts";
import { WheelFixture, type readWheel } from "./wheel-fixtures.ts";

export function headers(bytes: Buffer): Map<string, string[]> {
  const result = new Map<string, string[]>();
  for (const line of bytes.toString().split("\n")) {
    const separator = line.indexOf(": ");
    if (separator < 0) continue;
    const name = line.slice(0, separator);
    result.set(name, [...(result.get(name) ?? []), line.slice(separator + 2)]);
  }
  return result;
}

export function metadata(files: ReturnType<typeof readWheel>, name: string) {
  const info = wheelInfo(WheelFixture.pythonVersion, name);
  const file = (path: string) => files.find((entry) => entry.path === `${info}/${path}`) ?? assert.fail(path);
  const fields = headers(file(PythonPackage.metadata).bytes);
  assert.deepEqual(fields.get(WheelField.name), [name]);
  assert.deepEqual(fields.get(WheelField.version), [WheelFixture.pythonVersion]);
  assert.deepEqual(fields.get(WheelField.python), [PythonPackage.minimum]);
  assert.deepEqual(fields.get(WheelField.license), [PythonPackage.licenseFile]);
  return { fields, wheel: headers(file(PythonPackage.wheel).bytes), file };
}

import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { hostWheel, sourceLicenses } from "./wheel.ts";
import { PythonPackage, wheelInfo } from "./wheel-model.ts";
import { pythonVersion } from "./version.ts";
import { readZipEntry } from "./zip-entry.ts";

/** Rejects a wheel whose license or third-party notices differ from the repository's, like the npm platform-package check. */
export function assertSourceLicenses(root: string, wheel: string, npmVersion: string): void {
  const archive = readFileSync(wheel);
  const info = wheelInfo(pythonVersion(npmVersion));
  const source = sourceLicenses(root);
  // Compare with Buffer.equals: a failing deepEqual on the notices builds a byte-by-byte diff that takes
  // tens of seconds and many gigabytes, which killed the Linux CI runner.
  const matches = (path: string, expected: Buffer) => readZipEntry(archive, `${info}/${path}`)?.equals(expected) === true;
  assert.ok(matches(PythonPackage.license, source.license), "Wheel license does not match the source.");
  assert.ok(matches(PythonPackage.notices, source.notices), "Wheel third-party notices do not match the source.");
}

export function expectedHostWheel(directory: string, version: string, runtime: SupportedRuntime): { name: string; bytes: Buffer } {
  const archive = join(directory, `open-forge-${version}-${runtime}.tar.gz`);
  const extract = (path: string) => execFileSync("tar", ["-xOf", archive, path], { maxBuffer: 256 * 1024 * 1024 });
  return hostWheel(version, runtime, extract(PlatformPackages[runtime].nativeFileName), {
    license: extract(PythonPackage.licenseFile),
    notices: extract(PythonPackage.noticesFile),
  });
}

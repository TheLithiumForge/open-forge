import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import type { SupportedRuntime } from "../package-model.ts";
import { pythonVersion } from "./version.ts";
import { PythonPackage, PythonPackageName, WheelField, wheelInfo, wheelName, wheelScript, wheelTag } from "./wheel-model.ts";
import { wheelZip, type ZipEntry } from "./zip.ts";

const text = (path: string, value: string): ZipEntry => ({ path, bytes: Buffer.from(value) });

/** Our license and the third-party notices for the code compiled into the executable. */
export interface WheelLicenses {
  license: Buffer;
  notices: Buffer;
}

// The executable is the wheel's only payload. It installs as a script, so no Python code runs at startup.
export function wheelEntries(npmVersion: string, runtime: SupportedRuntime, native: Buffer, licenses: WheelLicenses): ZipEntry[] {
  const version = pythonVersion(npmVersion);
  const info = wheelInfo(version);
  const entries: ZipEntry[] = [
    { path: wheelScript(version, runtime), bytes: native, executable: true },
    text(
      `${info}/${PythonPackage.metadata}`,
      [
        "Metadata-Version: 2.4",
        `${WheelField.name}: ${PythonPackageName}`,
        `${WheelField.version}: ${version}`,
        "Summary: Open Forge native CLI",
        `${WheelField.python}: ${PythonPackage.minimum}`,
        "License-Expression: MIT",
        `${WheelField.license}: ${PythonPackage.licenseFile}`,
        `${WheelField.license}: ${PythonPackage.noticesFile}`,
        "",
        "",
      ].join("\n"),
    ),
    text(`${info}/${PythonPackage.wheel}`, `Wheel-Version: 1.0\nGenerator: open-forge\n${WheelField.pure}: false\n${WheelField.tag}: ${wheelTag(runtime)}\n`),
    { path: `${info}/${PythonPackage.license}`, bytes: licenses.license },
    { path: `${info}/${PythonPackage.notices}`, bytes: licenses.notices },
  ];
  const record = entries.map((entry) => `${entry.path},sha256=${createHash("sha256").update(entry.bytes).digest("base64url")},${entry.bytes.length}\n`).join("");
  entries.push(text(`${info}/${PythonPackage.record}`, `${record}${info}/${PythonPackage.record},,\n`));
  return entries;
}

export function hostWheel(npmVersion: string, runtime: SupportedRuntime, native: Buffer, licenses: WheelLicenses): { name: string; bytes: Buffer } {
  return { name: wheelName(pythonVersion(npmVersion), runtime), bytes: wheelZip(wheelEntries(npmVersion, runtime, native, licenses)) };
}

/** Reads both license files from the repository root, where every package that carries the executable takes them from. */
export function sourceLicenses(root: string): WheelLicenses {
  return { license: readFileSync(join(root, PythonPackage.licenseFile)), notices: readFileSync(join(root, PythonPackage.noticesFile)) };
}

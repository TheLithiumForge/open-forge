import { createHash } from "node:crypto";
import type { SupportedRuntime } from "../package-model.ts";
import { pythonVersion } from "./version.ts";
import { PythonPackage, PythonPackageName, WheelField, wheelInfo, wheelName, wheelScript, wheelTag } from "./wheel-model.ts";
import { wheelZip, type ZipEntry } from "./zip.ts";

const text = (path: string, value: string): ZipEntry => ({ path, bytes: Buffer.from(value) });

// The executable is the wheel's only payload. It installs as a script, so no Python code runs at startup.
export function wheelEntries(npmVersion: string, runtime: SupportedRuntime, native: Buffer, license: Buffer): ZipEntry[] {
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
        "",
        "",
      ].join("\n"),
    ),
    text(`${info}/${PythonPackage.wheel}`, `Wheel-Version: 1.0\nGenerator: open-forge\n${WheelField.pure}: false\n${WheelField.tag}: ${wheelTag(runtime)}\n`),
    { path: `${info}/${PythonPackage.license}`, bytes: license },
  ];
  const record = entries.map((entry) => `${entry.path},sha256=${createHash("sha256").update(entry.bytes).digest("base64url")},${entry.bytes.length}\n`).join("");
  entries.push(text(`${info}/${PythonPackage.record}`, `${record}${info}/${PythonPackage.record},,\n`));
  return entries;
}

export function hostWheel(npmVersion: string, runtime: SupportedRuntime, native: Buffer, license: Buffer): { name: string; bytes: Buffer } {
  return { name: wheelName(pythonVersion(npmVersion), runtime), bytes: wheelZip(wheelEntries(npmVersion, runtime, native, license)) };
}

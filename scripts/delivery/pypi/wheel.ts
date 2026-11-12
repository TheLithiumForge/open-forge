import { createHash } from "node:crypto";
import { PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { pythonVersion } from "./version.ts";
import { pythonEntryPoint } from "./entry-point.ts";
import { PythonPackage, wheelInfo, wheelTag } from "./wheel-model.ts";
import { wheelZip, type ZipEntry } from "./zip.ts";

export function wheelEntries(npmVersion: string, runtime: SupportedRuntime, native: Buffer, license: Buffer): ZipEntry[] {
  const version = pythonVersion(npmVersion);
  const info = wheelInfo(version);
  const text = (path: string, value: string): ZipEntry => ({ path, bytes: Buffer.from(value) });
  const entries: ZipEntry[] = [
    text(`${PythonPackage.module}/${PythonPackage.init}`, pythonEntryPoint(runtime)),
    { path: `${PythonPackage.module}/bin/${PlatformPackages[runtime].nativeFileName}`, bytes: native, executable: true },
    text(
      `${info}/${PythonPackage.metadata}`,
      `Metadata-Version: 2.4\nName: ${PythonPackage.name}\nVersion: ${version}\nSummary: Open Forge native CLI\nRequires-Python: ${PythonPackage.minimum}\nLicense-Expression: MIT\nLicense-File: ${PythonPackage.licenseFile}\n\n`,
    ),
    text(`${info}/${PythonPackage.wheel}`, `Wheel-Version: 1.0\nGenerator: open-forge\nRoot-Is-Purelib: false\nTag: ${wheelTag(runtime)}\n`),
    text(`${info}/${PythonPackage.entryPoints}`, `[console_scripts]\n${PythonPackage.name} = ${PythonPackage.module}:main\n`),
    { path: `${info}/${PythonPackage.license}`, bytes: license },
  ];
  const record = entries.map((entry) => `${entry.path},sha256=${createHash("sha256").update(entry.bytes).digest("base64url")},${entry.bytes.length}\n`).join("");
  entries.push(text(`${info}/${PythonPackage.record}`, `${record}${info}/${PythonPackage.record},,\n`));
  return entries;
}

export function createWheel(npmVersion: string, runtime: SupportedRuntime, native: Buffer, license: Buffer): Buffer {
  return wheelZip(wheelEntries(npmVersion, runtime, native, license));
}

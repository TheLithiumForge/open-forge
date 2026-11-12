import { createHash } from "node:crypto";
import { AliasPackageName, type SupportedRuntime } from "../package-model.ts";
import { AllTargets, readTargets } from "../targets.ts";
import { pythonVersion } from "./version.ts";
import { pythonEntryPoint } from "./entry-point.ts";
import { platformMarker } from "./environment-markers.ts";
import { PythonAliasPackage, PythonMainPackage, pythonExecutable, pythonModule, pythonPlatformPackage } from "./package-model.ts";
import { PythonPackage, wheelInfo, wheelName, wheelTag } from "./wheel-model.ts";
import { wheelZip, type ZipEntry } from "./zip.ts";

interface WheelRequest {
  name: string;
  version: string;
  runtime?: SupportedRuntime;
  requires: readonly string[];
  console: boolean;
  payload: ZipEntry[];
  license: Buffer;
}

const text = (path: string, value: string): ZipEntry => ({ path, bytes: Buffer.from(value) });

function buildWheel(request: WheelRequest): Buffer {
  const { name, version, runtime, license } = request;
  const info = wheelInfo(version, name);
  const entries = [
    ...request.payload,
    text(
      `${info}/${PythonPackage.metadata}`,
      [
        "Metadata-Version: 2.4",
        `Name: ${name}`,
        `Version: ${version}`,
        "Summary: Open Forge native CLI",
        `Requires-Python: ${PythonPackage.minimum}`,
        "License-Expression: MIT",
        `License-File: ${PythonPackage.licenseFile}`,
        ...request.requires.map((dependency) => `Requires-Dist: ${dependency}`),
        "",
        "",
      ].join("\n"),
    ),
    text(`${info}/${PythonPackage.wheel}`, `Wheel-Version: 1.0\nGenerator: open-forge\nRoot-Is-Purelib: ${runtime === undefined}\nTag: ${wheelTag(runtime)}\n`),
    ...(request.console ? [text(`${info}/${PythonPackage.entryPoints}`, `[console_scripts]\n${AliasPackageName} = ${pythonModule(PythonMainPackage)}:main\n`)] : []),
    { path: `${info}/${PythonPackage.license}`, bytes: license },
  ];
  const record = entries.map((entry) => `${entry.path},sha256=${createHash("sha256").update(entry.bytes).digest("base64url")},${entry.bytes.length}\n`).join("");
  entries.push(text(`${info}/${PythonPackage.record}`, `${record}${info}/${PythonPackage.record},,\n`));
  return wheelZip(entries);
}

export function hostWheels(npmVersion: string, runtime: SupportedRuntime, native: Buffer, license: Buffer, targets: readonly SupportedRuntime[] = AllTargets): Map<string, Buffer> {
  const version = pythonVersion(npmVersion);
  const name = pythonPlatformPackage(runtime);
  const requests: WheelRequest[] = [
    { name, version, runtime, requires: [], console: false, license, payload: [{ path: pythonExecutable(runtime), bytes: native, executable: true }] },
    {
      name: PythonMainPackage,
      version,
      license,
      console: true,
      requires: readTargets(targets).map((target) => `${pythonPlatformPackage(target)}==${version}; ${platformMarker(target)}`),
      payload: [text(`${pythonModule(PythonMainPackage)}/${PythonPackage.init}`, pythonEntryPoint())],
    },
    { name: PythonAliasPackage, version, license, console: true, requires: [`${PythonMainPackage}==${version}`], payload: [] },
  ];
  return new Map(requests.map((request) => [wheelName(version, request.name, request.runtime), buildWheel(request)]));
}

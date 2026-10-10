import assert from "node:assert/strict";
import { posix, win32 } from "node:path";

export const RunnerOs = { linux: "Linux", mac: "macOS", windows: "Windows" } as const;
export const RunnerNodePlatform = { mac: "darwin", windows: "win32" } as const;
export const RunnerVariable = { temporary: "RUNNER_TEMP", os: "RUNNER_OS", environment: "GITHUB_ENV" } as const;
export const PreparedVariable = { install: "DOTNET_INSTALL_DIR", root: "DOTNET_ROOT", temporary: "TMPDIR" } as const;
export const DotnetFolder = "dotnet";

export interface RunnerInput {
  os: string;
  temporary: string;
  physicalTemporary: string;
}

export function environmentAssignments(input: RunnerInput): Record<string, string> {
  const path = input.os === RunnerOs.windows ? win32 : posix;
  const dotnet = path.join(input.temporary, DotnetFolder);
  return {
    [PreparedVariable.install]: dotnet,
    [PreparedVariable.root]: dotnet,
    ...(input.os === RunnerOs.mac ? { [PreparedVariable.temporary]: `${input.physicalTemporary.replace(/\/+$/u, "")}/` } : {}),
  };
}

export function assignmentLines(assignments: Readonly<Record<string, string>>): string {
  return Object.entries(assignments)
    .map(([name, value]) => {
      assert.ok(!/[\r\n]/u.test(value), "Runner paths must fit on one environment line.");
      return `${name}=${value}\n`;
    })
    .join("");
}

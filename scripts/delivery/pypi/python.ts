import { spawnSync } from "node:child_process";

export const PythonCommand = { executable: "python", versioned: "python3", launcher: "py", select3: "-3", version: "--version" } as const;

const candidates = [
  { executable: PythonCommand.executable, args: [] },
  { executable: PythonCommand.versioned, args: [] },
  { executable: PythonCommand.launcher, args: [PythonCommand.select3] },
];

export function findPython(): { executable: string; args: string[] } | undefined {
  return candidates.find((candidate) => {
    const result = spawnSync(candidate.executable, [...candidate.args, PythonCommand.version], { encoding: "utf8", timeout: 10_000, windowsHide: true });
    return !result.error && result.status === 0;
  });
}

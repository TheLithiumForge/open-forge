import { spawnSync } from "node:child_process";

export const PythonCommand = { executable: "python", versioned: "python3", launcher: "py", select3: "-3", version: "--version" } as const;

export function pythonCandidates(preferLauncher = false) {
  const direct = [
    { executable: PythonCommand.executable, args: [] },
    { executable: PythonCommand.versioned, args: [] },
  ];
  const launcher = { executable: PythonCommand.launcher, args: [PythonCommand.select3] };
  return preferLauncher ? [launcher, ...direct] : [...direct, launcher];
}

export function findPython(preferLauncher = false): { executable: string; args: string[] } | undefined {
  return pythonCandidates(preferLauncher).find((candidate) => {
    const result = spawnSync(candidate.executable, [...candidate.args, PythonCommand.version], { encoding: "utf8", timeout: 10_000, windowsHide: true });
    return !result.error && result.status === 0;
  });
}

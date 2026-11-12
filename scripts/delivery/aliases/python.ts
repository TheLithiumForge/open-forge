import { spawnSync } from "node:child_process";

export function findPython(): { executable: string; args: string[] } | undefined {
  const candidates = [
    { executable: "python", args: [] },
    { executable: "python3", args: [] },
    { executable: "py", args: ["-3"] },
  ];
  return candidates.find((candidate) => {
    const result = spawnSync(candidate.executable, [...candidate.args, "--version"], { encoding: "utf8", timeout: 10_000, windowsHide: true });
    return !result.error && result.status === 0;
  });
}

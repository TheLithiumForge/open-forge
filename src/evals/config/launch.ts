import { spawn } from "node:child_process";
import { evalsRoot } from "../paths.ts";

export const CliScripts = { check: "grading/recheck.ts", judge: "grading/rejudge.ts" } as const;

export function launchNode(file: string, args: readonly string[] = [], cwd: string = evalsRoot): Promise<number> {
  return new Promise((resolve, reject) => {
    const child = spawn(process.execPath, [file, ...args], { cwd, stdio: "inherit", windowsHide: true });
    child.once("error", reject);
    child.once("exit", (code) => resolve(code ?? 1));
  });
}


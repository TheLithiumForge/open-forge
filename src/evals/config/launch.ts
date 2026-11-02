import { spawn } from "node:child_process";
import { join } from "node:path";
import { EvalPaths, evalsRoot } from "../paths.ts";

export const CliScripts = { recheck: "grading/recheck.ts", rejudge: "grading/rejudge.ts", server: "web/server/server.ts" } as const;

export function launchNode(file: string, args: readonly string[] = []): Promise<number> {
  return new Promise((resolve, reject) => {
    const child = spawn(process.execPath, [file, ...args], { cwd: evalsRoot, stdio: "inherit", windowsHide: true });
    child.once("error", reject);
    child.once("exit", (code) => resolve(code ?? 1));
  });
}

export async function launchUi(args: readonly string[] = []): Promise<number> {
  const built = await launchNode(join(EvalPaths.modules, "vite", "bin", "vite.js"), ["build", "--logLevel", "warn"]);
  return built === 0 ? launchNode(join(evalsRoot, CliScripts.server), args) : built;
}

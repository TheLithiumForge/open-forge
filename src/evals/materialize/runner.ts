import { spawn } from "node:child_process";

export interface CommandResult { readonly code: number | null; readonly stdout: string; readonly stderr: string }
export type CommandRunner = (command: string, args: readonly string[], cwd: string) => Promise<CommandResult>;
const CommandTimeoutMs = 120_000;

export const runCommand: CommandRunner = (command, args, cwd) => new Promise((resolve, reject) => {
  const child = spawn(command, args, { cwd, shell: false, windowsHide: true, stdio: ["ignore", "pipe", "pipe"] });
  let stdout = "";
  let stderr = "";
  child.stdout.on("data", (chunk: Buffer) => { stdout += chunk.toString(); });
  child.stderr.on("data", (chunk: Buffer) => { stderr += chunk.toString(); });
  const timeout = setTimeout(() => child.kill(), CommandTimeoutMs);
  child.on("error", (error) => { clearTimeout(timeout); reject(error); });
  child.on("close", (code) => { clearTimeout(timeout); resolve({ code, stdout, stderr }); });
});

import { spawn } from "node:child_process";
import { waitForProcess } from "./process.ts";

const StopTimeoutMs = 30_000;
// The folder goes in through the environment, so its characters never become part of a command.
const FolderVariable = "EVAL_RUN_FOLDER";
const windowsStop = `Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -like "*$env:${FolderVariable}*" -or $_.ExecutablePath -like "$env:${FolderVariable}*" } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }`;

/**
 * Stops every process still running from a run's folder, such as a dev server or a build helper the
 * agent started. They would hold files open and keep the agent's output pipe from closing.
 */
export async function stopProcessesUnder(folder: string): Promise<void> {
  const env = { ...process.env, [FolderVariable]: folder };
  const child =
    process.platform === "win32"
      ? spawn("powershell", ["-NoProfile", "-NonInteractive", "-Command", windowsStop], { env, windowsHide: true })
      : spawn("pkill", ["-f", folder], { env });
  await waitForProcess(child, { timeoutMs: StopTimeoutMs });
}

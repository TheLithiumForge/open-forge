import { spawnSync } from "node:child_process";

interface Invocation {
  executable: string;
  args: string[];
}

const WindowsProcessList =
  "Get-CimInstance Win32_Process | Sort-Object ProcessId | Format-Table -AutoSize -Wrap ProcessId,ParentProcessId,CreationDate,CommandLine | Out-String -Width 400";
// A stall report must not stall itself, so each helper process gets a bounded time.
const HelperTimeoutMilliseconds = 30_000;

/** The command that lists every process with its parent, start time and command line. */
export function processListInvocation(platform: NodeJS.Platform): Invocation {
  return platform === "win32"
    ? { executable: "powershell.exe", args: ["-NoProfile", "-NonInteractive", "-Command", WindowsProcessList] }
    : { executable: "ps", args: ["-eo", "pid,ppid,etime,args"] };
}

/** Lists running processes for a stall report. A failed or timed-out listing reports why instead. */
export function listProcesses(platform: NodeJS.Platform = process.platform): string {
  const invocation = processListInvocation(platform);
  const result = spawnSync(invocation.executable, invocation.args, { encoding: "utf8", shell: false, timeout: HelperTimeoutMilliseconds });
  return result.error ? `Could not list processes: ${result.error.message}\n` : `${result.stdout}${result.stderr}`;
}

/** Whether a child should lead its own process group, so the whole tree can be stopped together. */
export function leadsProcessGroup(platform: NodeJS.Platform = process.platform): boolean {
  return platform !== "win32";
}

/** Stops a process and every process it started. If the tree cannot be stopped, stops the process itself. */
export function stopProcessTree(pid: number, platform: NodeJS.Platform = process.platform): void {
  try {
    if (platform === "win32") {
      const result = spawnSync("taskkill", ["/PID", String(pid), "/T", "/F"], { stdio: "ignore", shell: false, timeout: HelperTimeoutMilliseconds });
      if (result.error === undefined && result.status === 0) return;
    } else {
      process.kill(-pid, "SIGKILL");
      return;
    }
  } catch {
    // Fall through to stopping the process alone.
  }
  try {
    process.kill(pid, "SIGKILL");
  } catch {
    // The process already exited.
  }
}

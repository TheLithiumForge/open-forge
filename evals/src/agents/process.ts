import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { createInterface } from "node:readline";

export interface ProcessResult {
  readonly code: number | null;
  readonly stdout: string;
  readonly stderr: string;
  readonly timedOut: boolean;
}

export interface WaitOptions {
  readonly timeoutMs: number;
  readonly input?: string;
  readonly onLine?: (line: string) => void;
}

// After a process exits, its last output can still be in the pipe for a moment.
const DrainMs = 1000;

/**
 * Waits for a process to exit, keeping its output. A process past its time limit is killed. Exit is
 * enough: a helper it started, such as a build service, can keep the pipes open long after.
 */
export function waitForProcess(child: ChildProcessWithoutNullStreams, options: WaitOptions): Promise<ProcessResult> {
  return new Promise((resolve) => {
    const stdout: string[] = [];
    const stderr: string[] = [];
    let timedOut = false;
    let settled = false;
    const timer = setTimeout(() => {
      timedOut = true;
      child.kill();
    }, options.timeoutMs);
    const finish = (code: number | null): void => {
      if (settled) return;
      settled = true;
      clearTimeout(timer);
      resolve({ code, stdout: stdout.join("\n"), stderr: stderr.join(""), timedOut });
    };
    createInterface({ input: child.stdout }).on("line", (line) => {
      stdout.push(line);
      options.onLine?.(line);
    });
    child.stderr.on("data", (chunk: Buffer) => stderr.push(chunk.toString()));
    child.on("error", (error) => {
      stderr.push(String(error));
      finish(null);
    });
    child.on("exit", (code) => setTimeout(() => finish(code), DrainMs));
    child.on("close", (code) => finish(code));
    if (options.input !== undefined) child.stdin.end(options.input);
  });
}

export function runCommand(command: string, args: readonly string[], options: WaitOptions & { readonly cwd?: string }): Promise<ProcessResult> {
  const child = spawn(command, args, { windowsHide: true, ...(options.cwd === undefined ? {} : { cwd: options.cwd }) });
  return waitForProcess(child, options);
}

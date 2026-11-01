import { spawn, type ChildProcessWithoutNullStreams } from "node:child_process";
import { createInterface } from "node:readline";

export interface ProcessResult {
  readonly code: number | null;
  readonly stdout: string;
  readonly stderr: string;
  readonly timedOut: boolean;
  /** True when output passed maxBytes, so the rest was dropped and the process was stopped. */
  readonly truncated?: boolean;
}

export interface WaitOptions {
  readonly timeoutMs: number;
  readonly input?: string;
  readonly onLine?: (line: string) => void;
  /** Keeps at most this many bytes of each stream. Untrusted output is capped while it's read, never after. */
  readonly maxBytes?: number;
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
    let truncated = false;
    let settled = false;
    const timer = setTimeout(() => {
      timedOut = true;
      child.kill();
    }, options.timeoutMs);
    const finish = (code: number | null): void => {
      if (settled) return;
      settled = true;
      clearTimeout(timer);
      const out = options.maxBytes === undefined ? stdout.join("\n") : stdout.join("");
      resolve({ code, stdout: out, stderr: stderr.join(""), timedOut, ...(truncated ? { truncated } : {}) });
    };
    const kept = { stdout: 0, stderr: 0 };
    const keep = (stream: keyof typeof kept, target: string[], chunk: Buffer, limit: number): void => {
      const room = Math.max(0, limit - kept[stream]);
      if (room > 0) target.push(chunk.subarray(0, room).toString());
      kept[stream] += Math.min(room, chunk.length);
      if (chunk.length > room && !truncated) {
        truncated = true;
        child.kill();
      }
    };
    const limit = options.maxBytes;
    if (limit === undefined) {
      createInterface({ input: child.stdout }).on("line", (line) => {
        stdout.push(line);
        options.onLine?.(line);
      });
    } else {
      // Raw chunks, so one endless line can't be buffered whole before the cap applies.
      child.stdout.on("data", (chunk: Buffer) => keep("stdout", stdout, chunk, limit));
    }
    child.stderr.on("data", (chunk: Buffer) => (limit === undefined ? stderr.push(chunk.toString()) : keep("stderr", stderr, chunk, limit)));
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

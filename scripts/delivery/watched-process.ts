import { spawn } from "node:child_process";
import { closeSync, fstatSync, openSync, readSync } from "node:fs";
import { leadsProcessGroup, stopProcessTree } from "./process-tree.ts";

export interface WatchedCommand {
  executable: string;
  args: readonly string[];
  cwd: string;
  log: string;
  deadlineMilliseconds: number;
  heartbeatMilliseconds: number;
  /** Receives text the process added to its log since the last call. */
  onOutput: (text: string) => void;
  onHeartbeat: (elapsedMilliseconds: number, logBytes: number) => void;
  /** Runs at the deadline while the process tree still exists. A failure here never prevents the stop. */
  beforeStop: () => void;
}

export type WatchedResult = { finished: true; status: number | null; signal: NodeJS.Signals | null } | { finished: false };

// After a stop, wait this long for the exit event before reporting the process as unfinished anyway.
const StopGraceMilliseconds = 30_000;
const StopSignals: readonly NodeJS.Signals[] = ["SIGINT", "SIGTERM"];

/** Reads what a log gained since the previous read. */
function logReader(descriptor: number): { read: () => string; size: () => number } {
  let offset = 0;
  const size = () => fstatSync(descriptor).size;
  const read = () => {
    const end = size();
    if (end <= offset) return "";
    const buffer = Buffer.alloc(end - offset);
    const length = readSync(descriptor, buffer, 0, buffer.length, offset);
    offset += length;
    return buffer.subarray(0, length).toString("utf8");
  };
  return { read, size };
}

/**
 * Runs a process with its output in a log, reporting progress at each heartbeat.
 * At the deadline it runs beforeStop, stops the whole process tree and reports an unfinished result.
 * SIGINT or SIGTERM to this process stops the tree too, so a cancelled run leaves no suite behind.
 */
export function runWatched(command: WatchedCommand): Promise<WatchedResult> {
  const writer = openSync(command.log, "w");
  const reader = openSync(command.log, "r");
  const log = logReader(reader);
  const started = Date.now();
  let expired = false;
  let settled = false;
  return new Promise<WatchedResult>((resolve, reject) => {
    const child = spawn(command.executable, command.args, { cwd: command.cwd, shell: false, stdio: ["ignore", writer, writer], detached: leadsProcessGroup() });
    let grace: NodeJS.Timeout | undefined;
    const stop = () => {
      if (child.pid !== undefined) stopProcessTree(child.pid);
      grace ??= setTimeout(() => finish({ finished: false }), StopGraceMilliseconds);
    };
    const heartbeat = setInterval(() => {
      command.onOutput(log.read());
      command.onHeartbeat(Date.now() - started, log.size());
    }, command.heartbeatMilliseconds);
    const deadline = setTimeout(() => {
      expired = true;
      try {
        command.onOutput(log.read());
        command.beforeStop();
      } catch (error) {
        process.stderr.write(`Stall diagnostics failed: ${error instanceof Error ? error.message : String(error)}\n`);
      } finally {
        stop();
      }
    }, command.deadlineMilliseconds);
    const settle = () => {
      settled = true;
      clearInterval(heartbeat);
      clearTimeout(deadline);
      if (grace !== undefined) clearTimeout(grace);
      for (const signal of StopSignals) process.off(signal, stop);
      command.onOutput(log.read());
      closeSync(writer);
      closeSync(reader);
    };
    const finish = (result: WatchedResult) => {
      if (settled) return;
      settle();
      resolve(result);
    };
    for (const signal of StopSignals) process.once(signal, stop);
    child.once("error", (error) => {
      if (settled) return;
      settle();
      reject(error);
    });
    child.once("exit", (status, signal) => finish(expired ? { finished: false } : { finished: true, status, signal }));
  });
}

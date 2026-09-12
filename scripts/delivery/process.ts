import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { closeSync, openSync } from "node:fs";

export function run(executable: string, args: readonly string[], root: string, log?: string): void {
  process.stdout.write(`${executable} ${args.join(" ")}\n`);
  const descriptor = log === undefined ? undefined : openSync(log, "w");
  try {
    const result = spawnSync(executable, args, { cwd: root, shell: false, stdio: descriptor === undefined ? "inherit" : ["ignore", descriptor, descriptor] });
    if (result.error) throw result.error;
    if (result.status !== 0) {
      process.exitCode = result.status ?? 1;
      throw new Error(`${executable} failed (${result.status ?? result.signal}); ${log ?? "see output"}`);
    }
  } finally {
    if (descriptor !== undefined) closeSync(descriptor);
  }
}

export function npm(args: readonly string[], root: string): void {
  const cli = process.env["npm_execpath"];
  assert.ok(cli, "Run this command through npm run.");
  run(process.execPath, [cli, ...args], root);
}

export function reportFailure(error: unknown): void {
  process.stderr.write(`${error instanceof Error ? error.message : String(error)}\n`);
  process.exitCode ||= 1;
}

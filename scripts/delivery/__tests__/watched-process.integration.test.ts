import assert from "node:assert/strict";
import { spawn } from "node:child_process";
import { once } from "node:events";
import { existsSync, mkdtempSync, readFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { setTimeout } from "node:timers/promises";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { runWatched, type WatchedCommand } from "../watched-process.ts";

const Fixture = fileURLToPath(new URL("fixtures/stalled-suite.ts", import.meta.url));
const Driver = fileURLToPath(new URL("fixtures/watched-driver.ts", import.meta.url));
const ExitStatus = 3;

function alive(pid: number): boolean {
  try {
    process.kill(pid, 0);
    return true;
  } catch {
    return false;
  }
}

function command(root: string, args: readonly string[], deadlineMilliseconds: number): { command: WatchedCommand; output: string[]; heartbeats: number[]; stops: number[] } {
  const output: string[] = [];
  const heartbeats: number[] = [];
  const stops: number[] = [];
  return {
    output,
    heartbeats,
    stops,
    command: {
      executable: process.execPath,
      args: [Fixture, ...args],
      cwd: root,
      log: join(root, "execution.log"),
      deadlineMilliseconds,
      heartbeatMilliseconds: 200,
      onOutput: (text) => output.push(text),
      onHeartbeat: (elapsed) => heartbeats.push(elapsed),
      beforeStop: () => stops.push(Date.now()),
    },
  };
}

test("a stalled process tree is stopped at its deadline after reporting progress", async (context) => {
  const root = mkdtempSync(join(tmpdir(), "forge-watched-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const pidFile = join(root, "idle.pid");
  const watched = command(root, [pidFile], 2_000);
  const result = await runWatched(watched.command);
  assert.deepEqual(result, { finished: false });
  assert.equal(watched.stops.length, 1);
  assert.ok(watched.heartbeats.length > 0);
  assert.match(watched.output.join(""), /\[Long Running Test\] 'Stalled test'/u);
  assert.ok(existsSync(pidFile));
  const idle = Number(readFileSync(pidFile, "utf8"));
  // An orphan stays visible until init reaps it, so allow a moment.
  for (let waited = 0; alive(idle) && waited < 5_000; waited += 100) await setTimeout(100);
  assert.equal(alive(idle), false);
});

test("a finished process reports its exit status and complete output", async (context) => {
  const root = mkdtempSync(join(tmpdir(), "forge-watched-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const watched = command(root, ["-", String(ExitStatus)], 60_000);
  const result = await runWatched(watched.command);
  assert.deepEqual(result, { finished: true, status: ExitStatus, signal: null });
  assert.equal(watched.stops.length, 0);
  assert.match(watched.output.join(""), /passed First test/u);
  assert.match(readFileSync(watched.command.log, "utf8"), /Stalled test/u);
});

// Windows cannot deliver SIGTERM to a handler, so the stop-on-signal path is proved on Linux and macOS.
test("SIGTERM to the runner stops the suite's process tree", { skip: process.platform === "win32" }, async (context) => {
  const root = mkdtempSync(join(tmpdir(), "forge-watched-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const pidFile = join(root, "idle.pid");
  const driver = spawn(process.execPath, [Driver, root], { stdio: ["ignore", "pipe", "inherit"] });
  let output = "";
  driver.stdout.on("data", (chunk: Buffer) => (output += chunk.toString("utf8")));
  for (let waited = 0; !existsSync(pidFile) && waited < 20_000; waited += 100) await setTimeout(100);
  assert.ok(existsSync(pidFile), "the stalled suite did not start");
  const idle = Number(readFileSync(pidFile, "utf8"));
  driver.kill("SIGTERM");
  const [status] = (await once(driver, "exit")) as [number | null];
  assert.equal(status, 0);
  assert.deepEqual(JSON.parse(output), { finished: true, status: null, signal: "SIGKILL" });
  // An orphan stays visible until init reaps it, so allow a moment.
  for (let waited = 0; alive(idle) && waited < 5_000; waited += 100) await setTimeout(100);
  assert.equal(alive(idle), false);
});

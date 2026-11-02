import { randomBytes } from "node:crypto";
import { cp, mkdir, rm } from "node:fs/promises";
import { basename, join } from "node:path";
import type { RunSpace } from "./containers/executor.ts";
import { scratchRoot } from "../paths.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import { setupPrompt } from "../materialize/delivery.ts";

export function suiteForSetup(suite: Suite, setup: string): Suite {
  return { ...suite, prompt: setupPrompt(suite, setup) };
}

const IdBytes = 6;
const notFrozen = new Set(["node_modules"]);
const RemoveRetries = 10;
const RemoveRetryMs = 500;

/** A short random ID. Folder names built from it say nothing about the setup. */
export function opaqueId(): string {
  return randomBytes(IdBytes).toString("hex");
}

/** A fresh folder for one run, holding a copy of the setup's snapshot and an empty home for the tools. */
export async function createRunSpace(project: string, snapshot: string): Promise<RunSpace> {
  const root = join(scratchRoot(), opaqueId());
  const space = { root, work: join(root, project), home: join(root, "home"), project };
  await mkdir(space.home, { recursive: true });
  await cp(snapshot, space.work, { recursive: true });
  return space;
}

/** A scratch folder outside any repository, for the judge's copy or the simulated user. */
export async function scratchFolder(): Promise<string> {
  const folder = join(scratchRoot(), opaqueId());
  await mkdir(folder, { recursive: true });
  return folder;
}

/** Keeps everything the agent wrote, apart from installed packages. */
export async function freezeFiles(space: RunSpace, destination: string): Promise<void> {
  await cp(space.work, destination, { recursive: true, filter: (source) => !notFrozen.has(basename(source)) });
}

/** Deletes a scratch folder. A file still locked on Windows leaves it behind instead of failing the run. */
export async function removeFolder(folder: string): Promise<void> {
  await rm(folder, { recursive: true, force: true, maxRetries: RemoveRetries, retryDelay: RemoveRetryMs }).catch(() => undefined);
}

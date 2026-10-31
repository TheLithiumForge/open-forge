import { createHash } from "node:crypto";
import { mkdir, readdir, readFile, rm } from "node:fs/promises";
import { dirname, join, relative } from "node:path";
import { localToolCommand, PinnedTool } from "../agents/pinned-tools.ts";
import { runCommand } from "../agents/process.ts";
import { EvalPaths } from "../paths.ts";
import type { Suite } from "../suite/suite.types.ts";

export interface SetupFile {
  readonly path: string;
  readonly sha256: string;
  readonly bytes: number;
}

/** What one setup started from, written once per batch. */
export interface SetupRecord {
  readonly setup: string;
  readonly label: string;
  readonly commands: readonly (readonly string[])[];
  readonly output: string;
  readonly files: readonly SetupFile[];
}

export interface Snapshot {
  readonly folder: string;
  readonly record: SetupRecord;
}

const InstallTimeoutMs = 5 * 60_000;

async function listFiles(root: string, folder: string = root): Promise<SetupFile[]> {
  const files: SetupFile[] = [];
  for (const entry of await readdir(folder, { withFileTypes: true })) {
    const path = join(folder, entry.name);
    if (entry.isDirectory()) files.push(...(await listFiles(root, path)));
    if (!entry.isFile()) continue;
    const content = await readFile(path);
    files.push({ path: relative(root, path).replaceAll("\\", "/"), sha256: createHash("sha256").update(content).digest("hex"), bytes: content.length });
  }
  return files;
}

/** Installs one setup into an empty project folder once. Every run of the setup starts from a copy of it. */
export async function prepareSnapshot(batchId: string, suite: Suite, setup: string): Promise<Snapshot> {
  const definition = suite.setups[setup];
  if (definition === undefined) throw new Error(`The suite has no setup called ${setup}.`);
  const folder = join(EvalPaths.scratch, `snapshot-${batchId}-${setup}`, suite.project);
  // A stopped server can leave an old snapshot behind. Each install starts from an empty folder.
  await rm(dirname(folder), { recursive: true, force: true, maxRetries: 3 });
  await mkdir(folder, { recursive: true });
  const openForge = localToolCommand(PinnedTool.openForge);
  const output: string[] = [];
  for (const command of definition.commands) {
    const result = await runCommand(openForge.command, [...openForge.args, ...command], { cwd: folder, timeoutMs: InstallTimeoutMs });
    output.push(`$ open-forge ${command.join(" ")}\n${result.stdout}${result.stderr}`);
    if (result.code !== 0) throw new Error(`Setting up ${definition.label} failed:\n${output.join("\n")}`);
  }
  return { folder, record: { setup, label: definition.label, commands: definition.commands, output: output.join("\n"), files: await listFiles(folder) } };
}

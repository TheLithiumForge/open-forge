import { existsSync } from "node:fs";
import { mkdir, readdir, readFile, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { EvalPaths } from "../paths.ts";
import { isRecord } from "./frontmatter.ts";
import { BrownfieldFiles, ChangeType, readBrownfieldCase, type BrownfieldCase } from "./brownfield-case.ts";

/**
 * A work packet hands several tasks over together. Its packet.json names the tasks and the request's wording, and
 * every other file in its folder is generated from the tasks: the planted bugs at once, the symptoms in one numbered
 * request, and all of the tasks' checks and answers. Run npm run packets --prefix src/evals after changing a task.
 */
export const PacketFile = "packet.json";
export const PacketVersion = 1;
const GeneratedNote = "Generated from its tasks by npm run packets --prefix src/evals, so edit packet.json or the tasks instead.";

export interface PacketTask { readonly id: string; readonly symptom: string }
export interface PacketDefinition {
  readonly id: string; readonly label: string; readonly title: string;
  readonly intro: string; readonly outro: string; readonly tasks: readonly PacketTask[]; readonly runMinutes?: number;
}

function text(value: unknown, field: string): string {
  if (typeof value !== "string" || !value.trim()) throw new Error(`A packet needs its ${field}.`);
  return value;
}

export async function readPacket(folder: string): Promise<PacketDefinition> {
  const json: unknown = JSON.parse(await readFile(join(folder, PacketFile), "utf8"));
  if (!isRecord(json) || json["schemaVersion"] !== PacketVersion || !Array.isArray(json["tasks"]) || json["tasks"].length < 2) throw new Error("A packet needs schemaVersion 1 and two or more tasks.");
  const tasks = json["tasks"].map((task: unknown): PacketTask => {
    if (!isRecord(task)) throw new Error("Invalid packet task.");
    return { id: text(task["id"], "task id"), symptom: text(task["symptom"], "task symptom") };
  });
  if (new Set(tasks.map((task) => task.id)).size !== tasks.length) throw new Error("A packet lists each task once.");
  const runMinutes = json["runMinutes"];
  if (runMinutes !== undefined && (typeof runMinutes !== "number" || !Number.isInteger(runMinutes) || runMinutes < 1)) throw new Error("A packet's runMinutes must be a whole number of minutes.");
  return { id: text(json["id"], "id"), label: text(json["label"], "label"), title: text(json["title"], "title"), intro: text(json["intro"], "intro"), outro: text(json["outro"], "outro"), tasks,
    ...(runMinutes === undefined ? {} : { runMinutes }) };
}

/** The packet folders under the cases, found by their packet.json. */
export async function packetFolders(root: string = EvalPaths.suites): Promise<string[]> {
  return (await readdir(root, { withFileTypes: true })).filter((entry) => entry.isDirectory() && existsSync(join(root, entry.name, PacketFile))).map((entry) => join(root, entry.name)).sort();
}

const json = (value: unknown): string => `${JSON.stringify(value, null, 2)}\n`;
// The tasks keep one check per line, so a packet's checks read and diff the same way.
const inline = (value: unknown): string => JSON.stringify(value).replace(/("(?:[^"\\]|\\.)*")|([,:])/g, (match, quoted: string | undefined) => quoted ?? `${match} `)
  .replace(/^\{/, "{ ").replace(/\}$/, " }").replace(/: \{(?! )/g, ": { ").replace(/\}(?=,| \})/g, " }");
const checksFile = (checks: readonly unknown[], expectations: Readonly<Record<string, unknown>>): string =>
  `{\n  "schemaVersion": 1,\n  "checks": [\n${checks.map((check) => `    ${inline(check)}`).join(",\n")}\n  ],\n  "regressionExpectations": ${inline(expectations)}\n}\n`;
const unique = (values: readonly string[]): string[] => [...new Set(values)];

async function taskChecks(task: BrownfieldCase): Promise<{ readonly checks: unknown[]; readonly regressionExpectations: Record<string, unknown> }> {
  const file: unknown = JSON.parse(await readFile(join(task.root, task.checks, "checks.json"), "utf8"));
  if (!isRecord(file) || !Array.isArray(file["checks"])) throw new Error(`Invalid checks for ${task.id}.`);
  const expectations = file["regressionExpectations"];
  return { checks: file["checks"], regressionExpectations: isRecord(expectations) ? expectations : {} };
}

/** Every generated file of a packet, keyed by its path in the packet folder. */
export async function packetFiles(packet: PacketDefinition, root: string = EvalPaths.suites): Promise<Record<string, string>> {
  const tasks = await Promise.all(packet.tasks.map((task) => readBrownfieldCase(join(root, task.id), task.id)));
  for (const task of tasks) if (task.specDelta !== null || task.seedPatch === null || task.altSolutionPatch !== null) throw new Error(`${task.id} changes the spec or has no planted bug, which packets don't combine yet.`);
  const read = (task: BrownfieldCase, path: string): Promise<string> => readFile(join(task.root, path), "utf8");
  const checks = await Promise.all(tasks.map(taskChecks));
  const expectations: Record<string, unknown> = {};
  for (const part of checks) for (const [id, expected] of Object.entries(part.regressionExpectations)) {
    if (expectations[id] !== undefined && expectations[id] !== expected) throw new Error(`The tasks expect different results for ${id}.`);
    expectations[id] = expected;
  }
  const answers = (await Promise.all(tasks.map(async (task) => JSON.parse(await read(task, "answers.json")) as unknown[]))).flat();
  const request = [packet.intro, "", ...packet.tasks.map((task, index) => `${index + 1}. ${task.symptom}`), "", packet.outro, ""].join("\n");
  const labels = packet.tasks.map((task) => task.id);
  return {
    [BrownfieldFiles.definition]: json({ schemaVersion: 1, id: packet.id, changeType: ChangeType.repair, title: packet.title, request: BrownfieldFiles.request, specDelta: null,
      seedPatch: BrownfieldFiles.seed, solutionPatch: BrownfieldFiles.solution, checks: BrownfieldFiles.checks, answers: BrownfieldFiles.answers,
      supersedes: unique(tasks.flatMap((task) => task.supersedes)), ...(packet.runMinutes === undefined ? {} : { runMinutes: packet.runMinutes }),
      diffScope: unique(tasks.flatMap((task) => task.diffScope)).sort() }),
    [BrownfieldFiles.request]: request,
    [BrownfieldFiles.seed]: (await Promise.all(tasks.map((task) => read(task, task.seedPatch ?? "")))).join(""),
    [BrownfieldFiles.solution]: (await Promise.all(tasks.map((task) => read(task, task.solutionPatch)))).join(""),
    [BrownfieldFiles.answers]: json(answers),
    [`${BrownfieldFiles.checks}/checks.json`]: checksFile(checks.flatMap((part) => part.checks), expectations),
    "README.md": [`# ${packet.label} ${packet.title}`, "", `${packet.label} hands over ${labels.map((id) => `[${id}](../${id}/README.md)`).join(", ")} as one work packet.`, "",
      "Its starting code holds every task's planted bug, its request lists their symptoms, and its checks are all of theirs with their own IDs, so each task's coverage reads from its prefix. Every other full-spec check runs as regression.", "",
      GeneratedNote, ""].join("\n"),
  };
}

/** Writes every packet's generated files. */
export async function writePackets(root: string = EvalPaths.suites): Promise<void> {
  for (const folder of await packetFolders(root)) {
    for (const [path, content] of Object.entries(await packetFiles(await readPacket(folder), root))) {
      await mkdir(dirname(join(folder, path)), { recursive: true });
      await writeFile(join(folder, path), content);
    }
  }
}

if (import.meta.main) await writePackets();

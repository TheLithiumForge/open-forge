import { join } from "node:path";
import { readdir, readFile } from "node:fs/promises";
import type { EvalConfig } from "../config/config.types.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import { evalsRoot, EvalPaths } from "../paths.ts";
import { fileDigests } from "./artifacts.ts";
import { createFingerprints, fingerprint, sha256 } from "./fingerprints.ts";
import { readJson } from "./json-files.ts";
import type { AssessmentVersions, ExperimentRecord, RunRecord } from "./records.types.ts";
import { BatchStatus, RecordSchemaVersion } from "./statuses.ts";

interface PackageInfo {
  readonly version: string;
  readonly dependencies: Readonly<Record<string, string>>;
}

export interface CaptureSources {
  readonly root: string;
  readonly modules: string;
  readonly suites: string;
  readonly checks: string;
  readonly docker: string;
  readonly imageDigests?: Readonly<Record<string, string | null>>;
}

const sources: CaptureSources = { root: evalsRoot, ...EvalPaths };
const loadedSuiteFiles = new Set(["suite.json", "prompt.md", "intent.json", "intent.local.json", "rubric.json"]);

async function harnessSources(root: string) {
  const folders = ["execution", "config", "grading", "history", "web/server", "web/ui"];
  const nested = await Promise.all(folders.map(async (folder) => ({ folder, files: await fileDigests(join(root, folder)) })));
  const files = (await readdir(root, { withFileTypes: true })).filter((entry) => entry.isFile()
    && (entry.name.endsWith(".ts") || entry.name === "package.json" || entry.name === "package-lock.json"));
  const topLevel = await Promise.all(files.map(async (entry) => {
    const bytes = await readFile(join(root, entry.name));
    return { path: entry.name, bytes: bytes.length, sha256: sha256(bytes) };
  }));
  return [...nested, { folder: ".", files: topLevel.sort((a, b) => a.path.localeCompare(b.path)) },
    { folder: "benchmarks", files: await fileDigests(join(root, "benchmarks"), new Set(["neon-pairs"])) }];
}

export async function gradingVersions(suite: Suite): Promise<AssessmentVersions> {
  return {
    checker: fingerprint(await fileDigests(EvalPaths.checks)), rubric: fingerprint(suite.rubric),
    judge: fingerprint(await fileDigests(join(evalsRoot, "grading", "judge"))),
  };
}

export async function captureExperiment(id: string, config: EvalConfig, suite: Suite, candidateLabel: string | null = null, paths: CaptureSources = sources): Promise<ExperimentRecord> {
  const cliRoot = join(paths.modules, "@thelithiumforge", "open-forge");
  const cli = await readJson<PackageInfo>(join(cliRoot, "package.json"));
  const pkg = await readJson<PackageInfo>(join(paths.root, "package.json"));
  const harnessSource = await harnessSources(paths.root);
  const pinnedTools = { ...pkg.dependencies, node: process.version };
  const imageDigests: Readonly<Record<string, string | null>> = paths.imageDigests ?? { eval: null };
  const versions = { checker: fingerprint(await fileDigests(paths.checks)), rubric: fingerprint(suite.rubric),
    judge: fingerprint(await fileDigests(join(paths.root, "grading", "judge"))) };
  const requestedModels = config.agents;
  const fingerprints = createFingerprints({
    cliPackage: await fileDigests(cliRoot),
    benchmarkInputs: { id: suite.id, project: suite.project, baseline: suite.baseline, prompt: suite.prompt, intent: suite.intent,
      files: (await fileDigests(join(paths.suites, suite.id))).filter((file) => !loadedSuiteFiles.has(file.path)) },
    setupContent: suite.setups, checks: await fileDigests(paths.checks), rubric: suite.rubric, harnessSource,
    tooling: { pinnedTools, imageDigests, images: await fileDigests(paths.docker) },
    requestedModels, reportedModels: null,
  });
  return {
    schemaVersion: RecordSchemaVersion, id, suite: suite.id, fingerprint: fingerprints.benchmarkInputs ?? "", intentEdited: suite.intentEdited,
    config, createdAt: new Date().toISOString(), status: BatchStatus.preparing, runs: [], revealed: false, legacy: false,
    versions: { openForge: cli.version, node: process.version, checker: versions.checker ?? "", rubric: versions.rubric ?? "", judge: versions.judge ?? "" },
    cliVersion: cli.version, candidateLabel, capturedAt: new Date().toISOString(), fingerprints,
    pinnedTools, imageDigests, requestedModels, reportedModels: null,
  };
}

export function withReportedModels(batch: ExperimentRecord, runs: readonly RunRecord[]): ExperimentRecord {
  const reportedModels = runs.map((run) => run.reportedModel);
  return { ...batch, reportedModels, fingerprints: { ...batch.fingerprints, reportedModels: fingerprint(reportedModels) } };
}

export function assertFrozenExecution(saved: ExperimentRecord, current: ExperimentRecord): void {
  const keys = ["cliPackage", "benchmarkInputs", "checks", "rubric", "harnessSource", "tooling", "requestedModels", "schema"] as const;
  if (keys.some((key) => saved.fingerprints[key] !== current.fingerprints[key])) throw new Error("The batch inputs or tooling changed. Start a new batch.");
}

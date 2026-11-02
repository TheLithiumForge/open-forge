import { join } from "node:path";
import { readdir, readFile } from "node:fs/promises";
import type { EvalConfig } from "../config/config.types.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import { LayerOneCase, suiteGradingInputs } from "../benchmarks/registry.ts";
import { evalsRoot, EvalPaths } from "../paths.ts";
import { fileDigests } from "./artifacts.ts";
import { createFingerprints, fingerprint, sha256 } from "./fingerprints.ts";
import { readJson } from "./json-files.ts";
import type { AssessmentVersions, ExperimentRecord, RunRecord } from "./records.types.ts";
import { BatchStatus, RecordSchemaVersion } from "./statuses.ts";
import { documentGradingInputs } from "../grading/document-layers.ts";
import { SpecVersion } from "../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";

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

// Layer 1 grades with the top-level checks alone, so a later layer's checker change leaves its version unchanged.
export const LaterLayerCheckers: ReadonlySet<string> = new Set(["spec-layers"]);

export function checkerDigests(root: string, suiteId: string): ReturnType<typeof fileDigests> {
  return suiteId === LayerOneCase ? fileDigests(root, LaterLayerCheckers) : fileDigests(root);
}

export async function gradingVersions(suite: Suite): Promise<AssessmentVersions> {
  const frozen = [...await suiteGradingInputs(suite.id), ...await documentGradingInputs(suite.id)];
  return {
    ...(suite.documents === undefined ? {} : { spec: fingerprint(suite.documents), specVersion: SpecVersion }),
    checker: fingerprint({ files: await checkerDigests(EvalPaths.checks, suite.id), frozen }), rubric: fingerprint({ questions: suite.rubric, frozen }),
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
  const frozen = [...await suiteGradingInputs(suite.id, paths.root), ...await documentGradingInputs(suite.id, paths.root)];
  const checkInputs = { files: await checkerDigests(paths.checks, suite.id), frozen };
  const rubricInputs = { questions: suite.rubric, frozen };
  const versions = { checker: fingerprint(checkInputs), rubric: fingerprint(rubricInputs),
    judge: fingerprint(await fileDigests(join(paths.root, "grading", "judge"))) };
  const requestedModels = config.agents;
  const fingerprints = createFingerprints({
    cliPackage: await fileDigests(cliRoot),
    benchmarkInputs: { id: suite.id, project: suite.project, baseline: suite.baseline, prompt: suite.prompt, intent: suite.intent,
      files: (await fileDigests(join(paths.suites, suite.id))).filter((file) => !loadedSuiteFiles.has(file.path)) },
    setupContent: suite.setups, checks: checkInputs, rubric: rubricInputs, harnessSource,
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

const FrozenKeys = ["cliPackage", "benchmarkInputs", "checks", "rubric", "harnessSource", "tooling", "requestedModels", "schema"] as const;
export type FrozenKey = (typeof FrozenKeys)[number];

export function frozenDifferences(saved: ExperimentRecord, current: ExperimentRecord): FrozenKey[] {
  return FrozenKeys.filter((key) => saved.fingerprints[key] !== current.fingerprints[key]);
}

export function assertFrozenExecution(saved: ExperimentRecord, current: ExperimentRecord): void {
  if (frozenDifferences(saved, current).length) throw new Error("The batch inputs or tooling changed. Start a new batch.");
}

/**
 * The record a resume continues under. Changed runner code alone may continue when the caller accepts it, and the
 * batch then lists it. Changed inputs, checks, rubric, tooling, models or schema always need a new batch.
 */
export function resumableExperiment(saved: ExperimentRecord, current: ExperimentRecord, acceptHarness: boolean, at = new Date().toISOString()): ExperimentRecord {
  const changed = frozenDifferences(saved, current);
  if (!changed.length) return saved;
  if (changed.some((key) => key !== "harnessSource")) throw new Error(`The batch's ${changed.join(", ")} changed. Start a new batch.`);
  if (!acceptHarness) throw new Error("The runner code changed since this batch started. Resume with --accept-harness to continue and record the change, or start a new batch.");
  const known = saved.harnessChanges ?? [];
  if (known.at(-1)?.harnessSource === current.fingerprints.harnessSource) return saved;
  return { ...saved, harnessChanges: [...known, { at, harnessSource: current.fingerprints.harnessSource }] };
}

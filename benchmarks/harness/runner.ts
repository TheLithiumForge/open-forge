#!/usr/bin/env bun

import fs from "node:fs/promises";
import path from "node:path";
import { createHash, randomUUID } from "node:crypto";
import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";

const DEFAULT_REPO_ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const DEFAULT_SCENARIOS_ROOT = path.join(DEFAULT_REPO_ROOT, "benchmarks", "building-blocks", "scenarios");
const DEFAULT_PRIMITIVES_ROOT = path.join(DEFAULT_REPO_ROOT, "benchmarks", "building-blocks", "primitives");
const DEFAULT_META_SCENARIOS_ROOT = path.join(DEFAULT_REPO_ROOT, "benchmarks", "meta-scenarios");
const PRIMITIVE_KINDS = ["directive", "pattern", "memory", "guidance", "workspace", "skill", "workflow", "tool"] as const;
const PRIMITIVE_ROUTES: Record<Exclude<PrimitiveKind, "tool">, string> = {
  directive: "directives",
  pattern: "patterns",
  memory: "memory",
  guidance: "guidance",
  workspace: "workspace",
  skill: "skills",
  workflow: "workflows",
};
const ORCHESTRATOR_REVIEW_SECTIONS = ["Behavior", "Outcome", "Comparison With Worker Review", "Limits"];
const WORKER_REVIEW_PLACEHOLDER = "Replace this file with the worker's complete response to `input/worker-review-prompt.md`.\n";
const TRACE_CAPTURE_PLACEHOLDER = "Replace with how the runtime-visible trace was captured, or why no trace surface was available.";

export type PrimitiveKind = typeof PRIMITIVE_KINDS[number];

export interface ScenarioDefinition {
  id: string;
}

export interface PrimitiveDefinition {
  id: string;
  kind: PrimitiveKind;
}

export interface MetaScenarioDefinition {
  id: string;
  scenario: string;
  primitives: string[];
  extensions: string[];
}

export interface Scenario extends ScenarioDefinition {
  directory: string;
  scenarioFile: string;
  promptFile: string;
  reviewFile: string;
  personaFile?: string;
  payloadDirectory?: string;
  payloadFiles: PayloadFile[];
}

export interface Primitive extends PrimitiveDefinition {
  directory: string;
  primitiveFile: string;
  reviewFile: string;
  payloadDirectory?: string;
  payloadFiles: PayloadFile[];
}

export interface MetaScenario extends MetaScenarioDefinition {
  directory: string;
  metaFile: string;
  reviewFile: string;
}

export interface PreparedRun {
  metaScenarioId: string;
  scenarioId: string;
  primitiveIds: string[];
  variantIds: string[];
  extensions: string[];
  workerPromptSha256: string;
  baselineTree: string;
  runDir: string;
  workspaceDir: string;
  recordDir: string;
  inputDir: string;
  compositionPath: string;
  reviewIndexPath: string;
  workerPromptPath: string;
  workerReviewPromptPath: string;
  orchestratorPromptPath: string;
  orchestratorAddendumPath?: string;
  scenarioReviewPath: string;
  metaReviewPath: string;
  primitiveReviewPaths: string[];
  variantReviewPaths: string[];
  workerReviewPath: string;
  orchestratorReviewPath: string;
  personaPath?: string;
  baselineCommit: string;
}

export interface FinishedRun {
  metaScenarioId: string;
  scenarioId: string;
  primitiveIds: string[];
  variantIds: string[];
  extensions: string[];
  workerPromptSha256: string;
  baselineTree: string;
  state: "finished";
  preparedAt: string;
  finishedAt: string;
  baselineCommit: string;
  finalCommit: string;
  doctor: {
    exitCode: number;
    errors: number | null;
    warnings: number | null;
  };
  findFollowRequired: {
    exitCode: number;
  };
}

interface CompositionPlan {
  metaScenarioId: string;
  scenarioId: string;
  primitiveIds: string[];
  variantIds: string[];
  extensions: string[];
  workerPromptSha256: string;
}

interface Composition extends CompositionPlan {
  baselineTree: string;
}

interface RunMetadata extends Composition {
  preparedAt: string;
  baselineCommit: string;
  source: {
    revision: string | null;
    dirty: boolean;
    status: string[] | null;
  };
}

interface PayloadFile {
  source: string;
  relative: string;
}

interface ProcessResult {
  exitCode: number;
  stdout: string;
  stderr: string;
}

export function validateScenarioDocument(value: unknown): ScenarioDefinition {
  const document = exactObject(value, "scenario.json", ["id"]);
  return { id: nonBlankString(document.id, "scenario.json id") };
}

export function validatePrimitiveDocument(value: unknown): PrimitiveDefinition {
  const document = exactObject(value, "primitive.json", ["id", "kind"]);
  const id = nonBlankString(document.id, "primitive.json id");
  if (typeof document.kind !== "string" || !(PRIMITIVE_KINDS as readonly string[]).includes(document.kind)) {
    throw new Error(`primitive.json kind must be one of: ${PRIMITIVE_KINDS.join(", ")}`);
  }
  return { id, kind: document.kind as PrimitiveKind };
}

export function validateMetaScenarioDocument(value: unknown): MetaScenarioDefinition {
  const document = exactObject(value, "meta.json", ["id", "scenario", "primitives", "extensions"]);
  const id = nonBlankString(document.id, "meta.json id");
  const scenario = nonBlankString(document.scenario, "meta.json scenario");
  const primitives = stringArray(document.primitives, "meta.json primitives", false);
  const extensions = stringArray(document.extensions, "meta.json extensions", true);
  return { id, scenario, primitives, extensions };
}

export async function discoverScenarios(scenariosRoot = DEFAULT_SCENARIOS_ROOT): Promise<Scenario[]> {
  const manifests = await discoverFiles(path.resolve(scenariosRoot), "scenario.json");
  const scenarios: Scenario[] = [];
  for (const scenarioFile of manifests) {
    const definition = validateScenarioDocument(await readJson(scenarioFile));
    const directory = path.dirname(scenarioFile);
    const promptFile = await requiredRegularFile(directory, "prompt.md", "Scenario");
    const reviewFile = await requiredRegularFile(directory, "review.md", "Scenario");
    const personaFile = await optionalRegularFile(directory, "persona.md", "Scenario");
    const payloadDirectory = await optionalRegularDirectory(directory, "payload", "Scenario");
    const payloadFiles = payloadDirectory ? await collectPayloadFiles(payloadDirectory, validateScenarioPayloadPath) : [];
    scenarios.push({
      ...definition,
      directory,
      scenarioFile,
      promptFile,
      reviewFile,
      ...(personaFile ? { personaFile } : {}),
      ...(payloadDirectory ? { payloadDirectory } : {}),
      payloadFiles,
    });
  }
  return uniqueById(scenarios, "scenario");
}

export async function discoverPrimitives(primitivesRoot = DEFAULT_PRIMITIVES_ROOT): Promise<Primitive[]> {
  const manifests = await discoverFiles(path.resolve(primitivesRoot), "primitive.json");
  const primitives: Primitive[] = [];
  for (const primitiveFile of manifests) {
    primitives.push(await readPrimitivePackage(path.dirname(primitiveFile)));
  }
  return uniqueById(primitives, "primitive");
}

export async function discoverMetaScenarios(metaScenariosRoot = DEFAULT_META_SCENARIOS_ROOT): Promise<MetaScenario[]> {
  const manifests = await discoverFiles(path.resolve(metaScenariosRoot), "meta.json");
  const metaScenarios: MetaScenario[] = [];
  for (const metaFile of manifests) {
    const definition = validateMetaScenarioDocument(await readJson(metaFile));
    const directory = path.dirname(metaFile);
    const reviewFile = await requiredRegularFile(directory, "review.md", "Meta-scenario");
    metaScenarios.push({ ...definition, directory, metaFile, reviewFile });
  }
  return uniqueById(metaScenarios, "meta-scenario");
}

export async function prepareMetaScenario(options: {
  metaScenarioId: string;
  repoRoot?: string;
  scenariosRoot?: string;
  primitivesRoot?: string;
  metaScenariosRoot?: string;
  runsRoot?: string;
  orchestratorFile?: string;
  variantDirectories?: string[];
  extraExtensions?: string[];
}): Promise<PreparedRun> {
  const repoRoot = path.resolve(options.repoRoot ?? DEFAULT_REPO_ROOT);
  const scenariosRoot = path.resolve(options.scenariosRoot ?? path.join(repoRoot, "benchmarks", "building-blocks", "scenarios"));
  const primitivesRoot = path.resolve(options.primitivesRoot ?? path.join(repoRoot, "benchmarks", "building-blocks", "primitives"));
  const metaScenariosRoot = path.resolve(options.metaScenariosRoot ?? path.join(repoRoot, "benchmarks", "meta-scenarios"));
  if (!options.runsRoot) throw new Error("--runs-root is required and must be outside the source repository");
  const runsRoot = path.resolve(options.runsRoot);
  await assertRunsRootOutsideRepository(repoRoot, runsRoot);

  const [scenarios, primitives, metaScenarios] = await Promise.all([
    discoverScenarios(scenariosRoot),
    discoverPrimitives(primitivesRoot),
    discoverMetaScenarios(metaScenariosRoot),
  ]);
  const metaScenario = metaScenarios.find((candidate) => candidate.id === options.metaScenarioId);
  if (!metaScenario) throw new Error(`Unknown meta-scenario id: ${options.metaScenarioId}`);
  const scenario = scenarios.find((candidate) => candidate.id === metaScenario.scenario);
  if (!scenario) throw new Error(`Meta-scenario ${metaScenario.id} references unknown scenario: ${metaScenario.scenario}`);
  const primitiveMap = new Map(primitives.map((primitive) => [primitive.id, primitive]));
  const selectedPrimitives = metaScenario.primitives.map((id) => {
    const primitive = primitiveMap.get(id);
    if (!primitive) throw new Error(`Meta-scenario ${metaScenario.id} references unknown primitive: ${id}`);
    return primitive;
  });

  const variants: Primitive[] = [];
  for (const directory of options.variantDirectories ?? []) {
    variants.push(await readPrimitivePackage(path.resolve(directory), "Variant"));
  }
  assertNoDuplicateIds([...primitives, ...variants], "Stable primitive and variant ids");

  const extras = options.extraExtensions ?? [];
  for (const [index, extension] of extras.entries()) {
    if (!isBundledExtensionId(extension)) throw new Error(`--extension value ${index + 1} must be a bundled extension id`);
  }
  assertUniqueStrings(extras, "--extension values");
  const extensions = [...metaScenario.extensions];
  for (const extension of extras) {
    if (extensions.includes(extension)) throw new Error(`Extension selected more than once: ${extension}`);
    extensions.push(extension);
  }
  const bundledIds = await discoverBundledExtensionIds(path.join(repoRoot, "src", "extensions"));
  for (const extension of extensions) {
    if (!bundledIds.has(extension)) throw new Error(`Unknown bundled extension id: ${extension}`);
  }

  const orchestratorFile = options.orchestratorFile !== undefined
    ? await requiredExternalFile(path.resolve(options.orchestratorFile), "Orchestrator addendum")
    : undefined;
  assertPayloadLayersDoNotCollide([
    { label: `scenario ${scenario.id}`, files: scenario.payloadFiles },
    ...selectedPrimitives.map((primitive) => ({ label: `primitive ${primitive.id}`, files: primitive.payloadFiles })),
    ...variants.map((variant) => ({ label: `variant ${variant.id}`, files: variant.payloadFiles })),
  ]);

  const compositionPlan: CompositionPlan = {
    metaScenarioId: metaScenario.id,
    scenarioId: scenario.id,
    primitiveIds: selectedPrimitives.map((primitive) => primitive.id),
    variantIds: variants.map((variant) => variant.id),
    extensions,
    workerPromptSha256: await sha256File(scenario.promptFile),
  };
  const source = await readSourceState(repoRoot);

  await fs.mkdir(runsRoot, { recursive: true });
  const runDir = path.join(runsRoot, randomUUID());
  await fs.mkdir(runDir);
  const workspaceDir = path.join(runDir, "workspace");
  const recordDir = path.join(runDir, "record");
  const inputDir = path.join(recordDir, "input");

  try {
    await fs.mkdir(workspaceDir);
    await fs.mkdir(inputDir, { recursive: true });
    await freezeInputs({
      repoRoot,
      inputDir,
      metaScenario,
      scenario,
      primitives: selectedPrimitives,
      variants,
      orchestratorFile,
    });

    await git(workspaceDir, "init", "--quiet");
    await git(workspaceDir, "config", "user.name", "Open Forge Benchmark");
    await git(workspaceDir, "config", "user.email", "benchmark@open-forge.invalid");
    await git(workspaceDir, "config", "commit.gpgsign", "false");

    await openForge(repoRoot, ["install", workspaceDir, "--pro"], "Core installation");
    if (extensions.length > 0) {
      await openForge(repoRoot, ["extend", "--ids", extensions.join(","), workspaceDir, "--pro"], "Extension installation");
    }
    await mergeFrozenPayload(path.join(inputDir, "scenario", "payload"), workspaceDir, `scenario ${scenario.id}`);
    for (const [index, primitive] of selectedPrimitives.entries()) {
      await mergeFrozenPayload(path.join(inputDir, "primitives", ordinal(index), "payload"), workspaceDir, `primitive ${primitive.id}`);
    }
    for (const [index, variant] of variants.entries()) {
      await mergeFrozenPayload(path.join(inputDir, "variants", ordinal(index), "payload"), workspaceDir, `variant ${variant.id}`);
    }

    await openForge(repoRoot, ["index", workspaceDir], "Indexing");
    const [doctor, findFollowRequired] = await Promise.all([
      openForge(repoRoot, ["doctor", "--json", workspaceDir], "Doctor"),
      openForge(repoRoot, ["find", "--follow-required", "--json", workspaceDir], "Required-route discovery"),
    ]);
    await Promise.all([
      fs.writeFile(path.join(recordDir, "prepare-doctor.json"), doctor.stdout),
      fs.writeFile(path.join(recordDir, "prepare-find-follow-required.json"), findFollowRequired.stdout),
    ]);

    await git(workspaceDir, "add", "-A");
    await git(workspaceDir, "commit", "--quiet", "--no-verify", "-m", "Benchmark baseline");
    const baselineCommit = (await git(workspaceDir, "rev-parse", "HEAD")).stdout.trim();
    const baselineTree = (await git(workspaceDir, "rev-parse", "HEAD^{tree}")).stdout.trim();
    const composition: Composition = { ...compositionPlan, baselineTree };
    await writeJson(path.join(inputDir, "composition.json"), composition);
    const metadata: RunMetadata = {
      ...composition,
      preparedAt: new Date().toISOString(),
      baselineCommit,
      source,
    };
    await writeJson(path.join(recordDir, "run.json"), metadata);

    const primitiveReviewPaths = selectedPrimitives.map((_, index) => path.join(inputDir, "primitives", ordinal(index), "review.md"));
    const variantReviewPaths = variants.map((_, index) => path.join(inputDir, "variants", ordinal(index), "review.md"));
    return {
      ...composition,
      runDir,
      workspaceDir,
      recordDir,
      inputDir,
      compositionPath: path.join(inputDir, "composition.json"),
      reviewIndexPath: path.join(inputDir, "review-order.md"),
      workerPromptPath: path.join(inputDir, "scenario", "prompt.md"),
      workerReviewPromptPath: path.join(inputDir, "worker-review-prompt.md"),
      orchestratorPromptPath: path.join(inputDir, "orchestrator-prompt.md"),
      ...(orchestratorFile ? { orchestratorAddendumPath: path.join(inputDir, "orchestrator-addendum.md") } : {}),
      scenarioReviewPath: path.join(inputDir, "scenario", "review.md"),
      metaReviewPath: path.join(inputDir, "meta", "review.md"),
      primitiveReviewPaths,
      variantReviewPaths,
      workerReviewPath: path.join(recordDir, "worker-review.md"),
      orchestratorReviewPath: path.join(recordDir, "orchestrator-review.md"),
      ...(scenario.personaFile ? { personaPath: path.join(inputDir, "scenario", "persona.md") } : {}),
      baselineCommit,
    };
  } catch (error) {
    await fs.rm(runDir, { recursive: true, force: true });
    throw error;
  }
}

export async function finishRun(runDirectory: string, options: { repoRoot?: string } = {}): Promise<FinishedRun> {
  const repoRoot = path.resolve(options.repoRoot ?? DEFAULT_REPO_ROOT);
  const runDir = path.resolve(runDirectory);
  const workspaceDir = path.join(runDir, "workspace");
  const recordDir = path.join(runDir, "record");
  await requireDirectory(workspaceDir, "Run workspace");
  await requireDirectory(recordDir, "Run record");

  const resultFile = path.join(recordDir, "result.json");
  if (await exists(resultFile)) throw new Error(`Run is already finished: ${runDir}`);

  await requireWorkerReview(path.join(recordDir, "worker-review.md"));
  await requireCompletedReview(path.join(recordDir, "orchestrator-review.md"), ORCHESTRATOR_REVIEW_SECTIONS);
  await requireTraceManifest(path.join(recordDir, "trace", "manifest.json"));

  const metadata = await readRunMetadata(path.join(recordDir, "run.json"));
  await git(workspaceDir, "cat-file", "-e", `${metadata.baselineCommit}^{commit}`);
  const recordedBaselineTree = (await git(workspaceDir, "rev-parse", `${metadata.baselineCommit}^{tree}`)).stdout.trim();
  if (recordedBaselineTree !== metadata.baselineTree) throw new Error("Run metadata baselineTree does not match baselineCommit");

  const preFinishStatus = await git(workspaceDir, "status", "--short", "--untracked-files=all");
  await fs.writeFile(path.join(recordDir, "pre-finish-status.txt"), preFinishStatus.stdout);

  const [doctor, findFollowRequired] = await Promise.all([
    runOpenForge(repoRoot, ["doctor", "--json", workspaceDir]),
    runOpenForge(repoRoot, ["find", "--follow-required", "--json", workspaceDir]),
  ]);
  await Promise.all([
    fs.writeFile(path.join(recordDir, "finish-doctor.stdout.txt"), doctor.stdout),
    fs.writeFile(path.join(recordDir, "finish-doctor.stderr.txt"), doctor.stderr),
    fs.writeFile(path.join(recordDir, "finish-find-follow-required.stdout.txt"), findFollowRequired.stdout),
    fs.writeFile(path.join(recordDir, "finish-find-follow-required.stderr.txt"), findFollowRequired.stderr),
  ]);

  await git(workspaceDir, "add", "-A");
  await git(workspaceDir, "commit", "--quiet", "--no-verify", "--allow-empty", "-m", "Benchmark final");
  const finalCommit = (await git(workspaceDir, "rev-parse", "HEAD")).stdout.trim();
  const patch = await git(workspaceDir, "diff", "--binary", `${metadata.baselineCommit}..${finalCommit}`, "--");
  await fs.writeFile(path.join(recordDir, "changes.patch"), patch.stdout);

  const result: FinishedRun = {
    metaScenarioId: metadata.metaScenarioId,
    scenarioId: metadata.scenarioId,
    primitiveIds: metadata.primitiveIds,
    variantIds: metadata.variantIds,
    extensions: metadata.extensions,
    workerPromptSha256: metadata.workerPromptSha256,
    baselineTree: metadata.baselineTree,
    state: "finished",
    preparedAt: metadata.preparedAt,
    finishedAt: new Date().toISOString(),
    baselineCommit: metadata.baselineCommit,
    finalCommit,
    doctor: doctorSummary(doctor),
    findFollowRequired: { exitCode: findFollowRequired.exitCode },
  };
  await writeJson(resultFile, result, true);
  return result;
}

async function freezeInputs(options: {
  repoRoot: string;
  inputDir: string;
  metaScenario: MetaScenario;
  scenario: Scenario;
  primitives: Primitive[];
  variants: Primitive[];
  orchestratorFile?: string;
}): Promise<void> {
  const { repoRoot, inputDir, metaScenario, scenario, primitives, variants, orchestratorFile } = options;

  const metaDirectory = path.join(inputDir, "meta");
  await fs.mkdir(metaDirectory);
  await fs.copyFile(metaScenario.metaFile, path.join(metaDirectory, "meta.json"));
  await fs.copyFile(metaScenario.reviewFile, path.join(metaDirectory, "review.md"));

  const scenarioDirectory = path.join(inputDir, "scenario");
  await fs.mkdir(scenarioDirectory);
  await fs.copyFile(scenario.scenarioFile, path.join(scenarioDirectory, "scenario.json"));
  await fs.copyFile(scenario.promptFile, path.join(scenarioDirectory, "prompt.md"));
  await fs.copyFile(scenario.reviewFile, path.join(scenarioDirectory, "review.md"));
  if (scenario.personaFile) await fs.copyFile(scenario.personaFile, path.join(scenarioDirectory, "persona.md"));
  await copyPayloadFiles(scenario.payloadFiles, path.join(scenarioDirectory, "payload"));

  for (const [index, primitive] of primitives.entries()) {
    await freezePrimitive(primitive, path.join(inputDir, "primitives", ordinal(index)));
  }
  for (const [index, variant] of variants.entries()) {
    await freezePrimitive(variant, path.join(inputDir, "variants", ordinal(index)));
  }

  await fs.copyFile(
    path.join(repoRoot, "benchmarks", "harness", "orchestrator", "worker-review-prompt.md"),
    path.join(inputDir, "worker-review-prompt.md"),
  );
  await fs.copyFile(
    path.join(repoRoot, "benchmarks", "harness", "orchestrator", "orchestrator-prompt.md"),
    path.join(inputDir, "orchestrator-prompt.md"),
  );
  if (orchestratorFile) await fs.copyFile(orchestratorFile, path.join(inputDir, "orchestrator-addendum.md"));
  await fs.writeFile(path.join(inputDir, "review-order.md"), reviewOrder(primitives.length, variants.length));

  const recordDir = path.dirname(inputDir);
  await fs.writeFile(path.join(recordDir, "worker-review.md"), WORKER_REVIEW_PLACEHOLDER);
  await fs.mkdir(path.join(recordDir, "trace", "raw"), { recursive: true });
  await writeJson(path.join(recordDir, "trace", "manifest.json"), {
    captureMethod: TRACE_CAPTURE_PLACEHOLDER,
    observableSurfaces: [],
    knownGaps: [],
    rawArtifacts: [],
  });
  await fs.writeFile(path.join(recordDir, "orchestrator-review.md"), `# Orchestrator Review

Draft Behavior, Outcome, and Limits independently from \`input/review-order.md\` before opening \`worker-review.md\`. Complete the comparison afterward.

## Behavior

## Outcome

## Comparison With Worker Review

## Limits
`);
}

async function freezePrimitive(primitive: Primitive, target: string): Promise<void> {
  await fs.mkdir(target, { recursive: true });
  await fs.copyFile(primitive.primitiveFile, path.join(target, "primitive.json"));
  await fs.copyFile(primitive.reviewFile, path.join(target, "review.md"));
  await copyPayloadFiles(primitive.payloadFiles, path.join(target, "payload"));
}

async function copyPayloadFiles(files: PayloadFile[], targetRoot: string): Promise<void> {
  if (files.length === 0) return;
  for (const file of files) {
    const target = path.join(targetRoot, file.relative);
    await fs.mkdir(path.dirname(target), { recursive: true });
    await fs.copyFile(file.source, target);
  }
}

function reviewOrder(primitiveCount: number, variantCount: number): string {
  const lines = [
    "# Review Order",
    "",
    "Read these hidden review inputs in order. Later reviews add combination-specific focus; they do not replace earlier outcome criteria.",
    "",
    "- [Scenario review](scenario/review.md)",
  ];
  for (let index = 0; index < primitiveCount; index += 1) {
    lines.push(`- [Primitive review ${index + 1}](primitives/${ordinal(index)}/review.md)`);
  }
  for (let index = 0; index < variantCount; index += 1) {
    lines.push(`- [Variant review ${index + 1}](variants/${ordinal(index)}/review.md)`);
  }
  lines.push("- [Meta-scenario review](meta/review.md)", "");
  return lines.join("\n");
}

async function readPrimitivePackage(directory: string, label = "Primitive"): Promise<Primitive> {
  await requireDirectory(directory, `${label} package`);
  const primitiveFile = await requiredRegularFile(directory, "primitive.json", label);
  const definition = validatePrimitiveDocument(await readJson(primitiveFile));
  const reviewFile = await requiredRegularFile(directory, "review.md", label);
  const payloadDirectory = await optionalRegularDirectory(directory, "payload", label);
  const payloadFiles = payloadDirectory
    ? await collectPayloadFiles(payloadDirectory, (relative) => validatePrimitivePayloadPath(relative, definition.kind))
    : [];
  if (payloadFiles.length === 0) {
    throw new Error(`${label} ${definition.id} must contain worker-visible payload`);
  }
  if (
    definition.kind === "tool"
    && !payloadFiles.some((file) => !pathSegments(file.relative).some((segment) => segment.toLowerCase() === ".agents"))
  ) {
    throw new Error(`Tool primitive ${definition.id} must contain at least one ordinary non-.agents file`);
  }
  return {
    ...definition,
    directory,
    primitiveFile,
    reviewFile,
    ...(payloadDirectory ? { payloadDirectory } : {}),
    payloadFiles,
  };
}

async function collectPayloadFiles(root: string, validate: (relative: string) => void): Promise<PayloadFile[]> {
  const files: PayloadFile[] = [];
  async function walk(directory: string): Promise<void> {
    const entries = await fs.readdir(directory, { withFileTypes: true });
    entries.sort((left, right) => left.name.localeCompare(right.name, "en"));
    for (const entry of entries) {
      const source = path.join(directory, entry.name);
      const stat = await fs.lstat(source);
      if (stat.isSymbolicLink()) throw new Error(`Benchmark input contains a linked entry: ${source}`);
      if (stat.isDirectory()) {
        if (entry.name.toLowerCase() === ".git") throw new Error(`Benchmark payload may not contain .git: ${source}`);
        await walk(source);
      } else if (stat.isFile()) {
        const relative = path.relative(root, source);
        validate(relative);
        files.push({ source, relative });
      } else {
        throw new Error(`Benchmark input contains an unsupported entry: ${source}`);
      }
    }
  }
  await walk(root);
  files.sort((left, right) => left.relative.localeCompare(right.relative, "en"));
  return files;
}

function validateScenarioPayloadPath(relative: string): void {
  const segments = pathSegments(relative);
  rejectAgentsFile(segments, "Scenario payload");
  if (segments.some((segment) => segment.toLowerCase() === ".agents")) {
    throw new Error(`Scenario payload may not contain .agents content: ${relative}`);
  }
}

function validatePrimitivePayloadPath(relative: string, kind: PrimitiveKind): void {
  const segments = pathSegments(relative);
  rejectAgentsFile(segments, `Primitive ${kind} payload`);
  if (kind === "tool") {
    const agentsIndex = segments.findIndex((segment) => segment.toLowerCase() === ".agents");
    if (
      agentsIndex !== -1
      && (agentsIndex !== 0 || segments.length < 3 || segments[1]?.toLowerCase() !== "workspace")
    ) {
      throw new Error(`Tool primitive .agents files must be under .agents/workspace: ${relative}`);
    }
    return;
  }
  const expected = PRIMITIVE_ROUTES[kind];
  if (segments.length < 3 || segments[0]?.toLowerCase() !== ".agents" || segments[1]?.toLowerCase() !== expected) {
    throw new Error(`Primitive ${kind} payload files must be under .agents/${expected}: ${relative}`);
  }
}

function rejectAgentsFile(segments: string[], label: string): void {
  if (segments.some((segment) => segment.toLowerCase() === ".git")) {
    throw new Error(`${label} may not contain .git`);
  }
  if (segments.at(-1)?.toLowerCase() === "agents.md") {
    throw new Error(`${label} may not contain AGENTS.md`);
  }
}

function pathSegments(relative: string): string[] {
  return relative.split(/[\\/]+/).filter(Boolean);
}

function assertPayloadLayersDoNotCollide(layers: Array<{ label: string; files: PayloadFile[] }>): void {
  const targets = new Map<string, { relative: string; label: string }>();
  for (const layer of layers) {
    for (const file of layer.files) {
      const key = portablePathKey(file.relative);
      const existing = targets.get(key);
      if (existing) {
        throw new Error(`Benchmark payload collision: ${file.relative} from ${layer.label} conflicts with ${existing.relative} from ${existing.label}`);
      }
      const segments = key.split("/");
      for (let index = 1; index < segments.length; index += 1) {
        const parent = segments.slice(0, index).join("/");
        const parentFile = targets.get(parent);
        if (parentFile) {
          throw new Error(`Benchmark payload collision: ${file.relative} from ${layer.label} is below file ${parentFile.relative} from ${parentFile.label}`);
        }
      }
      for (const [otherKey, other] of targets) {
        if (otherKey.startsWith(`${key}/`)) {
          throw new Error(`Benchmark payload collision: ${file.relative} from ${layer.label} is a file parent of ${other.relative} from ${other.label}`);
        }
      }
      targets.set(key, { relative: file.relative, label: layer.label });
    }
  }
}

function portablePathKey(relative: string): string {
  return pathSegments(relative).map((segment) => segment.normalize("NFC").toLowerCase()).join("/");
}

async function mergeFrozenPayload(payloadRoot: string, workspaceRoot: string, label: string): Promise<void> {
  const stat = await lstatIfExists(payloadRoot);
  if (!stat) return;
  if (!stat.isDirectory() || stat.isSymbolicLink()) throw new Error(`Frozen ${label} payload is invalid: ${payloadRoot}`);
  const files = await collectPayloadFiles(payloadRoot, () => {});
  for (const file of files) {
    const target = path.join(workspaceRoot, file.relative);
    if (await exists(target)) throw new Error(`Benchmark payload collision with composed workspace: ${file.relative} from ${label}`);
    for (let parent = path.dirname(target); parent !== workspaceRoot; parent = path.dirname(parent)) {
      const parentStat = await lstatIfExists(parent);
      if (parentStat && !parentStat.isDirectory()) {
        throw new Error(`Benchmark payload collision with composed workspace: ${file.relative} from ${label}`);
      }
    }
  }
  for (const file of files) {
    const target = path.join(workspaceRoot, file.relative);
    await fs.mkdir(path.dirname(target), { recursive: true });
    await fs.copyFile(file.source, target);
  }
}

async function discoverFiles(root: string, filename: string): Promise<string[]> {
  const files: string[] = [];
  async function walk(directory: string): Promise<void> {
    const entries = await fs.readdir(directory, { withFileTypes: true });
    entries.sort((left, right) => left.name.localeCompare(right.name, "en"));
    let manifest: string | undefined;
    for (const entry of entries) {
      const absolute = path.join(directory, entry.name);
      const stat = await fs.lstat(absolute);
      if (stat.isSymbolicLink()) throw new Error(`Benchmark registry contains a linked entry: ${absolute}`);
      if (stat.isFile() && entry.name === filename) manifest = absolute;
      else if (!stat.isFile() && !stat.isDirectory()) throw new Error(`Benchmark registry contains an unsupported entry: ${absolute}`);
    }
    if (manifest) {
      files.push(manifest);
      return;
    }
    for (const entry of entries) {
      if (entry.isDirectory()) await walk(path.join(directory, entry.name));
    }
  }
  const rootStat = await lstatIfExists(root);
  if (!rootStat) return [];
  if (!rootStat.isDirectory() || rootStat.isSymbolicLink()) throw new Error(`Benchmark registry root must be a regular directory: ${root}`);
  await walk(root);
  return files;
}

async function discoverBundledExtensionIds(root: string): Promise<Set<string>> {
  const ids = new Set<string>();
  for (const manifest of await discoverFiles(root, "extension.json")) {
    const value = await readJson(manifest);
    if (value === null || typeof value !== "object" || Array.isArray(value)) throw new Error(`Invalid bundled extension manifest: ${manifest}`);
    const id = (value as Record<string, unknown>).id;
    if (typeof id !== "string" || !isBundledExtensionId(id)) throw new Error(`Invalid bundled extension id in ${manifest}`);
    if (ids.has(id)) throw new Error(`Duplicate bundled extension id: ${id}`);
    ids.add(id);
  }
  return ids;
}

function exactObject(value: unknown, label: string, fields: string[]): Record<string, unknown> {
  if (value === null || typeof value !== "object" || Array.isArray(value)) throw new Error(`${label} must contain an object`);
  const document = value as Record<string, unknown>;
  const unknown = Object.keys(document).filter((key) => !fields.includes(key));
  const missing = fields.filter((key) => !(key in document));
  if (unknown.length > 0) throw new Error(`${label} contains unknown fields: ${unknown.join(", ")}`);
  if (missing.length > 0) throw new Error(`${label} is missing fields: ${missing.join(", ")}`);
  return document;
}

function nonBlankString(value: unknown, label: string): string {
  if (typeof value !== "string" || value.trim().length === 0) throw new Error(`${label} must be a non-empty string`);
  if (value !== value.trim()) throw new Error(`${label} must not start or end with whitespace`);
  return value;
}

function stringArray(value: unknown, label: string, extensionIds: boolean): string[] {
  if (!Array.isArray(value)) throw new Error(`${label} must be an array`);
  const result = value.map((item, index) => {
    const string = nonBlankString(item, `${label}[${index}]`);
    if (extensionIds && !isBundledExtensionId(string)) throw new Error(`${label}[${index}] must be a bundled extension id`);
    return string;
  });
  assertUniqueStrings(result, label);
  return result;
}

function isBundledExtensionId(value: string): boolean {
  return /^[a-z0-9][a-z0-9-]*$/.test(value);
}

function assertUniqueStrings(values: string[], label: string): void {
  const seen = new Set<string>();
  for (const value of values) {
    if (seen.has(value)) throw new Error(`${label} must not contain duplicates: ${value}`);
    seen.add(value);
  }
}

function assertNoDuplicateIds(values: Array<{ id: string }>, label: string): void {
  const seen = new Set<string>();
  for (const value of values) {
    if (seen.has(value.id)) throw new Error(`${label} must not contain duplicates: ${value.id}`);
    seen.add(value.id);
  }
}

function uniqueById<T extends { id: string }>(values: T[], label: string): T[] {
  values.sort((left, right) => left.id.localeCompare(right.id, "en"));
  const seen = new Set<string>();
  for (const value of values) {
    if (seen.has(value.id)) throw new Error(`Duplicate ${label} id: ${value.id}`);
    seen.add(value.id);
  }
  return values;
}

function ordinal(index: number): string {
  return String(index).padStart(2, "0");
}

async function readJson(file: string): Promise<unknown> {
  try {
    return JSON.parse(await fs.readFile(file, "utf8"));
  } catch (error) {
    throw new Error(`Unable to read ${file}: ${error instanceof Error ? error.message : String(error)}`);
  }
}

async function requiredRegularFile(directory: string, name: string, label: string): Promise<string> {
  const file = path.join(directory, name);
  const stat = await lstatIfExists(file);
  if (!stat?.isFile() || stat.isSymbolicLink()) throw new Error(`${label} requires regular sibling ${name}: ${directory}`);
  return file;
}

async function optionalRegularFile(directory: string, name: string, label: string): Promise<string | undefined> {
  const file = path.join(directory, name);
  const stat = await lstatIfExists(file);
  if (!stat) return undefined;
  if (!stat.isFile() || stat.isSymbolicLink()) throw new Error(`${label} ${name} must be a regular file: ${directory}`);
  return file;
}

async function optionalRegularDirectory(directory: string, name: string, label: string): Promise<string | undefined> {
  const value = path.join(directory, name);
  const stat = await lstatIfExists(value);
  if (!stat) return undefined;
  if (!stat.isDirectory() || stat.isSymbolicLink()) throw new Error(`${label} ${name} must be a regular directory: ${directory}`);
  return value;
}

async function requiredExternalFile(file: string, label: string): Promise<string> {
  const stat = await lstatIfExists(file);
  if (!stat?.isFile() || stat.isSymbolicLink()) throw new Error(`${label} must be an existing regular file, not a link: ${file}`);
  return file;
}

async function requireDirectory(directory: string, label: string): Promise<void> {
  const stat = await lstatIfExists(directory);
  if (!stat?.isDirectory() || stat.isSymbolicLink()) throw new Error(`${label} is missing or linked: ${directory}`);
}

async function lstatIfExists(value: string) {
  try {
    return await fs.lstat(value);
  } catch (error) {
    if (isMissing(error)) return null;
    throw error;
  }
}

async function exists(value: string): Promise<boolean> {
  return (await lstatIfExists(value)) !== null;
}

function isMissing(error: unknown): boolean {
  return error instanceof Error && "code" in error && (error as NodeJS.ErrnoException).code === "ENOENT";
}

async function readSourceState(repoRoot: string): Promise<RunMetadata["source"]> {
  const [revision, status] = await Promise.all([
    runProcess("git", ["rev-parse", "HEAD"], repoRoot),
    runProcess("git", ["status", "--short"], repoRoot),
  ]);
  const lines = status.exitCode === 0 ? status.stdout.trimEnd().split(/\r?\n/).filter(Boolean) : null;
  return {
    revision: revision.exitCode === 0 ? revision.stdout.trim() : null,
    dirty: lines === null || lines.length > 0,
    status: lines,
  };
}

async function readRunMetadata(file: string): Promise<RunMetadata> {
  const value = await readJson(file) as Partial<RunMetadata>;
  if (
    typeof value.metaScenarioId !== "string"
    || typeof value.scenarioId !== "string"
    || !Array.isArray(value.primitiveIds)
    || !Array.isArray(value.variantIds)
    || !Array.isArray(value.extensions)
    || typeof value.workerPromptSha256 !== "string"
    || !/^[0-9a-f]{64}$/.test(value.workerPromptSha256)
    || typeof value.baselineTree !== "string"
    || !/^[0-9a-f]{40,64}$/.test(value.baselineTree)
    || typeof value.preparedAt !== "string"
    || typeof value.baselineCommit !== "string"
  ) {
    throw new Error(`Invalid run metadata: ${file}`);
  }
  return value as RunMetadata;
}

async function sha256File(file: string): Promise<string> {
  return createHash("sha256").update(await fs.readFile(file)).digest("hex");
}

async function assertRunsRootOutsideRepository(repoRoot: string, runsRoot: string): Promise<void> {
  const [realRepoRoot, projectedRunsRoot] = await Promise.all([
    fs.realpath(repoRoot),
    projectPathThroughExistingAncestor(runsRoot),
  ]);
  if (sameOrInside(realRepoRoot, projectedRunsRoot) || sameOrInside(projectedRunsRoot, realRepoRoot)) {
    throw new Error(`Runs root must be outside the source repository: ${runsRoot}`);
  }
}

function sameOrInside(parent: string, candidate: string): boolean {
  const relative = path.relative(parent, candidate);
  return relative === "" || (relative !== ".." && !relative.startsWith(`..${path.sep}`) && !path.isAbsolute(relative));
}

async function projectPathThroughExistingAncestor(value: string): Promise<string> {
  let current = path.resolve(value);
  const suffix: string[] = [];
  while (!(await lstatIfExists(current))) {
    const parent = path.dirname(current);
    if (parent === current) throw new Error(`Unable to resolve an existing ancestor for runs root: ${value}`);
    suffix.unshift(path.basename(current));
    current = parent;
  }
  return path.join(await fs.realpath(current), ...suffix);
}

async function requireWorkerReview(file: string): Promise<void> {
  const review = await fs.readFile(file, "utf8");
  if (!review.trim() || review.includes(WORKER_REVIEW_PLACEHOLDER.trim())) {
    throw new Error(`${path.basename(file)} is incomplete; replace its placeholder with the worker's verbatim response`);
  }
}

async function requireTraceManifest(file: string): Promise<void> {
  const value = await readJson(file);
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new Error("trace/manifest.json must contain an object");
  }
  const manifest = value as Record<string, unknown>;
  if (
    typeof manifest.captureMethod !== "string"
    || manifest.captureMethod.trim().length === 0
    || manifest.captureMethod === TRACE_CAPTURE_PLACEHOLDER
  ) {
    throw new Error("trace/manifest.json is incomplete; describe the trace capture or its unavailable surface");
  }
  for (const field of ["observableSurfaces", "knownGaps", "rawArtifacts"]) {
    if (!Array.isArray(manifest[field]) || !(manifest[field] as unknown[]).every((item) => typeof item === "string")) {
      throw new Error(`trace/manifest.json ${field} must be an array of strings`);
    }
  }
}

async function requireCompletedReview(file: string, sections: string[]): Promise<void> {
  const review = await fs.readFile(file, "utf8");
  for (const section of sections) {
    const heading = new RegExp(`^## ${escapeRegex(section)}\\s*$`, "m").exec(review);
    if (!heading) throw new Error(`${path.basename(file)} is incomplete; missing ${section}`);
    const remainder = review.slice((heading.index ?? 0) + heading[0].length);
    const nextHeading = remainder.search(/^## /m);
    const body = (nextHeading < 0 ? remainder : remainder.slice(0, nextHeading)).trim();
    if (!body) throw new Error(`${path.basename(file)} is incomplete; ${section} is empty`);
  }
}

function escapeRegex(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function doctorSummary(result: ProcessResult): FinishedRun["doctor"] {
  try {
    const report = JSON.parse(result.stdout) as { errors?: unknown; warnings?: unknown };
    return {
      exitCode: result.exitCode,
      errors: typeof report.errors === "number" ? report.errors : null,
      warnings: typeof report.warnings === "number" ? report.warnings : null,
    };
  } catch {
    return { exitCode: result.exitCode, errors: null, warnings: null };
  }
}

async function openForge(repoRoot: string, args: string[], label: string): Promise<ProcessResult> {
  const result = await runOpenForge(repoRoot, args);
  requireSuccess(result, label);
  return result;
}

async function runOpenForge(repoRoot: string, args: string[]): Promise<ProcessResult> {
  return runProcess(process.execPath, [path.join(repoRoot, "src", "cli", "cli.ts"), ...args], repoRoot);
}

async function git(directory: string, ...args: string[]): Promise<ProcessResult> {
  const result = await runProcess("git", args, directory);
  requireSuccess(result, `git ${args[0] ?? "command"}`);
  return result;
}

function requireSuccess(result: ProcessResult, label: string): void {
  if (result.exitCode === 0) return;
  const output = [result.stdout.trim(), result.stderr.trim()].filter(Boolean).join("\n");
  throw new Error(`${label} failed with exit code ${result.exitCode}${output ? `:\n${output}` : ""}`);
}

function runProcess(command: string, args: string[], cwd: string): Promise<ProcessResult> {
  return new Promise((resolve, reject) => {
    const child = spawn(command, args, { cwd, windowsHide: true, stdio: ["ignore", "pipe", "pipe"] });
    const stdout: Buffer[] = [];
    const stderr: Buffer[] = [];
    child.stdout.on("data", (chunk) => stdout.push(Buffer.from(chunk)));
    child.stderr.on("data", (chunk) => stderr.push(Buffer.from(chunk)));
    child.on("error", reject);
    child.on("close", (exitCode) => resolve({
      exitCode: exitCode ?? 1,
      stdout: Buffer.concat(stdout).toString("utf8"),
      stderr: Buffer.concat(stderr).toString("utf8"),
    }));
  });
}

async function writeJson(file: string, value: unknown, exclusive = false): Promise<void> {
  await fs.writeFile(file, `${JSON.stringify(value, null, 2)}\n`, exclusive ? { flag: "wx" } : undefined);
}

export async function main(args = process.argv.slice(2)): Promise<void> {
  const [command, ...rest] = args;
  if (command === "list") {
    if (rest.length > 1) throw new Error("Usage: runner.ts list [meta-scenarios|scenarios|primitives]");
    const registry = rest[0] ?? "meta-scenarios";
    if (registry === "meta-scenarios") {
      for (const item of await discoverMetaScenarios()) console.log(item.id);
      return;
    }
    if (registry === "scenarios") {
      for (const item of await discoverScenarios()) console.log(item.id);
      return;
    }
    if (registry === "primitives") {
      for (const item of await discoverPrimitives()) console.log(item.id);
      return;
    }
    throw new Error("Usage: runner.ts list [meta-scenarios|scenarios|primitives]");
  }
  if (command === "prepare") {
    const metaScenarioId = rest[0];
    const usage = "Usage: runner.ts prepare <meta-scenario-id> --runs-root <external-dir> [--orchestrator <file>] [--variant <primitive-package-dir>]... [--extension <bundled-id>]...";
    if (!metaScenarioId) throw new Error(usage);
    let runsRoot: string | undefined;
    let orchestratorFile: string | undefined;
    const variantDirectories: string[] = [];
    const extraExtensions: string[] = [];
    for (let index = 1; index < rest.length; index += 1) {
      const value = rest[index];
      if (value === "--runs-root" && rest[index + 1]) runsRoot = path.resolve(rest[++index]);
      else if (value.startsWith("--runs-root=") && value.length > "--runs-root=".length) runsRoot = path.resolve(value.slice("--runs-root=".length));
      else if (value === "--orchestrator" && rest[index + 1]) orchestratorFile = path.resolve(rest[++index]);
      else if (value.startsWith("--orchestrator=") && value.length > "--orchestrator=".length) orchestratorFile = path.resolve(value.slice("--orchestrator=".length));
      else if (value === "--variant" && rest[index + 1]) variantDirectories.push(path.resolve(rest[++index]));
      else if (value.startsWith("--variant=") && value.length > "--variant=".length) variantDirectories.push(path.resolve(value.slice("--variant=".length)));
      else if (value === "--extension" && rest[index + 1]) extraExtensions.push(rest[++index]);
      else if (value.startsWith("--extension=") && value.length > "--extension=".length) extraExtensions.push(value.slice("--extension=".length));
      else throw new Error(usage);
    }
    if (!runsRoot) throw new Error(usage);
    console.log(JSON.stringify(await prepareMetaScenario({
      metaScenarioId,
      runsRoot,
      orchestratorFile,
      variantDirectories,
      extraExtensions,
    }), null, 2));
    return;
  }
  if (command === "finish" && rest.length === 1) {
    console.log(JSON.stringify(await finishRun(rest[0]), null, 2));
    return;
  }
  throw new Error("Usage: runner.ts <list|prepare|finish>");
}

if (import.meta.main) {
  await main().catch((error) => {
    console.error(`benchmark runner: ${error instanceof Error ? error.message : String(error)}`);
    process.exitCode = 1;
  });
}

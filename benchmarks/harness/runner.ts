#!/usr/bin/env bun

import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import { createHash, createHmac, randomBytes, randomUUID, timingSafeEqual } from "node:crypto";
import { fileURLToPath } from "node:url";

const SCHEMA_VERSION = 1;
const RUN_ID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const SAFE_ID_PATTERN = /^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$/;
const SEED_RATING_PATTERN = /^seed-[A-Za-z0-9._-]{1,122}$/;
const SHA256_PATTERN = /^[0-9a-f]{64}$/;
const FINAL_STATES = new Set(["complete", "invalid", "aborted"]);
const RESERVED_COMPONENT_SEGMENTS = new Set([".git", "node_modules", "orchestrator"]);
const WINDOWS_RESERVED_BASENAMES = /^(?:con|prn|aux|nul|com[1-9]|lpt[1-9])$/i;
const HARNESS_SOURCE_PATH = fileURLToPath(import.meta.url);
const CORE_RATING_IDS = [
  "directive-compliance",
  "memory-growth",
  "routing-behavior",
  "communication",
  "product-fidelity",
] as const;

type JsonValue = null | boolean | number | string | JsonValue[] | { [key: string]: JsonValue };
type JsonObject = { [key: string]: JsonValue };

export interface ComponentSpec {
  name: string;
  source: string;
  destination: string;
}

export interface OverrideSpec {
  path: string;
  from: string;
  to: string;
}

export interface ArtifactSpec {
  path: string;
}

export interface CliArtifactSpec extends ArtifactSpec {
  invocation: string[];
}

export interface RunSpec {
  schemaVersion: 1;
  experimentId: string;
  caseId: string;
  armId: string;
  replicate: number;
  evidenceClass: "engineering-smoke" | "pilot" | "confirmatory";
  sourceRepo: string;
  components: ComponentSpec[];
  overrides: OverrideSpec[];
  inputs: {
    cli: CliArtifactSpec;
    prompt: ArtifactSpec;
    rubric: ArtifactSpec;
  };
  model: {
    provider: string;
    id: string;
    revision: string;
    settings: JsonObject;
  };
  runtime: {
    name: string;
    version: string;
    adapter: string;
    settings: JsonObject;
  };
  isolation: {
    freshContext: "verified" | "requested" | "unverified";
    inheritedContext: "none" | "unknown" | "present";
    workerReceivesOnlyWorkspace: boolean;
    network: "disabled" | "enabled" | "unknown";
    filesystem: "workspace-only" | "broader" | "unknown";
  };
  controls: {
    planned: boolean;
    randomized: boolean;
    declaredTreatmentOnly: boolean;
    replicatePlanned: boolean;
    planId: string | null;
    planSha256: string | null;
  };
}

export interface EvaluationInput {
  schemaVersion: 1;
  status: "complete" | "invalid" | "aborted";
  objectiveAssertions: Array<{
    id: string;
    status: "pass" | "fail" | "error";
    command: string | null;
    exitCode: number | null;
    durationMs: number | null;
    evidenceTraceNames: string[];
    note: string | null;
  }>;
  subjectiveRatings: Array<{
    id: string;
    score: number;
    maxScore: number;
    rationale: string;
  }>;
  traces: Array<{
    name: string;
    path: string;
  }>;
  notes: string[];
}

export interface TreeEntry {
  path: string;
  type: "file";
  sha256: string;
  bytes: number;
  mode: number;
}

export interface TreeManifest {
  schemaVersion: 1;
  digest: string;
  entries: TreeEntry[];
}

interface SourceEntry {
  component: string;
  sourcePath: string;
  sourceRelativePath: string;
  targetPath: string;
  snapshotPath: string;
  sha256: string;
  bytes: number;
  mode: number;
}

interface ArtifactRecord {
  sourcePath: string;
  snapshotPath: string;
  sha256: string;
  bytes: number;
  mode: number;
  invocation?: string[];
}

interface CompositionPlan {
  allEntries: SourceEntry[];
  visibleEntries: SourceEntry[];
  consumedOverrides: OverrideSpec[];
}

interface GitCapture {
  head: string;
  tree: string;
  status: string;
  statusSha256: string;
  stagedDiff: Buffer;
  stagedDiffSha256: string;
  unstagedDiff: Buffer;
  unstagedDiffSha256: string;
  clean: boolean;
}

interface WorkspaceGitCapture extends GitCapture {
  baselineDiff: Buffer;
  baselineDiffSha256: string;
  baselineToHeadDiff: Buffer;
  baselineToHeadDiffSha256: string;
}

interface InvocationCapture {
  command: string[];
  exitCode: number;
  durationMs: number;
  stdout: Buffer;
  stdoutSha256: string;
  stderr: Buffer;
  stderrSha256: string;
  runtimeExecutable: {
    path: string;
    sha256: string;
    bytes: number;
  };
  commandExecutable: {
    path: string;
    sha256: string;
    bytes: number;
  };
}

interface ExecutableFingerprint {
  path: string;
  sha256: string;
  bytes: number;
}

interface RunState {
  schemaVersion: 1;
  runId: string;
  state: "prepared" | "complete" | "invalid" | "aborted";
  updatedAt: string;
}

export interface PreparedRun {
  runId: string;
  runDir: string;
  workspaceDir: string;
  evidenceDir: string;
  workerPromptPath: string;
  ownerToken: string;
  preparedRootSha256: string;
}

export interface BenchmarkResult extends JsonObject {
  schemaVersion: 1;
  runId: string;
  experimentId: string;
  caseId: string;
  armId: string;
  replicate: number;
  evidenceClass: string;
  status: "complete" | "invalid" | "aborted";
  preparedAt: string;
  finalizedAt: string;
  model: JsonObject;
  runtime: JsonObject;
  isolation: JsonObject;
  controls: JsonObject;
  provenance: JsonObject;
  hashes: JsonObject;
  objectiveAssertions: JsonValue[];
  subjectiveRatings: JsonValue[];
  traces: JsonValue[];
  notes: JsonValue[];
  contamination: JsonObject;
  eligibility: JsonObject;
}

export interface ValidationResult {
  ok: boolean;
  errors: string[];
}

export function stableStringify(value: JsonValue | unknown): string {
  return `${JSON.stringify(canonicalize(value), null, 2)}\n`;
}

function canonicalize(value: unknown): JsonValue {
  if (value === null || typeof value === "string" || typeof value === "boolean") return value;
  if (typeof value === "number") {
    if (!Number.isFinite(value)) throw new Error("Canonical JSON does not permit non-finite numbers");
    return value;
  }
  if (Array.isArray(value)) return value.map(canonicalize);
  if (typeof value === "object") {
    const result: JsonObject = {};
    for (const key of Object.keys(value as Record<string, unknown>).sort()) {
      const child = (value as Record<string, unknown>)[key];
      if (child === undefined) throw new Error(`Canonical JSON does not permit undefined at ${key}`);
      result[key] = canonicalize(child);
    }
    return result;
  }
  throw new Error(`Canonical JSON does not permit ${typeof value}`);
}

export function sha256(data: string | Buffer): string {
  return createHash("sha256").update(data).digest("hex");
}

function canonicalHash(value: unknown): string {
  return sha256(stableStringify(value));
}

async function readJson(file: string): Promise<unknown> {
  return JSON.parse(await fs.readFile(file, "utf8"));
}

async function writeCanonicalJson(file: string, value: unknown, exclusive = false): Promise<void> {
  await fs.mkdir(path.dirname(file), { recursive: true });
  if (exclusive) {
    const handle = await fs.open(file, "wx");
    try {
      await handle.writeFile(stableStringify(value), "utf8");
    } finally {
      await handle.close();
    }
    return;
  }
  await atomicWrite(file, stableStringify(value));
}

async function atomicWrite(file: string, content: string | Buffer): Promise<void> {
  await fs.mkdir(path.dirname(file), { recursive: true });
  const temporary = `${file}.${process.pid}.${randomUUID()}.tmp`;
  await fs.writeFile(temporary, content, { flag: "wx" });
  try {
    await fs.rename(temporary, file);
  } catch (error) {
    await fs.rm(temporary, { force: true });
    throw error;
  }
}

function objectAt(value: unknown, label: string): Record<string, unknown> {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    throw new Error(`${label} must be an object`);
  }
  return value as Record<string, unknown>;
}

function noUnknown(value: Record<string, unknown>, allowed: string[], label: string): void {
  const unexpected = Object.keys(value).filter((key) => !allowed.includes(key));
  if (unexpected.length > 0) throw new Error(`${label} contains unknown fields: ${unexpected.join(", ")}`);
}

function stringAt(value: unknown, label: string, options: { nonempty?: boolean } = {}): string {
  if (typeof value !== "string") throw new Error(`${label} must be a string`);
  if (options.nonempty !== false && value.trim().length === 0) throw new Error(`${label} must not be empty`);
  return value;
}

function nullableStringAt(value: unknown, label: string): string | null {
  return value === null ? null : stringAt(value, label);
}

function booleanAt(value: unknown, label: string): boolean {
  if (typeof value !== "boolean") throw new Error(`${label} must be a boolean`);
  return value;
}

function integerAt(value: unknown, label: string, minimum = Number.MIN_SAFE_INTEGER): number {
  if (!Number.isSafeInteger(value) || (value as number) < minimum) {
    throw new Error(`${label} must be a safe integer >= ${minimum}`);
  }
  return value as number;
}

function numberAt(value: unknown, label: string, minimum = 0): number {
  if (typeof value !== "number" || !Number.isFinite(value) || value < minimum) {
    throw new Error(`${label} must be a finite number >= ${minimum}`);
  }
  return value;
}

function arrayAt(value: unknown, label: string): unknown[] {
  if (!Array.isArray(value)) throw new Error(`${label} must be an array`);
  return value;
}

function enumAt<T extends string>(value: unknown, allowed: readonly T[], label: string): T {
  if (typeof value !== "string" || !allowed.includes(value as T)) {
    throw new Error(`${label} must be one of: ${allowed.join(", ")}`);
  }
  return value as T;
}

function idAt(value: unknown, label: string): string {
  const result = stringAt(value, label);
  if (!SAFE_ID_PATTERN.test(result)) throw new Error(`${label} is not a safe identifier`);
  return result;
}

function jsonObjectAt(value: unknown, label: string): JsonObject {
  const object = objectAt(value, label);
  return canonicalize(object) as JsonObject;
}

function resolveInputPath(specDir: string, value: unknown, label: string): string {
  return path.resolve(specDir, stringAt(value, label));
}

function normalizeTarget(value: string, label: string, allowRoot = true): string {
  if (value.includes("\0")) throw new Error(`${label} contains a NUL byte`);
  if (path.isAbsolute(value)) throw new Error(`${label} must be relative`);
  const portable = value.normalize("NFC").replaceAll("\\", "/");
  const normalized = path.posix.normalize(portable).replace(/^\.\//, "");
  if (normalized === ".." || normalized.startsWith("../") || normalized.includes("/../")) {
    throw new Error(`${label} escapes the workspace`);
  }
  if (normalized === "." || normalized === "") {
    if (!allowRoot) throw new Error(`${label} must name a file path`);
    return "";
  }
  for (const segment of normalized.split("/")) {
    if (segment.length === 0) continue;
    const baseName = segment.split(".", 1)[0] ?? "";
    if (/[<>:"|?*\u0000-\u001f]/.test(segment) || /[ .]$/.test(segment) || WINDOWS_RESERVED_BASENAMES.test(baseName)) {
      throw new Error(`${label} contains a path segment that is not portable across Windows: ${segment}`);
    }
  }
  return normalized;
}

function canonicalTextCompare(left: string, right: string): number {
  return Buffer.compare(Buffer.from(left.normalize("NFC"), "utf8"), Buffer.from(right.normalize("NFC"), "utf8"));
}

function sameOrInside(parent: string, candidate: string): boolean {
  const relative = path.relative(path.resolve(parent), path.resolve(candidate));
  return relative === "" || (!relative.startsWith("..") && !path.isAbsolute(relative));
}

function sameResolvedPath(left: string, right: string): boolean {
  return sameOrInside(left, right) && sameOrInside(right, left);
}

async function assertPlainDirectory(directory: string, label: string): Promise<string> {
  const stat = await fs.lstat(directory).catch(() => null);
  if (!stat || stat.isSymbolicLink() || !stat.isDirectory()) {
    throw new Error(`${label} must be an existing non-linked directory: ${directory}`);
  }
  return fs.realpath(directory);
}

async function assertDirectDirectoryChild(parent: string, child: string, expectedName: string, label: string): Promise<string> {
  const realParent = await assertPlainDirectory(parent, `${label} parent`);
  const realChild = await assertPlainDirectory(child, label);
  if (!sameResolvedPath(path.dirname(realChild), realParent) || !sameResolvedPath(realChild, path.join(realParent, expectedName))) {
    throw new Error(`${label} must remain the direct ${expectedName} child of ${realParent}`);
  }
  return realChild;
}

async function assertRunDirectoryLayout(runDir: string): Promise<{ runDir: string; evidenceDir: string; workspaceDir: string }> {
  const realRunDir = await assertPlainDirectory(runDir, "Run directory");
  const evidenceDir = await assertDirectDirectoryChild(realRunDir, path.join(runDir, "evidence"), "evidence", "Evidence directory");
  const workspaceDir = await assertDirectDirectoryChild(realRunDir, path.join(runDir, "workspace"), "workspace", "Workspace directory");
  return { runDir: realRunDir, evidenceDir, workspaceDir };
}

function assertExternalRunsRoot(runsRoot: string, sourceRepo: string): void {
  if (!path.isAbsolute(runsRoot)) throw new Error("--runs-root must be an absolute path");
  if (sameOrInside(sourceRepo, runsRoot) || sameOrInside(runsRoot, sourceRepo)) {
    throw new Error("--runs-root must not overlap sourceRepo");
  }
}

async function projectPathThroughExistingAncestor(candidate: string): Promise<string> {
  let ancestor = path.resolve(candidate);
  const missingSegments: string[] = [];
  while (true) {
    try {
      await fs.lstat(ancestor);
      const realAncestor = await fs.realpath(ancestor);
      return path.resolve(realAncestor, ...missingSegments.reverse());
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code !== "ENOENT") throw error;
      const parent = path.dirname(ancestor);
      if (parent === ancestor) throw new Error(`Unable to resolve an existing ancestor for path: ${candidate}`);
      missingSegments.push(path.basename(ancestor));
      ancestor = parent;
    }
  }
}

function assertOutsideRoots(candidate: string, roots: Array<{ label: string; path: string }>, label: string): void {
  for (const root of roots) {
    if (sameOrInside(root.path, candidate)) throw new Error(`${label} must be external to ${root.label}: ${candidate}`);
  }
}

export function validateRunSpecDocument(value: unknown, specDir = process.cwd()): RunSpec {
  const root = objectAt(value, "run spec");
  noUnknown(root, [
    "schemaVersion", "experimentId", "caseId", "armId", "replicate", "evidenceClass", "sourceRepo",
    "components", "overrides", "inputs", "model", "runtime", "isolation", "controls",
  ], "run spec");
  if (integerAt(root.schemaVersion, "run spec.schemaVersion") !== SCHEMA_VERSION) {
    throw new Error(`run spec.schemaVersion must be ${SCHEMA_VERSION}`);
  }

  const components = arrayAt(root.components, "run spec.components").map((item, index): ComponentSpec => {
    const component = objectAt(item, `run spec.components[${index}]`);
    noUnknown(component, ["name", "source", "destination"], `run spec.components[${index}]`);
    return {
      name: idAt(component.name, `run spec.components[${index}].name`),
      source: resolveInputPath(specDir, component.source, `run spec.components[${index}].source`),
      destination: normalizeTarget(stringAt(component.destination, `run spec.components[${index}].destination`, { nonempty: false }), `run spec.components[${index}].destination`),
    };
  });
  if (components.length === 0) throw new Error("run spec.components must not be empty");
  const names = new Set<string>();
  for (const component of components) {
    if (names.has(component.name)) throw new Error(`Duplicate component name: ${component.name}`);
    names.add(component.name);
  }

  const overrides = arrayAt(root.overrides, "run spec.overrides").map((item, index): OverrideSpec => {
    const override = objectAt(item, `run spec.overrides[${index}]`);
    noUnknown(override, ["path", "from", "to"], `run spec.overrides[${index}]`);
    const result = {
      path: normalizeTarget(stringAt(override.path, `run spec.overrides[${index}].path`), `run spec.overrides[${index}].path`, false),
      from: idAt(override.from, `run spec.overrides[${index}].from`),
      to: idAt(override.to, `run spec.overrides[${index}].to`),
    };
    if (result.from === result.to) throw new Error(`run spec.overrides[${index}] must change component ownership`);
    if (!names.has(result.from) || !names.has(result.to)) throw new Error(`run spec.overrides[${index}] names an unknown component`);
    return result;
  });
  const overrideKeys = new Set<string>();
  for (const override of overrides) {
    const key = `${override.path.toLowerCase()}\0${override.from}\0${override.to}`;
    if (overrideKeys.has(key)) throw new Error(`Duplicate override declaration for ${override.path}`);
    overrideKeys.add(key);
  }

  const inputs = objectAt(root.inputs, "run spec.inputs");
  noUnknown(inputs, ["cli", "prompt", "rubric"], "run spec.inputs");
  const cli = objectAt(inputs.cli, "run spec.inputs.cli");
  noUnknown(cli, ["path", "invocation"], "run spec.inputs.cli");
  const prompt = objectAt(inputs.prompt, "run spec.inputs.prompt");
  noUnknown(prompt, ["path"], "run spec.inputs.prompt");
  const rubric = objectAt(inputs.rubric, "run spec.inputs.rubric");
  noUnknown(rubric, ["path"], "run spec.inputs.rubric");

  const model = objectAt(root.model, "run spec.model");
  noUnknown(model, ["provider", "id", "revision", "settings"], "run spec.model");
  const runtime = objectAt(root.runtime, "run spec.runtime");
  noUnknown(runtime, ["name", "version", "adapter", "settings"], "run spec.runtime");
  const isolation = objectAt(root.isolation, "run spec.isolation");
  noUnknown(isolation, ["freshContext", "inheritedContext", "workerReceivesOnlyWorkspace", "network", "filesystem"], "run spec.isolation");
  const controls = objectAt(root.controls, "run spec.controls");
  noUnknown(controls, ["planned", "randomized", "declaredTreatmentOnly", "replicatePlanned", "planId", "planSha256"], "run spec.controls");
  const planSha256 = nullableStringAt(controls.planSha256, "run spec.controls.planSha256");
  if (planSha256 !== null && !SHA256_PATTERN.test(planSha256)) throw new Error("run spec.controls.planSha256 must be a SHA-256 hex digest or null");

  const invocation = arrayAt(cli.invocation, "run spec.inputs.cli.invocation").map((item, index) => stringAt(item, `run spec.inputs.cli.invocation[${index}]`, { nonempty: false }));
  if (!invocation.includes("{cli}") || !invocation.includes("{workspace}")) {
    throw new Error("run spec.inputs.cli.invocation must contain standalone {cli} and {workspace} arguments");
  }
  if (!(invocation[0] === "{cli}" || (invocation[0] === "{runtime}" && invocation[1] === "{cli}"))) {
    throw new Error("run spec.inputs.cli.invocation must execute {cli} directly or as the first script passed to {runtime}");
  }

  const result: RunSpec = {
    schemaVersion: 1,
    experimentId: idAt(root.experimentId, "run spec.experimentId"),
    caseId: idAt(root.caseId, "run spec.caseId"),
    armId: idAt(root.armId, "run spec.armId"),
    replicate: integerAt(root.replicate, "run spec.replicate", 0),
    evidenceClass: enumAt(root.evidenceClass, ["engineering-smoke", "pilot", "confirmatory"] as const, "run spec.evidenceClass"),
    sourceRepo: resolveInputPath(specDir, root.sourceRepo, "run spec.sourceRepo"),
    components,
    overrides,
    inputs: {
      cli: {
        path: resolveInputPath(specDir, cli.path, "run spec.inputs.cli.path"),
        invocation,
      },
      prompt: { path: resolveInputPath(specDir, prompt.path, "run spec.inputs.prompt.path") },
      rubric: { path: resolveInputPath(specDir, rubric.path, "run spec.inputs.rubric.path") },
    },
    model: {
      provider: stringAt(model.provider, "run spec.model.provider"),
      id: stringAt(model.id, "run spec.model.id"),
      revision: stringAt(model.revision, "run spec.model.revision"),
      settings: jsonObjectAt(model.settings, "run spec.model.settings"),
    },
    runtime: {
      name: stringAt(runtime.name, "run spec.runtime.name"),
      version: stringAt(runtime.version, "run spec.runtime.version"),
      adapter: stringAt(runtime.adapter, "run spec.runtime.adapter"),
      settings: jsonObjectAt(runtime.settings, "run spec.runtime.settings"),
    },
    isolation: {
      freshContext: enumAt(isolation.freshContext, ["verified", "requested", "unverified"] as const, "run spec.isolation.freshContext"),
      inheritedContext: enumAt(isolation.inheritedContext, ["none", "unknown", "present"] as const, "run spec.isolation.inheritedContext"),
      workerReceivesOnlyWorkspace: booleanAt(isolation.workerReceivesOnlyWorkspace, "run spec.isolation.workerReceivesOnlyWorkspace"),
      network: enumAt(isolation.network, ["disabled", "enabled", "unknown"] as const, "run spec.isolation.network"),
      filesystem: enumAt(isolation.filesystem, ["workspace-only", "broader", "unknown"] as const, "run spec.isolation.filesystem"),
    },
    controls: {
      planned: booleanAt(controls.planned, "run spec.controls.planned"),
      randomized: booleanAt(controls.randomized, "run spec.controls.randomized"),
      declaredTreatmentOnly: booleanAt(controls.declaredTreatmentOnly, "run spec.controls.declaredTreatmentOnly"),
      replicatePlanned: booleanAt(controls.replicatePlanned, "run spec.controls.replicatePlanned"),
      planId: nullableStringAt(controls.planId, "run spec.controls.planId"),
      planSha256,
    },
  };
  canonicalize(result);
  return result;
}

export function validateEvaluationDocument(value: unknown, evaluationDir = process.cwd()): EvaluationInput {
  const root = objectAt(value, "evaluation");
  noUnknown(root, ["schemaVersion", "status", "objectiveAssertions", "subjectiveRatings", "traces", "notes"], "evaluation");
  if (integerAt(root.schemaVersion, "evaluation.schemaVersion") !== SCHEMA_VERSION) throw new Error(`evaluation.schemaVersion must be ${SCHEMA_VERSION}`);

  const objectiveAssertions = arrayAt(root.objectiveAssertions, "evaluation.objectiveAssertions").map((item, index) => {
    const assertion = objectAt(item, `evaluation.objectiveAssertions[${index}]`);
    noUnknown(assertion, ["id", "status", "command", "exitCode", "durationMs", "evidenceTraceNames", "note"], `evaluation.objectiveAssertions[${index}]`);
    const exitCode = assertion.exitCode === null ? null : integerAt(assertion.exitCode, `evaluation.objectiveAssertions[${index}].exitCode`);
    const durationMs = assertion.durationMs === null ? null : numberAt(assertion.durationMs, `evaluation.objectiveAssertions[${index}].durationMs`);
    return {
      id: idAt(assertion.id, `evaluation.objectiveAssertions[${index}].id`),
      status: enumAt(assertion.status, ["pass", "fail", "error"] as const, `evaluation.objectiveAssertions[${index}].status`),
      command: nullableStringAt(assertion.command, `evaluation.objectiveAssertions[${index}].command`),
      exitCode,
      durationMs,
      evidenceTraceNames: arrayAt(assertion.evidenceTraceNames, `evaluation.objectiveAssertions[${index}].evidenceTraceNames`).map((name, traceIndex) => idAt(name, `evaluation.objectiveAssertions[${index}].evidenceTraceNames[${traceIndex}]`)),
      note: nullableStringAt(assertion.note, `evaluation.objectiveAssertions[${index}].note`),
    };
  });
  const assertionIds = new Set<string>();
  for (const assertion of objectiveAssertions) {
    if (assertionIds.has(assertion.id)) throw new Error(`Duplicate objective assertion id: ${assertion.id}`);
    assertionIds.add(assertion.id);
  }

  const subjectiveRatings = arrayAt(root.subjectiveRatings, "evaluation.subjectiveRatings").map((item, index) => {
    const rating = objectAt(item, `evaluation.subjectiveRatings[${index}]`);
    noUnknown(rating, ["id", "score", "maxScore", "rationale"], `evaluation.subjectiveRatings[${index}]`);
    const maximum = numberAt(rating.maxScore, `evaluation.subjectiveRatings[${index}].maxScore`, Number.MIN_VALUE);
    const score = numberAt(rating.score, `evaluation.subjectiveRatings[${index}].score`);
    if (maximum !== 2) throw new Error(`evaluation.subjectiveRatings[${index}].maxScore must be exactly 2`);
    if (score > 2) throw new Error(`evaluation.subjectiveRatings[${index}].score must be between 0 and 2`);
    return {
      id: idAt(rating.id, `evaluation.subjectiveRatings[${index}].id`),
      score,
      maxScore: maximum,
      rationale: stringAt(rating.rationale, `evaluation.subjectiveRatings[${index}].rationale`),
    };
  });
  const ratingIds = new Set<string>();
  for (const rating of subjectiveRatings) {
    if (ratingIds.has(rating.id)) throw new Error(`Duplicate subjective rating id: ${rating.id}`);
    ratingIds.add(rating.id);
    if (!(CORE_RATING_IDS as readonly string[]).includes(rating.id) && !SEED_RATING_PATTERN.test(rating.id)) {
      throw new Error(`Additional subjective rating ids must match seed- followed by a safe identifier: ${rating.id}`);
    }
  }
  for (const requiredId of CORE_RATING_IDS) {
    if (!ratingIds.has(requiredId)) throw new Error(`Missing required core subjective rating: ${requiredId}`);
  }

  const traces = arrayAt(root.traces, "evaluation.traces").map((item, index) => {
    const trace = objectAt(item, `evaluation.traces[${index}]`);
    noUnknown(trace, ["name", "path"], `evaluation.traces[${index}]`);
    return {
      name: idAt(trace.name, `evaluation.traces[${index}].name`),
      path: resolveInputPath(evaluationDir, trace.path, `evaluation.traces[${index}].path`),
    };
  });
  const traceNames = new Set<string>();
  for (const trace of traces) {
    if (traceNames.has(trace.name)) throw new Error(`Duplicate trace name: ${trace.name}`);
    traceNames.add(trace.name);
  }
  for (const assertion of objectiveAssertions) {
    for (const traceName of assertion.evidenceTraceNames) {
      if (!traceNames.has(traceName)) throw new Error(`Objective assertion ${assertion.id} references unknown trace ${traceName}`);
    }
  }

  return {
    schemaVersion: 1,
    status: enumAt(root.status, ["complete", "invalid", "aborted"] as const, "evaluation.status"),
    objectiveAssertions,
    subjectiveRatings,
    traces,
    notes: arrayAt(root.notes, "evaluation.notes").map((note, index) => stringAt(note, `evaluation.notes[${index}]`)),
  };
}

async function validateStoredEvaluation(value: unknown, evidenceDir: string): Promise<EvaluationInput> {
  const root = objectAt(value, "canonical evaluation");
  noUnknown(root, ["schemaVersion", "status", "objectiveAssertions", "subjectiveRatings", "traces", "notes"], "canonical evaluation");
  const traceInputs: Array<{ name: string; path: string }> = [];
  for (const [index, item] of arrayAt(root.traces, "canonical evaluation.traces").entries()) {
    const trace = objectAt(item, `canonical evaluation.traces[${index}]`);
    noUnknown(trace, ["name", "sha256", "bytes", "evidencePath"], `canonical evaluation.traces[${index}]`);
    const evidencePath = normalizeTarget(stringAt(trace.evidencePath, `canonical evaluation.traces[${index}].evidencePath`), `canonical evaluation.traces[${index}].evidencePath`, false);
    if (!evidencePath.startsWith("traces/")) throw new Error(`Canonical trace must stay under traces/: ${evidencePath}`);
    const content = await fs.readFile(path.join(evidenceDir, ...evidencePath.split("/")));
    if (sha256(content) !== stringAt(trace.sha256, `canonical evaluation.traces[${index}].sha256`)) throw new Error(`Trace hash mismatch: ${evidencePath}`);
    if (content.byteLength !== integerAt(trace.bytes, `canonical evaluation.traces[${index}].bytes`, 0)) throw new Error(`Trace byte count mismatch: ${evidencePath}`);
    traceInputs.push({ name: idAt(trace.name, `canonical evaluation.traces[${index}].name`), path: path.join(evidenceDir, ...evidencePath.split("/")) });
  }
  return validateEvaluationDocument({
    schemaVersion: root.schemaVersion,
    status: root.status,
    objectiveAssertions: root.objectiveAssertions,
    subjectiveRatings: root.subjectiveRatings,
    traces: traceInputs,
    notes: root.notes,
  }, evidenceDir);
}

async function collectSourceFiles(source: string, sourceRepo: string): Promise<Array<{ sourcePath: string; relativePath: string; sha256: string; bytes: number; mode: number }>> {
  const realSource = await fs.realpath(source).catch(() => null);
  if (!realSource) throw new Error(`Component source does not exist: ${source}`);
  const relativeToRepo = path.relative(sourceRepo, realSource);
  const sourceBoundary = relativeToRepo === "" || (!relativeToRepo.startsWith("..") && !path.isAbsolute(relativeToRepo))
    ? relativeToRepo
    : realSource;
  const sourceSegments = sourceBoundary.split(path.sep).filter(Boolean).map((segment) => segment.toLowerCase());
  const reservedSourceSegment = sourceSegments.find((segment) => RESERVED_COMPONENT_SEGMENTS.has(segment));
  if (reservedSourceSegment) {
    throw new Error(`Component source may not include reserved ${reservedSourceSegment} content; select the package payload or core src/open-forge directory explicitly: ${source}`);
  }
  const sourceStat = await fs.lstat(source);
  if (sourceStat.isSymbolicLink()) throw new Error(`Component source may not be a symlink: ${source}`);
  if (sourceStat.isFile()) {
    const content = await fs.readFile(source);
    return [{ sourcePath: source, relativePath: path.basename(source), sha256: sha256(content), bytes: content.byteLength, mode: sourceStat.mode & 0o777 }];
  }
  if (!sourceStat.isDirectory()) throw new Error(`Component source is not a file or directory: ${source}`);

  const entries: Array<{ sourcePath: string; relativePath: string; sha256: string; bytes: number; mode: number }> = [];
  async function walk(directory: string): Promise<void> {
    const children = await fs.readdir(directory, { withFileTypes: true });
    children.sort((left, right) => canonicalTextCompare(left.name, right.name));
    for (const child of children) {
      if (child.isDirectory() && RESERVED_COMPONENT_SEGMENTS.has(child.name.toLowerCase())) {
        throw new Error(`Component source contains reserved ${child.name} content; select its payload subtree explicitly: ${path.join(directory, child.name)}`);
      }
      const absolute = path.join(directory, child.name);
      const stat = await fs.lstat(absolute);
      if (stat.isSymbolicLink()) throw new Error(`Component entries may not be symlinks: ${absolute}`);
      if (stat.isDirectory()) {
        await walk(absolute);
      } else if (stat.isFile()) {
        const content = await fs.readFile(absolute);
        entries.push({
          sourcePath: absolute,
          relativePath: normalizeTarget(path.relative(source, absolute), `component path ${absolute}`, false),
          sha256: sha256(content),
          bytes: content.byteLength,
          mode: stat.mode & 0o777,
        });
      } else {
        throw new Error(`Component entries must be regular files: ${absolute}`);
      }
    }
  }
  await walk(source);
  return entries.sort((left, right) => canonicalTextCompare(left.relativePath, right.relativePath));
}

function collisionKey(target: string): string {
  return target.normalize("NFC").toLowerCase();
}

async function createCompositionPlan(spec: RunSpec): Promise<CompositionPlan> {
  const allEntries: SourceEntry[] = [];
  const visible = new Map<string, SourceEntry>();
  const consumed = new Set<number>();

  for (const component of spec.components) {
    const sourceStat = await fs.lstat(component.source);
    const files = await collectSourceFiles(component.source, spec.sourceRepo);
    for (const file of files) {
      const targetPath = sourceStat.isFile()
        ? normalizeTarget(component.destination, `destination for ${component.name}`, false)
        : normalizeTarget(path.posix.join(component.destination, file.relativePath), `target for ${component.name}`, false);
      const reservedTargetSegment = targetPath.split("/").find((segment) => RESERVED_COMPONENT_SEGMENTS.has(segment.toLowerCase()));
      if (reservedTargetSegment) throw new Error(`Component ${component.name} targets reserved workspace segment ${reservedTargetSegment}: ${targetPath}`);
      const entry: SourceEntry = {
        component: component.name,
        sourcePath: file.sourcePath,
        sourceRelativePath: file.relativePath,
        targetPath,
        snapshotPath: normalizeTarget(path.posix.join("snapshot", "components", component.name, file.relativePath), `snapshot path for ${component.name}`, false),
        sha256: file.sha256,
        bytes: file.bytes,
        mode: file.mode,
      };
      allEntries.push(entry);
      const key = collisionKey(targetPath);
      const previous = visible.get(key);
      if (previous) {
        const overrideIndex = spec.overrides.findIndex((override, index) =>
          !consumed.has(index)
          && collisionKey(override.path) === key
          && override.from === previous.component
          && override.to === component.name);
        if (overrideIndex < 0) {
          throw new Error(`Undeclared composition collision at ${targetPath}: ${previous.component} -> ${component.name}`);
        }
        consumed.add(overrideIndex);
      }
      visible.set(key, entry);
    }
  }

  if (consumed.size !== spec.overrides.length) {
    const unused = spec.overrides.filter((_, index) => !consumed.has(index)).map((override) => `${override.path}:${override.from}->${override.to}`);
    throw new Error(`Declared overrides were not consumed: ${unused.join(", ")}`);
  }

  const visibleEntries = [...visible.values()].sort((left, right) => canonicalTextCompare(collisionKey(left.targetPath), collisionKey(right.targetPath)));
  const visiblePaths = new Map(visibleEntries.map((entry) => [collisionKey(entry.targetPath), entry.targetPath]));
  for (const entry of visibleEntries) {
    const segments = entry.targetPath.split("/");
    for (let index = 1; index < segments.length; index += 1) {
      const parent = segments.slice(0, index).join("/");
      const parentFile = visiblePaths.get(collisionKey(parent));
      if (parentFile) throw new Error(`Composition path conflict: ${parentFile} is also a parent file of ${entry.targetPath}`);
    }
  }
  return {
    allEntries: allEntries.sort((left, right) => canonicalTextCompare(`${left.component}/${left.sourceRelativePath}`, `${right.component}/${right.sourceRelativePath}`)),
    visibleEntries,
    consumedOverrides: spec.overrides,
  };
}

async function collectArtifact(sourcePath: string, snapshotPath: string, invocation?: string[]): Promise<ArtifactRecord> {
  const stat = await fs.lstat(sourcePath).catch(() => null);
  if (!stat || !stat.isFile() || stat.isSymbolicLink()) throw new Error(`Input artifact must be a regular file: ${sourcePath}`);
  const content = await fs.readFile(sourcePath);
  return {
    sourcePath,
    snapshotPath,
    sha256: sha256(content),
    bytes: content.byteLength,
    mode: stat.mode & 0o777,
    ...(invocation ? { invocation } : {}),
  };
}

async function verifiedCopy(source: string, destination: string, expectedSha256: string, mode: number): Promise<void> {
  await fs.mkdir(path.dirname(destination), { recursive: true });
  await fs.copyFile(source, destination);
  await fs.chmod(destination, mode).catch(() => undefined);
  const actual = sha256(await fs.readFile(destination));
  if (actual !== expectedSha256) throw new Error(`Source changed while snapshotting: ${source}`);
}

let gitExecutableFingerprint: Promise<ExecutableFingerprint> | undefined;

async function getGitExecutableFingerprint(): Promise<ExecutableFingerprint> {
  gitExecutableFingerprint ??= (async () => {
    const discovered = Bun.which("git");
    if (!discovered) throw new Error("Git executable was not found");
    const executable = await fs.realpath(discovered).catch(() => path.resolve(discovered));
    const content = await fs.readFile(executable);
    return { path: executable, sha256: sha256(content), bytes: content.byteLength };
  })();
  return gitExecutableFingerprint;
}

const GIT_CONTROL_ENVIRONMENT = {
  GIT_CONFIG_NOSYSTEM: "1",
  GIT_CONFIG_GLOBAL: process.platform === "win32" ? "NUL" : "/dev/null",
  GIT_TERMINAL_PROMPT: "0",
  LC_ALL: "C",
  LANG: "C",
  TZ: "UTC",
};
const GIT_COMMAND_CONFIG = [
  "core.autocrlf=false",
  "core.filemode=true",
  "core.quotepath=true",
  "core.fsmonitor=false",
  `core.hooksPath=${process.platform === "win32" ? "NUL" : "/dev/null"}`,
];

function controlledGitEnvironment(): Record<string, string> {
  const environment: Record<string, string> = {};
  for (const [key, value] of Object.entries(process.env)) {
    if (value !== undefined && !key.toUpperCase().startsWith("GIT_")) environment[key] = value;
  }
  return { ...environment, ...GIT_CONTROL_ENVIRONMENT };
}

async function runGit(repo: string, args: string[]): Promise<Buffer> {
  const gitExecutable = await getGitExecutableFingerprint();
  const process = Bun.spawn([
    gitExecutable.path,
    ...GIT_COMMAND_CONFIG.flatMap((value) => ["-c", value]),
    ...args,
  ], { cwd: repo, env: controlledGitEnvironment(), stdout: "pipe", stderr: "pipe" });
  const [stdout, stderr, exitCode] = await Promise.all([
    new Response(process.stdout).arrayBuffer(),
    new Response(process.stderr).arrayBuffer(),
    process.exited,
  ]);
  if (exitCode !== 0) throw new Error(`git ${args.join(" ")} failed: ${Buffer.from(stderr).toString("utf8").trim()}`);
  return Buffer.from(stdout);
}

let gitCaptureQueue: Promise<void> = Promise.resolve();

async function captureGit(repo: string): Promise<GitCapture> {
  let release!: () => void;
  const predecessor = gitCaptureQueue;
  gitCaptureQueue = new Promise<void>((resolve) => { release = resolve; });
  await predecessor;
  try {
    const stat = await fs.stat(repo).catch(() => null);
    if (!stat?.isDirectory()) throw new Error(`sourceRepo is not a directory: ${repo}`);
    // Keep Git index readers sequential. Parallel read-only Git processes can still
    // contend on the Windows index file and make provenance capture nondeterministic.
    const headBuffer = await runGit(repo, ["rev-parse", "--verify", "HEAD"]);
    const treeBuffer = await runGit(repo, ["rev-parse", "HEAD^{tree}"]);
    const statusBuffer = await runGit(repo, ["status", "--porcelain=v2", "--branch", "--untracked-files=all"]);
    const stagedDiff = await runGit(repo, ["diff", "--cached", "--binary", "--no-ext-diff", "--no-textconv", "--src-prefix=a/", "--dst-prefix=b/"]);
    const unstagedDiff = await runGit(repo, ["diff", "--binary", "--no-ext-diff", "--no-textconv", "--src-prefix=a/", "--dst-prefix=b/"]);
    const status = statusBuffer.toString("utf8");
    const statusLines = status.split(/\r?\n/).filter(Boolean);
    return {
      head: headBuffer.toString("utf8").trim(),
      tree: treeBuffer.toString("utf8").trim(),
      status,
      statusSha256: sha256(statusBuffer),
      stagedDiff,
      stagedDiffSha256: sha256(stagedDiff),
      unstagedDiff,
      unstagedDiffSha256: sha256(unstagedDiff),
      clean: statusLines.every((line) => line.startsWith("#")),
    };
  } finally {
    release();
  }
}

let runtimeExecutableFingerprint: Promise<{ path: string; sha256: string; bytes: number }> | undefined;

async function getRuntimeExecutableFingerprint(): Promise<{ path: string; sha256: string; bytes: number }> {
  runtimeExecutableFingerprint ??= (async () => {
    const executable = path.resolve(process.execPath);
    const content = await fs.readFile(executable);
    return { path: executable, sha256: sha256(content), bytes: content.byteLength };
  })();
  return runtimeExecutableFingerprint;
}

function resolveInvocation(invocation: string[], cliPath: string, workspaceDir: string): string[] {
  const replacements: Record<string, string> = {
    "{runtime}": path.resolve(process.execPath),
    "{cli}": path.resolve(cliPath),
    "{workspace}": path.resolve(workspaceDir),
  };
  const command = invocation.map((token) => {
    let resolved = token;
    for (const [placeholder, replacement] of Object.entries(replacements)) resolved = resolved.replaceAll(placeholder, replacement);
    if (/\{[^}]+\}/.test(resolved)) throw new Error(`Unknown CLI invocation placeholder in: ${token}`);
    return resolved;
  });
  if (command.length === 0 || command[0].length === 0) throw new Error("CLI invocation must contain an executable");
  if (!path.isAbsolute(command[0])) {
    throw new Error("CLI invocation executable must resolve to an absolute path; use {runtime} or {cli}, not PATH lookup");
  }
  return command;
}

async function executeCliInvocation(invocation: string[], cliPath: string, workspaceDir: string): Promise<InvocationCapture> {
  const command = resolveInvocation(invocation, cliPath, workspaceDir);
  const runtimeExecutable = await getRuntimeExecutableFingerprint();
  const commandExecutable = path.resolve(command[0]) === runtimeExecutable.path
    ? runtimeExecutable
    : await (async () => {
      const content = await fs.readFile(command[0]);
      return { path: path.resolve(command[0]), sha256: sha256(content), bytes: content.byteLength };
    })();
  const started = performance.now();
  const child = Bun.spawn(command, { cwd: workspaceDir, stdout: "pipe", stderr: "pipe" });
  const [stdout, stderr, exitCode] = await Promise.all([
    new Response(child.stdout).arrayBuffer(),
    new Response(child.stderr).arrayBuffer(),
    child.exited,
  ]);
  const stdoutBuffer = Buffer.from(stdout);
  const stderrBuffer = Buffer.from(stderr);
  return {
    command,
    exitCode,
    durationMs: Math.round((performance.now() - started) * 1000) / 1000,
    stdout: stdoutBuffer,
    stdoutSha256: sha256(stdoutBuffer),
    stderr: stderrBuffer,
    stderrSha256: sha256(stderrBuffer),
    runtimeExecutable,
    commandExecutable,
  };
}

async function initializeWorkspaceGit(workspaceDir: string): Promise<{ commit: string; tree: string }> {
  await runGit(workspaceDir, ["init", "--quiet"]);
  await runGit(workspaceDir, ["config", "user.name", "Open Forge Benchmark"]);
  await runGit(workspaceDir, ["config", "user.email", "benchmark@open-forge.invalid"]);
  await runGit(workspaceDir, ["add", "-A"]);
  await runGit(workspaceDir, ["commit", "--quiet", "--allow-empty", "-m", "benchmark composition baseline"]);
  const [commit, tree] = await Promise.all([
    runGit(workspaceDir, ["rev-parse", "--verify", "HEAD"]),
    runGit(workspaceDir, ["rev-parse", "HEAD^{tree}"]),
  ]);
  return { commit: commit.toString("utf8").trim(), tree: tree.toString("utf8").trim() };
}

async function captureWorkspaceGit(workspaceDir: string, baselineCommit: string): Promise<WorkspaceGitCapture> {
  const capture = await captureGit(workspaceDir);
  const baselineDiff = await runGit(workspaceDir, ["diff", "--binary", "--no-ext-diff", "--no-textconv", baselineCommit, "--"]);
  const baselineToHeadDiff = await runGit(workspaceDir, ["diff", "--binary", "--no-ext-diff", "--no-textconv", `${baselineCommit}..HEAD`, "--"]);
  return {
    ...capture,
    baselineDiff,
    baselineDiffSha256: sha256(baselineDiff),
    baselineToHeadDiff,
    baselineToHeadDiffSha256: sha256(baselineToHeadDiff),
  };
}

function sameGitCapture(left: GitCapture, right: GitCapture): boolean {
  return left.head === right.head
    && left.tree === right.tree
    && left.statusSha256 === right.statusSha256
    && left.stagedDiffSha256 === right.stagedDiffSha256
    && left.unstagedDiffSha256 === right.unstagedDiffSha256;
}

async function createClaim(runsRoot: string, experimentId: string, requestedRunId?: string): Promise<{ runId: string; runDir: string; ownerToken: string; claimedAt: string }> {
  const realRunsRoot = await assertPlainDirectory(runsRoot, "Runs root");
  const requestedExperimentRoot = path.join(realRunsRoot, experimentId);
  try {
    await fs.mkdir(requestedExperimentRoot);
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code !== "EEXIST") throw error;
  }
  const experimentRoot = await assertDirectDirectoryChild(realRunsRoot, requestedExperimentRoot, experimentId, "Experiment run directory");
  for (let attempt = 0; attempt < 10; attempt += 1) {
    const runId = requestedRunId ?? randomUUID();
    if (!RUN_ID_PATTERN.test(runId)) throw new Error("Requested run ID must be a UUIDv4");
    const requestedRunDir = path.join(experimentRoot, runId);
    try {
      await fs.mkdir(requestedRunDir);
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EEXIST" && !requestedRunId) continue;
      throw new Error(`Run claim already exists: ${requestedRunDir}`);
    }
    const runDir = await assertDirectDirectoryChild(experimentRoot, requestedRunDir, runId, "Claimed run directory");
    const ownerToken = randomBytes(32).toString("hex");
    const claimedAt = new Date().toISOString();
    const evidenceDir = path.join(runDir, "evidence");
    await fs.mkdir(evidenceDir);
    await assertDirectDirectoryChild(runDir, evidenceDir, "evidence", "Evidence directory");
    await writeCanonicalJson(path.join(evidenceDir, "claim.json"), {
      schemaVersion: 1,
      runId,
      experimentId,
      runsRoot: realRunsRoot,
      runDir,
      ownerTokenSha256: sha256(ownerToken),
      claimedAt,
      hostname: os.hostname(),
      pid: process.pid,
    }, true);
    return { runId, runDir, ownerToken, claimedAt };
  }
  throw new Error("Unable to allocate a unique UUIDv4 run claim after 10 attempts");
}

async function writeState(evidenceDir: string, state: RunState): Promise<void> {
  await writeCanonicalJson(path.join(evidenceDir, "run-state.json"), state);
}

function normalizedFileMode(mode: number): number {
  return (mode & 0o111) !== 0 ? 0o755 : 0o644;
}

export async function buildTreeManifest(root: string, options: { allowGitDirectory?: boolean } = {}): Promise<TreeManifest> {
  await assertPlainDirectory(root, "Worker workspace root");
  const entries: TreeEntry[] = [];
  async function walk(directory: string): Promise<void> {
    const children = await fs.readdir(directory, { withFileTypes: true });
    children.sort((left, right) => canonicalTextCompare(left.name, right.name));
    for (const child of children) {
      const absolute = path.join(directory, child.name);
      const stat = await fs.lstat(absolute);
      if (stat.isSymbolicLink()) throw new Error(`Worker workspace may not contain a symbolic link or junction: ${absolute}`);
      if (child.name === ".git") {
        if (stat.isDirectory() && options.allowGitDirectory) continue;
        if (stat.isDirectory()) throw new Error(`Worker workspace contains a pre-existing .git directory before runner baseline initialization: ${absolute}`);
        throw new Error(`Worker workspace .git entry is not a regular Git directory: ${absolute}`);
      }
      const relative = normalizeTarget(path.relative(root, absolute), `tree path ${absolute}`, false);
      if (stat.isDirectory()) {
        await walk(absolute);
      } else if (stat.isFile()) {
        if (stat.nlink !== 1) throw new Error(`Worker workspace may not contain a hard-linked file: ${absolute}`);
        const content = await fs.readFile(absolute);
        entries.push({ path: relative, type: "file", sha256: sha256(content), bytes: content.byteLength, mode: normalizedFileMode(stat.mode) });
      } else {
        throw new Error(`Workspace contains unsupported filesystem entry: ${absolute}`);
      }
    }
  }
  await walk(root);
  entries.sort((left, right) => canonicalTextCompare(left.path, right.path));
  return { schemaVersion: 1, digest: canonicalHash(entries), entries };
}

async function buildGitTreeManifest(workspaceDir: string, commit: string): Promise<TreeManifest> {
  const raw = await runGit(workspaceDir, ["ls-tree", "-rz", "--full-tree", commit]);
  const records = raw.toString("utf8").split("\0").filter(Boolean);
  const entries: TreeEntry[] = [];
  for (const record of records) {
    const tab = record.indexOf("\t");
    if (tab < 0) throw new Error(`Unexpected git ls-tree record: ${record}`);
    const [mode, type, objectId] = record.slice(0, tab).split(" ");
    const relative = normalizeTarget(record.slice(tab + 1), "Git tree path", false);
    if (type !== "blob" || (mode !== "100644" && mode !== "100755")) {
      throw new Error(`Worker baseline contains a link or unsupported Git entry at ${relative} (${mode} ${type})`);
    }
    const content = await runGit(workspaceDir, ["cat-file", "blob", objectId]);
    entries.push({
      path: relative,
      type: "file",
      sha256: sha256(content),
      bytes: content.byteLength,
      mode: mode === "100755" ? 0o755 : 0o644,
    });
  }
  entries.sort((left, right) => canonicalTextCompare(left.path, right.path));
  return { schemaVersion: 1, digest: canonicalHash(entries), entries };
}

async function snapshotFinalWorkspace(workspaceDir: string, evidenceDir: string, manifest: TreeManifest): Promise<void> {
  const snapshotDir = path.join(evidenceDir, "final-workspace");
  await fs.mkdir(snapshotDir);
  for (const entry of manifest.entries) {
    const source = path.join(workspaceDir, ...entry.path.split("/"));
    const destination = path.join(snapshotDir, ...entry.path.split("/"));
    const stat = await fs.lstat(source);
    if (!stat.isFile() || stat.isSymbolicLink()) throw new Error(`Final workspace snapshot source is not a regular file: ${source}`);
    await verifiedCopy(source, destination, entry.sha256, entry.mode);
  }
  const snapshotted = await buildTreeManifest(snapshotDir);
  if (!compareCanonical(snapshotted, manifest)) throw new Error("Final workspace snapshot does not reproduce the finalized worker tree");
}

async function scanCanary(root: string, canary: string, options: { allowGitDirectory?: boolean } = {}): Promise<string[]> {
  await assertPlainDirectory(root, "Worker workspace root");
  const needle = Buffer.from(canary, "utf8");
  const hits: string[] = [];
  async function walk(directory: string): Promise<void> {
    const children = await fs.readdir(directory, { withFileTypes: true });
    for (const child of children) {
      const absolute = path.join(directory, child.name);
      const stat = await fs.lstat(absolute);
      if (stat.isSymbolicLink()) throw new Error(`Worker workspace may not contain a symbolic link or junction: ${absolute}`);
      if (child.name === ".git") {
        if (stat.isDirectory() && options.allowGitDirectory) continue;
        if (stat.isDirectory()) throw new Error(`Worker workspace contains a pre-existing .git directory before runner baseline initialization: ${absolute}`);
        throw new Error(`Worker workspace .git entry is not a regular Git directory: ${absolute}`);
      }
      if (stat.isDirectory()) await walk(absolute);
      else if (stat.isFile()) {
        if (stat.nlink !== 1) throw new Error(`Worker workspace may not contain a hard-linked file: ${absolute}`);
        if ((await fs.readFile(absolute)).includes(needle)) {
          hits.push(normalizeTarget(path.relative(root, absolute), `canary hit ${absolute}`, false));
        }
      } else throw new Error(`Workspace contains unsupported filesystem entry: ${absolute}`);
    }
  }
  await walk(root);
  return hits.sort(canonicalTextCompare);
}

export async function prepareRun(options: { specPath: string; runsRoot: string; runId?: string }): Promise<PreparedRun> {
  const specPath = path.resolve(options.specPath);
  const rawSpec = await fs.readFile(specPath);
  const spec = validateRunSpecDocument(JSON.parse(rawSpec.toString("utf8")), path.dirname(specPath));
  if (!path.isAbsolute(options.runsRoot)) throw new Error("--runs-root must be an absolute path");
  const requestedRunsRoot = path.resolve(options.runsRoot);
  const requestedSourceRepo = path.resolve(spec.sourceRepo);
  assertExternalRunsRoot(requestedRunsRoot, requestedSourceRepo);
  spec.sourceRepo = await fs.realpath(requestedSourceRepo);
  const projectedRunsRoot = await projectPathThroughExistingAncestor(requestedRunsRoot);
  assertExternalRunsRoot(projectedRunsRoot, spec.sourceRepo);
  await fs.mkdir(requestedRunsRoot, { recursive: true });
  const runsRoot = await fs.realpath(requestedRunsRoot);
  assertExternalRunsRoot(runsRoot, spec.sourceRepo);

  const plan = await createCompositionPlan(spec);
  const meta = {
    cli: await collectArtifact(spec.inputs.cli.path, `snapshot/meta/cli${path.extname(spec.inputs.cli.path)}`, spec.inputs.cli.invocation),
    prompt: await collectArtifact(spec.inputs.prompt.path, `snapshot/meta/prompt${path.extname(spec.inputs.prompt.path)}`),
    rubric: await collectArtifact(spec.inputs.rubric.path, `snapshot/meta/rubric${path.extname(spec.inputs.rubric.path)}`),
    harness: await collectArtifact(HARNESS_SOURCE_PATH, `snapshot/meta/harness${path.extname(HARNESS_SOURCE_PATH) || ".js"}`),
    sourceSpec: {
      sourcePath: specPath,
      snapshotPath: "snapshot/meta/run-spec.source.json",
      sha256: sha256(rawSpec),
      bytes: rawSpec.byteLength,
      mode: (await fs.stat(specPath)).mode & 0o777,
    } satisfies ArtifactRecord,
  };
  const orchestrationOnlySources = new Map([
    [path.resolve(meta.cli.sourcePath).toLowerCase(), "cli"],
    [path.resolve(meta.prompt.sourcePath).toLowerCase(), "prompt"],
    [path.resolve(meta.rubric.sourcePath).toLowerCase(), "rubric"],
    [path.resolve(meta.harness.sourcePath).toLowerCase(), "harness"],
  ]);
  for (const entry of plan.visibleEntries) {
    const artifactName = orchestrationOnlySources.get(path.resolve(entry.sourcePath).toLowerCase());
    if (artifactName) throw new Error(`Visible component ${entry.component} would expose orchestration-only ${artifactName} input at ${entry.targetPath}`);
  }
  const gitBefore = await captureGit(spec.sourceRepo);
  const claim = await createClaim(runsRoot, spec.experimentId, options.runId);
  const evidenceDir = path.join(claim.runDir, "evidence");
  const workspaceDir = path.join(claim.runDir, "workspace");
  const canary = `OPEN_FORGE_ORCHESTRATOR_CANARY_${randomUUID()}`;

  try {
    await fs.mkdir(workspaceDir);
    await assertRunDirectoryLayout(claim.runDir);
    await fs.mkdir(path.join(evidenceDir, "snapshot"), { recursive: true });
    await fs.mkdir(path.join(evidenceDir, "provenance"), { recursive: true });
    await atomicWrite(path.join(evidenceDir, "canary.txt"), `${canary}\n`);
    await writeCanonicalJson(path.join(evidenceDir, "spec.json"), spec);

    for (const entry of plan.allEntries) {
      await verifiedCopy(entry.sourcePath, path.join(evidenceDir, ...entry.snapshotPath.split("/")), entry.sha256, entry.mode);
    }
    for (const artifact of Object.values(meta)) {
      await verifiedCopy(artifact.sourcePath, path.join(evidenceDir, ...artifact.snapshotPath.split("/")), artifact.sha256, artifact.mode);
    }
    for (const entry of plan.visibleEntries) {
      const source = path.join(evidenceDir, ...entry.snapshotPath.split("/"));
      const destination = path.join(workspaceDir, ...entry.targetPath.split("/"));
      await verifiedCopy(source, destination, entry.sha256, entry.mode);
    }

    const cliPath = path.join(evidenceDir, ...meta.cli.snapshotPath.split("/"));
    const cliInvocation = await executeCliInvocation(spec.inputs.cli.invocation, cliPath, workspaceDir);
    await atomicWrite(path.join(evidenceDir, "provenance", "cli-stdout.txt"), cliInvocation.stdout);
    await atomicWrite(path.join(evidenceDir, "provenance", "cli-stderr.txt"), cliInvocation.stderr);
    if (cliInvocation.exitCode !== 0) {
      throw new Error(`Snapshotted CLI invocation failed with exit code ${cliInvocation.exitCode}: ${cliInvocation.stderr.toString("utf8").trim()}`);
    }

    // Reject links and other special entries before Git can traverse or normalize
    // them. The worker-visible boundary is regular files and directories only.
    const workerVisibleBeforeGit = await buildTreeManifest(workspaceDir);
    const preGitCanaryHits = await scanCanary(workspaceDir, canary);
    if (preGitCanaryHits.length > 0) throw new Error(`Orchestrator canary leaked into worker workspace: ${preGitCanaryHits.join(", ")}`);

    const workspaceBaseline = await initializeWorkspaceGit(workspaceDir);
    const workspaceBaselineCapture = await captureWorkspaceGit(workspaceDir, workspaceBaseline.commit);
    if (!workspaceBaselineCapture.clean) throw new Error("Workspace baseline commit is unexpectedly dirty");
    const baselineTreeManifest = await buildGitTreeManifest(workspaceDir, workspaceBaseline.commit);
    if (!compareCanonical(workerVisibleBeforeGit, baselineTreeManifest)) {
      throw new Error("Workspace Git baseline does not reproduce the worker-visible tree");
    }
    await atomicWrite(path.join(evidenceDir, "provenance", "workspace-baseline-status.txt"), workspaceBaselineCapture.status);

    const gitAfter = await captureGit(spec.sourceRepo);
    if (!sameGitCapture(gitBefore, gitAfter)) throw new Error("sourceRepo changed while the input snapshot was being captured");
    await atomicWrite(path.join(evidenceDir, "provenance", "git-status.txt"), gitAfter.status);
    await atomicWrite(path.join(evidenceDir, "provenance", "git-diff-staged.patch"), gitAfter.stagedDiff);
    await atomicWrite(path.join(evidenceDir, "provenance", "git-diff-unstaged.patch"), gitAfter.unstagedDiff);

    const inputManifest = {
      schemaVersion: 1,
      sourceSpecSha256: meta.sourceSpec.sha256,
      components: plan.allEntries,
      composition: plan.visibleEntries.map((entry) => ({ targetPath: entry.targetPath, component: entry.component, sha256: entry.sha256, bytes: entry.bytes, mode: entry.mode })),
      overrides: plan.consumedOverrides,
      artifacts: meta,
    };
    await writeCanonicalJson(path.join(evidenceDir, "input-manifest.json"), inputManifest);
    const workerVisible = await buildTreeManifest(workspaceDir, { allowGitDirectory: true });
    if (!compareCanonical(workerVisibleBeforeGit, workerVisible)) throw new Error("Worker-visible tree changed while establishing its Git baseline");
    await writeCanonicalJson(path.join(evidenceDir, "worker-visible.json"), workerVisible);
    const canaryHits = await scanCanary(workspaceDir, canary, { allowGitDirectory: true });
    if (canaryHits.length > 0) throw new Error(`Orchestrator canary leaked into worker workspace: ${canaryHits.join(", ")}`);

    const gitExecutable = await getGitExecutableFingerprint();
    await writeCanonicalJson(path.join(evidenceDir, "provenance.json"), {
      schemaVersion: 1,
      sourceRepo: spec.sourceRepo,
      harnessProducer: {
        sourcePath: meta.harness.sourcePath,
        snapshotPath: meta.harness.snapshotPath,
        sha256: meta.harness.sha256,
        bytes: meta.harness.bytes,
      },
      gitExecutable,
      gitControl: {
        environment: GIT_CONTROL_ENVIRONMENT,
        commandConfig: GIT_COMMAND_CONFIG,
      },
      git: {
        head: gitAfter.head,
        tree: gitAfter.tree,
        status: gitAfter.status,
        statusSha256: gitAfter.statusSha256,
        stagedDiffSha256: gitAfter.stagedDiffSha256,
        unstagedDiffSha256: gitAfter.unstagedDiffSha256,
        clean: gitAfter.clean,
      },
      traceHashes: {
        gitStatus: sha256(await fs.readFile(path.join(evidenceDir, "provenance", "git-status.txt"))),
        gitDiffStaged: sha256(await fs.readFile(path.join(evidenceDir, "provenance", "git-diff-staged.patch"))),
        gitDiffUnstaged: sha256(await fs.readFile(path.join(evidenceDir, "provenance", "git-diff-unstaged.patch"))),
        cliStdout: cliInvocation.stdoutSha256,
        cliStderr: cliInvocation.stderrSha256,
        workspaceBaselineStatus: workspaceBaselineCapture.statusSha256,
      },
      cliInvocation: {
        command: cliInvocation.command,
        exitCode: cliInvocation.exitCode,
        durationMs: cliInvocation.durationMs,
        stdoutSha256: cliInvocation.stdoutSha256,
        stderrSha256: cliInvocation.stderrSha256,
        runtimeExecutable: cliInvocation.runtimeExecutable,
        commandExecutable: cliInvocation.commandExecutable,
      },
      workspaceBaseline: {
        commit: workspaceBaseline.commit,
        tree: workspaceBaseline.tree,
        status: workspaceBaselineCapture.status,
        statusSha256: workspaceBaselineCapture.statusSha256,
        stagedDiffSha256: workspaceBaselineCapture.stagedDiffSha256,
        unstagedDiffSha256: workspaceBaselineCapture.unstagedDiffSha256,
      },
      snapshotExact: true,
    });
    await writeState(evidenceDir, { schemaVersion: 1, runId: claim.runId, state: "prepared", updatedAt: new Date().toISOString() });
    const preparedRootSha256 = await writePreparedSeal(evidenceDir, claim.runId, claim.ownerToken);

    return {
      runId: claim.runId,
      runDir: claim.runDir,
      workspaceDir,
      evidenceDir,
      workerPromptPath: path.join(evidenceDir, ...meta.prompt.snapshotPath.split("/")),
      ownerToken: claim.ownerToken,
      preparedRootSha256,
    };
  } catch (error) {
    await writeCanonicalJson(path.join(evidenceDir, "prepare-failure.json"), {
      schemaVersion: 1,
      failedAt: new Date().toISOString(),
      error: error instanceof Error ? error.message : String(error),
    }).catch(() => undefined);
    await writeState(evidenceDir, { schemaVersion: 1, runId: claim.runId, state: "invalid", updatedAt: new Date().toISOString() }).catch(() => undefined);
    throw error;
  }
}

async function readClaim(runDir: string): Promise<Record<string, unknown>> {
  const claim = objectAt(await readJson(path.join(runDir, "evidence", "claim.json")), "claim");
  noUnknown(claim, ["schemaVersion", "runId", "experimentId", "runsRoot", "runDir", "ownerTokenSha256", "claimedAt", "hostname", "pid"], "claim");
  if (!RUN_ID_PATTERN.test(stringAt(claim.runId, "claim.runId"))) throw new Error("claim.runId must be a UUIDv4");
  if (!SHA256_PATTERN.test(stringAt(claim.ownerTokenSha256, "claim.ownerTokenSha256"))) throw new Error("claim.ownerTokenSha256 must be a SHA-256 digest");
  const currentRunDir = await fs.realpath(runDir);
  const recordedRunDir = stringAt(claim.runDir, "claim.runDir");
  const recordedRunsRoot = stringAt(claim.runsRoot, "claim.runsRoot");
  const experimentId = idAt(claim.experimentId, "claim.experimentId");
  const runId = stringAt(claim.runId, "claim.runId");
  if (!path.isAbsolute(recordedRunDir) || !path.isAbsolute(recordedRunsRoot)) throw new Error("claim paths must be absolute");
  if (!sameResolvedPath(currentRunDir, recordedRunDir)) throw new Error("Current run directory does not match the canonical path bound into claim.json");
  if (!sameResolvedPath(currentRunDir, path.join(recordedRunsRoot, experimentId, runId))) throw new Error("claim run path does not match its runs root, experiment, and run ID");
  return claim;
}

async function enforceOwner(runDir: string, ownerToken: string): Promise<Record<string, unknown>> {
  const claim = await readClaim(runDir);
  const expected = Buffer.from(stringAt(claim.ownerTokenSha256, "claim.ownerTokenSha256"), "hex");
  const actual = Buffer.from(sha256(ownerToken), "hex");
  if (expected.length !== actual.length || !timingSafeEqual(expected, actual)) throw new Error("Owner token does not match this run claim");
  return claim;
}

interface SealedFile {
  path: string;
  sha256: string;
  bytes: number;
}

function keyedDigest(ownerToken: string, value: unknown): string {
  return createHmac("sha256", Buffer.from(ownerToken, "utf8")).update(stableStringify(value)).digest("hex");
}

function secureDigestEqual(left: string, right: string): boolean {
  if (!SHA256_PATTERN.test(left) || !SHA256_PATTERN.test(right)) return false;
  return timingSafeEqual(Buffer.from(left, "hex"), Buffer.from(right, "hex"));
}

async function collectSealedFiles(evidenceDir: string, excluded: Set<string>): Promise<SealedFile[]> {
  const files = await listRegularFiles(evidenceDir, excluded);
  const records: SealedFile[] = [];
  for (const relative of files) {
    const content = await fs.readFile(path.join(evidenceDir, ...relative.split("/")));
    records.push({ path: relative, sha256: sha256(content), bytes: content.byteLength });
  }
  return records;
}

async function writePreparedSeal(evidenceDir: string, runId: string, ownerToken: string): Promise<string> {
  const payload = {
    schemaVersion: 1,
    runId,
    runStateSha256: sha256(await fs.readFile(path.join(evidenceDir, "run-state.json"))),
    files: await collectSealedFiles(evidenceDir, new Set(["prepared-seal.json", "run-state.json"])),
  };
  const preparedRootSha256 = canonicalHash(payload);
  await writeCanonicalJson(path.join(evidenceDir, "prepared-seal.json"), {
    ...payload,
    preparedRootSha256,
    mac: keyedDigest(ownerToken, { ...payload, preparedRootSha256 }),
  });
  return preparedRootSha256;
}

async function verifyPreparedSeal(
  evidenceDir: string,
  runId: string,
  ownerToken: string,
  expectedPreparedRootSha256: string,
  requireExactPreparedDirectory: boolean,
): Promise<void> {
  if (!SHA256_PATTERN.test(expectedPreparedRootSha256)) throw new Error("Prepared root must be a SHA-256 digest");
  const seal = objectAt(await readJson(path.join(evidenceDir, "prepared-seal.json")), "prepared seal");
  noUnknown(seal, ["schemaVersion", "runId", "runStateSha256", "files", "preparedRootSha256", "mac"], "prepared seal");
  if (integerAt(seal.schemaVersion, "prepared seal.schemaVersion") !== 1 || seal.runId !== runId) throw new Error("Prepared seal does not identify this run");
  const files = arrayAt(seal.files, "prepared seal.files").map((item, index): SealedFile => {
    const record = objectAt(item, `prepared seal.files[${index}]`);
    noUnknown(record, ["path", "sha256", "bytes"], `prepared seal.files[${index}]`);
    const digest = stringAt(record.sha256, `prepared seal.files[${index}].sha256`);
    if (!SHA256_PATTERN.test(digest)) throw new Error(`prepared seal.files[${index}].sha256 is invalid`);
    return {
      path: normalizeTarget(stringAt(record.path, `prepared seal.files[${index}].path`), `prepared seal.files[${index}].path`, false),
      sha256: digest,
      bytes: integerAt(record.bytes, `prepared seal.files[${index}].bytes`, 0),
    };
  });
  const runStateSha256 = stringAt(seal.runStateSha256, "prepared seal.runStateSha256");
  if (!SHA256_PATTERN.test(runStateSha256)) throw new Error("prepared seal.runStateSha256 is invalid");
  const payload = { schemaVersion: 1, runId, runStateSha256, files };
  const recordedRoot = stringAt(seal.preparedRootSha256, "prepared seal.preparedRootSha256");
  if (!secureDigestEqual(recordedRoot, expectedPreparedRootSha256) || !secureDigestEqual(recordedRoot, canonicalHash(payload))) {
    throw new Error("Prepared root does not match the sealed prepared evidence");
  }
  const expectedMac = keyedDigest(ownerToken, { ...payload, preparedRootSha256: recordedRoot });
  if (!secureDigestEqual(stringAt(seal.mac, "prepared seal.mac"), expectedMac)) throw new Error("Prepared evidence authentication failed");
  for (const record of files) {
    const content = await fs.readFile(path.join(evidenceDir, ...record.path.split("/")));
    if (content.byteLength !== record.bytes || sha256(content) !== record.sha256) throw new Error(`Prepared sealed file was changed: ${record.path}`);
  }
  if (requireExactPreparedDirectory) {
    const currentStateSha256 = sha256(await fs.readFile(path.join(evidenceDir, "run-state.json")));
    if (!secureDigestEqual(runStateSha256, currentStateSha256)) throw new Error("Prepared run state was changed before finalization");
    const actual = await collectSealedFiles(evidenceDir, new Set(["prepared-seal.json", "run-state.json"]));
    if (!compareCanonical(actual, files)) throw new Error("Prepared evidence directory contains unsealed additions or removals");
  }
}

async function writeFinalSeal(evidenceDir: string, runId: string, ownerToken: string): Promise<void> {
  const checksums = await readJson(path.join(evidenceDir, "checksums.json"));
  const checksumsSha256 = canonicalHash(checksums);
  const payload = { schemaVersion: 1, runId, checksumsSha256 };
  await writeCanonicalJson(path.join(evidenceDir, "final-seal.json"), {
    ...payload,
    mac: keyedDigest(ownerToken, payload),
  });
}

async function verifyFinalSeal(evidenceDir: string, runId: string, ownerToken: string): Promise<void> {
  const seal = objectAt(await readJson(path.join(evidenceDir, "final-seal.json")), "final seal");
  noUnknown(seal, ["schemaVersion", "runId", "checksumsSha256", "mac"], "final seal");
  if (integerAt(seal.schemaVersion, "final seal.schemaVersion") !== 1 || seal.runId !== runId) throw new Error("Final seal does not identify this run");
  const checksums = await readJson(path.join(evidenceDir, "checksums.json"));
  const checksumsSha256 = canonicalHash(checksums);
  if (!secureDigestEqual(stringAt(seal.checksumsSha256, "final seal.checksumsSha256"), checksumsSha256)) throw new Error("Final seal checksum root does not match checksums.json");
  const payload = { schemaVersion: 1, runId, checksumsSha256 };
  if (!secureDigestEqual(stringAt(seal.mac, "final seal.mac"), keyedDigest(ownerToken, payload))) throw new Error("Final evidence authentication failed");
}

async function copyEvaluationTraces(evaluation: EvaluationInput, evidenceDir: string): Promise<Array<{ name: string; sha256: string; bytes: number; evidencePath: string }>> {
  const traceDir = path.join(evidenceDir, "traces");
  await fs.mkdir(traceDir, { recursive: true });
  const records: Array<{ name: string; sha256: string; bytes: number; evidencePath: string }> = [];
  for (let index = 0; index < evaluation.traces.length; index += 1) {
    const trace = evaluation.traces[index];
    const stat = await fs.lstat(trace.path).catch(() => null);
    if (!stat || !stat.isFile() || stat.isSymbolicLink() || stat.nlink !== 1) throw new Error(`Evaluation trace must remain an external, singly-linked regular file: ${trace.path}`);
    const content = await fs.readFile(trace.path);
    const extension = path.extname(trace.path).slice(0, 20);
    const evidencePath = normalizeTarget(path.posix.join("traces", `${String(index + 1).padStart(3, "0")}-${trace.name}${extension}`), `trace path ${trace.name}`, false);
    await atomicWrite(path.join(evidenceDir, ...evidencePath.split("/")), content);
    records.push({ name: trace.name, sha256: sha256(content), bytes: content.byteLength, evidencePath });
  }
  return records;
}

function deriveEffectiveStatus(status: "complete" | "invalid" | "aborted", canaryHits: string[]): "complete" | "invalid" | "aborted" {
  return canaryHits.length > 0 ? "invalid" : status;
}

function deriveEligibility(
  spec: RunSpec,
  status: string,
  provenance: Record<string, unknown>,
  canaryHits: string[],
  evaluation: EvaluationInput,
  traceRecords: Array<{ name: string }>,
): JsonObject {
  const causalReasons: string[] = [];
  const warnings: string[] = [];
  causalReasons.push("P0_CAUSAL_CONTROL_VERIFIER_UNAVAILABLE");
  if (status !== "complete") causalReasons.push(status === "aborted" ? "RUN_ABORTED" : "RUN_INVALID");
  if (spec.evidenceClass === "engineering-smoke") causalReasons.push("EVIDENCE_CLASS_ENGINEERING_ONLY");
  if (provenance.snapshotExact !== true) causalReasons.push("PROVENANCE_EXACT_SNAPSHOT_MISSING");
  const git = objectAt(provenance.git, "provenance.git");
  if (git.clean !== true) warnings.push("SOURCE_DIRTY_EXACT_SNAPSHOT");
  if (spec.isolation.freshContext !== "verified") causalReasons.push("ISOLATION_FRESH_CONTEXT_UNVERIFIED");
  const traceNames = new Set(traceRecords.map((trace) => trace.name));
  const isolationReceipt = evaluation.objectiveAssertions.find((assertion) => assertion.id === "isolation-fresh-context");
  if (!isolationReceipt
    || isolationReceipt.status !== "pass"
    || isolationReceipt.evidenceTraceNames.length === 0
    || isolationReceipt.evidenceTraceNames.some((name) => !traceNames.has(name))) {
    causalReasons.push("ISOLATION_RECEIPT_MISSING");
  }
  if (spec.isolation.inheritedContext !== "none") causalReasons.push(spec.isolation.inheritedContext === "present" ? "ISOLATION_INHERITED_CONTEXT_PRESENT" : "ISOLATION_INHERITED_CONTEXT_UNKNOWN");
  if (!spec.isolation.workerReceivesOnlyWorkspace) causalReasons.push("ISOLATION_WORKSPACE_BOUNDARY_UNVERIFIED");
  if (spec.isolation.network !== "disabled") causalReasons.push("ISOLATION_NETWORK_UNCONTROLLED");
  if (spec.isolation.filesystem !== "workspace-only") causalReasons.push("ISOLATION_FILESYSTEM_UNCONTROLLED");
  if (!spec.controls.planned) causalReasons.push("CONTROL_NOT_PREPLANNED");
  if (!spec.controls.randomized) causalReasons.push("CONTROL_ORDER_NOT_RANDOMIZED");
  if (!spec.controls.declaredTreatmentOnly) causalReasons.push("CONTROL_TREATMENT_DELTA_UNVERIFIED");
  if (!spec.controls.replicatePlanned) causalReasons.push("CONTROL_REPLICATION_UNPLANNED");
  if (spec.controls.planId === null) causalReasons.push("CONTROL_PLAN_ID_MISSING");
  if (spec.controls.planSha256 === null) causalReasons.push("CONTROL_PLAN_HASH_MISSING");
  if (canaryHits.length > 0) causalReasons.push("CANARY_EXPOSED");
  const uniqueCausalReasons = [...new Set(causalReasons)].sort();
  return {
    engineeringEligible: status === "complete" && canaryHits.length === 0,
    causalEligible: false,
    publicEligible: false,
    causalReasonCodes: uniqueCausalReasons,
    publicReasonCodes: ["PUBLIC_EVIDENCE_REQUIRES_CORPUS_REVIEW"],
    warnings: warnings.sort(),
  };
}

function validateTreeManifestDocument(value: unknown, label: string): TreeManifest {
  const root = objectAt(value, label);
  noUnknown(root, ["schemaVersion", "digest", "entries"], label);
  if (integerAt(root.schemaVersion, `${label}.schemaVersion`) !== 1) throw new Error(`${label}.schemaVersion must be 1`);
  const entries = arrayAt(root.entries, `${label}.entries`).map((item, index): TreeEntry => {
    const entry = objectAt(item, `${label}.entries[${index}]`);
    noUnknown(entry, ["path", "type", "sha256", "bytes", "mode"], `${label}.entries[${index}]`);
    const type = enumAt(entry.type, ["file"] as const, `${label}.entries[${index}].type`);
    const digest = stringAt(entry.sha256, `${label}.entries[${index}].sha256`);
    if (!SHA256_PATTERN.test(digest)) throw new Error(`${label}.entries[${index}].sha256 must be a SHA-256 digest`);
    const result: TreeEntry = {
      path: normalizeTarget(stringAt(entry.path, `${label}.entries[${index}].path`), `${label}.entries[${index}].path`, false),
      type,
      sha256: digest,
      bytes: integerAt(entry.bytes, `${label}.entries[${index}].bytes`, 0),
      mode: integerAt(entry.mode, `${label}.entries[${index}].mode`, 0),
    };
    if (result.mode !== 0o644 && result.mode !== 0o755) throw new Error(`${label}.entries[${index}].mode must be 0644 or 0755`);
    return result;
  });
  const sorted = [...entries].sort((left, right) => canonicalTextCompare(left.path, right.path));
  if (stableStringify(entries) !== stableStringify(sorted)) throw new Error(`${label}.entries must be sorted`);
  const digest = stringAt(root.digest, `${label}.digest`);
  if (!SHA256_PATTERN.test(digest) || digest !== canonicalHash(entries)) throw new Error(`${label}.digest does not match entries`);
  return { schemaVersion: 1, digest, entries };
}

export function validateResultDocument(value: unknown): BenchmarkResult {
  const root = objectAt(value, "result");
  noUnknown(root, [
    "schemaVersion", "runId", "experimentId", "caseId", "armId", "replicate", "evidenceClass", "status",
    "preparedAt", "finalizedAt", "model", "runtime", "isolation", "controls", "provenance", "hashes",
    "objectiveAssertions", "subjectiveRatings", "traces", "notes", "contamination", "eligibility",
  ], "result");
  if (integerAt(root.schemaVersion, "result.schemaVersion") !== 1) throw new Error("result.schemaVersion must be 1");
  if (!RUN_ID_PATTERN.test(stringAt(root.runId, "result.runId"))) throw new Error("result.runId must be a UUIDv4");
  idAt(root.experimentId, "result.experimentId");
  idAt(root.caseId, "result.caseId");
  idAt(root.armId, "result.armId");
  integerAt(root.replicate, "result.replicate", 0);
  enumAt(root.evidenceClass, ["engineering-smoke", "pilot", "confirmatory"] as const, "result.evidenceClass");
  enumAt(root.status, ["complete", "invalid", "aborted"] as const, "result.status");
  stringAt(root.preparedAt, "result.preparedAt");
  stringAt(root.finalizedAt, "result.finalizedAt");
  for (const field of ["model", "runtime", "isolation", "controls", "provenance", "hashes", "contamination"] as const) objectAt(root[field], `result.${field}`);
  for (const field of ["objectiveAssertions", "subjectiveRatings", "traces", "notes"] as const) arrayAt(root[field], `result.${field}`);
  const hashes = objectAt(root.hashes, "result.hashes");
  const hashNames = [
    "normalizedSpecSha256", "inputManifestSha256", "initialWorkerVisibleSha256", "initialWorkerTreeDigest",
    "finalWorkerTreeDigest", "finalWorkspaceSnapshotDigest", "evaluationSha256", "cliSha256", "promptSha256",
    "rubricSha256", "harnessSha256",
  ];
  noUnknown(hashes, hashNames, "result.hashes");
  for (const name of hashNames) {
    if (!SHA256_PATTERN.test(stringAt(hashes[name], `result.hashes.${name}`))) throw new Error(`result.hashes.${name} must be a SHA-256 digest`);
  }
  const eligibility = objectAt(root.eligibility, "result.eligibility");
  noUnknown(eligibility, ["engineeringEligible", "causalEligible", "publicEligible", "causalReasonCodes", "publicReasonCodes", "warnings"], "result.eligibility");
  booleanAt(eligibility.engineeringEligible, "result.eligibility.engineeringEligible");
  if (booleanAt(eligibility.causalEligible, "result.eligibility.causalEligible")) throw new Error("P0 runner may not mark evidence causalEligible");
  if (booleanAt(eligibility.publicEligible, "result.eligibility.publicEligible")) throw new Error("P0 runner may not mark evidence publicEligible");
  for (const field of ["causalReasonCodes", "publicReasonCodes", "warnings"] as const) {
    arrayAt(eligibility[field], `result.eligibility.${field}`).forEach((item, index) => idAt(item, `result.eligibility.${field}[${index}]`));
  }
  if (!arrayAt(eligibility.causalReasonCodes, "result.eligibility.causalReasonCodes").includes("P0_CAUSAL_CONTROL_VERIFIER_UNAVAILABLE")) {
    throw new Error("P0 causal ineligibility reason is required");
  }
  return root as BenchmarkResult;
}

type CanonicalTraceRecord = { name: string; sha256: string; bytes: number; evidencePath: string };

async function assembleResult(options: {
  claim: Record<string, unknown>;
  spec: RunSpec;
  evaluation: EvaluationInput;
  traceRecords: CanonicalTraceRecord[];
  effectiveStatus: "complete" | "invalid" | "aborted";
  finalizedAt: string;
  canaryHits: string[];
  provenance: Record<string, unknown>;
  workspaceFinalProvenance: Record<string, unknown>;
  finalWorkerVisible: TreeManifest;
  evidenceDir: string;
}): Promise<BenchmarkResult> {
  const inputManifestRaw = await fs.readFile(path.join(options.evidenceDir, "input-manifest.json"));
  const initialWorkerRaw = await fs.readFile(path.join(options.evidenceDir, "worker-visible.json"));
  const inputManifest = objectAt(JSON.parse(inputManifestRaw.toString("utf8")), "input manifest");
  const initialWorker = validateTreeManifestDocument(JSON.parse(initialWorkerRaw.toString("utf8")), "worker-visible manifest");
  const artifacts = objectAt(inputManifest.artifacts, "input manifest.artifacts");
  const cliArtifact = objectAt(artifacts.cli, "input manifest.artifacts.cli");
  const promptArtifact = objectAt(artifacts.prompt, "input manifest.artifacts.prompt");
  const rubricArtifact = objectAt(artifacts.rubric, "input manifest.artifacts.rubric");
  const harnessArtifact = objectAt(artifacts.harness, "input manifest.artifacts.harness");
  const result: BenchmarkResult = {
    schemaVersion: 1,
    runId: stringAt(options.claim.runId, "claim.runId"),
    experimentId: options.spec.experimentId,
    caseId: options.spec.caseId,
    armId: options.spec.armId,
    replicate: options.spec.replicate,
    evidenceClass: options.spec.evidenceClass,
    status: options.effectiveStatus,
    preparedAt: stringAt(options.claim.claimedAt, "claim.claimedAt"),
    finalizedAt: options.finalizedAt,
    model: options.spec.model,
    runtime: options.spec.runtime,
    isolation: options.spec.isolation,
    controls: options.spec.controls,
    provenance: {
      ...(options.provenance as JsonObject),
      workspaceFinal: options.workspaceFinalProvenance as JsonObject,
    },
    hashes: {
      normalizedSpecSha256: sha256(await fs.readFile(path.join(options.evidenceDir, "spec.json"))),
      inputManifestSha256: sha256(inputManifestRaw),
      initialWorkerVisibleSha256: sha256(initialWorkerRaw),
      initialWorkerTreeDigest: initialWorker.digest,
      finalWorkerTreeDigest: options.finalWorkerVisible.digest,
      finalWorkspaceSnapshotDigest: options.finalWorkerVisible.digest,
      evaluationSha256: sha256(await fs.readFile(path.join(options.evidenceDir, "evaluation.json"))),
      cliSha256: stringAt(cliArtifact.sha256, "input manifest.artifacts.cli.sha256"),
      promptSha256: stringAt(promptArtifact.sha256, "input manifest.artifacts.prompt.sha256"),
      rubricSha256: stringAt(rubricArtifact.sha256, "input manifest.artifacts.rubric.sha256"),
      harnessSha256: stringAt(harnessArtifact.sha256, "input manifest.artifacts.harness.sha256"),
    },
    objectiveAssertions: options.evaluation.objectiveAssertions as unknown as JsonValue[],
    subjectiveRatings: options.evaluation.subjectiveRatings as unknown as JsonValue[],
    traces: options.traceRecords as unknown as JsonValue[],
    notes: options.evaluation.notes,
    contamination: { canaryHits: options.canaryHits },
    eligibility: deriveEligibility(
      options.spec,
      options.effectiveStatus,
      options.provenance,
      options.canaryHits,
      options.evaluation,
      options.traceRecords,
    ),
  };
  validateResultDocument(result);
  return result;
}

export async function finalizeRun(options: { runDir: string; ownerToken: string; preparedRootSha256: string; evaluationPath: string }): Promise<BenchmarkResult> {
  const layout = await assertRunDirectoryLayout(path.resolve(options.runDir));
  const { runDir, evidenceDir, workspaceDir } = layout;
  const claim = await enforceOwner(runDir, options.ownerToken);
  const runId = stringAt(claim.runId, "claim.runId");
  const state = objectAt(await readJson(path.join(evidenceDir, "run-state.json")), "run state");
  if (state.state !== "prepared") throw new Error(`Run must be prepared before finalization; current state is ${String(state.state)}`);
  await verifyPreparedSeal(evidenceDir, runId, options.ownerToken, options.preparedRootSha256, true);

  // Complete every read-only validation before taking the non-recoverable lock.
  const spec = validateRunSpecDocument(await readJson(path.join(evidenceDir, "spec.json")), evidenceDir);
  const forbiddenEvaluationRoots = [
    { label: "runDir", path: await fs.realpath(runDir) },
    { label: "workspace", path: await fs.realpath(workspaceDir) },
    { label: "evidence", path: await fs.realpath(evidenceDir) },
    { label: "sourceRepo", path: await fs.realpath(spec.sourceRepo) },
  ];
  const requestedEvaluationPath = path.resolve(options.evaluationPath);
  const evaluationStat = await fs.lstat(requestedEvaluationPath).catch(() => null);
  if (!evaluationStat || !evaluationStat.isFile() || evaluationStat.isSymbolicLink() || evaluationStat.nlink !== 1) {
    throw new Error(`Evaluation input must be an external, singly-linked regular file: ${requestedEvaluationPath}`);
  }
  const evaluationPath = await fs.realpath(requestedEvaluationPath);
  assertOutsideRoots(evaluationPath, forbiddenEvaluationRoots, "Evaluation input");
  const evaluation = validateEvaluationDocument(await readJson(evaluationPath), path.dirname(evaluationPath));
  const preflightTraceFingerprints = new Map<string, { sha256: string; bytes: number }>();
  for (const trace of evaluation.traces) {
    const stat = await fs.lstat(trace.path).catch(() => null);
    if (!stat || !stat.isFile() || stat.isSymbolicLink() || stat.nlink !== 1) throw new Error(`Evaluation trace must be an external, singly-linked regular file: ${trace.path}`);
    const resolvedTracePath = await fs.realpath(trace.path);
    assertOutsideRoots(resolvedTracePath, forbiddenEvaluationRoots, `Evaluation trace ${trace.name}`);
    trace.path = resolvedTracePath;
    const content = await fs.readFile(resolvedTracePath);
    preflightTraceFingerprints.set(trace.name, { sha256: sha256(content), bytes: content.byteLength });
  }
  const provenance = objectAt(await readJson(path.join(evidenceDir, "provenance.json")), "provenance");
  const workspaceBaseline = objectAt(provenance.workspaceBaseline, "provenance.workspaceBaseline");
  const baselineCommit = stringAt(workspaceBaseline.commit, "provenance.workspaceBaseline.commit");
  const baselineTree = stringAt(workspaceBaseline.tree, "provenance.workspaceBaseline.tree");
  const actualBaselineTree = (await runGit(workspaceDir, ["rev-parse", `${baselineCommit}^{tree}`])).toString("utf8").trim();
  if (actualBaselineTree !== baselineTree) throw new Error("Workspace baseline Git tree does not match prepared provenance");
  const recordedInitialWorker = validateTreeManifestDocument(await readJson(path.join(evidenceDir, "worker-visible.json")), "worker-visible manifest");
  const reconstructedInitialWorker = await buildGitTreeManifest(workspaceDir, baselineCommit);
  if (!compareCanonical(recordedInitialWorker, reconstructedInitialWorker)) throw new Error("Prepared worker-visible manifest does not match its Git baseline");
  const canary = (await fs.readFile(path.join(evidenceDir, "canary.txt"), "utf8")).trimEnd();
  const preflightCanaryHits = await scanCanary(workspaceDir, canary, { allowGitDirectory: true });
  const preflightWorkerVisible = await buildTreeManifest(workspaceDir, { allowGitDirectory: true });

  try {
    await writeCanonicalJson(path.join(evidenceDir, "finalize.lock.json"), {
      schemaVersion: 1,
      runId,
      ownerTokenSha256: stringAt(claim.ownerTokenSha256, "claim.ownerTokenSha256"),
      preparedRootSha256: options.preparedRootSha256,
      acquiredAt: new Date().toISOString(),
      hostname: os.hostname(),
      pid: process.pid,
    }, true);
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === "EEXIST") {
      throw new Error("Finalization is already claimed; stale finalize locks require explicit recovery and are never auto-stolen");
    }
    throw error;
  }

  const lockedWorkerVisibleBeforeGit = await buildTreeManifest(workspaceDir, { allowGitDirectory: true });
  if (!compareCanonical(preflightWorkerVisible, lockedWorkerVisibleBeforeGit)) throw new Error("Worker workspace changed during finalization preflight");
  const workspaceFinalCapture = await captureWorkspaceGit(workspaceDir, baselineCommit);
  const finalWorkerVisible = await buildTreeManifest(workspaceDir, { allowGitDirectory: true });
  if (!compareCanonical(lockedWorkerVisibleBeforeGit, finalWorkerVisible)) throw new Error("Worker workspace changed while final Git provenance was captured");
  const canaryHits = await scanCanary(workspaceDir, canary, { allowGitDirectory: true });
  if (!compareCanonical(preflightCanaryHits, canaryHits)) throw new Error("Worker workspace contamination changed during finalization");
  await snapshotFinalWorkspace(workspaceDir, evidenceDir, finalWorkerVisible);

  const traceRecords = await copyEvaluationTraces(evaluation, evidenceDir);
  for (const trace of traceRecords) {
    const preflight = preflightTraceFingerprints.get(trace.name);
    if (!preflight || preflight.sha256 !== trace.sha256 || preflight.bytes !== trace.bytes) {
      throw new Error(`Evaluation trace changed during finalization: ${trace.name}`);
    }
  }
  const canonicalEvaluation = {
    schemaVersion: 1,
    status: evaluation.status,
    objectiveAssertions: evaluation.objectiveAssertions,
    subjectiveRatings: evaluation.subjectiveRatings,
    traces: traceRecords,
    notes: evaluation.notes,
  };
  await writeCanonicalJson(path.join(evidenceDir, "evaluation.json"), canonicalEvaluation);
  const effectiveStatus = deriveEffectiveStatus(evaluation.status, canaryHits);
  await writeCanonicalJson(path.join(evidenceDir, "final-worker-visible.json"), finalWorkerVisible);
  await fs.mkdir(path.join(evidenceDir, "workspace-final"), { recursive: true });
  await atomicWrite(path.join(evidenceDir, "workspace-final", "git-status.txt"), workspaceFinalCapture.status);
  await atomicWrite(path.join(evidenceDir, "workspace-final", "git-diff-staged.patch"), workspaceFinalCapture.stagedDiff);
  await atomicWrite(path.join(evidenceDir, "workspace-final", "git-diff-unstaged.patch"), workspaceFinalCapture.unstagedDiff);
  await atomicWrite(path.join(evidenceDir, "workspace-final", "git-diff-baseline.patch"), workspaceFinalCapture.baselineDiff);
  await atomicWrite(path.join(evidenceDir, "workspace-final", "git-diff-baseline-to-head.patch"), workspaceFinalCapture.baselineToHeadDiff);
  const workspaceFinalProvenance = {
    baselineCommit,
    baselineTree: stringAt(workspaceBaseline.tree, "provenance.workspaceBaseline.tree"),
    head: workspaceFinalCapture.head,
    tree: workspaceFinalCapture.tree,
    status: workspaceFinalCapture.status,
    statusSha256: workspaceFinalCapture.statusSha256,
    stagedDiffSha256: workspaceFinalCapture.stagedDiffSha256,
    unstagedDiffSha256: workspaceFinalCapture.unstagedDiffSha256,
    baselineDiffSha256: workspaceFinalCapture.baselineDiffSha256,
    baselineToHeadDiffSha256: workspaceFinalCapture.baselineToHeadDiffSha256,
    clean: workspaceFinalCapture.clean,
  };
  await writeCanonicalJson(path.join(evidenceDir, "workspace-final-provenance.json"), {
    schemaVersion: 1,
    ...workspaceFinalProvenance,
  });
  const finalizedAt = new Date().toISOString();
  const result = await assembleResult({
    claim,
    spec,
    evaluation,
    traceRecords,
    effectiveStatus,
    finalizedAt,
    canaryHits,
    provenance,
    workspaceFinalProvenance,
    finalWorkerVisible,
    evidenceDir,
  });
  await writeCanonicalJson(path.join(evidenceDir, "result.json"), result);
  const report = renderResult(result);
  await atomicWrite(path.join(evidenceDir, "report.md"), report);
  await writeState(evidenceDir, {
    schemaVersion: 1,
    runId: result.runId,
    state: effectiveStatus,
    updatedAt: result.finalizedAt,
  });
  await writeEvidenceChecksums(evidenceDir);
  await writeFinalSeal(evidenceDir, result.runId, options.ownerToken);
  return result;
}

async function listRegularFiles(root: string, excludedRelative: Set<string> = new Set()): Promise<string[]> {
  const files: string[] = [];
  async function walk(directory: string): Promise<void> {
    const children = await fs.readdir(directory, { withFileTypes: true });
    children.sort((left, right) => canonicalTextCompare(left.name, right.name));
    for (const child of children) {
      const absolute = path.join(directory, child.name);
      const relative = normalizeTarget(path.relative(root, absolute), `evidence path ${absolute}`, false);
      const stat = await fs.lstat(absolute);
      if (stat.isSymbolicLink()) throw new Error(`Evidence may not contain a symbolic link or junction: ${absolute}`);
      if (excludedRelative.has(relative)) {
        if (!stat.isFile()) throw new Error(`Excluded evidence path is not a regular file: ${absolute}`);
        continue;
      }
      if (stat.isDirectory()) await walk(absolute);
      else if (stat.isFile()) files.push(relative);
      else throw new Error(`Evidence contains unsupported entry: ${absolute}`);
    }
  }
  await walk(root);
  return files.sort(canonicalTextCompare);
}

async function evidenceChecksumDocument(evidenceDir: string): Promise<{ schemaVersion: 1; files: Array<{ path: string; sha256: string; bytes: number }> }> {
  const files = await listRegularFiles(evidenceDir, new Set(["checksums.json", "final-seal.json"]));
  const records = [];
  for (const relative of files) {
    const content = await fs.readFile(path.join(evidenceDir, ...relative.split("/")));
    records.push({ path: relative, sha256: sha256(content), bytes: content.byteLength });
  }
  return { schemaVersion: 1, files: records };
}

async function writeEvidenceChecksums(evidenceDir: string): Promise<void> {
  await writeCanonicalJson(path.join(evidenceDir, "checksums.json"), await evidenceChecksumDocument(evidenceDir));
}

function markdownCell(value: unknown): string {
  return String(value ?? "").replaceAll("|", "\\|").replaceAll("\r", " ").replaceAll("\n", " ");
}

function yesNo(value: unknown): string {
  return value === true ? "yes" : "no";
}

export function renderResult(value: BenchmarkResult | unknown): string {
  const result = validateResultDocument(value);
  const eligibility = objectAt(result.eligibility, "result.eligibility");
  const hashes = objectAt(result.hashes, "result.hashes");
  const provenance = objectAt(result.provenance, "result.provenance");
  const git = objectAt(provenance.git, "result.provenance.git");
  const invocation = objectAt(provenance.cliInvocation, "result.provenance.cliInvocation");
  const runtimeExecutable = objectAt(invocation.runtimeExecutable, "result.provenance.cliInvocation.runtimeExecutable");
  const commandExecutable = objectAt(invocation.commandExecutable, "result.provenance.cliInvocation.commandExecutable");
  const workspaceBaseline = objectAt(provenance.workspaceBaseline, "result.provenance.workspaceBaseline");
  const workspaceFinal = objectAt(provenance.workspaceFinal, "result.provenance.workspaceFinal");
  const assertions = result.objectiveAssertions as unknown as Array<Record<string, unknown>>;
  const ratings = result.subjectiveRatings as unknown as Array<Record<string, unknown>>;
  const traces = result.traces as unknown as Array<Record<string, unknown>>;
  const causalReasons = arrayAt(eligibility.causalReasonCodes, "result.eligibility.causalReasonCodes") as string[];
  const warnings = arrayAt(eligibility.warnings, "result.eligibility.warnings") as string[];
  const lines = [
    `# Benchmark run ${result.runId}`,
    "",
    `- Experiment: ${result.experimentId}`,
    `- Case / arm / replicate: ${result.caseId} / ${result.armId} / ${result.replicate}`,
    `- Evidence class: ${result.evidenceClass}`,
    `- Status: ${result.status}`,
    `- Prepared: ${result.preparedAt}`,
    `- Finalized: ${result.finalizedAt}`,
    "",
    "## Eligibility",
    "",
    `- Engineering eligible: ${yesNo(eligibility.engineeringEligible)}`,
    `- Causal eligible: ${yesNo(eligibility.causalEligible)}`,
    `- Public eligible: ${yesNo(eligibility.publicEligible)}`,
    `- Causal reason codes: ${causalReasons.length > 0 ? causalReasons.join(", ") : "none"}`,
    `- Warnings: ${warnings.length > 0 ? warnings.join(", ") : "none"}`,
    "",
    "Public eligibility is intentionally never granted by this P0 runner; corpus-level review is separate.",
    "",
    "## Provenance",
    "",
    `- Git HEAD: ${git.head}`,
    `- Git tree: ${git.tree}`,
    `- Git clean: ${yesNo(git.clean)}`,
    `- Git status SHA-256: ${git.statusSha256}`,
    `- Staged diff SHA-256: ${git.stagedDiffSha256}`,
    `- Unstaged diff SHA-256: ${git.unstagedDiffSha256}`,
    `- CLI command: ${markdownCell((invocation.command as unknown[]).join(" "))}`,
    `- Command executable SHA-256: ${commandExecutable.sha256}`,
    `- Runtime executable SHA-256: ${runtimeExecutable.sha256}`,
    `- CLI SHA-256: ${hashes.cliSha256}`,
    `- Harness producer SHA-256: ${hashes.harnessSha256}`,
    `- Prompt SHA-256: ${hashes.promptSha256}`,
    `- Rubric SHA-256: ${hashes.rubricSha256}`,
    `- Initial worker tree: ${hashes.initialWorkerTreeDigest}`,
    `- Final worker tree: ${hashes.finalWorkerTreeDigest}`,
    `- Final workspace snapshot: ${hashes.finalWorkspaceSnapshotDigest}`,
    `- Workspace baseline commit: ${workspaceBaseline.commit}`,
    `- Workspace baseline tree: ${workspaceBaseline.tree}`,
    `- Workspace final HEAD: ${workspaceFinal.head}`,
    `- Workspace final tree: ${workspaceFinal.tree}`,
    `- Workspace final status SHA-256: ${workspaceFinal.statusSha256}`,
    `- Workspace baseline delta SHA-256: ${workspaceFinal.baselineDiffSha256}`,
    `- Workspace baseline-to-HEAD SHA-256: ${workspaceFinal.baselineToHeadDiffSha256}`,
    "",
    "## Objective assertions",
    "",
    "| ID | Status | Exit | Duration (ms) | Command | Note |",
    "| --- | --- | ---: | ---: | --- | --- |",
    ...assertions.map((assertion) => `| ${markdownCell(assertion.id)} | ${markdownCell(assertion.status)} | ${markdownCell(assertion.exitCode)} | ${markdownCell(assertion.durationMs)} | ${markdownCell(assertion.command)} | ${markdownCell(assertion.note)} |`),
    ...(assertions.length === 0 ? ["| none | — | — | — | — | — |"] : []),
    "",
    "## Subjective ratings",
    "",
    "| ID | Score | Maximum | Rationale |",
    "| --- | ---: | ---: | --- |",
    ...ratings.map((rating) => `| ${markdownCell(rating.id)} | ${markdownCell(rating.score)} | ${markdownCell(rating.maxScore)} | ${markdownCell(rating.rationale)} |`),
    ...(ratings.length === 0 ? ["| none | — | — | — |"] : []),
    "",
    "## Trace artifacts",
    "",
    "| Name | SHA-256 | Bytes | Evidence path |",
    "| --- | --- | ---: | --- |",
    ...traces.map((trace) => `| ${markdownCell(trace.name)} | ${markdownCell(trace.sha256)} | ${markdownCell(trace.bytes)} | ${markdownCell(trace.evidencePath)} |`),
    ...(traces.length === 0 ? ["| none | — | — | — |"] : []),
    "",
    "## Notes",
    "",
    ...((result.notes as unknown as string[]).length > 0 ? (result.notes as unknown as string[]).map((note) => `- ${note}`) : ["- none"]),
    "",
  ];
  return lines.join("\n");
}

export async function renderRun(runDir: string): Promise<string> {
  return renderResult(await readJson(path.join(path.resolve(runDir), "evidence", "result.json")));
}

function compareCanonical(actual: unknown, expected: unknown): boolean {
  return stableStringify(actual) === stableStringify(expected);
}

export async function validateRun(
  runDirInput: string,
  credentials?: { ownerToken: string; preparedRootSha256: string },
): Promise<ValidationResult> {
  const errors: string[] = [];
  let layout: { runDir: string; evidenceDir: string; workspaceDir: string };
  try {
    layout = await assertRunDirectoryLayout(path.resolve(runDirInput));
  } catch (error) {
    return { ok: false, errors: [`run layout: ${error instanceof Error ? error.message : String(error)}`] };
  }
  const { runDir, evidenceDir, workspaceDir } = layout;
  const capture = async (label: string, action: () => Promise<void> | void): Promise<void> => {
    try {
      await action();
    } catch (error) {
      errors.push(`${label}: ${error instanceof Error ? error.message : String(error)}`);
    }
  };

  let stateValue: string | null = null;
  let claim: Record<string, unknown> | null = null;
  let actualCanaryHits: string[] = [];
  await capture("claim", async () => { claim = await readClaim(runDir); });
  await capture("authentication credentials", async () => {
    if (!credentials) throw new Error("validate requires ownerToken and preparedRootSha256");
    await enforceOwner(runDir, credentials.ownerToken);
    if (!SHA256_PATTERN.test(credentials.preparedRootSha256)) throw new Error("preparedRootSha256 must be a SHA-256 digest");
  });
  await capture("run state", async () => {
    const state = objectAt(await readJson(path.join(evidenceDir, "run-state.json")), "run state");
    noUnknown(state, ["schemaVersion", "runId", "state", "updatedAt"], "run state");
    if (integerAt(state.schemaVersion, "run state.schemaVersion") !== 1) throw new Error("schemaVersion must be 1");
    stateValue = enumAt(state.state, ["prepared", "complete", "invalid", "aborted"] as const, "run state.state");
    if (claim && state.runId !== claim.runId) throw new Error("run state runId does not match claim");
  });
  await capture("prepared evidence seal", async () => {
    if (!credentials || !claim || !stateValue) throw new Error("authentication or run state is unavailable");
    await verifyPreparedSeal(
      evidenceDir,
      stringAt(claim.runId, "claim.runId"),
      credentials.ownerToken,
      credentials.preparedRootSha256,
      stateValue === "prepared",
    );
  });
  await capture("spec", async () => { validateRunSpecDocument(await readJson(path.join(evidenceDir, "spec.json")), evidenceDir); });
  await capture("worker-visible manifest", async () => { validateTreeManifestDocument(await readJson(path.join(evidenceDir, "worker-visible.json")), "worker-visible manifest"); });
  await capture("canonical JSON", async () => {
    const generatedNames = [
      "claim.json", "spec.json", "input-manifest.json", "worker-visible.json", "provenance.json", "run-state.json",
      "prepared-seal.json", "finalize.lock.json", "evaluation.json", "final-worker-visible.json",
      "workspace-final-provenance.json", "result.json", "checksums.json", "final-seal.json",
    ];
    for (const name of generatedNames) {
      const file = path.join(evidenceDir, name);
      const raw = await fs.readFile(file, "utf8").catch((error) => {
        if ((error as NodeJS.ErrnoException).code === "ENOENT") return null;
        throw error;
      });
      if (raw === null) continue;
      const parsed = JSON.parse(raw);
      if (raw !== stableStringify(parsed)) throw new Error(`${name} is not canonical JSON`);
    }
  });
  await capture("snapshot files", async () => {
    const manifest = objectAt(await readJson(path.join(evidenceDir, "input-manifest.json")), "input manifest");
    const components = arrayAt(manifest.components, "input manifest.components");
    const artifacts = objectAt(manifest.artifacts, "input manifest.artifacts");
    const records: Record<string, unknown>[] = [
      ...components.map((entry, index) => objectAt(entry, `input manifest.components[${index}]`)),
      ...["cli", "prompt", "rubric", "harness", "sourceSpec"].map((name) => objectAt(artifacts[name], `input manifest.artifacts.${name}`)),
    ];
    for (const record of records) {
      const snapshotPath = normalizeTarget(stringAt(record.snapshotPath, "snapshotPath"), "snapshotPath", false);
      const content = await fs.readFile(path.join(evidenceDir, ...snapshotPath.split("/")));
      if (sha256(content) !== stringAt(record.sha256, "snapshot sha256")) throw new Error(`snapshot hash mismatch: ${snapshotPath}`);
      if (content.byteLength !== integerAt(record.bytes, "snapshot bytes", 0)) throw new Error(`snapshot byte count mismatch: ${snapshotPath}`);
    }
    const harness = objectAt(artifacts.harness, "input manifest.artifacts.harness");
    const currentHarness = await fs.readFile(HARNESS_SOURCE_PATH);
    if (
      sha256(currentHarness) !== stringAt(harness.sha256, "input manifest.artifacts.harness.sha256")
      || currentHarness.byteLength !== integerAt(harness.bytes, "input manifest.artifacts.harness.bytes", 1)
    ) {
      throw new Error("Current harness bytes do not match the snapshotted producer; validate with the recorded harness version");
    }
  });
  await capture("provenance", async () => {
    const provenance = objectAt(await readJson(path.join(evidenceDir, "provenance.json")), "provenance");
    const git = objectAt(provenance.git, "provenance.git");
    const recordedGitExecutable = objectAt(provenance.gitExecutable, "provenance.gitExecutable");
    if (!path.isAbsolute(stringAt(recordedGitExecutable.path, "provenance.gitExecutable.path"))) throw new Error("recorded Git executable path is not absolute");
    if (!SHA256_PATTERN.test(stringAt(recordedGitExecutable.sha256, "provenance.gitExecutable.sha256"))) throw new Error("recorded Git executable hash is invalid");
    integerAt(recordedGitExecutable.bytes, "provenance.gitExecutable.bytes", 1);
    const gitControl = objectAt(provenance.gitControl, "provenance.gitControl");
    if (!compareCanonical(gitControl.environment, GIT_CONTROL_ENVIRONMENT)) throw new Error("recorded Git control environment does not match the runner policy");
    if (!compareCanonical(gitControl.commandConfig, GIT_COMMAND_CONFIG)) throw new Error("recorded Git command configuration does not match the runner policy");
    const traces = objectAt(provenance.traceHashes, "provenance.traceHashes");
    const status = await fs.readFile(path.join(evidenceDir, "provenance", "git-status.txt"));
    const staged = await fs.readFile(path.join(evidenceDir, "provenance", "git-diff-staged.patch"));
    const unstaged = await fs.readFile(path.join(evidenceDir, "provenance", "git-diff-unstaged.patch"));
    const cliStdout = await fs.readFile(path.join(evidenceDir, "provenance", "cli-stdout.txt"));
    const cliStderr = await fs.readFile(path.join(evidenceDir, "provenance", "cli-stderr.txt"));
    const workspaceBaselineStatus = await fs.readFile(path.join(evidenceDir, "provenance", "workspace-baseline-status.txt"));
    if (sha256(status) !== git.statusSha256 || sha256(status) !== traces.gitStatus) throw new Error("git status trace hash mismatch");
    if (sha256(staged) !== git.stagedDiffSha256 || sha256(staged) !== traces.gitDiffStaged) throw new Error("staged diff trace hash mismatch");
    if (sha256(unstaged) !== git.unstagedDiffSha256 || sha256(unstaged) !== traces.gitDiffUnstaged) throw new Error("unstaged diff trace hash mismatch");
    if (status.toString("utf8") !== git.status) throw new Error("full git status does not match trace");
    const invocation = objectAt(provenance.cliInvocation, "provenance.cliInvocation");
    const command = arrayAt(invocation.command, "provenance.cliInvocation.command").map((item, index) => stringAt(item, `provenance.cliInvocation.command[${index}]`, { nonempty: false }));
    if (command.length === 0 || !path.isAbsolute(command[0])) throw new Error("recorded CLI command does not use an absolute executable");
    if (integerAt(invocation.exitCode, "provenance.cliInvocation.exitCode") !== 0) throw new Error("recorded CLI invocation did not succeed");
    if (sha256(cliStdout) !== invocation.stdoutSha256 || sha256(cliStdout) !== traces.cliStdout) throw new Error("CLI stdout trace hash mismatch");
    if (sha256(cliStderr) !== invocation.stderrSha256 || sha256(cliStderr) !== traces.cliStderr) throw new Error("CLI stderr trace hash mismatch");
    const runtimeExecutable = objectAt(invocation.runtimeExecutable, "provenance.cliInvocation.runtimeExecutable");
    if (!path.isAbsolute(stringAt(runtimeExecutable.path, "provenance.cliInvocation.runtimeExecutable.path"))) throw new Error("runtime executable path is not absolute");
    if (!SHA256_PATTERN.test(stringAt(runtimeExecutable.sha256, "provenance.cliInvocation.runtimeExecutable.sha256"))) throw new Error("runtime executable hash is invalid");
    integerAt(runtimeExecutable.bytes, "provenance.cliInvocation.runtimeExecutable.bytes", 1);
    const commandExecutable = objectAt(invocation.commandExecutable, "provenance.cliInvocation.commandExecutable");
    if (path.resolve(stringAt(commandExecutable.path, "provenance.cliInvocation.commandExecutable.path")) !== path.resolve(command[0])) throw new Error("command executable provenance does not match argv[0]");
    if (!SHA256_PATTERN.test(stringAt(commandExecutable.sha256, "provenance.cliInvocation.commandExecutable.sha256"))) throw new Error("command executable hash is invalid");
    integerAt(commandExecutable.bytes, "provenance.cliInvocation.commandExecutable.bytes", 1);
    const baseline = objectAt(provenance.workspaceBaseline, "provenance.workspaceBaseline");
    if (workspaceBaselineStatus.toString("utf8") !== baseline.status) throw new Error("workspace baseline status does not match trace");
    if (sha256(workspaceBaselineStatus) !== baseline.statusSha256 || sha256(workspaceBaselineStatus) !== traces.workspaceBaselineStatus) throw new Error("workspace baseline status trace hash mismatch");
    if (!/^[0-9a-f]{40,64}$/i.test(stringAt(baseline.commit, "provenance.workspaceBaseline.commit"))) throw new Error("workspace baseline commit is invalid");
    if (!/^[0-9a-f]{40,64}$/i.test(stringAt(baseline.tree, "provenance.workspaceBaseline.tree"))) throw new Error("workspace baseline tree is invalid");
  });
  await capture("workspace baseline reconstruction", async () => {
    const provenance = objectAt(await readJson(path.join(evidenceDir, "provenance.json")), "provenance");
    const baseline = objectAt(provenance.workspaceBaseline, "provenance.workspaceBaseline");
    const commit = stringAt(baseline.commit, "provenance.workspaceBaseline.commit");
    const recordedTree = stringAt(baseline.tree, "provenance.workspaceBaseline.tree");
    const actualTree = (await runGit(workspaceDir, ["rev-parse", `${commit}^{tree}`])).toString("utf8").trim();
    if (recordedTree !== actualTree) throw new Error("baseline commit tree does not match prepared provenance");
    const recordedManifest = validateTreeManifestDocument(await readJson(path.join(evidenceDir, "worker-visible.json")), "worker-visible manifest");
    const reconstructedManifest = await buildGitTreeManifest(workspaceDir, commit);
    if (!compareCanonical(recordedManifest, reconstructedManifest)) throw new Error("worker-visible manifest does not match baseline Git tree");
  });
  if (stateValue === "prepared") {
    await capture("prepared worker workspace", async () => {
      const recordedManifest = validateTreeManifestDocument(
        await readJson(path.join(evidenceDir, "worker-visible.json")),
        "worker-visible manifest",
      );
      const liveManifest = await buildTreeManifest(workspaceDir, { allowGitDirectory: true });
      if (!compareCanonical(recordedManifest, liveManifest)) {
        throw new Error("worker workspace does not match the sealed prepared worker-visible manifest");
      }
    });
  }
  await capture("canary separation", async () => {
    const canary = (await fs.readFile(path.join(evidenceDir, "canary.txt"), "utf8")).trimEnd();
    actualCanaryHits = await scanCanary(workspaceDir, canary, { allowGitDirectory: true });
    if (actualCanaryHits.length > 0 && stateValue === "prepared") throw new Error(`canary present in worker workspace: ${actualCanaryHits.join(", ")}`);
  });

  if (stateValue && FINAL_STATES.has(stateValue)) {
    await capture("finalize lock", async () => {
      const lock = objectAt(await readJson(path.join(evidenceDir, "finalize.lock.json")), "finalize lock");
      noUnknown(lock, ["schemaVersion", "runId", "ownerTokenSha256", "preparedRootSha256", "acquiredAt", "hostname", "pid"], "finalize lock");
      if (claim && (lock.runId !== claim.runId || lock.ownerTokenSha256 !== claim.ownerTokenSha256)) throw new Error("finalize lock does not match claim ownership");
      if (!credentials || lock.preparedRootSha256 !== credentials.preparedRootSha256) throw new Error("finalize lock does not match the externally supplied prepared root");
    });
    await capture("result schema", async () => {
      const result = validateResultDocument(await readJson(path.join(evidenceDir, "result.json")));
      const contamination = objectAt(result.contamination, "result.contamination");
      if (!compareCanonical(contamination.canaryHits, actualCanaryHits)) throw new Error("recorded canary hits do not match worker workspace scan");
      if (actualCanaryHits.length > 0 && result.status !== "invalid") throw new Error("a canary-exposed run must be invalid");
    });
    await capture("evaluation schema", async () => { await validateStoredEvaluation(await readJson(path.join(evidenceDir, "evaluation.json")), evidenceDir); });
    await capture("result and evaluation consistency", async () => {
      const result = validateResultDocument(await readJson(path.join(evidenceDir, "result.json")));
      const evaluation = objectAt(await readJson(path.join(evidenceDir, "evaluation.json")), "canonical evaluation");
      for (const field of ["objectiveAssertions", "subjectiveRatings", "traces", "notes"] as const) {
        if (!compareCanonical(result[field], evaluation[field])) throw new Error(`result.${field} does not match canonical evaluation`);
      }
    });
    await capture("final worker manifest", async () => {
      const recorded = validateTreeManifestDocument(await readJson(path.join(evidenceDir, "final-worker-visible.json")), "final worker manifest");
      const actual = await buildTreeManifest(workspaceDir, { allowGitDirectory: true });
      if (!compareCanonical(recorded, actual)) throw new Error("worker workspace changed after finalization");
      const snapshotted = await buildTreeManifest(path.join(evidenceDir, "final-workspace"));
      if (!compareCanonical(recorded, snapshotted)) throw new Error("final-workspace snapshot does not reconstruct the final worker manifest");
    });
    await capture("deterministic report", async () => {
      const result = validateResultDocument(await readJson(path.join(evidenceDir, "result.json")));
      const report = await fs.readFile(path.join(evidenceDir, "report.md"), "utf8");
      if (report !== renderResult(result)) throw new Error("report.md is not the deterministic rendering of result.json");
    });
    await capture("workspace final provenance", async () => {
      const recorded = objectAt(await readJson(path.join(evidenceDir, "workspace-final-provenance.json")), "workspace final provenance");
      noUnknown(recorded, ["schemaVersion", "baselineCommit", "baselineTree", "head", "tree", "status", "statusSha256", "stagedDiffSha256", "unstagedDiffSha256", "baselineDiffSha256", "baselineToHeadDiffSha256", "clean"], "workspace final provenance");
      const status = await fs.readFile(path.join(evidenceDir, "workspace-final", "git-status.txt"));
      const staged = await fs.readFile(path.join(evidenceDir, "workspace-final", "git-diff-staged.patch"));
      const unstaged = await fs.readFile(path.join(evidenceDir, "workspace-final", "git-diff-unstaged.patch"));
      const baseline = await fs.readFile(path.join(evidenceDir, "workspace-final", "git-diff-baseline.patch"));
      const baselineToHead = await fs.readFile(path.join(evidenceDir, "workspace-final", "git-diff-baseline-to-head.patch"));
      if (status.toString("utf8") !== recorded.status || sha256(status) !== recorded.statusSha256) throw new Error("workspace final status mismatch");
      if (sha256(staged) !== recorded.stagedDiffSha256) throw new Error("workspace staged diff hash mismatch");
      if (sha256(unstaged) !== recorded.unstagedDiffSha256) throw new Error("workspace unstaged diff hash mismatch");
      if (sha256(baseline) !== recorded.baselineDiffSha256) throw new Error("workspace baseline diff hash mismatch");
      if (sha256(baselineToHead) !== recorded.baselineToHeadDiffSha256) throw new Error("workspace baseline-to-HEAD diff hash mismatch");
      const live = await captureWorkspaceGit(workspaceDir, stringAt(recorded.baselineCommit, "workspace final provenance.baselineCommit"));
      const liveProvenance = {
        baselineCommit: recorded.baselineCommit,
        baselineTree: recorded.baselineTree,
        head: live.head,
        tree: live.tree,
        status: live.status,
        statusSha256: live.statusSha256,
        stagedDiffSha256: live.stagedDiffSha256,
        unstagedDiffSha256: live.unstagedDiffSha256,
        baselineDiffSha256: live.baselineDiffSha256,
        baselineToHeadDiffSha256: live.baselineToHeadDiffSha256,
        clean: live.clean,
      };
      const { schemaVersion: _recordedSchemaVersion, ...recordedWithoutSchema } = recorded;
      if (!compareCanonical(liveProvenance, recordedWithoutSchema)) throw new Error("live workspace Git provenance differs from the finalized record");
      const result = validateResultDocument(await readJson(path.join(evidenceDir, "result.json")));
      const resultWorkspace = objectAt(objectAt(result.provenance, "result.provenance").workspaceFinal, "result.provenance.workspaceFinal");
      const { schemaVersion: _schemaVersion, ...withoutSchemaVersion } = recorded;
      if (!compareCanonical(resultWorkspace, withoutSchemaVersion)) throw new Error("result workspace provenance does not match recorded final provenance");
    });
    await capture("derived result semantics", async () => {
      if (!claim) throw new Error("claim is unavailable");
      const spec = validateRunSpecDocument(await readJson(path.join(evidenceDir, "spec.json")), evidenceDir);
      const storedEvaluationDocument = objectAt(await readJson(path.join(evidenceDir, "evaluation.json")), "canonical evaluation");
      const evaluation = await validateStoredEvaluation(storedEvaluationDocument, evidenceDir);
      const traceRecords = arrayAt(storedEvaluationDocument.traces, "canonical evaluation.traces").map((item, index): CanonicalTraceRecord => {
        const trace = objectAt(item, `canonical evaluation.traces[${index}]`);
        return {
          name: stringAt(trace.name, `canonical evaluation.traces[${index}].name`),
          sha256: stringAt(trace.sha256, `canonical evaluation.traces[${index}].sha256`),
          bytes: integerAt(trace.bytes, `canonical evaluation.traces[${index}].bytes`, 0),
          evidencePath: stringAt(trace.evidencePath, `canonical evaluation.traces[${index}].evidencePath`),
        };
      });
      const provenance = objectAt(await readJson(path.join(evidenceDir, "provenance.json")), "provenance");
      const workspaceFinalDocument = objectAt(await readJson(path.join(evidenceDir, "workspace-final-provenance.json")), "workspace final provenance");
      const { schemaVersion: _workspaceSchemaVersion, ...workspaceFinalProvenance } = workspaceFinalDocument;
      const finalWorkerVisible = validateTreeManifestDocument(await readJson(path.join(evidenceDir, "final-worker-visible.json")), "final worker manifest");
      const state = objectAt(await readJson(path.join(evidenceDir, "run-state.json")), "run state");
      const effectiveStatus = deriveEffectiveStatus(evaluation.status, actualCanaryHits);
      if (state.state !== effectiveStatus) throw new Error("run state is not derived from canonical evaluation and contamination evidence");
      const expected = await assembleResult({
        claim,
        spec,
        evaluation,
        traceRecords,
        effectiveStatus,
        finalizedAt: stringAt(state.updatedAt, "run state.updatedAt"),
        canaryHits: actualCanaryHits,
        provenance,
        workspaceFinalProvenance,
        finalWorkerVisible,
        evidenceDir,
      });
      const recorded = validateResultDocument(await readJson(path.join(evidenceDir, "result.json")));
      if (!compareCanonical(recorded, expected)) throw new Error("result.json is not the deterministic derivation of sealed inputs, evaluation, provenance, and workspace state");
    });
    await capture("evidence checksums", async () => {
      const recorded = await readJson(path.join(evidenceDir, "checksums.json"));
      const expected = await evidenceChecksumDocument(evidenceDir);
      if (!compareCanonical(recorded, expected)) throw new Error("checksums.json does not match the evidence directory");
    });
    await capture("final evidence seal", async () => {
      if (!credentials || !claim) throw new Error("authentication credentials are unavailable");
      await verifyFinalSeal(evidenceDir, stringAt(claim.runId, "claim.runId"), credentials.ownerToken);
    });
  }

  return { ok: errors.length === 0, errors };
}

function parseFlags(arguments_: string[]): { command: string; flags: Map<string, string> } {
  const [command, ...rest] = arguments_;
  if (!command) throw new Error("Expected one of: prepare, finalize, validate, render");
  const flags = new Map<string, string>();
  for (let index = 0; index < rest.length; index += 2) {
    const flag = rest[index];
    const value = rest[index + 1];
    if (!flag?.startsWith("--") || value === undefined || value.startsWith("--")) throw new Error(`Expected --name value, received: ${rest.slice(index).join(" ")}`);
    if (flags.has(flag)) throw new Error(`Duplicate flag: ${flag}`);
    flags.set(flag, value);
  }
  return { command, flags };
}

function requiredFlag(flags: Map<string, string>, name: string): string {
  const value = flags.get(name);
  if (!value) throw new Error(`Missing required flag: ${name}`);
  return value;
}

async function main(): Promise<void> {
  const { command, flags } = parseFlags(process.argv.slice(2));
  if (command === "prepare") {
    const allowed = new Set(["--spec", "--runs-root"]);
    for (const flag of flags.keys()) if (!allowed.has(flag)) throw new Error(`Unknown prepare flag: ${flag}`);
    const prepared = await prepareRun({ specPath: requiredFlag(flags, "--spec"), runsRoot: requiredFlag(flags, "--runs-root") });
    process.stdout.write(stableStringify(prepared));
    return;
  }
  if (command === "finalize") {
    const allowed = new Set(["--run", "--owner-token", "--prepared-root", "--evaluation"]);
    for (const flag of flags.keys()) if (!allowed.has(flag)) throw new Error(`Unknown finalize flag: ${flag}`);
    const result = await finalizeRun({
      runDir: requiredFlag(flags, "--run"),
      ownerToken: requiredFlag(flags, "--owner-token"),
      preparedRootSha256: requiredFlag(flags, "--prepared-root"),
      evaluationPath: requiredFlag(flags, "--evaluation"),
    });
    process.stdout.write(stableStringify({ runId: result.runId, status: result.status, eligibility: result.eligibility }));
    return;
  }
  if (command === "validate") {
    const allowed = new Set(["--run", "--owner-token", "--prepared-root"]);
    for (const flag of flags.keys()) if (!allowed.has(flag)) throw new Error(`Unknown validate flag: ${flag}`);
    const validation = await validateRun(requiredFlag(flags, "--run"), {
      ownerToken: requiredFlag(flags, "--owner-token"),
      preparedRootSha256: requiredFlag(flags, "--prepared-root"),
    });
    process.stdout.write(stableStringify(validation));
    if (!validation.ok) process.exitCode = 1;
    return;
  }
  if (command === "render") {
    const allowed = new Set(["--run"]);
    for (const flag of flags.keys()) if (!allowed.has(flag)) throw new Error(`Unknown render flag: ${flag}`);
    process.stdout.write(await renderRun(requiredFlag(flags, "--run")));
    return;
  }
  throw new Error(`Unknown command: ${command}`);
}

if (import.meta.main) {
  main().catch((error) => {
    process.stderr.write(`${error instanceof Error ? error.message : String(error)}\n`);
    process.exitCode = 1;
  });
}

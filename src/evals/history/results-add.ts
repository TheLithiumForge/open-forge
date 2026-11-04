import { existsSync } from "node:fs";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { homedir, tmpdir, userInfo } from "node:os";
import { dirname, join, relative } from "node:path";
import { AuthVariables, CodexAuthFile } from "../config/auth.ts";
import { CodexRolloutFile } from "../execution/providers/codex-telemetry.ts";
import { EvalPaths } from "../paths.ts";
import { publishedFile } from "./artifacts.ts";
import { exportBatch } from "./export.ts";
import { sha256 } from "./fingerprints.ts";
import type { ArtifactRecord } from "./records.types.ts";
import { Redacted } from "./sanitize.ts";
import type { HistoryStore } from "./store.ts";

/** The results repository's own scripts stage and validate what this command adds, so the repository stays the judge of its format. */
export const ResultsScripts = { stage: join("scripts", "stage.ts"), validate: join("scripts", "validate.ts") } as const;
export const LeakKind = { secret: "secret", machinePath: "machine path", userName: "user name" } as const;
export type LeakKind = (typeof LeakKind)[keyof typeof LeakKind];
const MinimumSecretLength = 16;

export interface Leak { readonly file: string; readonly kind: LeakKind }
export interface Machine { readonly home: string; readonly user: string }

export interface AddResultsServices {
  readonly run: (script: string, args: readonly string[], cwd: string) => Promise<number>;
  readonly secrets?: () => Promise<readonly string[]>;
  readonly machine?: Machine;
  /** The batch came from results import, so it was scrubbed when first published and this machine's user name is no secret. */
  readonly imported?: boolean;
}

const strings = (value: unknown): string[] => typeof value === "string" ? [value]
  : Array.isArray(value) ? value.flatMap(strings) : value !== null && typeof value === "object" ? Object.values(value).flatMap(strings) : [];

/** A token's middle part is plain JSON naming the account, such as its email and user id. */
function tokenClaims(value: string): string[] {
  const [, body, signature] = value.split(".");
  if (body === undefined || signature === undefined) return [];
  try { return strings(JSON.parse(Buffer.from(body, "base64url").toString("utf8"))); } catch { return []; }
}

/** The values a sign-in file holds, and its tokens' claims except web addresses, which name the provider and not the account. */
export function authFileSecrets(auth: unknown): string[] {
  const found = strings(auth);
  return [...found, ...found.flatMap(tokenClaims).filter((claim) => !URL.canParse(claim))];
}

/** Every sign-in value this machine holds, so evidence can be checked for them without printing any. */
export async function signInSecrets(): Promise<string[]> {
  const values = Object.values(AuthVariables).map((name) => process.env[name] ?? "");
  const auth = join(EvalPaths.codexAuthHome, CodexAuthFile);
  if (existsSync(auth)) values.push(...authFileSecrets(JSON.parse(await readFile(auth, "utf8"))));
  return values.filter((value) => value.length >= MinimumSecretLength);
}

// A Codex transcript's first line names the signed-in user and account. The leak check still stops these values anywhere else.
const TranscriptAccountFields = /"(creator_user_id|creator_account_id)":"[^"]*"/g;

/** A transcript without its account fields, or the same bytes when it has none. */
export function scrubbedTranscript(bytes: Buffer): Buffer {
  const text = bytes.toString("utf8");
  const scrubbed = text.replace(TranscriptAccountFields, (_, field: string) => `"${field}":"${Redacted.account}"`);
  return scrubbed === text ? bytes : Buffer.from(scrubbed);
}

/** Finds evidence that would publish a sign-in or name the maintainer's machine. It reports files, never the values found. */
export function evidenceLeaks(files: ReadonlyMap<string, Buffer>, secrets: readonly string[], machine: Machine): Leak[] {
  const homes = [machine.home, machine.home.replaceAll("\\", "/"), machine.home.replace(/^([a-z]):\\/i, (_, drive: string) => `/${drive.toLowerCase()}/`).replaceAll("\\", "/")];
  const escaped = machine.user.replaceAll(/[.*+?^${}()|[\]\\]/g, "\\$&");
  // Encoded blobs such as signed reasoning can hold the name by chance, so it must stand apart from base64 characters.
  const user = new RegExp(`(?<![A-Za-z0-9+/=_-])${escaped}(?![A-Za-z0-9+/=_-])`, "i");
  const leaks: Leak[] = [];
  for (const [file, bytes] of files) {
    const text = bytes.toString("utf8"), lower = text.toLowerCase();
    if (secrets.some((secret) => bytes.includes(secret))) leaks.push({ file, kind: LeakKind.secret });
    else if (homes.some((home) => lower.includes(home.toLowerCase()))) leaks.push({ file, kind: LeakKind.machinePath });
    else if (machine.user.length > 0 && user.test(text)) leaks.push({ file, kind: LeakKind.userName });
  }
  return leaks;
}

/** Adds a finished batch, with its evidence, to a results repository, then validates the whole repository. */
export async function addResults(store: HistoryStore, batchId: string, repository: string, services: AddResultsServices): Promise<string> {
  if (!existsSync(join(repository, ResultsScripts.stage))) throw new Error(`${repository} isn't a results repository. Run git submodule update --init src/evals/results.`);
  if (!existsSync(join(repository, "node_modules"))) throw new Error(`Install the results repository first: npm ci --prefix ${repository}`);
  const batch = await store.readBatch(batchId);
  // Run evidence sits in each run folder and grading evidence in the assessment overlay. Staging reads both from one folder.
  const files = new Map<string, Buffer>();
  const artifacts: ArtifactRecord[] = [];
  for (const artifact of (await store.readArtifacts(batch)).filter(publishedFile)) {
    const [name, folder] = artifact.assessmentId === null ? [join("runs", artifact.runId, artifact.path), store.runFolder(batch.id, artifact.runId)]
      : [join("assessments", artifact.assessmentId, artifact.path), store.assessmentEvidenceFolder(batch.id, artifact.assessmentId)];
    const stored = await readFile(join(folder, artifact.path));
    const bytes = artifact.path === CodexRolloutFile ? scrubbedTranscript(stored) : stored;
    files.set(name, bytes);
    artifacts.push(bytes === stored ? artifact : { ...artifact, bytes: bytes.length, sha256: sha256(bytes) });
  }
  const machine = services.machine ?? { home: homedir(), user: userInfo().username };
  const leaks = evidenceLeaks(files, await (services.secrets ?? signInSecrets)(), services.imported === true ? { ...machine, user: "" } : machine);
  if (leaks.length > 0) throw new Error(`Evidence would publish private data, so nothing was added:\n${leaks.map((leak) => `${leak.kind}: ${leak.file}`).join("\n")}`);
  const scratch = await mkdtemp(join(tmpdir(), "eval-results-add-"));
  try {
    const exported = await exportBatch(store, batchId, join(scratch, "records"), { scrubMachineUser: services.imported !== true, artifacts });
    const evidence = join(scratch, "evidence");
    for (const [name, bytes] of files) {
      await mkdir(dirname(join(evidence, name)), { recursive: true });
      await writeFile(join(evidence, name), bytes);
    }
    if (await services.run(ResultsScripts.stage, [exported, "--artifacts", evidence], repository) !== 0) throw new Error("Staging the batch failed.");
    if (await services.run(ResultsScripts.validate, [], repository) !== 0) throw new Error("The results repository failed validation.");
    return join(repository, relative(join(scratch, "records"), exported));
  } finally { await rm(scratch, { recursive: true, force: true }); }
}

import { existsSync } from "node:fs";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { homedir, tmpdir, userInfo } from "node:os";
import { dirname, join, relative } from "node:path";
import { AuthVariables, CodexAuthFile } from "../config/auth.ts";
import { EvalPaths } from "../paths.ts";
import { exportBatch } from "./export.ts";
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

/** Every sign-in value this machine holds, so evidence can be checked for them without printing any. */
export async function signInSecrets(): Promise<string[]> {
  const values = Object.values(AuthVariables).map((name) => process.env[name] ?? "");
  const auth = join(EvalPaths.codexAuthHome, CodexAuthFile);
  const strings = (value: unknown): string[] => typeof value === "string" ? [value]
    : Array.isArray(value) ? value.flatMap(strings) : value !== null && typeof value === "object" ? Object.values(value).flatMap(strings) : [];
  if (existsSync(auth)) values.push(...strings(JSON.parse(await readFile(auth, "utf8"))));
  return values.filter((value) => value.length >= MinimumSecretLength);
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
  for (const artifact of await store.readArtifacts(batch)) {
    const [name, folder] = artifact.assessmentId === null ? [join("runs", artifact.runId, artifact.path), store.runFolder(batch.id, artifact.runId)]
      : [join("assessments", artifact.assessmentId, artifact.path), store.assessmentEvidenceFolder(batch.id, artifact.assessmentId)];
    files.set(name, await readFile(join(folder, artifact.path)));
  }
  const machine = services.machine ?? { home: homedir(), user: userInfo().username };
  const leaks = evidenceLeaks(files, await (services.secrets ?? signInSecrets)(), services.imported === true ? { ...machine, user: "" } : machine);
  if (leaks.length > 0) throw new Error(`Evidence would publish private data, so nothing was added:\n${leaks.map((leak) => `${leak.kind}: ${leak.file}`).join("\n")}`);
  const scratch = await mkdtemp(join(tmpdir(), "eval-results-add-"));
  try {
    const exported = await exportBatch(store, batchId, join(scratch, "records"), { scrubMachineUser: services.imported !== true });
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

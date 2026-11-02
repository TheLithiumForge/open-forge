import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import type { BuildResult } from "../../../history/results.types.ts";
import type { SuiteChecks } from "../../suite-checks.types.ts";
import { Isolation } from "../../../config/choices.ts";
import { buildProject } from "../../../grading/build.ts";
import { runContainerChecks, runContainerRegressions } from "./container.ts";
import { loadFixtures, materializeFixture, ReferenceFolder, type Fixture } from "./fixtures.ts";
import { LayerVersion } from "./layer.ts";
import { LayerOneCase, suiteGradingInputs } from "../../registry.ts";
import { checkerDigests, LaterLayerCheckers } from "../../../history/capture.ts";
import { fileDigests } from "../../../history/artifacts.ts";
import { fingerprint } from "../../../history/fingerprints.ts";

const ValidationTimeoutMs = 30 * 60_000;
export interface ValidationDependencies {
  readonly build: (project: string, root: string) => Promise<BuildResult>;
  readonly checks: SuiteChecks;
}
const dependencies: ValidationDependencies = {
  build: (work, root) => buildProject(Isolation.docker, { root, work, home: join(root, "home"), project: "neon-pairs" }, ValidationTimeoutMs),
  checks: runContainerChecks,
};

export interface ValidationResult { readonly fixture: string; readonly outcomes: readonly { id: string; expected: string; actual: string }[] }

export async function validateFixture(fixture: Fixture, tools: ValidationDependencies = dependencies): Promise<ValidationResult> {
  const root = await mkdtemp(join(tmpdir(), "eval-l1-validation-"));
  try {
    const work = join(root, "project");
    await materializeFixture(fileURLToPath(ReferenceFolder), work, fixture);
    const build = await tools.build(work, root);
    const report = await tools.checks({ project: work, build, html: build.html === null ? null : join(work, build.html), shotsFolder: join(root, "shots") });
    const actual = new Map(report.checks.map((check) => [check.id, check.status]));
    return { fixture: fixture.id, outcomes: Object.entries(fixture.expected).map(([id, expected]) => ({ id, expected, actual: actual.get(id) ?? "missing" })) };
  } finally {
    if (dirname(resolve(root)) !== resolve(tmpdir())) throw new Error("Validation cleanup escaped its temporary folder.");
    await rm(root, { recursive: true, force: true });
  }
}

export async function main(args: readonly string[]): Promise<void> {
  const fixtures = await loadFixtures();
  if (args.length === 1 && args[0] === "--plan") {
    for (const fixture of fixtures) process.stdout.write(`${fixture.id}: expected failures ${Object.entries(fixture.expected).filter(([, status]) => status === "fail").map(([id]) => id).join(", ") || "none"}. Not executed.\n`);
    return;
  }
  if (args.length && !(args.length === 1 && args[0] === "--regressions")) throw new Error("Run validate.ts, validate.ts --plan or validate.ts --regressions.");
  process.stdout.write(await runContainerRegressions());
  if (args[0] === "--regressions") return;
  const inputs = await validationInputs();
  let mismatches = 0;
  const results: ValidationResult[] = [];
  for (const fixture of fixtures) {
    const result = await validateFixture(fixture);
    results.push(result);
    for (const outcome of result.outcomes) {
      const same = outcome.expected === outcome.actual;
      if (!same) mismatches += 1;
      process.stdout.write(`${result.fixture}: ${outcome.id} ${outcome.actual}, expected ${outcome.expected}${same ? "" : " MISMATCH"}\n`);
    }
  }
  if (mismatches) throw new Error(`${mismatches} fixture expectations did not match.`);
  if (fingerprint(await validationInputs()) !== fingerprint(inputs)) throw new Error("Validation inputs changed while the fixtures ran.");
  await writeFile(new URL("../layers/L1.validation.json", import.meta.url), `${JSON.stringify({ schemaVersion: 1, gradingVersion: LayerVersion, status: "validated", inputs, results }, null, 2)}\n`);
}

// Later layers keep their checks and validation reports in spec-layers folders, so their runs can't change Layer 1's inputs.
async function validationInputs() {
  return { definitions: await suiteGradingInputs(LayerOneCase),
    checker: fingerprint(await checkerDigests(fileURLToPath(new URL("./", import.meta.url)), LayerOneCase)),
    reference: fingerprint(await fileDigests(fileURLToPath(ReferenceFolder))),
    fixtures: fingerprint(await fileDigests(fileURLToPath(new URL("../fixtures/", import.meta.url)), LaterLayerCheckers)),
  };
}

if (import.meta.main) main(process.argv.slice(2)).catch((error: unknown) => { process.stderr.write(`${error instanceof Error ? error.message : String(error)}\n`); process.exitCode = 1; });

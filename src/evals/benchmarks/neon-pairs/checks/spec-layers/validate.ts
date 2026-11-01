import { mkdtemp, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import type { BuildResult } from "../../../../history/results.types.ts";
import type { SuiteCheckInput } from "../../../suite-checks.types.ts";
import { Isolation } from "../../../../config/choices.ts";
import { buildProject } from "../../../../grading/build.ts";
import { auditFixtures, SpecLayerReference, loadSpecLayerFixtures, materializeFixture } from "./fixtures.ts";
import { LayerId, layerId, type LayerId as Layer } from "./requirements.ts";
import { runLayerChecks, type LayerOutput } from "./container.ts";
import { gradingFingerprint, loadDefinition } from "./load.ts";
import { SpecLayerTimeoutMs } from "./paths.ts";
import { Limitations } from "./limitations.ts";
import { removeTemporary } from "./temporary.ts";

export interface ValidationTools {
  readonly build: (project: string, root: string) => Promise<BuildResult>;
  readonly check: (layer: Layer, input: SuiteCheckInput) => Promise<LayerOutput>;
}
const tools: ValidationTools = {
  build: (work, root) => buildProject(Isolation.docker, { root, work, home: join(root, "home"), project: "neon-pairs" }, SpecLayerTimeoutMs),
  check: runLayerChecks,
};
export function compareExpectations(output: LayerOutput, expected: Readonly<Record<string, string>>, reference: boolean, requiredClauses: readonly string[] = []): string[] {
  const mismatches: string[] = [];
  for (const [clause, status] of Object.entries(expected)) if (output.assertions.find((item) => item.clause === clause)?.status !== status) mismatches.push(`${clause}: expected ${status}, got ${output.assertions.find((item) => item.clause === clause)?.status ?? "missing"}`);
  if (reference) {
    for (const clause of requiredClauses) if (!output.assertions.some((item) => item.clause === clause)) mismatches.push(`${clause}: missing assertion`);
    for (const item of output.assertions) {
      const status = Limitations[item.clause] ? "error" : "pass";
      if (item.status !== status) mismatches.push(`${item.clause}: expected ${status}, got ${item.status}`);
    }
    for (const check of output.checks.filter((item) => !item.id.startsWith("l2-") && !item.id.startsWith("l3-"))) if (check.status !== "pass") mismatches.push(`${check.id}: ${check.status}`);
  }
  return mismatches;
}
export async function validate(layer: Layer, reference = fileURLToPath(SpecLayerReference), dependencies = tools) {
  await auditFixtures(reference);
  const declared = await loadDefinition(layer);
  const fixtures = (await loadSpecLayerFixtures()).filter((fixture) => layer === LayerId.three || fixture.id === "reference");
  const results: { fixture: string; checks: LayerOutput["checks"]; assertions: LayerOutput["assertions"]; mismatches: string[] }[] = [];
  for (const fixture of fixtures) {
    const root = await mkdtemp(join(tmpdir(), "eval-spec-layers-validation-"));
    try {
      const work = join(root, "project"); await materializeFixture(reference, work, fixture);
      const build = await dependencies.build(work, root);
      const output = await dependencies.check(layer, { project: work, build, html: build.html ? join(work, build.html) : null, shotsFolder: join(root, "shots") });
      const mismatches = compareExpectations(output, fixture.expected, fixture.id === "reference", declared.checks.flatMap((check) => check.clauses));
      results.push({ fixture: fixture.id, checks: output.checks, assertions: output.assertions, mismatches });
      process.stdout.write(`${layer} ${fixture.id}: ${mismatches.length ? "FAIL" : "PASS"}\n`);
      for (const item of output.assertions.filter((item) => item.status !== "pass")) process.stdout.write(`  ${item.clause}: ${item.status}\n`);
      for (const mismatch of mismatches) process.stdout.write(`  ${mismatch}\n`);
    } finally { await removeTemporary(root, "eval-spec-layers-validation-"); }
  }
  return { schemaVersion: 1, layer, fingerprint: await gradingFingerprint(layer), results };
}
if (import.meta.main) {
  const [mode, selected = LayerId.three, reference] = process.argv.slice(2);
  if (mode === "--audit") process.stdout.write(`Exact-once fixture edits: ${(await auditFixtures(reference)).join(", ")}\n`);
  else if (mode === "--plan") {
    for (const layer of Object.values(LayerId)) {
      const definition = await loadDefinition(layer);
      process.stdout.write(`${definition.version}: ${definition.checks.length} checks, ${definition.checks.flatMap((check) => check.assertions).length} clause assertions, ${definition.review.length} review clauses, fingerprint ${await gradingFingerprint(layer)}\n`);
    }
  } else {
    if (mode !== "--run") throw new Error("Use --audit, --plan or --run L2|L3 [reference-folder].");
    const layer = layerId(selected), result = await validate(layer, reference);
    await writeFile(new URL(`../../fixtures/spec-layers/${layer}.validation.json`, import.meta.url), `${JSON.stringify(result, null, 2)}\n`);
    process.exitCode = result.results.some((item) => item.mismatches.length > 0) ? 1 : 0;
  }
}

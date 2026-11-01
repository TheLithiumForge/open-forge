import { readFile } from "node:fs/promises";
import { join } from "node:path";
import type { TestContext } from "node:test";
import { brownfieldFixture, BrownfieldFixture } from "../../materialize/brownfield-test-fixture.ts";
import { loadBrownfieldSuite } from "../../materialize/brownfield.ts";
import { loadDefinition } from "../../benchmarks/neon-pairs/checks/spec-layers/load.ts";
import { LayerId } from "../../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { writeNew } from "../../materialize/files.ts";
import type { CaseValidationServices } from "./validate.ts";

export const ValidationFixturePage = "dist/index.html";

export async function validationFixture(t: TestContext) {
  const fixture = await brownfieldFixture(t);
  const suite = await loadBrownfieldSuite(fixture.caseRoot, BrownfieldFixture.id, fixture.reference, fixture.destination);
  const definition = await loadDefinition(LayerId.three);
  const builds: string[] = [], phases: string[] = [];
  // The fake build never executes fixture code. Gates still use the real scope and protected-input checks.
  const tools: CaseValidationServices = {
    projectTests: async () => ({ unit: { code: 0, stdout: "1 passed", stderr: "", timedOut: false }, browser: { code: 0, stdout: "1 passed", stderr: "", timedOut: false } }),
    build: async (space) => {
      builds.push(await readFile(join(space.work, BrownfieldFixture.source), "utf8"));
      await writeNew(space.work, ValidationFixturePage, "<html>Fixture page</html>");
      return { passed: true, typecheck: true, seconds: 0, packages: null, html: ValidationFixturePage, bytes: null, log: "Fixture build." };
    },
    gates: { protectedRoots: [fixture.caseRoot], regression: async () => ({
      checks: definition.checks.map((check) => ({ id: check.id, area: check.area, status: CheckStatus.pass, evidence: "Fixture regression evidence." })),
      assertions: definition.checks.flatMap((check) => check.assertions.map((assertion) => ({ id: assertion.id, clause: assertion.clause, area: "regression", status: CheckStatus.pass, evidence: "Fixture assertion evidence." }))),
      review: definition.review, shots: [], performance: null,
    }), changes: async (_item, checks, input) => {
      const source = await readFile(join(input.project, BrownfieldFixture.source), "utf8");
      phases.push(source);
      return checks.map((check) => ({ id: `${check.kind}-${check.id}`, area: check.kind, status: source === BrownfieldFixture.repaired ? CheckStatus.pass : CheckStatus.fail, evidence: "Fixture change evidence." }));
    } },
  };
  return { ...fixture, suite, tools, builds, phases };
}

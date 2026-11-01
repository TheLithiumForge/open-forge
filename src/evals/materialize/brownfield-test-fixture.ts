import { join } from "node:path";
import type { TestContext } from "node:test";
import { sample, SampleFiles } from "./test-fixture.ts";
import { materializeSpecs } from "./specs.ts";
import { writeNew } from "./files.ts";
import { BrownfieldFiles, ChangeType } from "./brownfield-case.ts";
import { ChangeCheckFiles, ChangeCheckLimits, ChangeCheckVersion, ChangeKind } from "../grading/brownfield/change-checks.ts";
import { CheckStatus } from "../history/statuses.ts";

export const BrownfieldFixture = { id: "l4-fixture", source: "src/game.ts", original: "export const state = 'working';\n", broken: "export const state = 'broken';\n", repaired: "export const state = 'repaired';\n" } as const;
export const FixtureSeedPatch = `--- a/${BrownfieldFixture.source}\n+++ b/${BrownfieldFixture.source}\n@@ -1 +1 @@\n-${BrownfieldFixture.original.trimEnd()}\n+${BrownfieldFixture.broken.trimEnd()}\n`;

export async function brownfieldFixture(t: TestContext) {
  const base = await sample(t);
  await materializeSpecs(base);
  const reference = join(base.root, "reference"), root = join(base.root, BrownfieldFixture.id);
  await writeNew(reference, "package.json", "{}\n");
  await writeNew(reference, "README.md", "# Fixture project\nBuild the project with npm.\n");
  await writeNew(reference, ".gitignore", "dist/\n");
  await writeNew(reference, BrownfieldFixture.source, BrownfieldFixture.original);
  const definition = { id: BrownfieldFixture.id, changeType: ChangeType.combined, title: "Fixture change", request: BrownfieldFiles.request,
    specDelta: BrownfieldFiles.delta, seedPatch: BrownfieldFiles.seed, solutionPatch: BrownfieldFiles.solution, checks: BrownfieldFiles.checks, answers: BrownfieldFiles.answers,
    supersedes: ["R05.1"], diffScope: ["src/**", "tests/**"] };
  await writeNew(root, BrownfieldFiles.definition, JSON.stringify(definition));
  await writeNew(root, BrownfieldFiles.request, "Please add the new behavior and fix the broken behavior.\n");
  await writeNew(root, BrownfieldFiles.answers, "[]\n");
  await writeNew(root, `${BrownfieldFiles.delta}/${SampleFiles.rules}`, "# Rules\nMatch two cards and show the new behavior.\n");
  await writeNew(root, BrownfieldFiles.seed, FixtureSeedPatch);
  await writeNew(root, BrownfieldFiles.solution, FixtureSeedPatch.replaceAll(BrownfieldFixture.original.trimEnd(), BrownfieldFixture.broken.trimEnd()).replaceAll(`+${BrownfieldFixture.broken.trimEnd()}`, `+${BrownfieldFixture.repaired.trimEnd()}`));
  const checks = Array.from({ length: ChangeCheckLimits.minimum }, (_, index) => ({ id: `change-${index}`, kind: index % 2 ? ChangeKind.repair : ChangeKind.feature,
    expected: { starting: CheckStatus.fail, solution: CheckStatus.pass } }));
  await writeNew(root, `${BrownfieldFiles.checks}/${ChangeCheckFiles.manifest}`, JSON.stringify({ schemaVersion: ChangeCheckVersion, checks }));
  await writeNew(root, `${BrownfieldFiles.checks}/${ChangeCheckFiles.worker}`, "throw new Error('This fixture worker runs only through the verifier fake.');\n");
  return { ...base, caseRoot: root, reference, definition, checks };
}

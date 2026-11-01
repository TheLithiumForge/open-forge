import type { SuiteChecks } from "./suite-checks.types.ts";
import { runContainerChecks } from "./neon-pairs/checks/container.ts";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { evalsRoot } from "../paths.ts";
import { LayerFiles } from "./neon-pairs/checks/layer.ts";
import { DocumentCase } from "../materialize/delivery.ts";
import type { Suite } from "./suite.types.ts";
import { inventory, sha256, type FileHash } from "../materialize/files.ts";
import { ReferenceExcluded } from "../materialize/brownfield-inputs.ts";
import { runBrownfieldChecks } from "../grading/brownfield/gates.ts";
import { readBrownfieldCase } from "../materialize/brownfield-case.ts";
import type { ScopePolicy } from "../materialize/scope.types.ts";

export function checksForSuite(suite: Suite, baseline?: readonly FileHash[], scope?: ScopePolicy): SuiteChecks | undefined {
  const inputs = suite.brownfield;
  if (inputs === undefined) return suiteChecks[suite.id];
  if (!baseline?.length) throw new Error("Brownfield grading needs the saved starting setup manifest.");
  return (input) => runBrownfieldChecks(inputs, baseline, input, scope === undefined ? {} : { scope });
}

export const LayerOneCase = "greenfield-baseline";

/** Each suite's hidden checks, by suite ID. A new suite adds its entry here. */
export const suiteChecks: Readonly<Record<string, SuiteChecks>> = {
  [LayerOneCase]: runContainerChecks,
};

export async function suiteGradingInputs(id: string, root: string = evalsRoot): Promise<readonly { path: string; sha256: string }[]> {
  if (/^l4-[\w-]+$/.test(id)) {
    const base = join(root, "benchmarks", "neon-pairs");
    const item = await readBrownfieldCase(join(base, "cases", id), id);
    const files: FileHash[] = item.inputs.map((file) => ({ ...file, path: `cases/${id}/${file.path}` }));
    for (const folder of ["reference", "specs", "layers", "rubrics"]) {
      for (const file of await inventory(join(base, folder), ReferenceExcluded)) files.push({ path: `${folder}/${file.path}`, sha256: sha256(file.bytes) });
    }
    for (const path of ["requirements.json", "trace.json"]) files.push({ path, sha256: sha256(await readFile(join(base, path))) });
    return files;
  }
  const files = id === LayerOneCase ? Object.values(LayerFiles)
    : id === DocumentCase.two || id === DocumentCase.three ? ["requirements.json", `cases/${id}/inputs.json`] : [];
  return Promise.all(files.map(async (path) => ({ path,
    sha256: createHash("sha256").update(await readFile(join(root, "benchmarks", "neon-pairs", path))).digest("hex"),
  })));
}

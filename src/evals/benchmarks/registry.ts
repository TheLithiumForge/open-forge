import type { SuiteChecks } from "./suite-checks.types.ts";
import { runContainerChecks } from "./neon-pairs/checks/container.ts";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { evalsRoot } from "../paths.ts";
import { LayerFiles } from "./neon-pairs/checks/layer.ts";
import { DocumentCase } from "../materialize/delivery.ts";

/** Each suite's hidden checks, by suite ID. A new suite adds its entry here. */
export const suiteChecks: Readonly<Record<string, SuiteChecks>> = {
  "greenfield-baseline": runContainerChecks,
};

export async function suiteGradingInputs(id: string, root: string = evalsRoot): Promise<readonly { path: string; sha256: string }[]> {
  const files = id === "greenfield-baseline" ? Object.values(LayerFiles)
    : id === DocumentCase.two || id === DocumentCase.three ? ["requirements.json", `cases/${id}/inputs.json`] : [];
  return Promise.all(files.map(async (path) => ({ path,
    sha256: createHash("sha256").update(await readFile(join(root, "benchmarks", "neon-pairs", path))).digest("hex"),
  })));
}

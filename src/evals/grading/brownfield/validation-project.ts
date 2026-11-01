import { readFile, unlink, writeFile } from "node:fs/promises";
import { join } from "node:path";
import type { BrownfieldInputs } from "../../materialize/brownfield-inputs.ts";
import { inventory, writeNew } from "../../materialize/files.ts";
import { applyPatch } from "../../materialize/patch.ts";
import { materializeSetup, SetupKind } from "../../materialize/setup.ts";

export async function validationProject(inputs: BrownfieldInputs, destination: string, solution: boolean | string) {
  const result = await materializeSetup({ kind: SetupKind.plain, destination, frozen: inputs.specifications, projectFiles: inputs.reference,
    seedFiles: inputs.seed });
  if (!solution) return result.manifest;
  const original = await inventory(destination);
  const patch = await readFile(join(inputs.case.root, typeof solution === "string" ? solution : inputs.case.solutionPatch), "utf8");
  const next = applyPatch(original, patch);
  for (const file of original) if (!next.some((entry) => entry.path === file.path)) await unlink(join(destination, file.path));
  for (const file of next) {
    if (original.some((entry) => entry.path === file.path)) await writeFile(join(destination, file.path), file.bytes);
    else await writeNew(destination, file.path, file.bytes);
  }
  return result.manifest;
}

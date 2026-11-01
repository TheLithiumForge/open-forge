import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { evalsRoot } from "../../paths.ts";
import { hashes, inventory, sha256, type FileHash } from "../../materialize/files.ts";
import { hashesWithoutValidation } from "../../materialize/brownfield-case.ts";
import { ReferenceExcluded, type BrownfieldInputs } from "../../materialize/brownfield-inputs.ts";

export const ValidationSources = {
  folders: ["grading/brownfield", "materialize", "execution/containers", "images", "benchmarks/neon-pairs/checks", "benchmarks/neon-pairs/layers"],
  files: ["grading/build.ts", "grading/counting.ts", "grading/grading.types.ts", "history/statuses.ts", "materialize/patch.ts", "package-lock.json", "benchmarks/neon-pairs/requirements.json", "benchmarks/neon-pairs/trace.json"],
} as const;

export async function validationInputs(inputs: BrownfieldInputs) {
  const tooling: FileHash[] = [];
  for (const folder of ValidationSources.folders) {
    for (const file of await inventory(join(evalsRoot, folder), ReferenceExcluded)) tooling.push({ path: `${folder}/${file.path}`, sha256: sha256(file.bytes) });
  }
  for (const path of ValidationSources.files) tooling.push({ path, sha256: sha256(await readFile(join(evalsRoot, path))) });
  return { case: hashesWithoutValidation(await inventory(inputs.case.root), inputs.case.checks), reference: hashes(inputs.reference),
    referenceSource: hashes(await inventory(inputs.referenceRoot, ReferenceExcluded)), specificationSource: hashes(await inventory(inputs.specsRoot, ReferenceExcluded)),
    specifications: hashes(inputs.specifications.documents), manifest: sha256(JSON.stringify(inputs.specifications.manifest)), tooling };
}

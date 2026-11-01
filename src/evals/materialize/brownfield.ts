import { CaseTimeLimitMinutes, DocumentSetups } from "./delivery.ts";
import { CodeOnlySetup, type BrownfieldCase } from "./brownfield-case.ts";
import { brownfieldInputs, type BrownfieldInputs } from "./brownfield-inputs.ts";
import { orderedDocuments } from "./document-order.ts";
import { ForgePreset } from "./forge.ts";
import { sha256 } from "./files.ts";
import { SetupKind } from "./setup.ts";
import type { Suite, SetupDefinition } from "../benchmarks/suite.types.ts";
import { ChangeCheckFiles, readChangeChecks } from "../grading/brownfield/change-checks.ts";

export const BrownfieldSetups: Readonly<Record<string, SetupDefinition>> = {
  [DocumentSetups.plain]: { label: "Plain docs", commands: [], delivery: { kind: SetupKind.plain } },
  [DocumentSetups.linked]: { label: "Linked AGENTS.md", commands: [], delivery: { kind: SetupKind.linked } },
  [DocumentSetups.minimal]: { label: "Minimal", commands: [], delivery: { kind: SetupKind.forge, preset: ForgePreset.minimal } },
  [DocumentSetups.essentials]: { label: "Essentials", commands: [], delivery: { kind: SetupKind.forge, preset: ForgePreset.essentials } },
  [DocumentSetups.fullCore]: { label: "Full Core", commands: [], delivery: { kind: SetupKind.forge, preset: ForgePreset.fullCore } },
  [CodeOnlySetup]: { label: "Code only", commands: [], delivery: { kind: SetupKind.code } },
};

export function brownfieldSuite(inputs: BrownfieldInputs): Suite {
  const item: BrownfieldCase = inputs.case;
  return { id: item.id, title: item.title, project: "neon-pairs", baseline: DocumentSetups.plain, setups: BrownfieldSetups,
    prompt: item.request, intent: item.answers, rubric: [], intentEdited: false, runMinutes: CaseTimeLimitMinutes,
    documents: orderedDocuments(inputs.specifications.manifest, inputs.specifications.documents).map((file) => file.bytes.toString()),
    fingerprint: sha256(JSON.stringify({ inputs: item.inputs, seed: inputs.seed.map((file) => ({ path: file.path, sha256: sha256(file.bytes) })), specs: inputs.specifications.manifest })),
    comparisons: { routing: Object.keys(BrownfieldSetups).filter((setup) => setup !== CodeOnlySetup), information: [DocumentSetups.plain, CodeOnlySetup] }, brownfield: inputs };
}

export async function loadBrownfieldSuite(root: string, expectedId?: string, reference?: string, specs?: string): Promise<Suite> {
  const inputs = await brownfieldInputs(root, reference, specs, expectedId);
  await readChangeChecks(inputs.case);
  if (!inputs.case.inputs.some((file) => file.path === `${inputs.case.checks}/${ChangeCheckFiles.worker}`)) throw new Error("The case has no verifier change worker.");
  return brownfieldSuite(inputs);
}

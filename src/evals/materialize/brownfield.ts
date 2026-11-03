import { CaseTimeLimitMinutes, DocumentSetups, NeonPairsScopes } from "./delivery.ts";
import { CodeOnlySetup, type BrownfieldCase } from "./brownfield-case.ts";
import { brownfieldInputs, type BrownfieldInputs } from "./brownfield-inputs.ts";
import { orderedDocuments } from "./document-order.ts";
import { ForgePreset } from "./forge.ts";
import { sha256 } from "./files.ts";
import { SetupKind } from "./setup.ts";
import type { Suite, SetupDefinition } from "../benchmarks/suite.types.ts";
import { ChangeCheckFiles, readChangeChecks } from "../grading/brownfield/change-checks.ts";

export const BrownfieldSetups: Readonly<Record<string, SetupDefinition>> = {
  [DocumentSetups.organized]: { label: "Organized docs", commands: [], delivery: { kind: SetupKind.plain } },
  [DocumentSetups.linked]: { label: "Linked AGENTS.md", commands: [], delivery: { kind: SetupKind.linked } },
  [DocumentSetups.minimal]: { label: "Minimal", commands: [], delivery: { kind: SetupKind.forge, preset: ForgePreset.minimal } },
  [DocumentSetups.essentials]: { label: "Essentials", commands: [], delivery: { kind: SetupKind.forge, preset: ForgePreset.essentials } },
  [DocumentSetups.fullCore]: { label: "Full Core", commands: [], delivery: { kind: SetupKind.forge, preset: ForgePreset.fullCore } },
  [DocumentSetups.fullCoreScoped]: { label: "Full Core, scoped", commands: [], delivery: { kind: SetupKind.forge, preset: ForgePreset.fullCore, scopes: NeonPairsScopes } },
  [CodeOnlySetup]: { label: "Code only", commands: [], delivery: { kind: SetupKind.code } },
  [DocumentSetups.plain]: { label: "Plain docs", commands: [], delivery: { kind: SetupKind.plain, links: false } },
};

export function brownfieldSuite(inputs: BrownfieldInputs): Suite {
  const item: BrownfieldCase = inputs.case;
  return { id: item.id, title: item.title, project: "neon-pairs", baseline: DocumentSetups.organized, setups: BrownfieldSetups,
    prompt: item.request, intent: item.answers, rubric: [], intentEdited: false, runMinutes: item.runMinutes ?? CaseTimeLimitMinutes,
    documents: orderedDocuments(inputs.specifications.manifest, inputs.specifications.documents).map((file) => file.bytes.toString()),
    fingerprint: sha256(JSON.stringify({ inputs: item.inputs, seed: inputs.seed.map((file) => ({ path: file.path, sha256: sha256(file.bytes) })), specs: inputs.specifications.manifest })),
    comparisons: { routing: Object.keys(BrownfieldSetups).filter((setup) => setup !== CodeOnlySetup), information: [DocumentSetups.organized, CodeOnlySetup],
      scopes: [DocumentSetups.organized, DocumentSetups.fullCore, DocumentSetups.fullCoreScoped] }, brownfield: inputs };
}

export async function loadBrownfieldSuite(root: string, expectedId?: string, reference?: string, specs?: string): Promise<Suite> {
  const inputs = await brownfieldInputs(root, reference, specs, expectedId);
  const declared = await readChangeChecks(inputs.case);
  const needsWorker = declared.some((check) => check.clause === undefined);
  if (needsWorker && !inputs.case.inputs.some((file) => file.path === `${inputs.case.checks}/${ChangeCheckFiles.worker}`)) throw new Error("The case has no verifier change worker.");
  return brownfieldSuite(inputs);
}

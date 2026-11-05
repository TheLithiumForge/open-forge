import { join, relative } from "node:path";
import { evalsRoot } from "../paths.ts";
import { CaseTimeLimitMinutes, DocumentSetups, NeonPairsOverload, NeonPairsScopes, OverloadSetups } from "./delivery.ts";
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
  // Overloaded docs add project notes to Organized docs. With memory, the same notes sit on their Open Forge Memory routes.
  [OverloadSetups.docs]: { label: "Overloaded docs", commands: [], delivery: { kind: SetupKind.plain, overload: NeonPairsOverload } },
  [OverloadSetups.memory]: { label: "Full Core, scoped, with memory", commands: [], delivery: { kind: SetupKind.forge, preset: ForgePreset.fullCore, scopes: NeonPairsScopes, overload: NeonPairsOverload } },
};

/** The install ladder's scoped rungs, which only a case with its own pack offers. */
export const PackSetups = { minimalScoped: "minimal-scoped", essentialsScoped: "essentials-scoped" } as const;
/** Setups that read the same on a case with its own pack. Flat Full Core is the one deliberately improper use of Open Forge. */
const PackSetupsAsTheyAre: ReadonlySet<string> = new Set([CodeOnlySetup, DocumentSetups.plain, DocumentSetups.fullCore]);

/**
 * The setups of one case. A case with its own scopes puts Organized docs in the same folders as Open Forge, as a careful
 * team would with that many documents, so the framework's routing is all that separates them. Every other Open Forge
 * setup uses those folders too, so the install ladder changes only how much is installed, and flat Full Core differs
 * from Full Core, scoped only in its organization.
 */
function caseSetups(item: BrownfieldCase): Readonly<Record<string, SetupDefinition>> {
  if (item.scopes === undefined) return BrownfieldSetups;
  const scopes = relative(evalsRoot, join(item.root, item.scopes)).replaceAll("\\", "/");
  const scoped = (label: string, preset: ForgePreset): SetupDefinition => ({ label, commands: [], delivery: { kind: SetupKind.forge, preset, scopes } });
  return { ...Object.fromEntries(Object.entries(BrownfieldSetups).filter(([id]) => PackSetupsAsTheyAre.has(id))),
    [DocumentSetups.organized]: { label: "Organized docs", commands: [], delivery: { kind: SetupKind.plain, scopes } },
    [PackSetups.minimalScoped]: scoped("Minimal, scoped", ForgePreset.minimal),
    [PackSetups.essentialsScoped]: scoped("Essentials, scoped", ForgePreset.essentials),
    [DocumentSetups.fullCoreScoped]: scoped("Full Core, scoped", ForgePreset.fullCore) };
}

export function brownfieldSuite(inputs: BrownfieldInputs): Suite {
  const item: BrownfieldCase = inputs.case;
  const setups = caseSetups(item);
  return { id: item.id, title: item.title, project: "neon-pairs", baseline: DocumentSetups.organized, setups,
    prompt: item.request, intent: item.answers, rubric: [], intentEdited: false, runMinutes: item.runMinutes ?? CaseTimeLimitMinutes,
    documents: orderedDocuments(inputs.specifications.manifest, inputs.specifications.documents).map((file) => file.bytes.toString()),
    fingerprint: sha256(JSON.stringify({ inputs: item.inputs, seed: inputs.seed.map((file) => ({ path: file.path, sha256: sha256(file.bytes) })), specs: inputs.specifications.manifest })),
    comparisons: { routing: Object.keys(setups).filter((setup) => setup !== CodeOnlySetup), information: [DocumentSetups.organized, CodeOnlySetup],
      scopes: [DocumentSetups.organized, DocumentSetups.fullCore, DocumentSetups.fullCoreScoped] }, brownfield: inputs };
}

export async function loadBrownfieldSuite(root: string, expectedId?: string, reference?: string, specs?: string): Promise<Suite> {
  const inputs = await brownfieldInputs(root, reference, specs, expectedId);
  const declared = await readChangeChecks(inputs.case);
  const needsWorker = declared.some((check) => check.clause === undefined);
  if (needsWorker && !inputs.case.inputs.some((file) => file.path === `${inputs.case.checks}/${ChangeCheckFiles.worker}`)) throw new Error("The case has no verifier change worker.");
  return brownfieldSuite(inputs);
}

import type { Suite } from "../benchmarks/suite.types.ts";
import { SetupKind } from "./setup.ts";
import type { ForgePreset } from "./forge.ts";

/** How a setup delivers the spec. A scope layout, relative to the evals folder, routes an Open Forge spec by scope folders. */
/** False removes the links between documents, for Plain docs. */
export interface DocumentDelivery { readonly kind: SetupKind; readonly preset?: ForgePreset; readonly scopes?: string; readonly links?: false; readonly overload?: string }
export const DocumentPointer = "Read docs/README.md and follow its linked requirements for this project.";
/** Plain docs have no links to follow, so their pointer names the folder instead. */
export const PlainPointer = "Read docs/README.md and the other documents in docs/ for this project's requirements.";
/** The registered line a team's leader gets after the request, so it delegates and every task gets its own subagent. */
export const OrchestratorLine = "Plan the work first. Then hand each task to its own subagent with the context it needs, and don't edit the code yourself.";
export const CaseTimeLimitMinutes = 60;
export const DocumentCase = { two: "l2-defined-idea", three: "l3-full-spec" } as const;
/** The Neon Pairs scope layout, relative to the evals folder. The full-spec suite names the same file. */
export const NeonPairsScopes = "benchmarks/neon-pairs/scopes.json";
/** The Neon Pairs notes beside the spec, relative to the evals folder. */
export const NeonPairsOverload = "benchmarks/neon-pairs/overload/manifest.json";
/** Setups with the notes a real project collects, as a mixed docs folder or on Open Forge Memory routes. */
export const OverloadSetups = { docs: "overloaded-docs", memory: "full-core-scoped-memory" } as const;
/** Plain docs are the documents without links between them. Organized docs link them, as a careful team would. */
export const DocumentSetups = { organized: "organized-docs", plain: "plain-docs", linked: "linked-agents", minimal: "minimal", essentials: "essentials", fullCore: "full-core", inline: "spec-in-prompt", fullCoreScoped: "full-core-scoped" } as const;

export function setupPrompt(suite: Suite, setup: string): string {
  const delivery = suite.setups[setup]?.delivery;
  if (!delivery || delivery.kind === SetupKind.code) return suite.prompt;
  if (!suite.documents?.length) throw new Error("A document setup needs frozen product documents.");
  const pointer = delivery.kind === SetupKind.inline ? suite.documents.join("\n\n") : delivery.links === false ? PlainPointer : DocumentPointer;
  return `${suite.prompt}\n\n${pointer}`;
}

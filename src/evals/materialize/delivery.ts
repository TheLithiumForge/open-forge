import { WorkMode } from "../config/choices.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import { SetupKind } from "./setup.ts";
import type { ForgePreset } from "./forge.ts";

/** How a setup delivers the spec. A scope layout, relative to the evals folder, routes an Open Forge spec by scope folders. */
export interface DocumentDelivery { readonly kind: SetupKind; readonly preset?: ForgePreset; readonly scopes?: string }
export const DocumentPointer = "Read docs/README.md and follow its linked requirements for this project.";
/** The registered line an orchestrated setup adds after the request, so the orchestrator delegates and every task gets its own subagent. */
export const OrchestratorLine = "Plan the work first. Then hand each task to its own subagent with the context it needs, and don't edit the code yourself.";
export const CaseTimeLimitMinutes = 60;
export const DocumentCase = { two: "l2-defined-idea", three: "l3-full-spec" } as const;
/** The Neon Pairs scope layout, relative to the evals folder. The full-spec suite names the same file. */
export const NeonPairsScopes = "benchmarks/neon-pairs/scopes.json";
export const DocumentSetups = { plain: "plain-docs", linked: "linked-agents", minimal: "minimal", essentials: "essentials", fullCore: "full-core", inline: "spec-in-prompt", fullCoreScoped: "full-core-scoped" } as const;

export function setupPrompt(suite: Suite, setup: string): string {
  const definition = suite.setups[setup], delivery = definition?.delivery;
  const orchestrated = definition?.workMode === WorkMode.orchestrated ? `\n\n${OrchestratorLine}` : "";
  if (!delivery || delivery.kind === SetupKind.code) return `${suite.prompt}${orchestrated}`;
  if (!suite.documents?.length) throw new Error("A document setup needs frozen product documents.");
  return `${suite.prompt}\n\n${delivery.kind === SetupKind.inline ? suite.documents.join("\n\n") : DocumentPointer}${orchestrated}`;
}

import type { Suite } from "../benchmarks/suite.types.ts";
import { SetupKind } from "./setup.ts";
import type { ForgePreset } from "./forge.ts";

export interface DocumentDelivery { readonly kind: SetupKind; readonly preset?: ForgePreset }
export const DocumentPointer = "Read docs/README.md and follow its linked requirements for this project.";
export const CaseTimeLimitMinutes = 60;
export const DocumentCase = { two: "l2-defined-idea", three: "l3-full-spec" } as const;
export const DocumentSetups = { plain: "plain-docs", linked: "linked-agents", minimal: "minimal", essentials: "essentials", fullCore: "full-core", inline: "spec-in-prompt" } as const;

export function setupPrompt(suite: Suite, setup: string): string {
  const delivery = suite.setups[setup]?.delivery;
  if (!delivery) return suite.prompt;
  if (delivery.kind === SetupKind.code) return suite.prompt;
  if (!suite.documents?.length) throw new Error("A document setup needs frozen product documents.");
  return `${suite.prompt}\n\n${delivery.kind === SetupKind.inline ? suite.documents.join("\n\n") : DocumentPointer}`;
}

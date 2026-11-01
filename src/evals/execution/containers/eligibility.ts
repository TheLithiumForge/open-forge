import { Isolation } from "../../config/choices.ts";

export const ExecutionLabelFile = "execution.json";
export const LocalExecutionLabel = { isolation: Isolation.local, fitForScoredResults: false, reason: "Local isolation is not fit for scored results." } as const;

export function executionLabel(isolation: Isolation): object {
  return isolation === Isolation.local ? LocalExecutionLabel : { isolation, fitForScoredResults: true };
}

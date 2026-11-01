import type { SuiteChecks } from "./suite-checks.types.ts";
import { runNeonPairsChecks } from "./neon-pairs/checks/neon-pairs.ts";

/** Each suite's hidden checks, by suite ID. A new suite adds its entry here. */
export const suiteChecks: Readonly<Record<string, SuiteChecks>> = {
  "greenfield-baseline": runNeonPairsChecks,
};

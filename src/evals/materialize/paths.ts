import { fileURLToPath } from "node:url";

export const MaterializePaths = {
  source: fileURLToPath(new URL("../../../.agents/memory/crystallized/documents/evals/neon-pairs/spec/", import.meta.url)),
  specs: fileURLToPath(new URL("../benchmarks/neon-pairs/specs/", import.meta.url)),
  projection: fileURLToPath(new URL("../benchmarks/neon-pairs/projection.json", import.meta.url)),
} as const;

import { fileURLToPath } from "node:url";

export const MaterializePaths = {
  source: fileURLToPath(new URL("../../../.agents/memory/crystallized/documents/evals/neon-pairs/spec/", import.meta.url)),
  specs: fileURLToPath(new URL("../benchmarks/neon-pairs/specs/", import.meta.url)),
  projection: fileURLToPath(new URL("../benchmarks/neon-pairs/projection.json", import.meta.url)),
  requirements: fileURLToPath(new URL("../benchmarks/neon-pairs/requirements.json", import.meta.url)),
  playerSpecs: fileURLToPath(new URL("../benchmarks/neon-pairs/cases/l2-defined-idea/specs/", import.meta.url)),
  reference: fileURLToPath(new URL("../benchmarks/neon-pairs/reference/", import.meta.url)),
} as const;

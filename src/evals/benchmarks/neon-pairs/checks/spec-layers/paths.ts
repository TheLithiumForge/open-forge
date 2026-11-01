export const SpecLayerContainer = { harness: "/opt/evals/spec-layers", worker: "benchmarks/neon-pairs/checks/spec-layers/worker.ts", command: "/opt/evals/spec-layers/benchmarks/neon-pairs/checks/spec-layers/command-worker.ts", input: "/opt/evals/spec-layers/input.json", page: "/opt/evals/spec-layers/index.html", evidence: "/opt/evals/spec-layers/evidence", report: "report.json", commands: "/work/.eval-command-results", savedCommands: "/opt/evals/spec-layers/evidence/commands", shots: "shots" } as const;
export const SpecLayerEnvironment = { OPEN_FORGE_LATER_WORKER: "1", PLAYWRIGHT_BROWSERS_PATH: "/ms-playwright", HOME: "/home/agent" } as const;
export const SpecLayerPackages = ["playwright", "playwright-core", "pngjs", "typescript"] as const;
export const SpecLayerTimeoutMs = 90 * 60_000;

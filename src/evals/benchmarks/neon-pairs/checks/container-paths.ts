import { ContainerPaths } from "../../../execution/containers/settings.ts";

export const CheckContainer = { harness: "/opt/evals/checks", input: `${ContainerPaths.work}/input.json`, page: `${ContainerPaths.work}/index.html`, report: "report.json", shots: "shots", worker: "benchmarks/neon-pairs/checks/worker.ts", project: ContainerPaths.work, output: ContainerPaths.output } as const;
export const CheckWorkerEnvironment = { OPEN_FORGE_CHECK_WORKER: "1", HOME: ContainerPaths.home, PLAYWRIGHT_BROWSERS_PATH: "/ms-playwright" } as const;

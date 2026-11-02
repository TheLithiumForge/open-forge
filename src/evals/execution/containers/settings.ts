export interface ContainerLimits {
  readonly cpus: number;
  readonly memory: string;
  readonly processes: number;
}

export const DefaultContainerLimits: ContainerLimits = { cpus: 2, memory: "4g", processes: 512 };
// A copy out of a container moves the whole project, installed packages included, so a busy machine needs minutes.
export const ContainerTimeouts = { commandMs: 30_000, copyMs: 10 * 60_000, imageMs: 20 * 60_000 } as const;
export const ContainerPaths = {
  work: "/work",
  home: "/home/agent",
  buildHome: "/home/build",
  installHome: "/root",
  prepare: "/opt/evals/prepare.ts",
  collect: "/opt/evals/collect.ts",
  output: "/output",
  stopTools: "/opt/evals/stop-tools.ts",
} as const;
export const ContainerUsers = { root: "0:0", agent: "10000:10000", build: "10001:10001" } as const;
export const StageKind = { agent: "agent", verifier: "verifier", assessor: "assessor" } as const;
export type StageKind = (typeof StageKind)[keyof typeof StageKind];

export function containerLimits(value: ContainerLimits = DefaultContainerLimits): ContainerLimits {
  if (!Number.isFinite(value.cpus) || value.cpus <= 0) throw new Error("Container CPUs must be positive.");
  if (!/^[1-9]\d*(?:[kmg])?$/i.test(value.memory)) throw new Error("Container memory must be a positive Docker memory size.");
  if (!Number.isSafeInteger(value.processes) || value.processes < 1) throw new Error("Container processes must be a positive integer.");
  return { cpus: value.cpus, memory: value.memory, processes: value.processes };
}

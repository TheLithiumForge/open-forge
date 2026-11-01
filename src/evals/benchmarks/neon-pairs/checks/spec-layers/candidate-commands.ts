import { CommandId } from "./commands.ts";
import { SpecLayerContainer } from "./paths.ts";

/** The candidate's own npm commands. The checker runs them as the build account and reads results only from docker exec. */
export const CandidateLimits = { commandSeconds: 600, devSeconds: 5, killAfterSeconds: 5, outputBytes: 2 * 1024 * 1024, timedOutCode: 124 } as const;
export const StaleFault = { source: SpecLayerContainer.staleFault, target: "/work/src/__eval_type_error.ts", text: 'const __evalTypeError: number = "type error";\nexport {};\n' } as const;

export function commandArguments(id: string, vector?: string): string[] {
  switch (id) {
    case CommandId.ci: return ["ci", "--ignore-scripts", "--no-audit", "--no-fund"];
    case CommandId.test: return ["test"];
    case CommandId.build: return ["run", "build"];
    case CommandId.typecheck: return ["run", "typecheck"];
    case CommandId.dev: return ["run", "dev", "--", "--host", "127.0.0.1"];
    case CommandId.e2e: case CommandId.stale: return ["run", "test:e2e"];
    case CommandId.replay:
      if (!vector || !/^vector-\d+\.json$/.test(vector)) throw new Error("Invalid replay input filename.");
      return ["run", "--silent", "replay", "--", `${SpecLayerContainer.vectors}/${vector}`];
    default: throw new Error("Unknown verifier command.");
  }
}

/** Every command runs under timeout, so a hung or detached candidate can't hold the stage. */
export function candidateCommand(id: string, vector?: string): string[] {
  const seconds = id === CommandId.dev ? CandidateLimits.devSeconds : CandidateLimits.commandSeconds;
  return ["timeout", `--kill-after=${CandidateLimits.killAfterSeconds}s`, `${seconds}s`, "npm", ...commandArguments(id, vector)];
}

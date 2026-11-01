import { existsSync } from "node:fs";
import { readdir, readFile } from "node:fs/promises";
import { join } from "node:path";
import type { ProcessResult } from "../process.ts";
import { requireDockerResult } from "./docker.ts";
import { evalImageTag } from "./docker-image.ts";
import { collectProject, copyProjectIn } from "./files.ts";
import type { RunSpace } from "./executor.ts";
import { ContainerPaths, ContainerUsers, StageKind } from "./settings.ts";
import { ContainerStage, type StageOptions } from "./stage.ts";

export const VerifierCommands = {
  install: ["npm", "install", "--ignore-scripts", "--no-audit", "--no-fund"],
  ci: ["npm", "ci", "--ignore-scripts", "--no-audit", "--no-fund"],
  build: ["npm", "run", "build"],
  typecheck: ["npm", "exec", "--no", "--", "tsc", "--noEmit", "-p"],
} as const;
export const VerifierEnvironment = { HOME: ContainerPaths.buildHome, npm_config_offline: "true", npm_config_audit: "false", npm_config_fund: "false", npm_config_cache: `${ContainerPaths.buildHome}/.npm` } as const;

export interface VerifierResult {
  readonly build: ProcessResult;
  readonly typecheck: boolean | null;
}

export async function verifierConfigs(work: string): Promise<string[]> {
  const configs: string[] = [];
  for (const name of (await readdir(work)).filter((file) => /^tsconfig.*\.json$/.test(file)).sort()) {
    const text = await readFile(join(work, name), "utf8");
    if (text.includes('"include"') || text.includes('"files"')) configs.push(name);
  }
  return configs;
}

export async function verifyInContainer(space: RunSpace, options: StageOptions): Promise<VerifierResult> {
  const stage = new ContainerStage(options);
  const configs = await verifierConfigs(space.work);
  try {
    await stage.open(evalImageTag(StageKind.verifier));
    await copyProjectIn(stage, space, true);
    await stage.prepare(ContainerPaths.work);
    const install = existsSync(join(space.work, "package-lock.json")) ? VerifierCommands.ci : VerifierCommands.install;
    requireDockerResult(await stage.exec(install, ContainerUsers.root, { HOME: ContainerPaths.installHome }));
    await stage.prepare(ContainerPaths.work);
    await stage.offline();
    const build = await stage.exec(VerifierCommands.build, ContainerUsers.build, VerifierEnvironment);
    const checked: boolean[] = [];
    for (const config of configs) {
      const result = await stage.exec([...VerifierCommands.typecheck, config], ContainerUsers.build, VerifierEnvironment);
      checked.push(result.code === 0 && !result.timedOut);
    }
    requireDockerResult(await stage.exec(["node", ContainerPaths.collect], ContainerUsers.build, VerifierEnvironment));
    await stage.stop();
    await collectProject(stage, space, ContainerPaths.output);
    return { build, typecheck: checked.length === 0 ? null : checked.every(Boolean) };
  } finally {
    await stage.remove();
  }
}

import { existsSync } from "node:fs";
import { join } from "node:path";
import { ContainerStage, type StageOptions } from "../../execution/containers/stage.ts";
import { ContainerPaths, ContainerUsers, StageKind } from "../../execution/containers/settings.ts";
import { evalImageTag } from "../../execution/containers/docker-image.ts";
import { requireDockerResult } from "../../execution/containers/docker.ts";
import { copyProjectIn, collectProject } from "../../execution/containers/files.ts";
import { VerifierCommands, VerifierEnvironment } from "../../execution/containers/verifier.ts";
import type { RunSpace } from "../../execution/containers/executor.ts";
import type { ProcessResult } from "../../execution/process.ts";
import { passedTests } from "../../benchmarks/neon-pairs/checks/spec-layers/test-summary.ts";

export const ProjectTestCommands = { unit: ["npm", "test"], browser: ["npm", "run", "test:e2e"] } as const;
export const ProjectTestTimeoutMs = 90 * 60_000;
const OutputBytes = 1024 * 1024;
const EvidenceCharacters = 16_384;
export interface ProjectTests { readonly unit: ProcessResult; readonly browser: ProcessResult }

export function projectTestPassed(result: ProcessResult): boolean {
  return result.code === 0 && !result.timedOut && result.truncated !== true && passedTests(`${result.stdout}\n${result.stderr}`);
}

export function projectTestEvidence(result: ProcessResult): string {
  return `Exit code: ${result.code}. Timed out: ${result.timedOut}. Output truncated: ${result.truncated === true}.\n${result.stdout}\n${result.stderr}`.slice(0, EvidenceCharacters);
}

export async function testProject(space: RunSpace, options: StageOptions = { timeoutMs: ProjectTestTimeoutMs }): Promise<ProjectTests> {
  const stage = new ContainerStage(options);
  try {
    await stage.open(evalImageTag(StageKind.verifier));
    await copyProjectIn(stage, space, true);
    await stage.prepare(ContainerPaths.work);
    const install = existsSync(join(space.work, "package-lock.json")) ? VerifierCommands.ci : VerifierCommands.install;
    requireDockerResult(await stage.exec(install, ContainerUsers.root, { HOME: ContainerPaths.installHome }));
    await stage.prepare(ContainerPaths.work);
    await stage.offline();
    const run = async (command: readonly string[]): Promise<ProcessResult> => {
      try { return await stage.exec(command, ContainerUsers.build, VerifierEnvironment, undefined, OutputBytes); }
      finally { requireDockerResult(await stage.exec(["node", ContainerPaths.stopTools], ContainerUsers.build, VerifierEnvironment)); }
    };
    const unit = await run(ProjectTestCommands.unit), browser = await run(ProjectTestCommands.browser);
    requireDockerResult(await stage.exec(["node", ContainerPaths.collect], ContainerUsers.build, VerifierEnvironment));
    await stage.stop();
    await collectProject(stage, space, ContainerPaths.output);
    return { unit, browser };
  } finally { await stage.remove(); }
}

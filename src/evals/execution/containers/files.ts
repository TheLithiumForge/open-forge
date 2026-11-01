import { cp, mkdtemp, mkdir, rename, rm } from "node:fs/promises";
import { basename, join, sep } from "node:path";
import type { RunSpace } from "./executor.ts";
import { ContainerPaths } from "./settings.ts";
import type { ContainerStage } from "./stage.ts";

export const PrivateProjectFiles = new Set([".codex", ".claude", ".auth", ".npmrc", ".yarnrc", ".yarnrc.yml", ".netrc", ".git-credentials"]);
export const VerifierExcludedFiles = new Set([...PrivateProjectFiles, "node_modules", ".git"]);

export function projectFileAllowed(path: string, verifier = false): boolean {
  const name = basename(path);
  return !(verifier ? VerifierExcludedFiles : PrivateProjectFiles).has(name) && !name.startsWith(".env");
}

export async function copyProjectIn(stage: ContainerStage, space: RunSpace, verifier = false): Promise<void> {
  const staging = await mkdtemp(join(space.root, "input-"));
  try {
    await cp(space.work, staging, { recursive: true, filter: (source) => projectFileAllowed(source, verifier) });
    await stage.copyIn(`${staging}${sep}.`, ContainerPaths.work);
  } finally {
    await rm(staging, { recursive: true, force: true });
  }
}

export async function collectProject(stage: ContainerStage, space: RunSpace, source: string = ContainerPaths.work): Promise<void> {
  const staging = await mkdtemp(join(space.root, "output-"));
  try {
    await stage.copyOut(`${source}/.`, staging);
    await rm(space.work, { recursive: true, force: true });
    await rename(staging, space.work);
  } finally {
    await rm(staging, { recursive: true, force: true });
  }
}

export async function freshHome(stage: ContainerStage, root: string, home: string): Promise<void> {
  const staging = await mkdtemp(join(root, "home-"));
  try {
    await mkdir(home, { recursive: true });
    await stage.copyIn(`${staging}${sep}.`, ContainerPaths.home);
  } finally {
    await rm(staging, { recursive: true, force: true });
  }
}

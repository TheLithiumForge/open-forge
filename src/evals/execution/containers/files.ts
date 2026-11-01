import { cp, mkdtemp, mkdir, readdir, rename, rm } from "node:fs/promises";
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

/** An agent can plant links in its container that point at this machine's files, so only plain files and folders come out. */
export async function dropLinks(root: string): Promise<number> {
  let dropped = 0;
  for (const entry of await readdir(root, { withFileTypes: true })) {
    const path = join(root, entry.name);
    if (entry.isDirectory()) dropped += await dropLinks(path);
    else if (!entry.isFile()) {
      await rm(path, { recursive: true, force: true });
      dropped += 1;
    }
  }
  return dropped;
}

/** Replaces a host folder with a container folder's plain files and folders. */
export async function copyOutPlain(stage: ContainerStage, source: string, target: string, scratch: string): Promise<void> {
  const staging = await mkdtemp(join(scratch, "output-"));
  try {
    await stage.copyOut(`${source}/.`, staging);
    await dropLinks(staging);
    await rm(target, { recursive: true, force: true });
    await rename(staging, target);
  } finally {
    await rm(staging, { recursive: true, force: true });
  }
}

export async function collectProject(stage: ContainerStage, space: RunSpace, source: string = ContainerPaths.work): Promise<void> {
  await copyOutPlain(stage, source, space.work, space.root);
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

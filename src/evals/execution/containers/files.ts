import { cp, lstat, mkdtemp, mkdir, readFile, readdir, rename, rm } from "node:fs/promises";
import { basename, extname, isAbsolute, join, relative, resolve, sep } from "node:path";
import type { RunSpace } from "./executor.ts";
import { ContainerPaths } from "./settings.ts";
import type { ContainerStage } from "./stage.ts";

export const PrivateProjectFiles = new Set([".codex", ".claude", ".auth", ".npmrc", ".yarnrc", ".yarnrc.yml", ".netrc", ".git-credentials"]);
export const VerifierExcludedFiles = new Set([...PrivateProjectFiles, "node_modules", ".git"]);
export const AssessorImageLimits = { count: 12, bytes: 5 * 1024 * 1024 } as const;
export const ImageMediaType = { png: "image/png", jpeg: "image/jpeg" } as const;
const PngSignature = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]);
const JpegSignature = Buffer.from([255, 216, 255]);
export interface AssessorImage {
  readonly name: string;
  readonly mediaType: (typeof ImageMediaType)[keyof typeof ImageMediaType];
  readonly bytes: Buffer;
}

/** Image paths stay under the run's folder. Every component must be plain before any bytes are read. */
export async function readAssessorImages(root: string, paths: readonly string[]): Promise<AssessorImage[]> {
  if (paths.length > AssessorImageLimits.count) throw new Error("Too many assessor images.");
  const folder = await lstat(root);
  if (!folder.isDirectory() || folder.isSymbolicLink()) throw new Error("Assessor image folder must be a plain folder.");
  const images: AssessorImage[] = [];
  for (const path of paths) {
    const name = relative(resolve(root), resolve(path));
    if (!name || isAbsolute(name) || name.split(sep).includes("..")) throw new Error("Assessor image is outside the run folder.");
    let current = resolve(root);
    const parts = name.split(sep);
    for (const [index, part] of parts.entries()) {
      current = join(current, part);
      const file = await lstat(current);
      if (file.isSymbolicLink() || (index < parts.length - 1 ? !file.isDirectory() : !file.isFile() || file.nlink !== 1 || file.size >= AssessorImageLimits.bytes)) throw new Error("Assessor images must be plain files under the image size limit.");
    }
    const bytes = await readFile(current);
    if (bytes.length >= AssessorImageLimits.bytes) throw new Error("Assessor image exceeds the image size limit.");
    const extension = extname(name).toLowerCase();
    const png = extension === ".png" && bytes.subarray(0, PngSignature.length).equals(PngSignature);
    const jpeg = [".jpg", ".jpeg"].includes(extension) && bytes.subarray(0, JpegSignature.length).equals(JpegSignature) && bytes.subarray(-2).equals(Buffer.from([255, 217]));
    if (!png && !jpeg) throw new Error("Assessor images must be PNG or JPEG files with matching file content.");
    images.push({ name: basename(name), mediaType: png ? ImageMediaType.png : ImageMediaType.jpeg, bytes });
  }
  return images;
}

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

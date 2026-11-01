import { readFileSync } from "node:fs";
import { createHash } from "node:crypto";
import { join } from "node:path";
import { EvalPaths, evalsRoot } from "../../paths.ts";
import { docker, requireDockerResult, type Docker } from "./docker.ts";
import { ContainerTimeouts, StageKind, type StageKind as ImageKind } from "./settings.ts";
import { lockedImageArguments, ImageLockFile } from "../../images/lock.ts";

interface PackageManifest {
  readonly dependencies: Readonly<Record<string, string>>;
}

const manifest: PackageManifest = JSON.parse(readFileSync(join(evalsRoot, "package.json"), "utf8"));
// The tag changes with the lock and with every file an image is built from, so an edited image never reuses an old build.
const ImageSources = [ImageLockFile, "agent.Dockerfile", "verifier.Dockerfile", "assessor.Dockerfile", "prepare.ts", "collect.ts", "stop-tools.ts"] as const;
const lockKey = ImageSources.reduce((hash, name) => hash.update(name).update(readFileSync(join(EvalPaths.docker, name))), createHash("sha256")).digest("hex").slice(0, 12);

/** The image carries the same tool versions as package.json, so local and Docker runs match. */
export const ImageVersions = {
  claude: manifest.dependencies["@anthropic-ai/claude-code"] ?? "",
  codex: manifest.dependencies["@openai/codex"] ?? "",
  openForge: manifest.dependencies["@thelithiumforge/open-forge"] ?? "",
} as const;

export function evalImageTag(kind: ImageKind = StageKind.agent): string {
  return `open-forge-evals-${kind}:claude-${ImageVersions.claude}-codex-${ImageVersions.codex}-forge-${ImageVersions.openForge}-${lockKey}`;
}

export async function dockerAvailable(client: Docker = docker): Promise<boolean> {
  const info = await client.run(["info", "--format", "{{.ServerVersion}}"], { timeoutMs: ContainerTimeouts.commandMs });
  return info.code === 0 && !info.timedOut;
}

export async function buildEvalImages(client: Docker = docker, ensure = false, lock?: unknown): Promise<void> {
  // Validate the complete lock before Docker can build any image.
  const locked = lockedImageArguments(lock);
  const tools = [`CLAUDE_VERSION=${ImageVersions.claude}`, `CODEX_VERSION=${ImageVersions.codex}`, `OPEN_FORGE_VERSION=${ImageVersions.openForge}`];
  for (const kind of Object.values(StageKind)) {
    const tag = evalImageTag(kind);
    if (ensure) {
      const inspected = await client.run(["image", "inspect", tag], { timeoutMs: ContainerTimeouts.commandMs });
      if (inspected.code === 0 && !inspected.timedOut) continue;
    }
    requireDockerResult(await client.run(["build", "--tag", tag, "--file", join(EvalPaths.docker, `${kind}.Dockerfile`),
      ...[...locked, ...tools].flatMap((value) => ["--build-arg", value]), EvalPaths.docker], { timeoutMs: ContainerTimeouts.imageMs }));
  }
}

export async function ensureEvalImage(client: Docker = docker): Promise<void> {
  await buildEvalImages(client, true);
}

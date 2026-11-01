import { readFileSync } from "node:fs";
import { join } from "node:path";
import { EvalPaths, evalsRoot } from "../../paths.ts";
import { runCommand } from "../process.ts";

interface PackageManifest {
  readonly dependencies: Readonly<Record<string, string>>;
}

const manifest: PackageManifest = JSON.parse(readFileSync(join(evalsRoot, "package.json"), "utf8"));

/** The image carries the same tool versions as package.json, so local and Docker runs match. */
export const ImageVersions = {
  claude: manifest.dependencies["@anthropic-ai/claude-code"] ?? "",
  codex: manifest.dependencies["@openai/codex"] ?? "",
  openForge: manifest.dependencies["@thelithiumforge/open-forge"] ?? "",
} as const;

const DockerLimits = { checkMs: 30_000, buildMs: 20 * 60_000 } as const;

export function evalImageTag(): string {
  return `open-forge-evals:claude-${ImageVersions.claude}-codex-${ImageVersions.codex}`;
}

export async function dockerAvailable(): Promise<boolean> {
  const info = await runCommand("docker", ["info", "--format", "{{.ServerVersion}}"], { timeoutMs: DockerLimits.checkMs });
  return info.code === 0;
}

/** Builds the image the first time it's needed. Later runs reuse it. */
export async function ensureEvalImage(): Promise<void> {
  const tag = evalImageTag();
  const inspect = await runCommand("docker", ["image", "inspect", tag], { timeoutMs: DockerLimits.checkMs });
  if (inspect.code === 0) return;
  const versions = [`CLAUDE_VERSION=${ImageVersions.claude}`, `CODEX_VERSION=${ImageVersions.codex}`];
  const build = await runCommand("docker", ["build", "-t", tag, ...versions.flatMap((value) => ["--build-arg", value]), EvalPaths.docker], {
    timeoutMs: DockerLimits.buildMs,
  });
  if (build.code !== 0) throw new Error(`Building the eval image failed:\n${build.stderr.slice(-4000)}`);
}

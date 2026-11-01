import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { Isolation } from "../../config/choices.ts";
import { buildProject } from "../../grading/build.ts";
import type { RunSpace } from "./executor.ts";
import type { StageOptions } from "./stage.ts";

export const SmokeMarker = "OPEN_FORGE_EVAL_SMOKE";
export const SmokeTimeoutMs = 120_000;

/** Runs only the credential-free verifier, with a tiny project and no model calls. */
export async function smokeVerifier(options: Omit<StageOptions, "timeoutMs"> = {}): Promise<void> {
  const root = await mkdtemp(join(tmpdir(), "eval-verifier-"));
  const space: RunSpace = { root, work: join(root, "project"), home: join(root, "home"), project: "project" };
  try {
    await mkdir(space.work);
    await mkdir(space.home);
    await writeFile(join(space.work, "package.json"), JSON.stringify({ private: true, type: "module", scripts: { build: "node build.ts" } }));
    await writeFile(join(space.work, "build.ts"), `import { mkdir, writeFile } from "node:fs/promises";\nawait mkdir("dist", { recursive: true });\nawait writeFile("dist/index.html", "<html><body>${SmokeMarker}</body></html>");\n`);
    const result = await buildProject(Isolation.docker, space, SmokeTimeoutMs, options);
    if (!result.passed || result.html === null) throw new Error(`Verifier smoke failed: ${result.log}`);
    if (!(await readFile(join(space.work, result.html), "utf8")).includes(SmokeMarker)) throw new Error("The verifier smoke page is missing its marker.");
  } finally { await rm(root, { recursive: true, force: true }); }
}

if (import.meta.main) {
  await smokeVerifier();
  console.log("Verifier smoke passed.");
}

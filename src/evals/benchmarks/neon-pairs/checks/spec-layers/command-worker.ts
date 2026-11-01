import { spawnSync } from "node:child_process";
import { mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { CommandId } from "./commands.ts";
import { SpecLayerContainer } from "./paths.ts";

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
      return ["run", "--silent", "replay", "--", `${SpecLayerContainer.commands}/${vector}`];
    default: throw new Error("Unknown verifier command.");
  }
}

export async function isolatedCommand(id: string, output: string, vector?: string): Promise<void> {
  if (!/^(?:command-[a-z]+|replay-\d+)\.json$/.test(output)) throw new Error("Invalid command output filename.");
  await mkdir(SpecLayerContainer.commands, { recursive: true });
  const fault = "/work/src/__eval_type_error.ts";
  if (id === CommandId.stale) await writeFile(fault, 'const __evalTypeError: number = "type error";\nexport {};\n');
  try {
    const args = commandArguments(id, vector);
    const result = spawnSync(id === CommandId.dev ? "timeout" : "npm", id === CommandId.dev ? ["--kill-after=2s", "5s", "npm", ...args] : args,
      { cwd: "/work", encoding: "utf8", timeout: 10 * 60_000, maxBuffer: 2 * 1024 * 1024, env: { ...process.env, HOME: "/home/build", PLAYWRIGHT_BROWSERS_PATH: "/ms-playwright" } });
    let html: string | undefined;
    if (id === CommandId.build) try { html = await readFile("/work/dist/index.html", "utf8"); } catch { /* A failed build can leave no HTML. */ }
    await writeFile(join(SpecLayerContainer.commands, output), JSON.stringify({ code: result.status, timedOut: result.error !== undefined, stdout: result.stdout ?? "", stderr: result.stderr ?? "", ...(html === undefined ? {} : { html }) }));
  } finally { if (id === CommandId.stale) await rm(fault, { force: true }); }
}
if (import.meta.main) {
  if (process.platform !== "linux" || process.getuid?.() !== 10001) throw new Error("Candidate commands require the isolated build user.");
  const [id, output, vector] = process.argv.slice(2);
  if (!id || !output) throw new Error("Expected command and result filename.");
  await isolatedCommand(id, output, vector);
}

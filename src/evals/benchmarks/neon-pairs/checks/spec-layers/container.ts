import { cp, lstat, mkdir, mkdtemp, readFile, writeFile } from "node:fs/promises";
import { dirname, join, sep } from "node:path";
import { tmpdir } from "node:os";
import { PNG } from "pngjs";
import type { SuiteCheckInput, SuiteCheckOutput } from "../../../suite-checks.types.ts";
import { evalsRoot } from "../../../../paths.ts";
import { ContainerStage, type StageOptions } from "../../../../execution/containers/stage.ts";
import { ContainerPaths, ContainerUsers, StageKind } from "../../../../execution/containers/settings.ts";
import { copyOutPlain, copyProjectIn } from "../../../../execution/containers/files.ts";
import { evalImageTag } from "../../../../execution/containers/docker-image.ts";
import { requireDockerResult } from "../../../../execution/containers/docker.ts";
import { SpecLayerContainer, SpecLayerEnvironment, SpecLayerPackages, SpecLayerTimeoutMs } from "./paths.ts";
import { loadDefinition } from "./load.ts";
import { LayerId, type LayerId as Layer } from "./requirements.ts";
import { complete, Evidence, type AssertionOutcome } from "./outcomes.ts";
import { CommandId, buildOutcomes, commandResult, replayOutcomes, type CommandResult } from "./commands.ts";
import { replayVectors } from "./vectors.ts";
import { parseReport } from "./report.ts";
import { removeTemporary } from "./temporary.ts";

export interface LayerOutput extends SuiteCheckOutput { readonly assertions: readonly AssertionOutcome[]; readonly review: readonly { readonly clause: string; readonly reason: string }[] }
const Environment = { HOME: ContainerPaths.buildHome, PLAYWRIGHT_BROWSERS_PATH: "/ms-playwright", npm_config_audit: "false", npm_config_fund: "false" } as const;
async function json(path: string): Promise<unknown> {
  const info = await lstat(path);
  if (!info.isFile() || info.isSymbolicLink() || info.size > 12 * 1024 * 1024) throw new Error("Verifier JSON must be a bounded plain file.");
  return JSON.parse(await readFile(path, "utf8"));
}

export async function runLayerChecks(layer: Layer, input: SuiteCheckInput, options: StageOptions = input.stage ?? { timeoutMs: SpecLayerTimeoutMs }): Promise<LayerOutput> {
  const declared = await loadDefinition(layer), evidence = new Evidence();
  const selected = { applicable: declared.stated, stated: declared.stated, working: declared.working };
  const result = (inherited: SuiteCheckOutput["checks"] = [], shots: readonly string[] = []): LayerOutput => {
    for (const check of declared.checks) for (const assertion of check.assertions) if (!evidence.seen.has(assertion.clause)) evidence.record(assertion.clause, "error", "The checker returned no assertion for this clause.");
    return { checks: complete(declared, evidence.outcomes, inherited), selection: selected, performance: null, shots, assertions: evidence.outcomes, review: declared.review };
  };
  const blocked = (cause: string): SuiteCheckOutput["checks"] => declared.checks.filter((check) => check.inheritedFrom).map((check) => ({ id: check.id, area: check.area, status: "blocked", evidence: cause }));
  if (!input.html) {
    for (const check of declared.checks) for (const assertion of check.assertions) evidence.record(assertion.clause, "blocked", "The failed build left no page to check.");
    return result(blocked("The failed build left no page to check."));
  }
  const scratch = await mkdtemp(join(tmpdir(), "eval-spec-layers-checks-")), stage = new ContainerStage(options);
  try {
    const harness = join(scratch, "harness"), source = join(evalsRoot, "benchmarks", "neon-pairs");
    for (const folder of ["checks", "layers"]) await cp(join(source, folder), join(harness, "benchmarks", "neon-pairs", folder), { recursive: true });
    await cp(join(source, "requirements.json"), join(harness, "benchmarks", "neon-pairs", "requirements.json"));
    for (const file of ["history/statuses.ts", "execution/containers/settings.ts"]) { await mkdir(dirname(join(harness, file)), { recursive: true }); await cp(join(evalsRoot, file), join(harness, file)); }
    for (const name of SpecLayerPackages) await cp(join(evalsRoot, "node_modules", name), join(harness, "node_modules", name), { recursive: true });
    await writeFile(join(harness, "package.json"), JSON.stringify({ type: "module" }));
    await writeFile(join(harness, "input.json"), JSON.stringify({ layer, build: { typecheck: input.build.typecheck } }));
    await cp(input.html, join(harness, "index.html"));
    await stage.open(evalImageTag(StageKind.verifier));
    await copyProjectIn(stage, { root: scratch, work: input.project, home: join(scratch, "home"), project: "neon-pairs" }, true);
    await stage.copyIn(harness, SpecLayerContainer.harness);
    await stage.prepare(ContainerPaths.work);
    // The root-owned parent prevents candidate processes from replacing the checker folder.
    await stage.exec(["mkdir", "-m", "700", SpecLayerContainer.evidence], ContainerUsers.root).then(requireDockerResult);
    await stage.exec(["chown", ContainerUsers.agent, SpecLayerContainer.evidence], ContainerUsers.root).then(requireDockerResult);
    await stage.exec(["mkdir", "-m", "700", SpecLayerContainer.savedCommands], ContainerUsers.agent).then(requireDockerResult);
    await stage.exec(["mkdir", SpecLayerContainer.commands], ContainerUsers.build).then(requireDockerResult);
    const run = async (id: CommandId, output = `command-${id}.json`, vector?: string): Promise<void> => {
      requireDockerResult(await stage.exec(["node", SpecLayerContainer.command, id, output, ...(vector ? [vector] : [])], ContainerUsers.build, Environment));
      requireDockerResult(await stage.exec(["cp", "--no-dereference", `${SpecLayerContainer.commands}/${output}`, `${SpecLayerContainer.savedCommands}/${output}`], ContainerUsers.agent));
    };
    await run(CommandId.ci);
    const early = join(scratch, "early");
    await copyOutPlain(stage, SpecLayerContainer.savedCommands, early, scratch);
    const install = commandResult(await json(join(early, `command-${CommandId.ci}.json`)));
    if (install.code !== 0 || install.timedOut) {
      for (const check of declared.checks) for (const assertion of check.assertions) evidence.record(assertion.clause, assertion.clause === "T07.1" ? install.timedOut ? "error" : "fail" : "blocked", "The locked dependency installation failed before dependent checks.");
      return result(blocked("The locked dependency installation failed."));
    }
    await stage.offline();
    for (const id of [CommandId.test, CommandId.build, ...(layer === LayerId.three ? [CommandId.typecheck, CommandId.dev, CommandId.e2e, CommandId.stale] : [])]) await run(id);
    const vectors = layer === LayerId.three ? replayVectors() : [];
    const vectorFiles = join(scratch, "vectors"); await mkdir(vectorFiles);
    for (const [index, vector] of vectors.entries()) await writeFile(join(vectorFiles, `vector-${index}.json`), vector.text);
    await stage.copyIn(`${vectorFiles}${sep}.`, SpecLayerContainer.commands);
    for (const index of vectors.keys()) await run(CommandId.replay, `replay-${index}.json`, `vector-${index}.json`);
    requireDockerResult(await stage.exec(["node", ContainerPaths.stopTools], ContainerUsers.build, Environment));
    requireDockerResult(await stage.exec(["node", `${SpecLayerContainer.harness}/${SpecLayerContainer.worker}`], ContainerUsers.agent, SpecLayerEnvironment));
    await stage.stop();
    const collected = join(scratch, "collected"), commands = join(scratch, "commands");
    await copyOutPlain(stage, SpecLayerContainer.evidence, collected, scratch);
    await copyOutPlain(stage, SpecLayerContainer.savedCommands, commands, scratch);
    const report = await parseReport(await json(join(collected, SpecLayerContainer.report)), declared);
    for (const assertion of report.assertions) evidence.record(assertion.clause, assertion.status, assertion.evidence);
    const results = new Map<CommandId, CommandResult>();
    for (const id of Object.values(CommandId).filter((id) => id !== CommandId.replay && (layer === LayerId.three || [CommandId.ci, CommandId.test, CommandId.build].some((item) => item === id)))) results.set(id, commandResult(await json(join(commands, `command-${id}.json`))));
    buildOutcomes(results, evidence, layer === LayerId.three);
    replayOutcomes(vectors, await Promise.all(vectors.map((_, index) => json(join(commands, `replay-${index}.json`)).then(commandResult))), evidence);
    await mkdir(input.shotsFolder, { recursive: true });
    for (const name of report.inherited.shots) {
      const image = await readFile(join(collected, SpecLayerContainer.shots, name));
      if (image.length > 8 * 1024 * 1024 || image.length < 24 || image.readUInt32BE(16) * image.readUInt32BE(20) > 16 * 1024 * 1024) throw new Error("Verifier image exceeds its limits.");
      PNG.sync.read(image); await writeFile(join(input.shotsFolder, name), image);
    }
    return result(report.inherited.checks, report.inherited.shots);
  } catch (error) {
    for (const check of declared.checks) for (const assertion of check.assertions) if (!evidence.seen.has(assertion.clause)) evidence.record(assertion.clause, "error", error instanceof Error ? error.message : String(error));
    return result();
  } finally { try { await stage.remove(); } finally { await removeTemporary(scratch, "eval-spec-layers-checks-"); } }
}
export const runLayer2Checks = (input: SuiteCheckInput): Promise<LayerOutput> => runLayerChecks(LayerId.two, input);
export const runLayer3Checks = (input: SuiteCheckInput): Promise<LayerOutput> => runLayerChecks(LayerId.three, input);

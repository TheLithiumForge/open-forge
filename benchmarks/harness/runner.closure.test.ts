import { describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import path from "node:path";
import {
  pathExists,
  repoRoot,
  requireSuccess,
  runGit,
  runProcess,
  useTestSandbox,
  writeText,
} from "../../tests/support/index.ts";
import {
  discoverMetaScenarios,
  discoverPrimitives,
  discoverScenarios,
  finishRun,
  main,
  prepareMetaScenario,
  type PreparedRun,
  type PrimitiveKind,
} from "./runner.ts";

const sandbox = useTestSandbox("open-forge-benchmark-composition-closure");

describe("benchmark composition lifecycle", () => {
  test("discovers all three recursive registries while folders remain organization only", async () => {
    const root = await sandbox.createDirectory("discovery");
    const fixture = roots(root);
    await writeScenario(path.join(fixture.scenariosRoot, "teams", "alpha", "folder-is-not-id"), "z scenario");
    await writeScenario(path.join(fixture.scenariosRoot, "misc", "anything"), "a scenario", {
      "fixtures/scenario.json": "{\"ordinary\":true}\n",
    });
    await writePrimitive(path.join(fixture.primitivesRoot, "memory", "folder-is-not-id"), {
      id: "remember this",
      kind: "memory",
      payload: { ".agents/memory/crystallized/documents/fact.md": "# Fact\n" },
    });
    await writeMeta(path.join(fixture.metaScenariosRoot, "nested", "folder-is-not-id"), {
      id: "composed case",
      scenario: "a scenario",
      primitives: ["remember this"],
      extensions: [],
    });

    expect((await discoverScenarios(fixture.scenariosRoot)).map((item) => item.id)).toEqual(["a scenario", "z scenario"]);
    expect((await discoverPrimitives(fixture.primitivesRoot)).map((item) => item.id)).toEqual(["remember this"]);
    expect((await discoverMetaScenarios(fixture.metaScenariosRoot)).map((item) => item.id)).toEqual(["composed case"]);

    await writeScenario(path.join(fixture.scenariosRoot, "duplicate"), "a scenario");
    await expect(discoverScenarios(fixture.scenariosRoot)).rejects.toThrow("Duplicate scenario id: a scenario");
  });

  test("enforces pure scenario, primitive route, local tool, and link boundaries", async () => {
    const scenarioAgents = await sandbox.createDirectory("guard-scenario-agents");
    await writeScenario(path.join(scenarioAgents, "scenario"), "bad", {
      ".agents/memory/fact.md": "hidden framework context\n",
    });
    await expect(discoverScenarios(scenarioAgents)).rejects.toThrow("may not contain .agents content");

    const scenarioInstructions = await sandbox.createDirectory("guard-scenario-instructions");
    await writeScenario(path.join(scenarioInstructions, "scenario"), "bad", {
      "project/AGENTS.md": "extra worker instruction\n",
    });
    await expect(discoverScenarios(scenarioInstructions)).rejects.toThrow("may not contain AGENTS.md");

    const wrongRoute = await sandbox.createDirectory("guard-wrong-route");
    await writePrimitive(path.join(wrongRoute, "primitive"), {
      id: "wrong route",
      kind: "pattern",
      payload: { ".agents/memory/crystallized/documents/not-a-pattern.md": "# Wrong\n" },
    });
    await expect(discoverPrimitives(wrongRoute)).rejects.toThrow("must be under .agents/patterns");

    const toolAgents = await sandbox.createDirectory("guard-tool-agents");
    await writePrimitive(path.join(toolAgents, "primitive"), {
      id: "bad tool",
      kind: "tool",
      payload: { ".agents/tools/tool.txt": "not ordinary\n" },
    });
    await expect(discoverPrimitives(toolAgents)).rejects.toThrow(".agents files must be under .agents/workspace");

    const emptyPrimitive = await sandbox.createDirectory("guard-empty-primitive");
    await writePrimitive(path.join(emptyPrimitive, "primitive"), { id: "empty", kind: "memory" });
    await expect(discoverPrimitives(emptyPrimitive)).rejects.toThrow("must contain worker-visible payload");

    const routedOnlyTool = await sandbox.createDirectory("guard-routed-only-tool");
    await writePrimitive(path.join(routedOnlyTool, "primitive"), {
      id: "routed only",
      kind: "tool",
      payload: { ".agents/workspace/local-tool.md": "# Local Tool\n" },
    });
    await expect(discoverPrimitives(routedOnlyTool)).rejects.toThrow("at least one ordinary non-.agents file");

    const validTool = await sandbox.createDirectory("guard-valid-tool");
    await writePrimitive(path.join(validTool, "primitive"), {
      id: "local fixture tool",
      kind: "tool",
      payload: {
        "tools/fixture/primitive.json": "{\"ordinary\":true}\n",
        ".agents/workspace/local-tool.md": "# Local Tool\n",
      },
    });
    expect((await discoverPrimitives(validTool)).map((item) => item.id)).toEqual(["local fixture tool"]);

    const linkedRoot = await sandbox.createDirectory("guard-linked");
    const real = path.join(linkedRoot, "real");
    const linked = path.join(linkedRoot, "linked");
    await writeScenario(real, "linked scenario");
    try {
      await fs.symlink(real, linked, "junction");
      await expect(discoverScenarios(linkedRoot)).rejects.toThrow("linked entry");
    } catch (error) {
      if (!["EPERM", "EACCES"].includes((error as NodeJS.ErrnoException).code ?? "")) throw error;
    }
  });

  test("resolves and freezes a meta-scenario, external variant, extra extension, addendum, and review order", async () => {
    const fixture = await createCompositionFixture("prepare");
    const variantDirectory = path.join(fixture.root, "external", "variant");
    await writePrimitive(variantDirectory, {
      id: "run-local memory",
      kind: "memory",
      payload: { ".agents/memory/crystallized/documents/run-local.md": "# Run Local\n\nFrozen treatment.\n" },
      review: "Review the run-local treatment.\n",
    });
    const addendumFile = path.join(fixture.root, "orchestrator-addendum.md");
    const addendum = Buffer.from("# Extra Review\r\n\r\nRetain this exact input.\r\n", "utf8");
    await fs.writeFile(addendumFile, addendum);

    const prepared: PreparedRun = await prepareMetaScenario({
      metaScenarioId: fixture.metaScenarioId,
      ...fixture.options,
      variantDirectories: [variantDirectory],
      extraExtensions: ["development-toolkit"],
      orchestratorFile: addendumFile,
    });

    expect(prepared).toMatchObject({
      metaScenarioId: fixture.metaScenarioId,
      scenarioId: fixture.scenarioId,
      primitiveIds: [fixture.primitiveId],
      variantIds: ["run-local memory"],
      extensions: ["development-toolkit"],
    });
    expect(JSON.parse(await fs.readFile(prepared.compositionPath, "utf8"))).toEqual({
      metaScenarioId: fixture.metaScenarioId,
      scenarioId: fixture.scenarioId,
      primitiveIds: [fixture.primitiveId],
      variantIds: ["run-local memory"],
      extensions: ["development-toolkit"],
      workerPromptSha256: prepared.workerPromptSha256,
      baselineTree: prepared.baselineTree,
    });
    expect(prepared.workerPromptSha256).toMatch(/^[0-9a-f]{64}$/);
    expect(prepared.baselineTree).toMatch(/^[0-9a-f]{40,64}$/);
    expect(requireSuccessValue(await runGit(prepared.workspaceDir, "rev-parse", "HEAD^{tree}"), "git tree").trim()).toBe(
      prepared.baselineTree,
    );
    expect(await fs.readFile(path.join(prepared.workspaceDir, "project.txt"), "utf8")).toBe("scenario fixture\n");
    expect(await fs.readFile(path.join(prepared.workspaceDir, ".agents", "memory", "crystallized", "documents", "stable.md"), "utf8")).toContain("Stable");
    expect(await fs.readFile(path.join(prepared.workspaceDir, ".agents", "memory", "crystallized", "documents", "run-local.md"), "utf8")).toContain("Frozen treatment");
    expect(await fs.readFile(path.join(prepared.workspaceDir, "open-forge.extensions.json"), "utf8")).toContain("development-toolkit");
    expect(await fs.readFile(prepared.orchestratorAddendumPath!)).toEqual(addendum);

    for (const name of [
      "composition.json",
      "review-order.md",
      "worker-review-prompt.md",
      "orchestrator-prompt.md",
      "orchestrator-addendum.md",
    ]) expect(await pathExists(path.join(prepared.inputDir, name))).toBe(true);
    for (const name of ["meta/meta.json", "meta/review.md", "scenario/scenario.json", "scenario/prompt.md", "scenario/review.md", "scenario/payload/project.txt", "primitives/00/primitive.json", "primitives/00/payload/.agents/memory/crystallized/documents/stable.md", "variants/00/primitive.json", "variants/00/review.md", "variants/00/payload/.agents/memory/crystallized/documents/run-local.md"]) {
      expect(await pathExists(path.join(prepared.inputDir, ...name.split("/")))).toBe(true);
    }
    const order = await fs.readFile(prepared.reviewIndexPath, "utf8");
    expect(order.indexOf("Scenario review")).toBeLessThan(order.indexOf("Primitive review 1"));
    expect(order.indexOf("Primitive review 1")).toBeLessThan(order.indexOf("Variant review 1"));
    expect(order.indexOf("Variant review 1")).toBeLessThan(order.indexOf("Meta-scenario review"));
    expect(await pathExists(path.join(prepared.recordDir, "trace", "raw"))).toBe(true);
    expect(await fs.readFile(path.join(prepared.recordDir, "trace", "manifest.json"), "utf8")).toContain("Replace with");
    expect(await pathExists(path.join(prepared.workspaceDir, "composition.json"))).toBe(false);
    expect(await pathExists(path.join(prepared.workspaceDir, "review-order.md"))).toBe(false);

    await fs.writeFile(path.join(variantDirectory, "review.md"), "mutated after preparation\n");
    expect(await fs.readFile(prepared.variantReviewPaths[0], "utf8")).toBe("Review the run-local treatment.\n");
    expect(requireSuccessValue(await runGit(prepared.workspaceDir, "status", "--short"), "git status")).toBe("");
  }, 120_000);

  test("rejects unresolved ids, external id ambiguity, and payload collisions before allocating a run", async () => {
    const unknown = await createCompositionFixture("unknown", { primitiveReference: "missing primitive" });
    await expect(prepareMetaScenario({ metaScenarioId: unknown.metaScenarioId, ...unknown.options })).rejects.toThrow(
      "references unknown primitive",
    );
    expect(await pathExists(unknown.options.runsRoot)).toBe(false);

    const duplicate = await createCompositionFixture("duplicate-variant");
    const duplicateVariant = path.join(duplicate.root, "external", "duplicate");
    await writePrimitive(duplicateVariant, {
      id: duplicate.primitiveId,
      kind: "memory",
      payload: { ".agents/memory/crystallized/documents/other.md": "# Other\n" },
    });
    await expect(prepareMetaScenario({
      metaScenarioId: duplicate.metaScenarioId,
      ...duplicate.options,
      variantDirectories: [duplicateVariant],
    })).rejects.toThrow("Stable primitive and variant ids must not contain duplicates");
    expect(await pathExists(duplicate.options.runsRoot)).toBe(false);

    const collision = await createCompositionFixture("collision");
    const collisionVariant = path.join(collision.root, "external", "collision");
    await writePrimitive(collisionVariant, {
      id: "different id",
      kind: "memory",
      payload: { ".agents/memory/crystallized/documents/stable.md": "# Conflicting Bytes\n" },
    });
    await expect(prepareMetaScenario({
      metaScenarioId: collision.metaScenarioId,
      ...collision.options,
      variantDirectories: [collisionVariant],
    })).rejects.toThrow("Benchmark payload collision");
    expect(await pathExists(collision.options.runsRoot)).toBe(false);
  });

  test("requires a non-overlapping external runs root", async () => {
    const fixture = await createCompositionFixture("runs-root-boundary");
    await expect(prepareMetaScenario({
      metaScenarioId: fixture.metaScenarioId,
      scenariosRoot: fixture.options.scenariosRoot,
      primitivesRoot: fixture.options.primitivesRoot,
      metaScenariosRoot: fixture.options.metaScenariosRoot,
      repoRoot,
    })).rejects.toThrow("--runs-root is required");
    await expect(prepareMetaScenario({
      metaScenarioId: fixture.metaScenarioId,
      ...fixture.options,
      runsRoot: path.join(repoRoot, "benchmarks", "runs"),
    })).rejects.toThrow("Runs root must be outside");
    await expect(prepareMetaScenario({
      metaScenarioId: fixture.metaScenarioId,
      ...fixture.options,
      runsRoot: path.dirname(repoRoot),
    })).rejects.toThrow("Runs root must be outside");
  });

  test("checked-in control and trap use one pure scenario and every meta-scenario prepares through the CLI", async () => {
    const metaScenarios = await discoverMetaScenarios();
    const control = metaScenarios.find((item) => item.id === "ledger-immutable-control");
    const trap = metaScenarios.find((item) => item.id === "ledger-current-truth-trap");
    expect(control?.scenario).toBe("ledger-remove-planning");
    expect(trap?.scenario).toBe(control?.scenario);
    expect(trap?.primitives).toEqual(expect.arrayContaining(control?.primitives ?? []));

    const root = await sandbox.createDirectory("checked-in");
    const runsRoot = path.join(root, "runs");
    const runnerFile = path.join(repoRoot, "benchmarks", "harness", "runner.ts");
    const listed = await runProcess([process.execPath, runnerFile, "list"], { cwd: repoRoot });
    requireSuccess(listed, "benchmark list");
    const ids = listed.stdout.trim().split(/\r?\n/).filter(Boolean);
    expect(ids).toEqual(metaScenarios.map((item) => item.id));
    expect(ids.length).toBeGreaterThan(0);
    const preparedById = new Map<string, PreparedRun>();

    for (const id of ids) {
      const result = await runProcess([
        process.execPath,
        runnerFile,
        "prepare",
        id,
        "--runs-root",
        runsRoot,
      ], { cwd: repoRoot });
      requireSuccess(result, `benchmark prepare ${id}`);
      const prepared = JSON.parse(result.stdout) as PreparedRun;
      preparedById.set(id, prepared);
      expect(prepared.metaScenarioId).toBe(id);
      expect(await pathExists(prepared.compositionPath)).toBe(true);
      expect(await pathExists(prepared.reviewIndexPath)).toBe(true);
      const doctor = JSON.parse(await fs.readFile(path.join(prepared.recordDir, "prepare-doctor.json"), "utf8"));
      expect(doctor).toMatchObject({ errors: 0, warnings: 0 });
      expect(await pathExists(path.join(prepared.recordDir, "prepare-find-follow-required.json"))).toBe(true);
      expect(requireSuccessValue(await runGit(prepared.workspaceDir, "status", "--short"), "git status")).toBe("");
    }

    const preparedControl = preparedById.get("ledger-immutable-control")!;
    const preparedTrap = preparedById.get("ledger-current-truth-trap")!;
    expect(preparedTrap.scenarioId).toBe(preparedControl.scenarioId);
    expect(preparedTrap.workerPromptSha256).toBe(preparedControl.workerPromptSha256);
    expect(preparedTrap.baselineTree).not.toBe(preparedControl.baselineTree);
  }, 300_000);

  test("finish requires honest trace metadata, preserves validator failures, and records composition identity", async () => {
    const fixture = await createCompositionFixture("finish");
    const prepared = await prepareMetaScenario({ metaScenarioId: fixture.metaScenarioId, ...fixture.options });
    await fs.appendFile(path.join(prepared.workspaceDir, "project.txt"), "worker change\n");
    await fs.writeFile(path.join(prepared.workspaceDir, ".agents", "directives", "index.md"), "# Conflicting entrypoint\n");
    await fs.writeFile(path.join(prepared.recordDir, "worker-review.md"), "Worker account.\n");
    await fs.writeFile(path.join(prepared.recordDir, "orchestrator-review.md"), completeReview());

    await expect(finishRun(prepared.runDir, { repoRoot })).rejects.toThrow("trace/manifest.json is incomplete");
    await fs.writeFile(path.join(prepared.recordDir, "trace", "manifest.json"), `${JSON.stringify({
      captureMethod: "The orchestrator observed messages and tool calls through the test runtime.",
      observableSurfaces: ["messages", "tool calls"],
      knownGaps: ["No private latent reasoning was exposed."],
      rawArtifacts: [],
    }, null, 2)}\n`);

    const result = await finishRun(prepared.runDir, { repoRoot });
    expect(result).toMatchObject({
      metaScenarioId: fixture.metaScenarioId,
      scenarioId: fixture.scenarioId,
      primitiveIds: [fixture.primitiveId],
      variantIds: [],
      extensions: [],
      workerPromptSha256: prepared.workerPromptSha256,
      baselineTree: prepared.baselineTree,
      state: "finished",
      doctor: { exitCode: 1 },
    });
    expect(await fs.readFile(path.join(prepared.recordDir, "changes.patch"), "utf8")).toContain("worker change");
    expect(JSON.parse(await fs.readFile(path.join(prepared.recordDir, "result.json"), "utf8"))).toEqual(result);
    await expect(finishRun(prepared.runDir, { repoRoot })).rejects.toThrow("already finished");
  }, 120_000);

  test("CLI parsing keeps orchestrator addenda separate and permits repeatable variants and extensions", async () => {
    await expect(main(["list", "unknown"])).rejects.toThrow("list [meta-scenarios|scenarios|primitives]");
    await expect(main(["prepare", "case", "--variant"])).rejects.toThrow("--variant <primitive-package-dir>");
    await expect(main(["prepare", "case", "--extension"])).rejects.toThrow("--extension <bundled-id>");
    await expect(main(["prepare", "case", "--orchestrator"])).rejects.toThrow("--orchestrator <file>");
  });
});

function roots(root: string) {
  return {
    scenariosRoot: path.join(root, "benchmarks", "building-blocks", "scenarios"),
    primitivesRoot: path.join(root, "benchmarks", "building-blocks", "primitives"),
    metaScenariosRoot: path.join(root, "benchmarks", "meta-scenarios"),
    runsRoot: path.join(root, "runs"),
  };
}

async function createCompositionFixture(
  name: string,
  changes: { primitiveReference?: string } = {},
): Promise<{
  root: string;
  scenarioId: string;
  primitiveId: string;
  metaScenarioId: string;
  options: ReturnType<typeof roots>;
}> {
  const root = await sandbox.createDirectory(name);
  const options = roots(root);
  const scenarioId = `${name} scenario`;
  const primitiveId = `${name} stable memory`;
  const metaScenarioId = `${name} composition`;
  await writeScenario(path.join(options.scenariosRoot, "organization", "scenario"), scenarioId, {
    "project.txt": "scenario fixture\n",
  });
  await writePrimitive(path.join(options.primitivesRoot, "memory", "stable"), {
    id: primitiveId,
    kind: "memory",
    payload: { ".agents/memory/crystallized/documents/stable.md": "# Stable\n\nAccepted fixture.\n" },
  });
  await writeMeta(path.join(options.metaScenariosRoot, "organization", "composition"), {
    id: metaScenarioId,
    scenario: scenarioId,
    primitives: [changes.primitiveReference ?? primitiveId],
    extensions: [],
  });
  return { root, scenarioId, primitiveId, metaScenarioId, options };
}

async function writeScenario(directory: string, id: string, payload: Record<string, string> = {}): Promise<void> {
  await writeText(path.join(directory, "scenario.json"), `${JSON.stringify({ id }, null, 2)}\n`);
  await writeText(path.join(directory, "prompt.md"), "Complete the ordinary task.\n");
  await writeText(path.join(directory, "review.md"), "Review the task outcome.\n");
  for (const [relative, content] of Object.entries(payload)) await writeText(path.join(directory, "payload", relative), content);
}

async function writePrimitive(
  directory: string,
  options: { id: string; kind: PrimitiveKind; payload?: Record<string, string>; review?: string },
): Promise<void> {
  await writeText(path.join(directory, "primitive.json"), `${JSON.stringify({ id: options.id, kind: options.kind }, null, 2)}\n`);
  await writeText(path.join(directory, "review.md"), options.review ?? "Review the primitive effect.\n");
  for (const [relative, content] of Object.entries(options.payload ?? {})) {
    await writeText(path.join(directory, "payload", relative), content);
  }
}

async function writeMeta(
  directory: string,
  value: { id: string; scenario: string; primitives: string[]; extensions: string[] },
): Promise<void> {
  await writeText(path.join(directory, "meta.json"), `${JSON.stringify(value, null, 2)}\n`);
  await writeText(path.join(directory, "review.md"), "Review the composed relationship.\n");
}

function completeReview(): string {
  return `# Orchestrator Review

## Behavior
Observed behavior.

## Outcome
Observed outcome.

## Comparison With Worker Review
Compared accounts.

## Limits
Bounded conclusion.
`;
}

function requireSuccessValue(result: { exitCode: number; stdout: string; stderr: string }, label: string): string {
  requireSuccess(result, label);
  return result.stdout;
}

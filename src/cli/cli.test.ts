import { afterEach, describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";

const cliFile = path.join(import.meta.dir, "cli.ts");
const temporaryRoots: string[] = [];

afterEach(async () => {
  await Promise.all(temporaryRoots.splice(0).map((root) => fs.rm(root, { recursive: true, force: true })));
});

describe("category index generation", () => {
  test("replaces only the bounded generated region", async () => {
    const root = await createRoot();
    const category = await createCategory(root, "workspace", `# Workspace

Stable category contract.

## Entries

<!-- open-forge:generated-index:start -->
- old.md - Old entry - #Old
<!-- open-forge:generated-index:end -->
`);
    await writeRoute(category, "repositories.md", "Repository routes", ["Workspace", "Route"]);

    const result = await runCli("index", root);
    const content = await fs.readFile(path.join(category, "_workspace.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(content).toContain("Stable category contract.");
    expect(content).toContain("<!-- open-forge:generated-index:start -->");
    expect(content).toContain("- `repositories.md` - Repository routes - #Workspace #Route");
    expect(content).not.toContain("Old entry");
  });

  test("migrates a legacy final entries section", async () => {
    const root = await createRoot();
    const category = await createCategory(root, "patterns", `# Patterns

Stable category contract.

## Entries

- old.md - Old entry - #Old
`);
    await writeRoute(category, "components.md", "Component patterns", ["Pattern"]);

    const result = await runCli("index", root);
    const content = await fs.readFile(path.join(category, "_patterns.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(content).toContain("Stable category contract.");
    expect(content).toContain("<!-- open-forge:generated-index:start -->");
    expect(content).toContain("- `components.md` - Component patterns - #Pattern");
    expect(content).not.toContain("Old entry");
  });

  test("appends the complete generated section when absent", async () => {
    const root = await createRoot();
    const category = await createCategory(root, "workflows", `# Workflows

Stable category contract.
`);

    const result = await runCli("index", root);
    const content = await fs.readFile(path.join(category, "_workflows.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(content).toEndWith(`## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
`);
  });

  test("rejects incomplete markers without changing the file", async () => {
    const root = await createRoot();
    const original = `# Templates

## Entries

<!-- open-forge:generated-index:start -->
- keep.md - Keep this - #Keep
`;
    const category = await createCategory(root, "templates", original);

    const result = await runCli("index", root);
    const content = await fs.readFile(path.join(category, "_templates.md"), "utf8");

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("markers are incomplete or duplicated");
    expect(content).toBe(original);
  });

  test("rejects authored content inside an unmarked legacy section", async () => {
    const root = await createRoot();
    const original = `# Sessions

## Entries

This paragraph is authored content.
`;
    const category = await createCategory(root, "sessions", original);

    const result = await runCli("index", root);
    const content = await fs.readFile(path.join(category, "_sessions.md"), "utf8");

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("contains authored content");
    expect(content).toBe(original);
  });

  test("routes categories through child entrypoints at arbitrary depth", async () => {
    const root = await createRoot();
    const workspace = await createCategory(root, "workspace", "# Workspace\n");
    const repositories = await createCategory(workspace, "repositories", "# Repositories\n");
    const services = await createCategory(repositories, "services", "# Services\n");
    await writeRoute(repositories, "api.md", "API repository", ["Repository"]);
    await writeRoute(services, "billing.md", "Billing service", ["Service"]);

    const result = await runCli("index", root);
    const parent = await fs.readFile(path.join(workspace, "_workspace.md"), "utf8");
    const child = await fs.readFile(path.join(repositories, "_repositories.md"), "utf8");
    const grandchild = await fs.readFile(path.join(services, "_services.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(parent).toContain("- `repositories/_repositories.md` - No description - #Index");
    expect(child).toContain("- `api.md` - API repository - #Repository");
    expect(child).toContain("- `services/_services.md` - No description - #Index");
    expect(parent).not.toContain("services/_services.md");
    expect(grandchild).toContain("- `billing.md` - Billing service - #Service");
  });
});

describe("loader category registry", () => {
  test("generates direct active categories from their entrypoint metadata", async () => {
    const root = await createRoot();
    await fs.writeFile(path.join(root, "loader.md"), `# Loader

Stable loading contract.
`);
    const workspace = await createCategory(root, "workspace", `---
open-forge:
  description: Important workspace destinations
  tags: [OpenForge, Workspace, Index]
---

# Workspace
`);
    await createCategory(workspace, "repositories", "# Repositories\n");
    await fs.mkdir(path.join(root, "archive"));

    const result = await runCli("index", root);
    const loader = await fs.readFile(path.join(root, "loader.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(loader).toContain("Stable loading contract.");
    expect(loader).toContain("- `workspace/_workspace.md` - Important workspace destinations - #OpenForge #Workspace #Index");
    expect(loader).not.toContain("repositories/_repositories.md");
    expect(loader).not.toContain("archive");
  });

  for (const alias of ["index.md", "_index.md", "references.md", "_references.md"]) {
    test(`supports ${alias} as a compatibility category entrypoint`, async () => {
      const root = await createRoot();
      await fs.writeFile(path.join(root, "loader.md"), "# Loader\n");
      const external = await createCategory(root, "external", `---
description: External tool routes
tags: [External, Index]
---

# External
`, alias);
      await writeRoute(external, "source.md", "External source", ["External"]);

      const result = await runCli("index", root);
      const loader = await fs.readFile(path.join(root, "loader.md"), "utf8");
      const entrypoint = await fs.readFile(path.join(external, alias), "utf8");

      expect(result.exitCode).toBe(0);
      expect(loader).toContain(`- \`external/${alias}\` - External tool routes - #External #Index`);
      expect(entrypoint).toContain("- `source.md` - External source - #External");
    });
  }

  test("rejects multiple entrypoint names in one folder before writing", async () => {
    const root = await createRoot();
    const loaderOriginal = "# Loader\n";
    await fs.writeFile(path.join(root, "loader.md"), loaderOriginal);
    const category = await createCategory(root, "mixed", "# Canonical\n");
    const aliasOriginal = "# Compatibility alias\n";
    await fs.writeFile(path.join(category, "index.md"), aliasOriginal);

    const result = await runCli("index", root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("Multiple category entrypoints found");
    expect(await fs.readFile(path.join(root, "loader.md"), "utf8")).toBe(loaderOriginal);
    expect(await fs.readFile(path.join(category, "index.md"), "utf8")).toBe(aliasOriginal);
  });
});

describe("install", () => {
  test("installs merged core category entrypoints", async () => {
    const root = await createRoot();

    const result = await runCli("install", root);
    const directives = await fs.readFile(path.join(root, ".agents", "directives", "_directives.md"), "utf8");
    const guidelines = await fs.readFile(path.join(root, ".agents", "guidelines", "_guidelines.md"), "utf8");
    const memory = await fs.readFile(path.join(root, ".agents", "memory", "_memory.md"), "utf8");
    const workingMemory = await fs.readFile(path.join(root, ".agents", "memory", "working", "_working.md"), "utf8");
    const emergingMemory = await fs.readFile(path.join(root, ".agents", "memory", "emerging", "_emerging.md"), "utf8");
    const crystallizedMemory = await fs.readFile(path.join(root, ".agents", "memory", "crystallized", "_crystallized.md"), "utf8");
    const archivedMemory = await fs.readFile(path.join(root, ".agents", "memory", "archived", "_archived.md"), "utf8");
    const patterns = await fs.readFile(path.join(root, ".agents", "patterns", "_patterns.md"), "utf8");
    const skills = await fs.readFile(path.join(root, ".agents", "skills", "_skills.md"), "utf8");
    const workflows = await fs.readFile(path.join(root, ".agents", "workflows", "_workflows.md"), "utf8");
    const workspace = await fs.readFile(path.join(root, ".agents", "workspace", "_workspace.md"), "utf8");
    const loader = await fs.readFile(path.join(root, ".agents", "loader.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(directives).toContain("Every directive file beside this entrypoint is workspace-wide; load all of them.");
    expect(directives).toContain("Child directive entrypoints define positive scope through path, description, and tags.");
    expect(directives).toContain("Load child directive routes when their path, description, tags, or defined tag behavior match the current work.");
    expect(directives).not.toContain("Child categories may define a work scope or organize directives");
    expect(directives).not.toContain("always load");
    expect(directives).toContain("<!-- open-forge:generated-index:start -->");
    expect(guidelines).toContain("Every guideline identifies its scenario, preferred approach, reasoning, and relevant tradeoffs.");
    expect(guidelines).toContain("<!-- open-forge:generated-index:start -->");
    expect(memory).toContain("Memory is the workspace state record: current truth, live work, candidate learning, and useful history.");
    expect(memory).toContain("- Use loader-defined `#Contextual` and `#CurrentTruth` tags to distinguish context from accepted current truth.");
    expect(memory).toContain("- Memory records state; it must not own operational behavior.");
    expect(memory).toContain("- Move behavior, reusable form, guidance, capability, workflow, or workspace routing out of Memory and into the matching `#Core` route, including user-created `#Core` categories and files.");
    expect(memory).not.toContain("Core category");
    expect(memory).toContain("- `working/_working.md` - Working memory alive in current work - #OpenForge #Memory #Working #Index #Contextual #LoadWithParentEntrypoint");
    expect(memory).toContain("- `crystallized/_crystallized.md` - Accepted durable memory and current truth - #OpenForge #Memory #Crystallized #Index #CurrentTruth #LoadWithParentEntrypoint");
    expect(memory).toContain("- `emerging/_emerging.md` - Candidate memory becoming useful but not accepted truth - #OpenForge #Memory #Emerging #Index #Contextual #Candidate");
    expect(memory).not.toContain("#Emerging #Index #LoadWithParentEntrypoint");
    expect(loader).toContain("Open Forge is a routing system.");
    expect(loader).toContain("Apply defined tag behavior when reading generated `Entries`.");
    expect(loader).toContain("- `.agents/directives/_directives.md` - Mandatory workspace modifiers; load for every request - #OpenForge #Core #Directive #Index #LoadWithParentEntrypoint");
    expect(loader).toContain("- `.agents/memory/_memory.md` - Memory state routes for human-AI work - #OpenForge #Memory #Index #LoadWithParentEntrypoint");
    expect(loader).toContain("## Tags");
    expect(loader).toContain("### Axioms");
    expect(loader).toContain("- Defined tags have framework meaning when they appear in loaded content or generated `Entries`.");
    expect(loader).toContain("- Undefined tags are routing and search signals; read the entry path, description, and loaded entrypoint for their meaning.");
    expect(loader).toContain("- Entries without a load-policy tag are on-demand routes selected by the current request.");
    expect(loader).toContain("- Tag spelling and casing are stable.");
    expect(loader).toContain("- Workspace-wide tag behavior belongs here and must stay short.");
    expect(loader).toContain("### Defined Tags");
    expect(loader).toContain("- `#LoadWithParentEntrypoint` - Load this entry immediately after its parent entrypoint, in listed order. Applies only inside already loaded `Entries`.");
    expect(loader).toContain("- `#Core` - Layer 1: base routing, workspace orientation, and agent primitive routes.");
    expect(loader).toContain("- `#Memory` - Layer 2: persisted workspace state and memory routes.");
    expect(loader).toContain("- `#Extension` - Layer 3: optional module, pack, template, integration, and support routes.");
    expect(loader).toContain("- `#Contextual` - Supporting context, not accepted current truth unless restored, validated, accepted, or promoted.");
    expect(loader).toContain("- `#CurrentTruth` - Accepted current memory within its stated scope; still below user instructions, runtime safety, platform constraints, and declared external sources of truth.");
    expect(workingMemory).toContain("Working memory is live context for active or recently interrupted work.");
    expect(workingMemory).toContain("tags: [OpenForge, Memory, Working, Index, Contextual, LoadWithParentEntrypoint]");
    expect(emergingMemory).toContain("Emerging memory is useful material that has not become accepted current memory.");
    expect(emergingMemory).toContain("tags: [OpenForge, Memory, Emerging, Index, Contextual, Candidate]");
    expect(emergingMemory).toContain("- Move operational material to the matching `#Core` route, including user-created `#Core` categories and files, instead of accepting it as Memory.");
    expect(crystallizedMemory).toContain("Crystallized memory is accepted current memory within its stated scope.");
    expect(crystallizedMemory).toContain("tags: [OpenForge, Memory, Crystallized, Index, CurrentTruth, LoadWithParentEntrypoint]");
    expect(crystallizedMemory).toContain("- Move operational material to the matching `#Core` route, including user-created `#Core` categories and files, instead of keeping it as crystallized memory.");
    expect(archivedMemory).toContain("Archived memory is preserved context that is no longer current.");
    expect(archivedMemory).toContain("tags: [OpenForge, Memory, Archived, Index, Contextual, Historical]");
    expect(patterns).toContain("Treat an applicable pattern as the established default shape for its scope.");
    expect(patterns).toContain("<!-- open-forge:generated-index:start -->");
    expect(skills).toContain("Every skill file defines one bounded capability, where it applies, and the expected result.");
    expect(skills).toContain("<!-- open-forge:generated-index:start -->");
    expect(workflows).toContain("Every workflow defines its goal, starting context, ordered work shape, expected outputs, and completion or handoff condition.");
    expect(workflows).toContain("<!-- open-forge:generated-index:start -->");
    expect(workspace).toContain("## Axioms");
    expect(workspace).toContain("<!-- open-forge:generated-index:start -->");
    expect(loader).not.toContain("Load `directives` for every request when it appears in `Entries`.");
    expect(loader).not.toContain("Load `memory` after `directives` when it appears in `Entries`.");
    expect(loader).toContain("- `.agents/guidelines/_guidelines.md` - Contextual guidance for recurring decisions and scenarios - #OpenForge #Core #Guideline #Index");
    expect(loader).toContain("- `.agents/patterns/_patterns.md` - Concrete reusable shapes for inspectable work - #OpenForge #Core #Pattern #Index");
    expect(loader).toContain("- `.agents/skills/_skills.md` - Bounded reusable agent capabilities and scoped routes - #OpenForge #Core #Skill #Index");
    expect(loader).toContain("- `.agents/workflows/_workflows.md` - Goal-oriented agent modules and scoped workflow routes - #OpenForge #Core #Workflow #Index");
    expect(loader).toContain("- `.agents/workspace/_workspace.md` - Important workspace destinations and their scope - #OpenForge #Core #Workspace #Index");
    expect(loader).not.toContain("## Route Categories");
    expect(await exists(path.join(root, ".agents", "constants.md"))).toBe(false);
    expect(await exists(path.join(root, ".agents", "workspace", "_workspace-open-forge.md"))).toBe(false);
  });
});

async function createRoot(): Promise<string> {
  const root = await fs.mkdtemp(path.join(os.tmpdir(), "open-forge-cli-"));
  temporaryRoots.push(root);
  return root;
}

async function createCategory(parent: string, name: string, content: string, entrypointName = `_${name}.md`): Promise<string> {
  const category = path.join(parent, name);
  await fs.mkdir(category, { recursive: true });
  await fs.writeFile(path.join(category, entrypointName), content);
  return category;
}

async function writeRoute(folder: string, name: string, description: string, tags: string[]): Promise<void> {
  await fs.writeFile(path.join(folder, name), `---
open-forge:
  description: ${description}
  tags: [${tags.join(", ")}]
---

# ${description}
`);
}

async function runCli(command: "index" | "install", root: string): Promise<{ exitCode: number; stderr: string }> {
  const child = Bun.spawn([process.execPath, cliFile, command, root], {
    stdout: "ignore",
    stderr: "pipe"
  });
  const [exitCode, stderr] = await Promise.all([
    child.exited,
    new Response(child.stderr).text()
  ]);

  return { exitCode, stderr };
}

async function exists(file: string): Promise<boolean> {
  try {
    await fs.access(file);
    return true;
  } catch {
    return false;
  }
}

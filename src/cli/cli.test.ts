import { afterEach, describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import { createExtensionSelectionState, toggleExtensionSelection, type ExtensionDependencyInfo } from "./cli.ts";

const cliFile = path.join(import.meta.dir, "cli.ts");
const temporaryRoots: string[] = [];

afterEach(async () => {
  await Promise.all(temporaryRoots.splice(0).map((root) => fs.rm(root, { recursive: true, force: true })));
});

describe("extension catalogue selection state", () => {
  const extensions: ExtensionDependencyInfo[] = [
    { id: "base-skill", dependencies: [] },
    { id: "shared-workflow", dependencies: ["base-skill"] },
    { id: "feature-flow", dependencies: ["shared-workflow"] },
    { id: "review-flow", dependencies: ["shared-workflow"] }
  ];

  test("auto-selects transitive dependencies as required", () => {
    const state = createExtensionSelectionState(extensions, ["feature-flow"]);

    expect([...state.direct]).toEqual(["feature-flow"]);
    expect([...state.required].sort()).toEqual(["base-skill", "shared-workflow"]);
  });

  test("keeps directly selected dependencies direct and locks required-only entries", () => {
    const state = createExtensionSelectionState(extensions, ["feature-flow", "shared-workflow"]);
    const locked = toggleExtensionSelection(extensions, state, "base-skill");

    expect([...state.direct]).toEqual(["feature-flow", "shared-workflow"]);
    expect([...state.required]).toEqual(["base-skill"]);
    expect(locked).toBe(state);
  });

  test("releases dependencies only after their last dependent is deselected", () => {
    let state = createExtensionSelectionState(extensions, ["feature-flow", "review-flow"]);
    state = toggleExtensionSelection(extensions, state, "feature-flow");

    expect([...state.direct]).toEqual(["review-flow"]);
    expect([...state.required].sort()).toEqual(["base-skill", "shared-workflow"]);

    state = toggleExtensionSelection(extensions, state, "review-flow");
    expect([...state.direct]).toEqual([]);
    expect([...state.required]).toEqual([]);
  });

  test("downgrades a deselected direct dependency to required while it is still needed", () => {
    const state = createExtensionSelectionState(extensions, ["feature-flow", "shared-workflow"]);
    const next = toggleExtensionSelection(extensions, state, "shared-workflow");

    expect([...next.direct]).toEqual(["feature-flow"]);
    expect([...next.required].sort()).toEqual(["base-skill", "shared-workflow"]);
  });
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

  test("validates every generated index before writing any of them", async () => {
    const root = await createRoot();
    const valid = await createCategory(root, "alpha", `# Alpha

## Entries

<!-- open-forge:generated-index:start -->
- stale.md - Stale - #Old
<!-- open-forge:generated-index:end -->
`);
    await writeRoute(valid, "current.md", "Current route", ["Pattern"]);
    const invalid = await createCategory(root, "zeta", `# Zeta

## Entries

<!-- open-forge:generated-index:start -->
- keep.md - Keep - #Keep
`);
    const originalValid = await fs.readFile(path.join(valid, "_alpha.md"), "utf8");
    const originalInvalid = await fs.readFile(path.join(invalid, "_zeta.md"), "utf8");

    const result = await runCli("index", root);

    expect(result.exitCode).toBe(1);
    expect(await fs.readFile(path.join(valid, "_alpha.md"), "utf8")).toBe(originalValid);
    expect(await fs.readFile(path.join(invalid, "_zeta.md"), "utf8")).toBe(originalInvalid);
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
    expect(parent).toContain("- `repositories/_repositories.md` - No description -");
    expect(child).toContain("- `api.md` - API repository - #Repository");
    expect(child).toContain("- `services/_services.md` - No description -");
    expect(parent).not.toContain("services/_services.md");
    expect(grandchild).toContain("- `billing.md` - Billing service - #Service");
  });

  test("indexes native skill packages under skills routes", async () => {
    const root = await createRoot();
    const agents = path.join(root, ".agents");
    const skills = await createCategory(agents, "skills", "# Skills\n");
    const implementation = path.join(skills, "implementation");
    await fs.mkdir(path.join(implementation, "references"), { recursive: true });
    await fs.writeFile(path.join(implementation, "SKILL.md"), `---
name: implementation
description: Implementation capability for fitting, testing, coding, and verifying changes
---

# Implementation
`);
    await fs.writeFile(path.join(implementation, "references", "fit-change.md"), "# Fit Change\n");
    await writeRoute(skills, "legacy.md", "Legacy skill file", ["Skill"]);

    const result = await runCli("index", root);
    const content = await fs.readFile(path.join(skills, "_skills.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(content).toContain("- `implementation/SKILL.md` - Implementation capability for fitting, testing, coding, and verifying changes - #Skill");
    expect(content).not.toContain("references/fit-change.md");
    expect(content).not.toContain("legacy.md");
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
  description: Workspace routes that point to important project locations and explain when to use them
  tags: [LoadNow, Workspace]
---

# Workspace
`);
    await createCategory(workspace, "repositories", "# Repositories\n");
    await fs.mkdir(path.join(root, "archive"));

    const result = await runCli("index", root);
    const loader = await fs.readFile(path.join(root, "loader.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(loader).toContain("Stable loading contract.");
    expect(loader).toContain("- `workspace/_workspace.md` - Workspace routes that point to important project locations and explain when to use them - #LoadNow #Workspace");
    expect(loader).not.toContain("repositories/_repositories.md");
    expect(loader).not.toContain("archive");
  });

  for (const alias of ["index.md", "_index.md", "references.md", "_references.md"]) {
    test(`supports ${alias} as a compatibility category entrypoint`, async () => {
      const root = await createRoot();
      await fs.writeFile(path.join(root, "loader.md"), "# Loader\n");
      const external = await createCategory(root, "external", `---
description: External tool routes
tags: [External, Tool]
---

# External
`, alias);
      await writeRoute(external, "source.md", "External source", ["External"]);

      const result = await runCli("index", root);
      const loader = await fs.readFile(path.join(root, "loader.md"), "utf8");
      const entrypoint = await fs.readFile(path.join(external, alias), "utf8");

      expect(result.exitCode).toBe(0);
      expect(loader).toContain(`- \`external/${alias}\` - External tool routes - #External #Tool`);
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
    const guidance = await fs.readFile(path.join(root, ".agents", "guidance", "_guidance.md"), "utf8");
    const memory = await fs.readFile(path.join(root, ".agents", "memory", "_memory.md"), "utf8");
    const workingMemory = await fs.readFile(path.join(root, ".agents", "memory", "working", "_working.md"), "utf8");
    const handoffsMemory = await fs.readFile(path.join(root, ".agents", "memory", "working", "handoffs", "_handoffs.md"), "utf8");
    const sessionsMemory = await fs.readFile(path.join(root, ".agents", "memory", "working", "sessions", "_sessions.md"), "utf8");
    const emergingMemory = await fs.readFile(path.join(root, ".agents", "memory", "emerging", "_emerging.md"), "utf8");
    const analysisMemory = await fs.readFile(path.join(root, ".agents", "memory", "emerging", "analysis", "_analysis.md"), "utf8");
    const ideasMemory = await fs.readFile(path.join(root, ".agents", "memory", "emerging", "ideas", "_ideas.md"), "utf8");
    const observationsMemory = await fs.readFile(path.join(root, ".agents", "memory", "emerging", "observations", "_observations.md"), "utf8");
    const crystallizedMemory = await fs.readFile(path.join(root, ".agents", "memory", "crystallized", "_crystallized.md"), "utf8");
    const decisionsMemory = await fs.readFile(path.join(root, ".agents", "memory", "crystallized", "decisions", "_decisions.md"), "utf8");
    const documentsMemory = await fs.readFile(path.join(root, ".agents", "memory", "crystallized", "documents", "_documents.md"), "utf8");
    const archivedMemory = await fs.readFile(path.join(root, ".agents", "memory", "archived", "_archived.md"), "utf8");
    const patterns = await fs.readFile(path.join(root, ".agents", "patterns", "_patterns.md"), "utf8");
    const skills = await fs.readFile(path.join(root, ".agents", "skills", "_skills.md"), "utf8");
    const workflows = await fs.readFile(path.join(root, ".agents", "workflows", "_workflows.md"), "utf8");
    const workspace = await fs.readFile(path.join(root, ".agents", "workspace", "_workspace.md"), "utf8");
    const loader = await fs.readFile(path.join(root, ".agents", "loader.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(directives).toContain("Read every direct directive file far enough to evaluate its explicit `Applies To`; root placement alone does not make a directive workspace-wide.");
    expect(directives).toContain("Every directive file declares one positive `Applies To` scope before its Axioms.");
    expect(directives).toContain("a hybrid category entrypoint may state `inherited`");
    expect(directives).not.toContain("Child categories may define a work scope or organize directives");
    expect(directives).not.toContain("always load");
    expect(directives).toContain("<!-- open-forge:generated-index:start -->");
    expect(guidance).toContain("Every guidance file identifies its scenario, preferred approach, reasoning, and relevant tradeoffs.");
    expect(guidance).toContain("<!-- open-forge:generated-index:start -->");
    expect(memory).toContain("Memory is self-growing markdown memory for workspace state: current truth, live work, AI communication, current records, historical records, and candidate learning.");
    expect(memory).toContain("- Use loader-defined #Contextual and #CurrentTruth tags to distinguish context from accepted current truth.");
    expect(memory).toContain("- Memory records state; it must not own operational behavior.");
    expect(memory).toContain("- Write useful durable state to the matching #Memory route when safe and allowed.");
    expect(memory).toContain("- Move material between #Memory routes when its state or owner changes.");
    expect(memory).toContain("- Extract behavior, reusable form, guidance, capability, workflow, or workspace routing to the matching #Core route, including user-created #Core categories and files.");
    expect(memory).toContain("Obtain user accord before promoting inferred material or creating normative #Core/#CurrentTruth from it.");
    expect(loader).toContain("- Axioms of loaded ancestor `entrypoints` apply to all routes below them; a child `entrypoint` adds only what is specific to its scope.");
    expect(loader).toContain("- Writing files inside existing routes is normal use; add child categories when they improve routing, ownership, or clarity, and give new root routes clear scope.");
    expect(memory).not.toContain("Core category");
    expect(memory).toContain("- `working/_working.md` - Temporary memory that helps agents continue or resume active work - #LoadNow #Memory #Working #Contextual");
    expect(memory).toContain("- `crystallized/_crystallized.md` - Accepted durable memory and current truth - #LoadNow #Memory #Crystallized #CurrentTruth");
    expect(memory).toContain("- `emerging/_emerging.md` - Candidate memory that may be useful but is not accepted truth yet - #KeepInMind #Memory #Emerging #OrganicGrowth #Contextual #Candidate");
    expect(memory).not.toContain("Candidate memory that may be useful but is not accepted truth yet - #LoadNow");
    expect(loader).toContain("- `entrypoint` - Markdown file that makes a folder routable.");
    expect(loader).toContain("- `entry` - Generated line under `Entries` that points to a sibling markdown file, direct child `entrypoint`, or native skill package entrypoint.");
    expect(loader).toContain("- `framework route` - Core route installed and managed by Open Forge.");
    expect(loader).toContain("- `scope route` - Local route used to narrow meaning or ownership for routes below it.");
    expect(loader).toContain("- `scoped framework route` - `framework route` initialized inside a `scope route`.");
    expect(loader).toContain("- `slug` - Concrete folder name used in a route path.");
    expect(loader).toContain("- `axiom` - Mandatory instruction in a loaded Open Forge file.");
    expect(loader).toContain("- Open Forge routes agents through small markdown `entrypoints`.");
    expect(loader).toContain("- Follow all loaded `axioms` unless a higher-priority user, platform, safety, or external source-of-truth instruction conflicts; report unresolved conflicts.");
    expect(loader).toContain("- A folder is routable only when it contains one recognized `entrypoint`.");
    expect(loader).toContain("- `Entries` list sibling markdown files, direct child `entrypoints`, and supported native skill packages inside skills routes.");
    expect(loader).toContain("- To route into nested folders, every folder in the path needs its own `entrypoint`.");
    expect(loader).toContain("- `scope routes` use the same mechanism: add `slug` folders with `entrypoints` before, after, or between `framework routes` when they make ownership clearer.");
    expect(loader).toContain("- `scoped framework routes` work only when their framework `entrypoint` exists inside the scope.");
    expect(loader).toContain("- `.agents/memory/[scope]/crystallized/documents/_documents.md` - `scoped framework route` under a scope that owns memory states.");
    expect(loader).toContain("- In every loaded `entrypoint`, read `Entries` and load entries that fit the request or carry a defined load-policy tag.");
    expect(loader).toContain("- Before non-trivial work, default to one clearly matching workflow.");
    expect(loader).toContain("- Honor an explicit user request to use no workflow, proceed directly, or equivalent language without asking again.");
    expect(loader).toContain("## CLI");
    expect(loader).toContain("`open-forge chain <route> --heading <title>`");
    expect(loader).toContain("- Immediately read #LoadNow and #KeepInMind entries when they appear in loaded `Entries`, in listed order.");
    expect(loader).toContain("- Before ending meaningful work, recheck the #KeepInMind entries actually loaded and perform their follow-ups; `open-forge find --tag KeepInMind --bodies` discovers workspace-wide candidates but is not a receipt of the active loaded chain.");
    expect(loader).toContain("- `.agents/directives/_directives.md` - Mandatory instructions agents must follow when they apply to the current work - #LoadNow #Core #Directive");
    expect(loader).toContain("- `.agents/memory/_memory.md` - Self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning - #LoadNow #Memory #OrganicGrowth");
    expect(loader).toContain("## Tags");
    expect(loader).toContain("### Axioms");
    expect(loader).toContain("- Defined tags have framework meaning when they appear in loaded content or generated `Entries`.");
    expect(loader).toContain("- Undefined tags are routing and search signals; read the `entry` path, description, and loaded `entrypoint` for their meaning.");
    expect(loader).toContain("- `Entries` without a load-policy tag are on-demand routes selected by the current request.");
    expect(loader).toContain("- Tag spelling and casing are stable.");
    expect(loader).toContain("- Workspace-wide tag behavior belongs here and must stay short.");
    expect(loader).toContain("### Defined Tags");
    expect(loader).toContain("- #LoadNow - Read this `entry` when it appears in loaded `Entries`, in listed order. If the target is a category `entrypoint`, read that file first; its own `Entries` then apply the same rule.");
    expect(loader).toContain("- #KeepInMind - Read this `entry` like #LoadNow, keep its instructions active while working, and recheck it before ending meaningful work to perform the follow-ups it requires.");
    expect(loader).toContain("- #Core - Base routing, workspace orientation, and agent primitive routes.");
    expect(loader).toContain("- #Memory - Self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning.");
    expect(loader).toContain("- #Extension - Optional extension payload, template, integration, and support routes.");
    expect(loader).toContain("- #Contextual - Supporting context, not accepted current truth unless restored, validated, accepted, or promoted.");
    expect(loader).toContain("- #CurrentTruth - Accepted current memory within its stated scope; still below user instructions, runtime safety, platform constraints, and declared external sources of truth.");
    expect(workingMemory).toContain("Working memory is live context for active or recently interrupted work.");
    expect(workingMemory).toContain("tags: [LoadNow, Memory, Working, Contextual]");
    expect(workingMemory).toContain("- Keep working memory small, current, and easy to replace.");
    expect(workingMemory).not.toContain("Use `handoffs/`");
    expect(workingMemory).toContain("- `handoffs/_handoffs.md` - Static, concise, accurate transfer notes that help agents or humans resume work after context breaks - #LoadNow #Memory #Handoff #AgentCommunication #Contextual");
    expect(workingMemory).toContain("- `sessions/_sessions.md` - Raw chronological records of what happened during work sessions - #LoadNow #Memory #Session #WorkHistory #Contextual");
    expect(handoffsMemory).toContain("Handoffs are static, concise, accurate, rereadable transfer notes for resuming work across agents, subagents, threads, workflows, or humans.");
    expect(handoffsMemory).toContain("tags: [LoadNow, Memory, Handoff, AgentCommunication, Contextual]");
    expect(handoffsMemory).toContain("- Create or update a handoff when work is transferred, delegated, interrupted, or handed to another agent or human.");
    expect(sessionsMemory).toContain("Sessions are raw chronological memory records of work while it happens.");
    expect(sessionsMemory).toContain("tags: [LoadNow, Memory, Session, WorkHistory, Contextual]");
    expect(sessionsMemory).toContain("- If useful work context does not clearly belong elsewhere yet, write it as session context first and reclassify it later.");
    expect(emergingMemory).toContain("Emerging memory is candidate material that may be useful but is not accepted truth yet.");
    expect(emergingMemory).toContain("tags: [KeepInMind, Memory, Emerging, OrganicGrowth, Contextual, Candidate]");
    expect(emergingMemory).toContain("- Read `Entries` when current work needs useful material that is not accepted truth or produces candidate material.");
    expect(emergingMemory).toContain("- Before ending meaningful work, read `Entries` to route candidate material produced during the work.");
    expect(emergingMemory).toContain("- Keep uncertainty, source, and scope visible.");
    expect(emergingMemory).not.toContain("Use `analysis/`");
    expect(emergingMemory).toContain("- Promote accepted memory to a crystallized #Memory route; archive stale, rejected, or superseded material.");
    expect(emergingMemory).toContain("- `analysis/_analysis.md` - Structured reasoning, investigation, or comparison that is useful but not accepted truth - #LoadNow #Memory #Analysis #Reasoning #Contextual #Candidate");
    expect(emergingMemory).toContain("- `ideas/_ideas.md` - Future possibilities, experiments, open questions, and options to explore later - #LoadNow #Memory #Idea #Exploration #OrganicGrowth #Contextual #Candidate");
    expect(emergingMemory).toContain("- `observations/_observations.md` - Agent-noticed findings that may become learning, memory, or Core updates - #KeepInMind #Memory #Observation #AgentLearning #OrganicGrowth #Contextual #Candidate");
    expect(analysisMemory).toContain("Analysis is structured reasoning, investigation, or comparison that is useful but not accepted truth.");
    expect(analysisMemory).toContain("tags: [LoadNow, Memory, Analysis, Reasoning, Contextual, Candidate]");
    expect(ideasMemory).toContain("Ideas are future possibilities, experiments, open questions, and options to explore later.");
    expect(ideasMemory).toContain("tags: [LoadNow, Memory, Idea, Exploration, OrganicGrowth, Contextual, Candidate]");
    expect(observationsMemory).toContain("Observations are agent-noticed grounded findings - facts, signals, constraints, recurring behavior, risks, and evidence - that help future agents learn from work.");
    expect(observationsMemory).toContain("tags: [KeepInMind, Memory, Observation, AgentLearning, OrganicGrowth, Contextual, Candidate]");
    expect(observationsMemory).toContain("- Before ending meaningful work, write observations for grounded findings that may matter later; write them before the final response or report the blocker.");
    expect(observationsMemory).toContain("- Verify observations before treating them as current.");
    expect(crystallizedMemory).toContain("Crystallized memory is accepted durable memory and current truth within its stated scope.");
    expect(crystallizedMemory).toContain("tags: [LoadNow, Memory, Crystallized, CurrentTruth]");
    expect(crystallizedMemory).toContain("- Update, split, merge, or reshape existing crystallized memory instead of creating parallel current truth.");
    expect(crystallizedMemory).not.toContain("Use `documents/`");
    expect(crystallizedMemory).not.toContain("scoped decision route");
    expect(crystallizedMemory).toContain("- Archive or link superseded crystallized material with enough context to understand the change.");
    expect(crystallizedMemory).toContain("- `decisions/_decisions.md` - Accepted rationale that explains important choices and their consequences - #LoadNow #Memory #Decision #Rationale #CurrentTruth");
    expect(crystallizedMemory).toContain("- `documents/_documents.md` - Durable accepted records, or routes to those records, for long-form project knowledge - #LoadNow #Memory #Document #Record #CurrentTruth");
    expect(decisionsMemory).toContain("Decisions are accepted rationale for important choices that may need to be understood later.");
    expect(decisionsMemory).toContain("tags: [LoadNow, Memory, Decision, Rationale, CurrentTruth]");
    expect(decisionsMemory).toContain("Decisions explain why a choice was made; the chosen behavior, record, route, or external state belongs to its owning route or system.");
    expect(decisionsMemory).toContain("- Keep alternatives, tradeoffs, constraints, and consequences only when they help future work.");
    expect(documentsMemory).toContain("Documents are durable accepted records, or routes to those records, for long-form project knowledge.");
    expect(documentsMemory).toContain("tags: [LoadNow, Memory, Document, Record, CurrentTruth]");
    expect(archivedMemory).toContain("Archived memory is historical context kept after it is no longer current truth.");
    expect(archivedMemory).toContain("tags: [LoadNow, Memory, Archived, Contextual, Historical]");
    expect(archivedMemory).toContain("- Preserve origin, the reason material was archived, and what replaced it when a replacement exists.");
    expect(patterns).toContain("Treat an applicable pattern as the established default shape for its scope; use a different shape only for a deliberate reason.");
    expect(patterns).toContain("<!-- open-forge:generated-index:start -->");
    expect(skills).toContain("Prefer the native skill shape: `.agents/skills/{skill-name}/SKILL.md`.");
    expect(skills).toContain("<!-- open-forge:generated-index:start -->");
    expect(workflows).toContain("- Before non-trivial work, read `Entries` and load matching workflows.");
    expect(workflows).toContain("- Every workflow recipe defines `Mode`, `Goal`, `Required Routes`, `Constraints`, `Steps`, `Loop`, `Outputs`, and `Completion`, in that order.");
    expect(workflows).toContain("- A child `entrypoint` used only to organize descendant workflows may contain category Axioms and Entries without recipe headings.");
    expect(workflows).toContain("- `Mode` is `linear` or `iterative`.");
    expect(workflows).toContain("- `Constraints` always exists and states `- none` when no workflow-specific invariant applies.");
    expect(workflows).toContain("- Generated `Entries` list what a workflow contains; `Required Routes` list what it needs from elsewhere.");
    expect(workflows).toContain("- Read every `Required Routes` route before Step 1; a route that cannot be read is a blocker to report, not a step to skip. \"none\" is a valid value.");
    expect(workflows).toContain("<!-- open-forge:generated-index:start -->");
    expect(workspace).toContain("## Axioms");
    expect(workspace).toContain("<!-- open-forge:generated-index:start -->");
    expect(loader).not.toContain("Load `directives` for every request when it appears in `Entries`.");
    expect(loader).not.toContain("Load `memory` after `directives` when it appears in `Entries`.");
    expect(loader).toContain("- `.agents/guidance/_guidance.md` - Contextual advice for recurring choices, tradeoffs, and work scenarios - #LoadNow #Core #Guidance");
    expect(loader).toContain("- `.agents/patterns/_patterns.md` - Concrete reusable shapes for code, files, APIs, documents, and other inspectable work - #LoadNow #Core #Pattern");
    expect(loader).toContain("- `.agents/skills/_skills.md` - Reusable agent capability packages with clear use cases and expected results - #LoadNow #Core #Skill");
    expect(loader).toContain("- `.agents/workflows/_workflows.md` - Repeatable markdown workflow recipes for reaching a defined goal - #LoadNow #Core #Workflow");
    expect(loader).toContain("- `.agents/workspace/_workspace.md` - Workspace routes that point to important project locations and explain when to use them - #LoadNow #Core #Workspace");
    expect(loader).not.toContain("## Route Categories");
    expect(await exists(path.join(root, ".agents", "constants.md"))).toBe(false);
    expect(await exists(path.join(root, ".agents", "workspace", "_workspace-open-forge.md"))).toBe(false);
  });

  test("updates scoped framework route entrypoints by path shape", async () => {
    const root = await createRoot();
    const scopedDocuments = path.join(root, ".agents", "memory", "customer-facing", "mobile-app", "crystallized", "platform", "documents");
    const scopedDecisions = path.join(root, ".agents", "memory", "mobile-app", "crystallized", "decisions");
    await fs.mkdir(scopedDocuments, { recursive: true });
    await fs.mkdir(scopedDecisions, { recursive: true });
    await fs.writeFile(path.join(scopedDocuments, "_documents.md"), `# Old Scoped Documents

Old scoped framework copy.

## Entries

<!-- open-forge:generated-index:start -->
- old.md - Old route - #Old
<!-- open-forge:generated-index:end -->
`);
    await fs.writeFile(path.join(scopedDecisions, "_decisions.md"), `# Old Scoped Decisions

Old scoped decisions framework copy.
`);
    await writeRoute(scopedDocuments, "architecture.md", "Scoped architecture truth", ["Memory", "Document", "CurrentTruth"]);

    const result = await runCli("install", root);
    const scopedContent = await fs.readFile(path.join(scopedDocuments, "_documents.md"), "utf8");
    const scopedDecisionContent = await fs.readFile(path.join(scopedDecisions, "_decisions.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(scopedContent).toContain("Documents are durable accepted records, or routes to those records, for long-form project knowledge.");
    expect(scopedContent).not.toContain("Old scoped framework copy.");
    expect(scopedContent).toContain("- `architecture.md` - Scoped architecture truth - #Memory #Document #CurrentTruth");
    expect(scopedDecisionContent).toContain("Decisions are accepted rationale for important choices");
    expect(scopedDecisionContent).not.toContain("Old scoped decisions framework copy.");
  });

  test("does not treat local scope routes as scoped framework routes", async () => {
    const root = await createRoot();
    const reactPatterns = path.join(root, ".agents", "patterns", "mobile-app", "react");
    await fs.mkdir(reactPatterns, { recursive: true });
    await fs.writeFile(path.join(reactPatterns, "_react.md"), `# React

Custom React route.
`);

    const result = await runCli("install", root);
    const reactContent = await fs.readFile(path.join(reactPatterns, "_react.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(reactContent).toContain("Custom React route.");
    expect(reactContent).not.toContain("Concrete reusable shapes");
    expect(reactContent).toContain("- none - No entries - #Empty");
  });

  test("installs a local extension overlay and rebuilds indexes", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    const extensionPatterns = path.join(extension, ".agents", "patterns", "react");
    await fs.mkdir(extensionPatterns, { recursive: true });
    await fs.writeFile(path.join(extensionPatterns, "_react.md"), `---
open-forge:
  description: React component patterns for this workspace
  tags: [Extension, Core, Pattern, React]
---

# React

React patterns for this workspace.
`);
    await writeRoute(extensionPatterns, "components.md", "Reusable React component shape", ["Pattern", "React"]);

    expect((await runCli("install", root)).exitCode).toBe(0);
    const result = await runCli("extend", extension, root);
    const patterns = await fs.readFile(path.join(root, ".agents", "patterns", "_patterns.md"), "utf8");
    const react = await fs.readFile(path.join(root, ".agents", "patterns", "react", "_react.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(patterns).toContain("- `react/_react.md` - React component patterns for this workspace - #Extension #Core #Pattern #React");
    expect(react).toContain("- `components.md` - Reusable React component shape - #Pattern #React");
  });

  test("installs only payload from a local extension package", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    const extensionPatterns = path.join(extension, "payload", ".agents", "patterns", "react");
    await fs.mkdir(extensionPatterns, { recursive: true });
    await fs.writeFile(path.join(extension, "extension.json"), `${JSON.stringify({ name: "React Pack", description: "React extension" }, null, 2)}\n`);
    await fs.writeFile(path.join(extensionPatterns, "_react.md"), `---
open-forge:
  description: React component patterns for this workspace
  tags: [Extension, Core, Pattern, React]
---

# React
`);

    expect((await runCli("install", root)).exitCode).toBe(0);
    const result = await runCli("extend", extension, root);

    expect(result.exitCode).toBe(0);
    expect(await exists(path.join(root, "extension.json"))).toBe(false);
    expect(await exists(path.join(root, ".agents", "patterns", "react", "_react.md"))).toBe(true);
  });

  test("installs an empty package without requiring a pre-existing target", async () => {
    const parent = await createRoot();
    const root = path.join(parent, "target");
    const extension = await createRoot();
    await fs.mkdir(path.join(extension, "payload", ".agents"), { recursive: true });
    await fs.writeFile(path.join(extension, "extension.json"), JSON.stringify({ name: "Empty Pack" }));

    const result = await runCli("extend", extension, root);

    expect(result.exitCode).toBe(0);
    expect(await exists(root)).toBe(true);
    expect(result.stdout).toContain("Created 0, updated 0, left 0 extension files unchanged");
  });

  test("installs a bundled first-party extension by id", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const extensionPatterns = path.join(extensionsRoot, "review-pack", "payload", ".agents", "patterns", "reviews");
    await fs.mkdir(extensionPatterns, { recursive: true });
    await fs.writeFile(path.join(extensionPatterns, "_reviews.md"), `---
open-forge:
  description: Review patterns bundled with Open Forge
  tags: [Extension, Core, Pattern, Review]
---

# Reviews

Review patterns bundled with Open Forge.
`);
    await writeRoute(extensionPatterns, "pull-requests.md", "Pull request review shape", ["Pattern", "Review"]);

    expect((await runCli("install", root)).exitCode).toBe(0);
    const result = await runCliWithEnv("extend", ["review-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });
    const patterns = await fs.readFile(path.join(root, ".agents", "patterns", "_patterns.md"), "utf8");
    const reviews = await fs.readFile(path.join(root, ".agents", "patterns", "reviews", "_reviews.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(patterns).toContain("- `reviews/_reviews.md` - Review patterns bundled with Open Forge - #Extension #Core #Pattern #Review");
    expect(reviews).toContain("- `pull-requests.md` - Pull request review shape - #Pattern #Review");
  });

  test("lists bundled first-party extensions with descriptions", async () => {
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "alpha-pack", "Alpha Pack", "Alpha extension");
    await createBundledExtension(extensionsRoot, "beta-pack", "Beta Pack", "Beta extension");

    const result = await runCliWithEnv("extend", ["--list"], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("- alpha-pack - Alpha extension");
    expect(result.stdout).toContain("- beta-pack - Beta extension");
    expect(result.stdout).toContain("- alpha-pack - Alpha extension (contents: pattern)");
    expect(result.stdout).toContain("- beta-pack - Beta extension (contents: pattern)");
  });

  test("derives every advertised content kind from payload paths", async () => {
    const extensionsRoot = await createRoot();
    const patterns = await createBundledExtension(extensionsRoot, "mixed-pack", "Mixed Pack", "Mixed extension");
    const payload = path.resolve(patterns, "..", "..", "..");
    const agents = path.join(payload, ".agents");
    const files = [
      path.join(agents, "skills", "mixed", "SKILL.md"),
      path.join(agents, "workflows", "mixed", "_mixed.md"),
      path.join(agents, "directives", "mixed.md"),
      path.join(agents, "guidance", "mixed.md"),
      path.join(agents, "workspace", "mixed.md"),
      path.join(agents, "memory", "mixed.md"),
      path.join(payload, "templates", "mixed.md")
    ];
    for (const file of files) {
      await fs.mkdir(path.dirname(file), { recursive: true });
      await fs.writeFile(file, "content\n");
    }

    const result = await runCliWithEnv("extend", ["--list"], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("(contents: skill, workflow, directive, guidance, pattern, workspace, memory, other)");
  });

  test("installs multiple bundled extensions by id list", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const alphaPatterns = await createBundledExtension(extensionsRoot, "alpha-pack", "Alpha Pack", "Alpha extension");
    const betaPatterns = await createBundledExtension(extensionsRoot, "beta-pack", "Beta Pack", "Beta extension");
    await writeRoute(alphaPatterns, "alpha.md", "Alpha pattern", ["Pattern", "Alpha"]);
    await writeRoute(betaPatterns, "beta.md", "Beta pattern", ["Pattern", "Beta"]);

    expect((await runCli("install", root)).exitCode).toBe(0);
    const result = await runCliWithEnv("extend", ["--ids", "alpha-pack,beta-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });
    const patterns = await fs.readFile(path.join(root, ".agents", "patterns", "_patterns.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(patterns).toContain("- `alpha-pack/_alpha-pack.md` - Alpha extension - #Extension #Core #Pattern");
    expect(patterns).toContain("- `beta-pack/_beta-pack.md` - Beta extension - #Extension #Core #Pattern");
  });

  test("installs bundled dependencies before the requested extension", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const sharedPatterns = await createBundledExtension(extensionsRoot, "shared-pack", "Shared Pack", "Shared extension");
    const featurePatterns = await createBundledExtension(extensionsRoot, "feature-pack", "Feature Pack", "Feature extension", ["shared-pack"]);
    await writeRoute(sharedPatterns, "shared.md", "Shared pattern", ["Pattern", "Shared"]);
    await writeRoute(featurePatterns, "feature.md", "Feature pattern", ["Pattern", "Feature"]);

    expect((await runCli("install", root)).exitCode).toBe(0);
    const result = await runCliWithEnv("extend", ["feature-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("bundled:shared-pack, bundled:feature-pack");
    expect(await exists(path.join(root, ".agents", "patterns", "shared-pack", "shared.md"))).toBe(true);
    expect(await exists(path.join(root, ".agents", "patterns", "feature-pack", "feature.md"))).toBe(true);
  });

  test("rejects a missing dependency before writing any files", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "feature-pack", "Feature Pack", "Feature extension", ["missing-pack"]);
    const sentinel = path.join(root, "sentinel.txt");
    await fs.writeFile(sentinel, "keep\n");

    const result = await runCliWithEnv("extend", ["feature-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("missing-pack (required by feature-pack)");
    expect(await fs.readdir(root)).toEqual(["sentinel.txt"]);
    expect(await fs.readFile(sentinel, "utf8")).toBe("keep\n");
  });

  test("rejects dependency cycles before writing any files", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "alpha-pack", "Alpha Pack", "Alpha extension", ["beta-pack"]);
    await createBundledExtension(extensionsRoot, "beta-pack", "Beta Pack", "Beta extension", ["alpha-pack"]);

    const result = await runCliWithEnv("extend", ["alpha-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("Extension dependency cycle: alpha-pack -> beta-pack -> alpha-pack");
    expect(await fs.readdir(root)).toEqual([]);
  });

  test("rejects conflicting extension files before writing any files", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "alpha-pack", "Alpha Pack", "Alpha extension");
    await createBundledExtension(extensionsRoot, "beta-pack", "Beta Pack", "Beta extension");
    const alphaShared = path.join(extensionsRoot, "alpha-pack", "payload", ".agents", "shared.md");
    const betaShared = path.join(extensionsRoot, "beta-pack", "payload", ".agents", "shared.md");
    await fs.writeFile(alphaShared, "alpha\n");
    await fs.writeFile(betaShared, "beta\n");

    const result = await runCliWithEnv("extend", ["--ids", "alpha-pack,beta-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("Extension file collision at .agents/shared.md");
    expect(await fs.readdir(root)).toEqual([]);
  });

  test("rejects case-only extension collisions for portable compositions", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const alphaPayload = path.join(extensionsRoot, "alpha-pack", "payload", ".agents");
    const betaPayload = path.join(extensionsRoot, "beta-pack", "payload", ".agents");
    await fs.mkdir(alphaPayload, { recursive: true });
    await fs.mkdir(betaPayload, { recursive: true });
    await fs.writeFile(path.join(extensionsRoot, "alpha-pack", "extension.json"), JSON.stringify({ name: "Alpha" }));
    await fs.writeFile(path.join(extensionsRoot, "beta-pack", "extension.json"), JSON.stringify({ name: "Beta" }));
    await fs.writeFile(path.join(alphaPayload, "Route.md"), "alpha\n");
    await fs.writeFile(path.join(betaPayload, "route.md"), "beta\n");

    const result = await runCliWithEnv("extend", ["--ids", "alpha-pack,beta-pack", root, "--dry-run"], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("Extension file collision at .agents/route.md");
    expect(await fs.readdir(root)).toEqual([]);
  });

  test("rejects a planned file that is also another planned file's parent", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const alphaPayload = path.join(extensionsRoot, "alpha-pack", "payload");
    const betaAgents = path.join(extensionsRoot, "beta-pack", "payload", ".agents");
    await fs.mkdir(alphaPayload, { recursive: true });
    await fs.mkdir(betaAgents, { recursive: true });
    await fs.writeFile(path.join(extensionsRoot, "alpha-pack", "extension.json"), JSON.stringify({ name: "Alpha" }));
    await fs.writeFile(path.join(extensionsRoot, "beta-pack", "extension.json"), JSON.stringify({ name: "Beta" }));
    await fs.writeFile(path.join(alphaPayload, ".agents"), "parent file\n");
    await fs.writeFile(path.join(betaAgents, "route.md"), "child file\n");

    const result = await runCliWithEnv("extend", ["--ids", "alpha-pack,beta-pack", root, "--dry-run"], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("planned file .agents is also a parent of .agents/route.md");
    expect(await fs.readdir(root)).toEqual([]);
  });

  test("deduplicates byte-identical files shared by extensions", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "alpha-pack", "Alpha Pack", "Alpha extension");
    await createBundledExtension(extensionsRoot, "beta-pack", "Beta Pack", "Beta extension");
    const alphaShared = path.join(extensionsRoot, "alpha-pack", "payload", ".agents", "shared.md");
    const betaShared = path.join(extensionsRoot, "beta-pack", "payload", ".agents", "shared.md");
    await fs.writeFile(alphaShared, "same\n");
    await fs.writeFile(betaShared, "same\n");

    const result = await runCliWithEnv("extend", ["--ids", "alpha-pack,beta-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(0);
    expect(await fs.readFile(path.join(root, ".agents", "shared.md"), "utf8")).toBe("same\n");
    expect(result.stdout).toContain("Created 3");
  });

  test("previews the resolved plan without writing the target", async () => {
    const parent = await createRoot();
    const root = path.join(parent, "not-created");
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "shared-pack", "Shared Pack", "Shared extension");
    await createBundledExtension(extensionsRoot, "feature-pack", "Feature Pack", "Feature extension", ["shared-pack"]);

    const result = await runCliWithEnv("extend", ["feature-pack", root, "--dry-run"], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("Resolved in dependency order: bundled:shared-pack, bundled:feature-pack");
    expect(result.stdout).toContain("No files were written");
    expect(result.stdout).toContain("Planned files:");
    expect(result.stdout).toContain("- create .agents/patterns/feature-pack/_feature-pack.md");
    expect(await exists(root)).toBe(false);
  });

  test("lists bundled dependencies", async () => {
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "feature-pack", "Feature Pack", "Feature extension", ["shared-pack"]);

    const result = await runCliWithEnv("extend", ["--list"], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("- feature-pack - Feature extension (requires: shared-pack) (contents: pattern)");
  });

  test("lists and installs dependency-only convenience packs without writing package files", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "shared-pack", "Shared Pack", "Shared extension");
    await createBundledExtension(extensionsRoot, "feature-pack", "Feature Pack", "Feature extension");
    await createDependencyPack(extensionsRoot, "workflow-suite", "Workflow Suite", "Convenience selection", ["shared-pack", "feature-pack"]);

    const listed = await runCliWithEnv("extend", ["--list"], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });
    const installed = await runCliWithEnv("extend", ["workflow-suite", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(listed.exitCode).toBe(0);
    expect(listed.stdout).toContain("- workflow-suite - Convenience selection (requires: shared-pack, feature-pack) (contents: pack)");
    expect(installed.exitCode).toBe(0);
    expect(installed.stdout).toContain("bundled:shared-pack, bundled:feature-pack, bundled:workflow-suite");
    expect(await exists(path.join(root, "extension.json"))).toBe(false);
    expect(await exists(path.join(root, "README.md"))).toBe(false);
    expect(await exists(path.join(root, ".agents", "patterns", "shared-pack", "_shared-pack.md"))).toBe(true);
    expect(await exists(path.join(root, ".agents", "patterns", "feature-pack", "_feature-pack.md"))).toBe(true);
  });

  test("preserves dependency-only pack semantics when installing a copied local pack", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const copiedPack = await createRoot();
    await createBundledExtension(extensionsRoot, "shared-pack", "Shared Pack", "Shared extension");
    await fs.writeFile(path.join(copiedPack, "extension.json"), `${JSON.stringify({
      name: "Copied Workflow Suite",
      description: "A locally copied dependency-only pack",
      dependencies: ["shared-pack"]
    }, null, 2)}\n`);
    await fs.writeFile(path.join(copiedPack, "README.md"), "Pack authoring documentation that must not replace the project README.\n");

    expect((await runCli("install", root)).exitCode).toBe(0);
    await fs.writeFile(path.join(root, "README.md"), "Project README sentinel.\n");
    const result = await runCliWithEnv("extend", [copiedPack, root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("bundled:shared-pack");
    expect(result.stdout).toContain(`local:${copiedPack}`);
    expect(await fs.readFile(path.join(root, "README.md"), "utf8")).toBe("Project README sentinel.\n");
    expect(await exists(path.join(root, ".agents", "patterns", "shared-pack", "_shared-pack.md"))).toBe(true);
  });

  test("does not catalogue or install a manifest-only no-op extension", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createDependencyPack(extensionsRoot, "empty-pack", "Empty Pack", "Does nothing", []);
    await fs.mkdir(path.join(extensionsRoot, "empty-pack", "payload", ".agents"), { recursive: true });

    const listed = await runCliWithEnv("extend", ["--list"], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });
    const installed = await runCliWithEnv("extend", ["empty-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(listed.exitCode).toBe(0);
    expect(listed.stdout).toContain("No bundled Open Forge extensions");
    expect(listed.stdout).not.toContain("empty-pack");
    expect(installed.exitCode).toBe(1);
    expect(installed.stderr).toContain("empty-pack has neither payload files nor dependencies");
    expect(await fs.readdir(root)).toEqual([]);
  });

  test("rejects malformed extension manifests before writing", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "feature-pack", "Feature Pack", "Feature extension");
    await fs.writeFile(path.join(extensionsRoot, "feature-pack", "extension.json"), JSON.stringify({ name: "Feature", dependencies: "shared-pack" }));

    const result = await runCliWithEnv("extend", ["feature-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("dependencies must be an array");
    expect(await fs.readdir(root)).toEqual([]);
  });

  test("rejects null manifest fields instead of treating them as absent", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "feature-pack", "Feature Pack", "Feature extension");
    await fs.writeFile(path.join(extensionsRoot, "feature-pack", "extension.json"), JSON.stringify({ name: null, dependencies: null }));

    const result = await runCliWithEnv("extend", ["feature-pack", root, "--dry-run"], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("name must be a non-empty string");
    expect(await fs.readdir(root)).toEqual([]);
  });

  test("rejects unknown manifest fields before dependency resolution", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "sentinel.txt"), "must not install\n");
    await fs.writeFile(path.join(extension, "extension.json"), JSON.stringify({
      name: "Misspelled Dependencies",
      dependancies: ["shared-pack"]
    }));

    const result = await runCli("extend", extension, root, "--dry-run");

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("unknown field dependancies");
    expect(await fs.readdir(root)).toEqual([]);
  });

  test("rolls back extension payload files when index validation fails", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    const badRoute = path.join(extension, "payload", ".agents", "patterns", "bad");
    await fs.mkdir(badRoute, { recursive: true });
    await fs.writeFile(path.join(extension, "extension.json"), JSON.stringify({ name: "Bad Pack" }));
    await fs.writeFile(path.join(badRoute, "_bad.md"), `---
open-forge:
  description: Invalid generated region for rollback coverage
  tags: [Extension, Pattern]
---

# Bad

## Entries

<!-- open-forge:generated-index:start -->
- incomplete.md - Incomplete - #Bad
`);

    expect((await runCli("install", root)).exitCode).toBe(0);
    const patternsBefore = await fs.readFile(path.join(root, ".agents", "patterns", "_patterns.md"), "utf8");
    const preview = await runCli("extend", extension, root, "--dry-run");
    const result = await runCli("extend", extension, root);

    expect(preview.exitCode).toBe(1);
    expect(preview.stderr).toContain("markers are incomplete");
    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("markers are incomplete");
    expect(await exists(path.join(root, ".agents", "patterns", "bad"))).toBe(false);
    expect(await fs.readFile(path.join(root, ".agents", "patterns", "_patterns.md"), "utf8")).toBe(patternsBefore);
  });

  test("rejects installing an extension into its own source tree", async () => {
    const extension = await createRoot();
    const payload = path.join(extension, "payload", ".agents");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "route.md"), "route\n");
    const target = path.join(extension, "nested-target");

    const result = await runCli("extend", extension, target);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("must not be the extension source or a directory inside it");
    expect(await exists(target)).toBe(false);
  });

  test("rejects a non-existent target projected through a link into its extension source", async () => {
    const parent = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload", ".agents");
    const linkedExtension = path.join(parent, "linked-extension");
    const target = path.join(linkedExtension, "nested-target");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "route.md"), "must not write back into the extension\n");
    await fs.symlink(extension, linkedExtension, process.platform === "win32" ? "junction" : "dir");

    const preview = await runCli("extend", extension, target, "--dry-run");
    const result = await runCli("extend", extension, target);

    expect(preview.exitCode).toBe(1);
    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("must not be the extension source or a directory inside it");
    expect(await exists(path.join(extension, "nested-target"))).toBe(false);
  });

  test("rejects a target symlink or junction that redirects extension writes", async () => {
    const root = await createRoot();
    const outside = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload", ".agents");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "escaped.md"), "must stay contained\n");
    await fs.symlink(outside, path.join(root, ".agents"), process.platform === "win32" ? "junction" : "dir");

    const result = await runCli("extend", extension, root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("contains a symbolic link or junction");
    expect(await exists(path.join(outside, "escaped.md"))).toBe(false);
  });

  test("rejects a target root that is itself a symlink or junction", async () => {
    const parent = await createRoot();
    const outside = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload", ".agents");
    const linkedTarget = path.join(parent, "linked-target");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "escaped.md"), "must stay contained\n");
    await fs.symlink(outside, linkedTarget, process.platform === "win32" ? "junction" : "dir");

    const result = await runCli("extend", extension, linkedTarget);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("target root is a symbolic link or junction");
    expect(await exists(path.join(outside, ".agents", "escaped.md"))).toBe(false);
  });

  test("rejects symlinks or junctions inside an extension source", async () => {
    const root = await createRoot();
    const outside = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(outside, "escaped.md"), "must not be imported\n");
    await fs.symlink(outside, path.join(payload, "linked-source"), process.platform === "win32" ? "junction" : "dir");

    const result = await runCli("extend", extension, root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("source contains a symbolic link or junction");
    expect(await exists(path.join(root, "linked-source", "escaped.md"))).toBe(false);
  });

  test("rejects a local extension source root that is itself a symlink or junction", async () => {
    const root = await createRoot();
    const parent = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload", ".agents");
    const linkedSource = path.join(parent, "linked-extension");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "route.md"), "must not be imported through an alias\n");
    await fs.symlink(extension, linkedSource, process.platform === "win32" ? "junction" : "dir");

    const result = await runCli("extend", linkedSource, root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("source root is a symbolic link or junction");
    expect(await exists(path.join(root, ".agents", "route.md"))).toBe(false);
  });

  test("rejects a hard-linked planned target before preview or installation", async () => {
    const root = await createRoot();
    const outside = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload");
    const outsideFile = path.join(outside, "shared-inode.txt");
    const targetFile = path.join(root, "artifact.txt");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "artifact.txt"), "replacement\n");
    await fs.writeFile(outsideFile, "external sentinel\n");
    await fs.link(outsideFile, targetFile);

    const preview = await runCli("extend", extension, root, "--dry-run");
    const result = await runCli("extend", extension, root);

    expect(preview.exitCode).toBe(1);
    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("multiple hard links");
    expect(await fs.readFile(outsideFile, "utf8")).toBe("external sentinel\n");
    expect(await fs.readFile(targetFile, "utf8")).toBe("external sentinel\n");
  });

  test("rejects hard-linked index files before an empty extension can regenerate them", async () => {
    const root = await createRoot();
    const outside = await createRoot();
    const extension = await createRoot();
    await fs.mkdir(path.join(extension, "payload"), { recursive: true });
    expect((await runCli("install", root)).exitCode).toBe(0);
    const indexFile = path.join(root, ".agents", "workflows", "_workflows.md");
    const outsideLink = path.join(outside, "linked-workflows-index.md");
    await fs.link(indexFile, outsideLink);
    const before = await fs.readFile(indexFile, "utf8");

    const result = await runCli("extend", extension, root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("Extension target index file has multiple hard links");
    expect(await fs.readFile(indexFile, "utf8")).toBe(before);
    expect(await fs.readFile(outsideLink, "utf8")).toBe(before);
  });

  test("rejects a linked .agents index root before writing an outside-only payload", async () => {
    const root = await createRoot();
    const outside = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload");
    expect((await runCli("install", outside)).exitCode).toBe(0);
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "support.txt"), "must not install\n");
    await fs.symlink(path.join(outside, ".agents"), path.join(root, ".agents"), process.platform === "win32" ? "junction" : "dir");

    const result = await runCli("extend", extension, root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("target index root contains a symbolic link or junction");
    expect(await exists(path.join(root, "support.txt"))).toBe(false);
  });

  test("rejects a linked .agents index root even when the extension plan is empty", async () => {
    const root = await createRoot();
    const outside = await createRoot();
    const extension = await createRoot();
    expect((await runCli("install", outside)).exitCode).toBe(0);
    await fs.mkdir(path.join(extension, "payload"), { recursive: true });
    await fs.symlink(path.join(outside, ".agents"), path.join(root, ".agents"), process.platform === "win32" ? "junction" : "dir");

    const result = await runCli("extend", extension, root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("target index root contains a symbolic link or junction");
  });

  test("rejects portable path aliases already present in the target", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    const existingSkill = path.join(root, ".agents", "skills", "Quality", "SKILL.md");
    const payloadSkill = path.join(extension, "payload", ".agents", "skills", "quality", "SKILL.md");
    await fs.mkdir(path.dirname(existingSkill), { recursive: true });
    await fs.mkdir(path.dirname(payloadSkill), { recursive: true });
    await fs.writeFile(existingSkill, "existing skill\n");
    await fs.writeFile(payloadSkill, "replacement skill\n");

    const result = await runCli("extend", extension, root, "--dry-run");

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("aliases planned portable path");
    expect(await fs.readFile(existingSkill, "utf8")).toBe("existing skill\n");
  });

  test("classifies a Windows case-folded AGENTS.md plan as baseline-loading", async () => {
    if (process.platform !== "win32") {
      return;
    }

    const root = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "agents.md"), "new entrypoint\n");

    const result = await runCli("extend", extension, root, "--dry-run");

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("Scope review: 0 routed, 1 baseline-loading, 0 skill-executable, 0 outside-.agents files.");
    expect(result.stdout).toContain("- create agents.md");
    expect(await exists(path.join(root, "agents.md"))).toBe(false);
  });

  test("classifies only a skill package's direct scripts child as executable", async () => {
    const parent = await createRoot();
    const root = path.join(parent, "target");
    const extension = await createRoot();
    const referencesScripts = path.join(extension, "payload", ".agents", "skills", "quality", "references", "scripts");
    const skillScripts = path.join(extension, "payload", ".agents", "skills", "quality", "scripts");
    await fs.mkdir(referencesScripts, { recursive: true });
    await fs.mkdir(skillScripts, { recursive: true });
    await fs.writeFile(path.join(referencesScripts, "example.md"), "documentation\n");
    await fs.writeFile(path.join(skillScripts, "verify.mjs"), "export {};\n");

    const result = await runCli("extend", extension, root, "--dry-run");

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("Scope review: 1 routed, 0 baseline-loading, 1 skill-executable, 0 outside-.agents files.");
    expect(await exists(root)).toBe(false);
  });

  test("requires a TTY for interactive bundled extension selection", async () => {
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "alpha-pack", "Alpha Pack", "Alpha extension");

    const result = await runCliWithEnv("extend", [], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("Interactive extension selection requires a TTY");
  });
});

describe("Git-checkpointed install commands", () => {
  test("fails closed outside Git in noninteractive mode without mutating the target", async () => {
    const parent = await createRoot();
    const target = path.join(parent, "untracked-target");

    const result = await runCliDefault("install", target);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("requires a Git repository for reviewable diffs");
    expect(result.stderr).toContain("--pro");
    expect(await exists(target)).toBe(false);
  });

  test("uses Core and each extension transaction as real Git review checkpoints", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    await initializeGitRepository(root);
    const extensionFile = path.join(extension, "payload", ".agents", "patterns", "checkpoint.md");
    await fs.mkdir(path.dirname(extensionFile), { recursive: true });
    await fs.writeFile(extensionFile, "checkpoint pattern\n");

    const core = await runCliDefault("install", root);
    expect(core.exitCode).toBe(0);
    expect(core.stdout).toContain("review the resulting diff and commit it before the next install");
    expect(core.stdout).toContain("open-forge extend --list");

    const blocked = await runCliDefault("extend", extension, root);
    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("requires a clean Git checkpoint");
    expect(await exists(path.join(root, ".agents", "patterns", "checkpoint.md"))).toBe(false);

    await commitAll(root, "Install Core");
    const installed = await runCliDefault("extend", extension, root);
    expect(installed.exitCode).toBe(0);
    expect(installed.stdout).toContain("Extension transaction checkpoint: review the resulting diff and commit it before the next install");
    expect(await fs.readFile(path.join(root, ".agents", "patterns", "checkpoint.md"), "utf8")).toBe("checkpoint pattern\n");

    const nextBlocked = await runCliDefault("install", root);
    expect(nextBlocked.exitCode).toBe(1);
    expect(nextBlocked.stderr).toContain("requires a clean Git checkpoint");
  });

  test("scopes cleanliness to a nested target instead of unrelated monorepo files", async () => {
    const root = await createRoot();
    const target = path.join(root, "apps", "demo");
    await fs.mkdir(target, { recursive: true });
    await fs.writeFile(path.join(root, "outside.txt"), "baseline\n");
    await fs.writeFile(path.join(target, ".keep"), "baseline\n");
    await initializeGitRepository(root);
    await commitAll(root, "Baseline");
    await fs.writeFile(path.join(root, "outside.txt"), "unrelated dirty sibling\n");

    const result = await runCliDefault("install", target);

    expect(result.exitCode).toBe(0);
    expect(await exists(path.join(target, "AGENTS.md"))).toBe(true);
    expect((await gitCommand(root, "status", "--porcelain=v1", "--", "outside.txt")).stdout).toContain("outside.txt");
  });

  test("rejects ignored planned Core files because Git cannot protect their diff", async () => {
    const root = await createRoot();
    await fs.writeFile(path.join(root, ".gitignore"), ".agents/\n");
    await initializeGitRepository(root);
    await commitAll(root, "Ignore routed tree");

    const blocked = await runCliDefault("install", root);

    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("Git ignores planned install file");
    expect(await exists(path.join(root, "AGENTS.md"))).toBe(false);
    expect(await exists(path.join(root, ".agents"))).toBe(false);

    const bypassed = await runCliDefault("install", root, "--pro");
    expect(bypassed.exitCode).toBe(0);
    expect(bypassed.stdout).toContain("--pro bypassed Git and Core checkpoint policy");
  });

  test("rejects an ignored planned extension file without partial output", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    await initializeGitRepository(root);
    expect((await runCliDefault("install", root)).exitCode).toBe(0);
    await commitAll(root, "Install Core");
    await fs.appendFile(path.join(root, ".gitignore"), ".agents/patterns/ignored.md\n");
    await commitAll(root, "Ignore extension target");
    const extensionFile = path.join(extension, "payload", ".agents", "patterns", "ignored.md");
    await fs.mkdir(path.dirname(extensionFile), { recursive: true });
    await fs.writeFile(extensionFile, "ignored extension pattern\n");

    const blocked = await runCliDefault("extend", extension, root);

    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("Git ignores planned install file");
    expect(await exists(path.join(root, ".agents", "patterns", "ignored.md"))).toBe(false);
    expect((await gitCommand(root, "status", "--porcelain=v1")).stdout).toBe("");
  });

  test("rejects an extension that could hide its own outputs with Git control files", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    await initializeGitRepository(root);
    expect((await runCliDefault("install", root)).exitCode).toBe(0);
    await commitAll(root, "Install Core");
    await fs.mkdir(path.join(extension, "payload", ".agents", "patterns"), { recursive: true });
    await fs.writeFile(path.join(extension, "payload", ".gitignore"), ".agents/patterns/hidden.md\n");
    await fs.writeFile(path.join(extension, "payload", ".agents", "patterns", "hidden.md"), "hidden pattern\n");

    const blocked = await runCliDefault("extend", extension, root);

    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("may not write Git control path .gitignore");
    expect(await exists(path.join(root, ".gitignore"))).toBe(false);
    expect(await exists(path.join(root, ".agents", "patterns", "hidden.md"))).toBe(false);
    expect((await gitCommand(root, "status", "--porcelain=v1")).stdout).toBe("");
  });

  test("rejects nested .git control directories instead of silently omitting them", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    await initializeGitRepository(root);
    expect((await runCliDefault("install", root)).exitCode).toBe(0);
    await commitAll(root, "Install Core");
    const nestedGitConfig = path.join(extension, "payload", "nested", ".git", "config");
    await fs.mkdir(path.dirname(nestedGitConfig), { recursive: true });
    await fs.writeFile(nestedGitConfig, "[core]\n");

    const blocked = await runCliDefault("extend", extension, root);

    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("may not include Git control path");
    expect(blocked.stderr).toContain(".git");
    expect(await exists(path.join(root, "nested"))).toBe(false);
    expect((await gitCommand(root, "status", "--porcelain=v1")).stdout).toBe("");
  });

  test("treats Git pathspec-magic target names literally", async () => {
    if (process.platform === "win32") return;
    const root = await createRoot();
    const target = path.join(root, ":!demo");
    await fs.mkdir(target, { recursive: true });
    await fs.writeFile(path.join(target, "baseline.txt"), "baseline\n");
    await initializeGitRepository(root);
    await commitAll(root, "Baseline");
    await fs.writeFile(path.join(target, "baseline.txt"), "dirty\n");

    const blocked = await runCliDefault("install", target);

    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("requires a clean Git checkpoint");
    expect(await exists(path.join(target, "AGENTS.md"))).toBe(false);
  });

  test("keeps read-only catalogue and dry-run commands available without Git or Core", async () => {
    const parent = await createRoot();
    const target = path.join(parent, "preview-target");

    const listed = await runCliDefault("extend", "--list");
    const preview = await runCliDefault("extend", "dev-workflow", target, "--dry-run");

    expect(listed.exitCode).toBe(0);
    expect(listed.stdout).toContain("dev-workflow");
    expect(preview.exitCode).toBe(0);
    expect(preview.stdout).toContain("No files were written");
    expect(await exists(target)).toBe(false);
  });

  test("requires Core before a normal extension but preserves expert overlay-only installation", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    await initializeGitRepository(root);
    await fs.mkdir(path.join(extension, "payload"), { recursive: true });
    await fs.writeFile(path.join(extension, "payload", "overlay.txt"), "expert overlay\n");

    const blocked = await runCliDefault("extend", extension, root);
    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("Core is not installed");
    expect(await exists(path.join(root, "overlay.txt"))).toBe(false);

    const bypassed = await runCliDefault("extend", extension, root, "--pro");
    expect(bypassed.exitCode).toBe(0);
    expect(await fs.readFile(path.join(root, "overlay.txt"), "utf8")).toBe("expert overlay\n");
  });

  test("does not mistake unrelated anchor filenames for an installed Core checkpoint", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    await fs.mkdir(path.join(root, ".agents"), { recursive: true });
    await fs.writeFile(path.join(root, "AGENTS.md"), "# Unrelated agent notes\n");
    await fs.writeFile(path.join(root, ".agents", "loader.md"), "# Unrelated loader\n");
    await fs.mkdir(path.join(extension, "payload"), { recursive: true });
    await fs.writeFile(path.join(extension, "payload", "overlay.txt"), "must not install\n");
    await initializeGitRepository(root);
    await commitAll(root, "Unrelated anchors");

    const blocked = await runCliDefault("extend", extension, root);

    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("Core is not installed");
    expect(await exists(path.join(root, "overlay.txt"))).toBe(false);
  });

  test("reports that an unchanged clean reinstall needs no new commit", async () => {
    const root = await createRoot();
    await initializeGitRepository(root);
    expect((await runCliDefault("install", root)).exitCode).toBe(0);
    await commitAll(root, "Install Core");

    const result = await runCliDefault("install", root);

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("Git reports no target changes; no new commit is needed");
    expect((await gitCommand(root, "status", "--porcelain=v1")).stdout).toBe("");
  });

  test("rolls back all Core writes when a derived index plan is invalid", async () => {
    const root = await createRoot();
    const custom = path.join(root, ".agents", "custom", "_custom.md");
    await fs.mkdir(path.dirname(custom), { recursive: true });
    await fs.writeFile(path.join(root, "AGENTS.md"), "# Existing agent notes\n");
    await fs.writeFile(custom, `# Custom

## Entries

<!-- open-forge:generated-index:start -->
- none - Broken region - #Empty
`);
    await initializeGitRepository(root);
    await commitAll(root, "Malformed baseline");
    const beforeAgents = await fs.readFile(path.join(root, "AGENTS.md"), "utf8");
    const beforeCustom = await fs.readFile(custom, "utf8");

    const blocked = await runCliDefault("install", root);

    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("generated index markers");
    expect(await fs.readFile(path.join(root, "AGENTS.md"), "utf8")).toBe(beforeAgents);
    expect(await fs.readFile(custom, "utf8")).toBe(beforeCustom);
    expect(await exists(path.join(root, ".agents", "loader.md"))).toBe(false);
    expect((await gitCommand(root, "status", "--porcelain=v1")).stdout).toBe("");
  });

  test("rejects a hard-linked scoped Core rewrite before mutation", async () => {
    const root = await createRoot();
    const shared = path.join(root, "shared.md");
    const scoped = path.join(root, ".agents", "memory", "demo", "crystallized", "_crystallized.md");
    await fs.mkdir(path.dirname(scoped), { recursive: true });
    await fs.writeFile(shared, "# Shared inode\n");
    await fs.link(shared, scoped);
    await initializeGitRepository(root);
    await commitAll(root, "Hard-linked baseline");

    const blocked = await runCliDefault("install", root);

    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("multiple hard links");
    expect(await fs.readFile(shared, "utf8")).toBe("# Shared inode\n");
    expect(await exists(path.join(root, ".agents", "loader.md"))).toBe(false);
    expect((await gitCommand(root, "status", "--porcelain=v1")).stdout).toBe("");
  });

  test("rejects a portable case alias before writing the Core baseline", async () => {
    const root = await createRoot();
    const alias = path.join(root, "agents.md");
    await fs.writeFile(alias, "# Existing lower-case file\n");

    const blocked = await runCli("install", root);

    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("aliases planned portable path");
    expect(await fs.readFile(alias, "utf8")).toBe("# Existing lower-case file\n");
    expect(await exists(path.join(root, ".agents", "loader.md"))).toBe(false);
  });
});

describe("find command", () => {
  async function createFindRoot(): Promise<string> {
    const root = await createRoot();
    await fs.writeFile(path.join(root, "loader.md"), `# Loader

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
`);
    const guides = await createCategory(root, "guides", `---
open-forge:
  description: Guide routes
  tags: [Guide]
---

# Guides

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
`);
    await writeRoute(guides, "alpha.md", "Alpha guide", ["Guide", "KeepInMind"]);
    const notes = await createCategory(root, "notes", "# Notes\n");
    await writeRoute(notes, "beta.md", "Beta note", ["Note"]);
    await runCli("index", root);
    return root;
  }

  test("filters routed files by tag", async () => {
    const root = await createFindRoot();

    const result = await runCli("find", "--tag", "KeepInMind", root);

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("- `guides/alpha.md` - Alpha guide - #Guide #KeepInMind");
    expect(result.stdout).not.toContain("beta.md");
  });

  test("prints paths and bodies", async () => {
    const root = await createFindRoot();

    const paths = await runCli("find", "--tag", "Note", "--paths", root);
    const bodies = await runCli("find", "--tag", "Note", "--bodies", root);

    expect(paths.stdout.trim()).toBe("notes/beta.md");
    expect(bodies.stdout).toContain("----- notes/beta.md -----");
    expect(bodies.stdout).toContain("# Beta note");
  });

  test("expands a route by depth through generated entries", async () => {
    const root = await createFindRoot();

    const result = await runCli("find", "--route", "guides/_guides.md", "--depth", "1", "--paths", root);

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("guides/_guides.md");
    expect(result.stdout).toContain("guides/alpha.md");
    expect(result.stdout).not.toContain("beta.md");
  });

  test("emits machine-readable json", async () => {
    const root = await createFindRoot();

    const result = await runCli("find", "--tag", "KeepInMind", "--json", root);
    const items = JSON.parse(result.stdout) as Array<{ route: string; description: string; tags: string[] }>;

    expect(items).toHaveLength(1);
    expect(items[0].route).toBe("guides/alpha.md");
    expect(items[0].tags).toContain("KeepInMind");
  });

  test("follows required routes and reports missing ones as blockers", async () => {
    const root = await createFindRoot();
    const workflows = await createCategory(root, "workflows", "# Workflows\n");
    const skillFolder = path.join(root, "skills", "helper");
    await fs.mkdir(skillFolder, { recursive: true });
    await createCategory(root, "skills", "# Skills\n");
    await fs.writeFile(path.join(skillFolder, "SKILL.md"), `---
name: helper
description: Helper capability
---

# Helper
`);
    await fs.writeFile(path.join(workflows, "ship.md"), `# Ship

## Required Routes

Read every route below before Step 1.

- \`skills/helper/SKILL.md\` - helper capability

## Steps

1. Ship it.
`);
    await runCli("index", root);

    const followed = await runCli("find", "--route", "workflows/ship.md", "--follow-required", "--paths", root);
    expect(followed.exitCode).toBe(0);
    expect(followed.stdout).toContain("workflows/ship.md");
    expect(followed.stdout).toContain("skills/helper/SKILL.md");

    await fs.rm(path.join(skillFolder, "SKILL.md"));
    const missing = await runCli("find", "--route", "workflows/ship.md", "--follow-required", "--paths", root);
    expect(missing.exitCode).toBe(1);
    expect(missing.stderr).toContain("blocker");
  });

  test("ignores Required Routes examples inside tilde fences while honoring explicit none", async () => {
    const root = await createFindRoot();
    const workflows = await createCategory(root, "workflows", "# Workflows\n");
    await fs.writeFile(path.join(workflows, "direct.md"), [
      "# Direct",
      "",
      "## Required Routes",
      "",
      "~~~~markdown",
      "- `skills/missing/SKILL.md` - fenced example only",
      "```",
      "~~~~",
      "",
      "- none",
      "",
      "## Steps",
      "",
      "1. Proceed directly.",
      ""
    ].join("\n"));
    await runCli("index", root);

    const followed = await runCli("find", "--route", "workflows/direct.md", "--follow-required", "--paths", root);

    expect(followed.exitCode).toBe(0);
    expect(followed.stderr).toBe("");
    expect(followed.stdout.trim()).toBe("workflows/direct.md");
  });

  test("rejects a linked routed category across find, chain, doctor, and index", async () => {
    const root = await createRoot();
    const outside = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const outsidePatterns = path.join(outside, "patterns");
    await fs.mkdir(outsidePatterns, { recursive: true });
    await fs.writeFile(path.join(outsidePatterns, "_patterns.md"), `---
open-forge:
  description: Outside patterns
  tags: [Pattern]
---

# Outside Patterns
`);
    const linkedPatterns = path.join(root, ".agents", "patterns");
    await fs.rm(linkedPatterns, { recursive: true });
    await fs.symlink(outsidePatterns, linkedPatterns, process.platform === "win32" ? "junction" : "dir");

    const found = await runCli("find", "--tag", "Pattern", root);
    const chained = await runCli("chain", ".agents/patterns/_patterns.md", root);
    const diagnosed = await runCli("doctor", "--json", root);
    const indexed = await runCli("index", root);

    expect(found.exitCode).toBe(1);
    expect(found.stderr).toContain("symbolic link or junction");
    expect(chained.exitCode).toBe(1);
    expect(chained.stderr).toContain("escapes the target workspace");
    expect(diagnosed.exitCode).toBe(1);
    expect(JSON.parse(diagnosed.stdout).errors).toBeGreaterThan(0);
    expect(diagnosed.stdout).toContain("symbolic link or junction");
    expect(indexed.exitCode).toBe(1);
    expect(indexed.stderr).toContain("symbolic link or junction");
  });
});

describe("chain command", () => {
  test("prints loader-to-target heading inheritance with overwrite adjacency", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    expect((await runCli("create", "category", "patterns/product/components", root)).exitCode).toBe(0);
    const productEntrypoint = path.join(root, ".agents", "patterns", "product", "_product.md");
    const componentEntrypoint = path.join(root, ".agents", "patterns", "product", "components", "_components.md");
    const target = path.join(root, ".agents", "patterns", "product", "components", "rules.md");
    const targetOverwrite = path.join(root, ".agents", "patterns", "product", "components", "rules.overwrite.md");
    const productText = await fs.readFile(productEntrypoint, "utf8");
    await fs.writeFile(productEntrypoint, productText);
    await fs.writeFile(path.join(path.dirname(productEntrypoint), "_product.overwrite.md"), "# Product Override\n\n## Axioms\n\n- Keep product vocabulary stable.\n");
    const componentText = (await fs.readFile(componentEntrypoint, "utf8")).replace(
      "- inherited - No local axioms; loaded ancestor axioms remain active.",
      "- none"
    );
    await fs.writeFile(componentEntrypoint, componentText);
    await fs.writeFile(target, "# Rules\n\n## Axioms\n\n- Keep component boundaries visible.\n\n## Evidence\n\n- Target evidence.\n");
    await fs.writeFile(targetOverwrite, "# Rules Override\n\n## Axioms\n\n- Prefer the narrower component owner.\n");
    expect((await runCli("index", root)).exitCode).toBe(0);
    const before = await fs.readFile(target, "utf8");

    const result = await runCli("chain", ".agents/patterns/product/components/rules.md", "--heading", "Axioms", "--json", root);

    expect(result.exitCode).toBe(0);
    const parsed = JSON.parse(result.stdout) as { chain: Array<{ route: string; kind: string; overwriteOf?: string; heading: { status: string } }> };
    expect(parsed.chain.map((item) => item.route)).toEqual([
      ".agents/loader.md",
      ".agents/patterns/_patterns.md",
      ".agents/patterns/product/_product.md",
      ".agents/patterns/product/_product.overwrite.md",
      ".agents/patterns/product/components/_components.md",
      ".agents/patterns/product/components/rules.md",
      ".agents/patterns/product/components/rules.overwrite.md"
    ]);
    expect(parsed.chain.map((item) => item.heading.status)).toEqual([
      "content",
      "content",
      "declared-inherited",
      "content",
      "declared-none",
      "content",
      "content"
    ]);
    expect(parsed.chain[3].overwriteOf).toBe(".agents/patterns/product/_product.md");
    expect(parsed.chain[6].overwriteOf).toBe(".agents/patterns/product/components/rules.md");
    expect(await fs.readFile(target, "utf8")).toBe(before);

    const arbitrary = await runCli("chain", ".agents/patterns/product/components/rules.md", "--heading", "Evidence", root);
    expect(arbitrary.exitCode).toBe(0);
    expect(arbitrary.stdout).toContain("Target evidence.");
    expect(arbitrary.stdout).toContain("[absent]");
  });

  test("includes a native skill entrypoint before an internal skill resource", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const skill = path.join(root, ".agents", "skills", "external", "SKILL.md");
    const reference = path.join(root, ".agents", "skills", "external", "references", "check.md");
    await fs.mkdir(path.dirname(reference), { recursive: true });
    await fs.writeFile(skill, "---\nname: external\ndescription: External fixture skill.\n---\n\n# External\n\n## Axioms\n\n- Use the skill contract.\n");
    await fs.writeFile(reference, "# Check\n\n## Axioms\n\n- Check the external result.\n");
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("chain", ".agents/skills/external/references/check.md", "--json", root);

    expect(result.exitCode).toBe(0);
    const routes = (JSON.parse(result.stdout) as { chain: Array<{ route: string }> }).chain.map((item) => item.route);
    expect(routes).toEqual([
      ".agents/loader.md",
      ".agents/skills/_skills.md",
      ".agents/skills/external/SKILL.md",
      ".agents/skills/external/references/check.md"
    ]);
  });

  test("rejects traversal and leaves the OS-temporary workspace unchanged", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const loader = path.join(root, ".agents", "loader.md");
    const before = await fs.readFile(loader, "utf8");

    const result = await runCli("chain", "../outside.md", "--heading", "Axioms", root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("must not escape or change the workspace path");
    expect(await fs.readFile(loader, "utf8")).toBe(before);
  });
});

describe("doctor command", () => {
  test("reports no problems for a fresh install", async () => {
    const root = await createRoot();
    await runCli("install", root);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("no problems found");
  });

  test("stops before reading a symlinked loader after route-tree safety fails", async () => {
    const root = await createRoot();
    const outside = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const loader = path.join(root, ".agents", "loader.md");
    const outsideLoader = path.join(outside, "loader.md");
    await fs.writeFile(outsideLoader, "# Outside\n\n<!-- open-forge:generated-index:start -->\n");
    await fs.rm(loader);
    try {
      await fs.symlink(outsideLoader, loader, "file");
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EPERM" || (error as NodeJS.ErrnoException).code === "EACCES") return;
      throw error;
    }

    const result = await runCli("doctor", "--json", root);
    const report = JSON.parse(result.stdout) as { errors: number; warnings: number; findings: Array<{ message: string }> };

    expect(result.exitCode).toBe(1);
    expect(report.errors).toBeGreaterThan(0);
    expect(report.findings.some((finding) => finding.message.includes("symbolic link or junction"))).toBe(true);
    expect(report.findings.some((finding) => finding.message.includes("generated index markers"))).toBe(false);
  });

  test("stops before recomputing regions when a category entrypoint is a file symlink", async () => {
    const root = await createRoot();
    const outside = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const entrypoint = path.join(root, ".agents", "patterns", "_patterns.md");
    const outsideEntrypoint = path.join(outside, "_patterns.md");
    await fs.writeFile(outsideEntrypoint, "# Outside Patterns\n");
    await fs.rm(entrypoint);
    try {
      await fs.symlink(outsideEntrypoint, entrypoint, "file");
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EPERM" || (error as NodeJS.ErrnoException).code === "EACCES") return;
      throw error;
    }

    const result = await runCli("doctor", "--json", root);
    const report = JSON.parse(result.stdout) as { errors: number; warnings: number; findings: Array<{ message: string }> };

    expect(result.exitCode).toBe(1);
    expect(report.errors).toBeGreaterThan(0);
    expect(report.findings.some((finding) => finding.message.includes("symbolic link or junction"))).toBe(true);
    expect(report.findings.some((finding) => finding.message.includes("stale"))).toBe(false);
  });

  test("warns on stale regions and errors on unresolved entries", async () => {
    const root = await createRoot();
    const guides = await createCategory(root, "guides", "# Guides\n");
    await writeRoute(guides, "alpha.md", "Alpha guide", ["Guide"]);
    await runCli("index", root);

    await writeRoute(guides, "gamma.md", "Gamma guide", ["Guide"]);
    const stale = await runCli("doctor", root);
    expect(stale.exitCode).toBe(0);
    expect(stale.stdout).toContain("stale");

    await fs.rm(path.join(guides, "alpha.md"));
    const broken = await runCli("doctor", root);
    expect(broken.exitCode).toBe(1);
    expect(broken.stdout).toContain("does not resolve: alpha.md");
  });

  test("warns on retired tags, orphan overwrites, and unreachable files", async () => {
    const root = await createRoot();
    const guides = await createCategory(root, "guides", "# Guides\n");
    await writeRoute(guides, "old.md", "Old guide", ["OpenForge", "Guide"]);
    await fs.writeFile(path.join(guides, "ghost.overwrite.md"), "# Ghost overwrite\n");
    const unrouted = path.join(root, "loose");
    await fs.mkdir(unrouted, { recursive: true });
    await fs.writeFile(path.join(unrouted, "stray.md"), "# Stray\n");
    await runCli("index", root);

    const result = await runCli("doctor", "--json", root);
    const report = JSON.parse(result.stdout) as { errors: number; warnings: number; findings: Array<{ level: string; message: string }> };

    expect(result.exitCode).toBe(0);
    expect(report.errors).toBe(0);
    expect(report.findings.some((finding) => finding.message.includes("retired load-policy tag"))).toBe(true);
    expect(report.findings.some((finding) => finding.message.includes("overwrite companion has no base file"))).toBe(true);
    expect(report.findings.some((finding) => finding.message.includes("not reachable through generated routing"))).toBe(true);
  });

  test("errors on required routes that do not resolve", async () => {
    const root = await createRoot();
    const workflows = await createCategory(root, "workflows", "# Workflows\n");
    await fs.writeFile(path.join(workflows, "ship.md"), `# Ship

## Required Routes

- \`skills/missing/SKILL.md\` - not installed

## Steps

1. Ship it.
`);
    await runCli("index", root);

    const result = await runCli("doctor", root);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("required route does not resolve: skills/missing/SKILL.md");
  });

  test("enforces ordered workflow Mode and always-present Constraints", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const workflow = path.join(root, ".agents", "workflows", "invalid.md");
    await fs.writeFile(workflow, `---
open-forge:
  description: Invalid workflow fixture
  tags: [Workflow]
---

# Invalid

## Mode

goal-seeking

## Goal

- outcome: demonstrate validation

## Required Routes

none

## Steps

1. Run.

## Loop

Linear.

## Outputs

- result

## Completion

- [ ] done
`);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("workflow must define exactly one Constraints section");
    expect(result.stdout).toContain("workflow Mode must be linear or iterative");
  });

  test("accepts a linear workflow with explicit none constraints", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const workflow = path.join(root, ".agents", "workflows", "linear.md");
    await fs.writeFile(workflow, `---
open-forge:
  description: Valid linear workflow fixture
  tags: [Workflow]
---

# Linear

~~~text
## Goal
\`\`\`
~~~

## Mode

linear

## Goal

- outcome: one result

## Required Routes

none

## Constraints

- none

## Steps

1. Produce the result.

## Loop

Execute the Steps once; no loop.

## Outputs

- result

## Completion

- [ ] result exists
`);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", "--json", root);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(JSON.parse(result.stdout)).toMatchObject({ errors: 0, warnings: 0 });
  });

  test("rejects workflow contract sections that are not level-2 headings", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const workflow = path.join(root, ".agents", "workflows", "wrong-level.md");
    await fs.writeFile(workflow, [
      "---",
      "open-forge:",
      "  description: Wrong heading level workflow fixture",
      "  tags: [Workflow]",
      "---",
      "",
      "# Wrong Level",
      "",
      "## Mode",
      "",
      "linear",
      "",
      "## Goal",
      "",
      "- outcome: one result",
      "",
      "### Required Routes",
      "",
      "- none",
      "",
      "## Constraints",
      "",
      "- none",
      "",
      "## Steps",
      "",
      "1. Produce the result.",
      "",
      "## Loop",
      "",
      "Execute once.",
      "",
      "## Outputs",
      "",
      "- result",
      "",
      "## Completion",
      "",
      "- [ ] result exists",
      ""
    ].join("\n"));
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("workflow Required Routes section must use a level-2 Markdown heading");
  });

  test("rejects misordered workflow sections and invalid Constraints sentinels", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const workflow = path.join(root, ".agents", "workflows", "misordered.md");
    await fs.writeFile(workflow, `---
open-forge:
  description: Misordered workflow fixture
  tags: [Workflow]
---

# Misordered

## Goal

- outcome: demonstrate validation

## Mode

iterative

## Required Routes

none

## Constraints

- inherited

## Steps

1. Run.

## Loop

Repeat until accepted.

## Outputs

- result

## Completion

- [ ] done
`);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("workflow section Goal is out of order");
    expect(result.stdout).toContain("inherited is not a workflow constraint sentinel");
  });

  test("requires explicit directive applicability before Axioms", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const directive = path.join(root, ".agents", "directives", "fixture.md");
    await fs.writeFile(directive, `---
open-forge:
  description: Directive without explicit scope
  tags: [Directive]
---

# Fixture

## Axioms

- Do the thing.
`);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("directive must declare one non-empty Applies To section");
  });

  test("does not treat headings inside mismatched fenced-code markers as directive scope", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const directive = path.join(root, ".agents", "directives", "fenced.md");
    await fs.writeFile(directive, `---
open-forge:
  description: Fenced directive fixture
  tags: [Directive]
---

# Fenced

~~~text
## Applies To

workspace-wide
\`\`\`
~~~

## Axioms

- Do the thing.
`);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("directive must declare one non-empty Applies To section");
  });

  test("infers workflow and directive validation from routed category ancestry", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    await fs.writeFile(path.join(root, ".agents", "workflows", "untagged.md"), "# Untagged Workflow\n\n## Steps\n\n1. Run.\n");
    await fs.writeFile(path.join(root, ".agents", "directives", "untagged.md"), "# Untagged Directive\n\n## Axioms\n\n- Be explicit.\n");
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("workflow must define exactly one Mode section");
    expect(result.stdout).toContain("directive must declare one non-empty Applies To section");
  });

  test("validates explicitly tagged primitives under a neutral custom route", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    expect((await runCli("create", "category", "custom", root)).exitCode).toBe(0);
    const custom = path.join(root, ".agents", "custom");
    await fs.writeFile(path.join(custom, "flow.md"), [
      "---",
      "open-forge:",
      "  description: Explicit workflow in a neutral route",
      "  tags: [Workflow]",
      "---",
      "",
      "# Flow",
      "",
      "## Steps",
      "",
      "1. Run.",
      ""
    ].join("\n"));
    await fs.writeFile(path.join(custom, "rule.md"), [
      "---",
      "open-forge:",
      "  description: Explicit directive in a neutral route",
      "  tags: [Directive]",
      "---",
      "",
      "# Rule",
      "",
      "## Axioms",
      "",
      "- Be explicit.",
      ""
    ].join("\n"));
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("workflow must define exactly one Mode section");
    expect(result.stdout).toContain("directive must declare one non-empty Applies To section");
  });

  test("treats Workflow and Directive as topical tags when another primitive owns the file", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const topical = path.join(root, ".agents", "memory", "emerging", "ideas", "topical.md");
    await fs.writeFile(topical, [
      "---",
      "open-forge:",
      "  description: Memory about workflow and directive design",
      "  tags: [Idea, Workflow, Directive]",
      "---",
      "",
      "# Topical Memory",
      "",
      "This is a memory record, not an executable workflow or directive.",
      ""
    ].join("\n"));
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", "--json", root);

    expect(result.exitCode).toBe(0);
    expect(JSON.parse(result.stdout)).toMatchObject({ errors: 0, warnings: 0 });
  });

  test("allows organizational workflow categories and workflow-local primitive routes", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    expect((await runCli("create", "category", "workflows/frontend/patterns", root)).exitCode).toBe(0);
    const localPatterns = path.join(root, ".agents", "workflows", "frontend", "patterns");
    const localEntrypoint = await fs.readFile(path.join(localPatterns, "_patterns.md"), "utf8");
    expect(localEntrypoint).toContain("tags: [Pattern]");
    await writeRoute(localPatterns, "shape.md", "Frontend shape", ["Pattern"]);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", "--json", root);

    expect(result.exitCode).toBe(0);
    expect(JSON.parse(result.stdout)).toMatchObject({ errors: 0, warnings: 0 });
  });
});

describe("create command", () => {
  test("scaffolds a category chain with entrypoints and reindexes", async () => {
    const root = await createRoot();
    await runCli("install", root);

    const result = await runCli("create", "category", "patterns/react/components", root);
    const parent = await fs.readFile(path.join(root, ".agents", "patterns", "_patterns.md"), "utf8");
    const react = await fs.readFile(path.join(root, ".agents", "patterns", "react", "_react.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain(".agents/patterns/react/_react.md");
    expect(result.stdout).toContain(".agents/patterns/react/components/_components.md");
    expect(react).toContain("tags: [Pattern]");
    expect(parent).toContain("- `react/_react.md` - TODO - when to select this route and what it provides - #Pattern");

    const doctorResult = await runCli("doctor", root);
    expect(doctorResult.exitCode).toBe(0);
  });

  test("refuses an already routable category", async () => {
    const root = await createRoot();
    await runCli("install", root);

    const result = await runCli("create", "category", "patterns", root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("already routable");
  });

  test("scaffolds an extension package", async () => {
    const root = await createRoot();

    const result = await runCli("create", "extension", "my-pack", root);
    const metadata = JSON.parse(await fs.readFile(path.join(root, "my-pack", "extension.json"), "utf8")) as { name: string };

    expect(result.exitCode).toBe(0);
    expect(metadata.name).toBe("My Pack");
    expect(await exists(path.join(root, "my-pack", "README.md"))).toBe(true);
    expect(await exists(path.join(root, "my-pack", "payload", ".agents"))).toBe(true);
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

async function createBundledExtension(root: string, id: string, name: string, description: string, dependencies: string[] = []): Promise<string> {
  const patterns = path.join(root, id, "payload", ".agents", "patterns", id);
  await fs.mkdir(patterns, { recursive: true });
  await fs.writeFile(path.join(root, id, "extension.json"), `${JSON.stringify({ name, description, dependencies }, null, 2)}\n`);
  await fs.writeFile(path.join(patterns, `_${id}.md`), `---
open-forge:
  description: ${description}
  tags: [Extension, Core, Pattern]
---

# ${name}
`);
  return patterns;
}

async function createDependencyPack(root: string, id: string, name: string, description: string, dependencies: string[]): Promise<void> {
  const packageRoot = path.join(root, id);
  await fs.mkdir(packageRoot, { recursive: true });
  await fs.writeFile(path.join(packageRoot, "extension.json"), `${JSON.stringify({ name, description, dependencies }, null, 2)}\n`);
  await fs.writeFile(path.join(packageRoot, "README.md"), "Authoring documentation that must not be installed.\n");
}

type CliCommand = "index" | "install" | "extend" | "find" | "chain" | "doctor" | "create";

async function runCli(command: CliCommand, ...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  return runCliWithEnv(command, args, {});
}

async function runCliWithEnv(command: CliCommand, args: string[], env: Record<string, string>): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  const commandArgs = (command === "install" || command === "extend") && !args.includes("--pro") ? [...args, "--pro"] : args;
  return spawnCli(command, commandArgs, env);
}

async function runCliDefault(command: CliCommand, ...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  return spawnCli(command, args, {});
}

async function spawnCli(command: CliCommand, args: string[], env: Record<string, string>): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  const child = Bun.spawn([process.execPath, cliFile, command, ...args], {
    env: { ...process.env, ...env },
    stdout: "pipe",
    stderr: "pipe"
  });
  const [exitCode, stdout, stderr] = await Promise.all([
    child.exited,
    new Response(child.stdout).text(),
    new Response(child.stderr).text()
  ]);

  return { exitCode, stdout, stderr };
}

async function initializeGitRepository(root: string): Promise<void> {
  expect((await gitCommand(root, "init")).exitCode).toBe(0);
  expect((await gitCommand(root, "config", "user.email", "open-forge-tests@example.invalid")).exitCode).toBe(0);
  expect((await gitCommand(root, "config", "user.name", "Open Forge Tests")).exitCode).toBe(0);
}

async function commitAll(root: string, message: string): Promise<void> {
  expect((await gitCommand(root, "add", "-A")).exitCode).toBe(0);
  expect((await gitCommand(root, "commit", "-m", message)).exitCode).toBe(0);
}

async function gitCommand(root: string, ...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  const child = Bun.spawn(["git", "-C", root, ...args], { stdout: "pipe", stderr: "pipe" });
  const [exitCode, stdout, stderr] = await Promise.all([
    child.exited,
    new Response(child.stdout).text(),
    new Response(child.stderr).text()
  ]);
  return { exitCode, stdout, stderr };
}

async function exists(file: string): Promise<boolean> {
  try {
    await fs.access(file);
    return true;
  } catch {
    return false;
  }
}

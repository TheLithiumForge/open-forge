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
    expect(directives).toContain("Every directive file beside this `entrypoint` is workspace-wide; read all of them.");
    expect(directives).toContain("Child directive `entrypoints` define positive scope through path, description, and tags.");
    expect(directives).toContain("Load child directive routes when their path, description, tags, or defined tag behavior match the current work.");
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
    expect(loader).toContain("- Immediately read #LoadNow and #KeepInMind entries when they appear in loaded `Entries`, in listed order.");
    expect(loader).toContain("- Before ending meaningful work, recheck loaded #KeepInMind entries and perform the follow-ups they require; `open-forge find --tag KeepInMind --bodies` prints them in one call when the CLI is available.");
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
    expect(workflows).toContain("- Every workflow defines `Goal`, `Required Routes`, `Steps`, `Loop`, `Outputs`, and `Completion`; it adds `Constraints` only when cross-step invariants exist.");
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
});

describe("doctor command", () => {
  test("reports no problems for a fresh install", async () => {
    const root = await createRoot();
    await runCli("install", root);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("no problems found");
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

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("required route does not resolve: skills/missing/SKILL.md");
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

async function createBundledExtension(root: string, id: string, name: string, description: string): Promise<string> {
  const patterns = path.join(root, id, "payload", ".agents", "patterns", id);
  await fs.mkdir(patterns, { recursive: true });
  await fs.writeFile(path.join(root, id, "extension.json"), `${JSON.stringify({ name, description }, null, 2)}\n`);
  await fs.writeFile(path.join(patterns, `_${id}.md`), `---
open-forge:
  description: ${description}
  tags: [Extension, Core, Pattern]
---

# ${name}
`);
  return patterns;
}

async function runCli(command: "index" | "install" | "extend" | "find" | "doctor" | "create", ...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  return runCliWithEnv(command, args, {});
}

async function runCliWithEnv(command: "index" | "install" | "extend" | "find" | "doctor" | "create", args: string[], env: Record<string, string>): Promise<{ exitCode: number; stdout: string; stderr: string }> {
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

async function exists(file: string): Promise<boolean> {
  try {
    await fs.access(file);
    return true;
  } catch {
    return false;
  }
}

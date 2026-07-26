import { describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import path from "node:path";
import {
  commitAll,
  initializeGitRepository,
  pathExists as exists,
  repoPath,
  runCli as executeCli,
  runGit as gitCommand,
  snapshotTree,
  snapshotTreeState,
  useTestSandbox
} from "../../tests/support/index.ts";

const cliFile = repoPath("src", "cli", "cli.ts");
const sandbox = useTestSandbox("open-forge-cli");

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
    expect(content).toContain("- [Repository routes](repositories.md) - #Workspace #Route");
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
    expect(content).toContain("- [Component patterns](components.md) - #Pattern");
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
    expect(parent).toContain("- [No description](repositories/_repositories.md) - #Index");
    expect(child).toContain("- [API repository](api.md) - #Repository");
    expect(child).toContain("- [No description](services/_services.md) - #Index");
    expect(parent).not.toContain("services/_services.md");
    expect(grandchild).toContain("- [Billing service](billing.md) - #Service");
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
    expect(content).toContain("- [Implementation capability for fitting, testing, coding, and verifying changes](implementation/SKILL.md) - #Skill");
    expect(content).not.toContain("references/fit-change.md");
    expect(content).not.toContain("legacy.md");
  });

  test("escapes link labels, encodes route segments, and follows the decoded generated route", async () => {
    const root = await createRoot();
    const guides = await createCategory(root, "guides", "# Guides\n");
    const routeName = "route (draft) #1% ready.md";
    await writeRoute(guides, routeName, "Use [draft] \\ path #NotATag", ["Guide", "Actual"]);

    expect((await runCli("index", root)).exitCode).toBe(0);
    const index = await fs.readFile(path.join(guides, "_guides.md"), "utf8");
    expect(index).toContain(
      "- [Use \\[draft\\] \\\\ path #NotATag](route%20%28draft%29%20%231%25%20ready.md) - #Guide #Actual"
    );

    const expanded = await runCli("find", "--route", "guides/_guides.md", "--depth", "1", "--paths", root);
    expect(expanded.exitCode).toBe(0);
    expect(expanded.stdout).toContain(`guides/${routeName}`);
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
    expect(loader).toContain("- [Workspace routes that point to important project locations and explain when to use them](workspace/_workspace.md) - #LoadNow #Workspace");
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
      expect(loader).toContain(`- [External tool routes](external/${alias}) - #External #Tool`);
      expect(entrypoint).toContain("- [External source](source.md) - #External");
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
  test("installs a structurally valid and idempotent Core route tree", async () => {
    const root = await createRoot();

    const result = await runCli("install", root);
    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    const installedLoader = await fs.readFile(path.join(root, ".agents", "loader.md"), "utf8");
    expect(installedLoader).toContain("](directives/_directives.md) - #LoadNow");
    expect(installedLoader).not.toContain("](.agents/");
    expect(installedLoader).toContain("#Evergreen - Material that must stay aligned");
    expect(installedLoader).toContain("A request to act also accepts any decision required to perform that action");
    expect(installedLoader).toContain("update only affected #Evergreen material you may edit");

    const installedMemory = await fs.readFile(path.join(root, ".agents", "memory", "_memory.md"), "utf8");
    expect(installedMemory).toContain("Memory may record any subject, including how work is performed");

    const installedDocuments = await fs.readFile(
      path.join(root, ".agents", "memory", "crystallized", "documents", "_documents.md"),
      "utf8"
    );
    expect(installedDocuments).not.toContain("#Evergreen documents");

    const installedDirectives = await fs.readFile(path.join(root, ".agents", "directives", "_directives.md"), "utf8");
    expect(installedDirectives).not.toContain("truth-maintenance.md");

    const installedTemplates = await fs.readFile(path.join(root, ".agents", "templates", "_templates.md"), "utf8");
    expect(installedTemplates).toContain("Templates are reusable source artifacts intended to be instantiated");
    expect(installedTemplates).toContain("tags: [Core, Template]");
    expect(installedTemplates).toContain("Use a linked matching #Core owner");
    expect(installedTemplates).toContain("Make the need each template satisfies and the primary question or result it answers visible");

    const doctorResult = await runCli("doctor", "--json", root);
    expect(doctorResult.stderr).toBe("");
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });

    const loadNowResult = await runCli("find", "--tag", "LoadNow", "--json", root);
    expect(loadNowResult.exitCode).toBe(0);
    const loadNowRoutes = (JSON.parse(loadNowResult.stdout) as Array<{ route: string }>).map((entry) => entry.route);
    expect(loadNowRoutes).toEqual(expect.arrayContaining([
      ".agents/directives/_directives.md",
      ".agents/guidance/_guidance.md",
      ".agents/memory/_memory.md",
      ".agents/patterns/_patterns.md",
      ".agents/skills/_skills.md",
      ".agents/workflows/_workflows.md",
      ".agents/workspace/_workspace.md"
    ]));
    expect(loadNowRoutes).not.toContain(".agents/templates/_templates.md");

    const templateResult = await runCli("find", "--tag", "Template", "--json", root);
    expect(templateResult.exitCode).toBe(0);
    const templateRoutes = (JSON.parse(templateResult.stdout) as Array<{ route: string }>).map((entry) => entry.route);
    expect(templateRoutes).toContain(".agents/templates/_templates.md");

    const keepInMindResult = await runCli("find", "--tag", "KeepInMind", "--json", root);
    expect(keepInMindResult.exitCode).toBe(0);
    const keepInMindRoutes = (JSON.parse(keepInMindResult.stdout) as Array<{ route: string }>).map((entry) => entry.route);
    expect(keepInMindRoutes).toEqual(expect.arrayContaining([
      ".agents/memory/emerging/_emerging.md",
      ".agents/memory/emerging/observations/_observations.md"
    ]));

    const before = await snapshotTree(root);
    const indexResult = await runCli("index", root);
    expect(indexResult.exitCode).toBe(0);
    expect(await snapshotTree(root)).toEqual(before);
  });

  test("updates scoped framework route entrypoints by path shape", async () => {
    const root = await createRoot();
    const scopedDocuments = path.join(root, ".agents", "memory", "customer-facing", "mobile-app", "crystallized", "platform", "documents");
    const scopedDecisions = path.join(root, ".agents", "memory", "mobile-app", "crystallized", "decisions");
    const scopedTemplates = path.join(root, ".agents", "workflows", "frontend", "templates");
    await fs.mkdir(scopedDocuments, { recursive: true });
    await fs.mkdir(scopedDecisions, { recursive: true });
    await fs.mkdir(scopedTemplates, { recursive: true });
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
    await fs.writeFile(path.join(scopedTemplates, "_templates.md"), `# Old Scoped Templates

Old scoped templates framework copy.
`);
    await writeRoute(scopedDocuments, "architecture.md", "Scoped architecture truth", ["Memory", "Document", "CurrentTruth"]);

    const result = await runCli("install", root);
    const scopedContent = await fs.readFile(path.join(scopedDocuments, "_documents.md"), "utf8");
    const scopedDecisionContent = await fs.readFile(path.join(scopedDecisions, "_decisions.md"), "utf8");
    const scopedTemplateContent = await fs.readFile(path.join(scopedTemplates, "_templates.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(scopedContent).toContain("Documents are durable accepted records, or routes to those records, for long-form project knowledge.");
    expect(scopedContent).not.toContain("Old scoped framework copy.");
    expect(scopedContent).toContain("- [Scoped architecture truth](architecture.md) - #Memory #Document #CurrentTruth");
    expect(scopedDecisionContent).toContain("Decisions are accepted rationale for important choices");
    expect(scopedDecisionContent).not.toContain("Old scoped decisions framework copy.");
    expect(scopedTemplateContent).toContain("Templates are reusable source artifacts intended to be instantiated");
    expect(scopedTemplateContent).not.toContain("Old scoped templates framework copy.");
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
    expect(patterns).toContain("- [React component patterns for this workspace](react/_react.md) - #Extension #Core #Pattern #React");
    expect(react).toContain("- [Reusable React component shape](components.md) - #Pattern #React");
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
    await fs.writeFile(path.join(extensionsRoot, "review-pack", "extension.json"), JSON.stringify({
      id: "review-pack",
      name: "Review Pack",
      description: "Review patterns bundled with Open Forge",
      dependencies: []
    }));
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
    expect(patterns).toContain("- [Review patterns bundled with Open Forge](reviews/_reviews.md) - #Extension #Core #Pattern #Review");
    expect(reviews).toContain("- [Pull request review shape](pull-requests.md) - #Pattern #Review");
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

  test("discovers nested bundled packages by stable manifest id instead of folder name", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const packageRoot = path.join(extensionsRoot, "support", "physical-folder");
    await fs.mkdir(path.join(packageRoot, "payload"), { recursive: true });
    await fs.writeFile(path.join(packageRoot, "extension.json"), `${JSON.stringify({
      id: "stable-package-id",
      name: "Stable Package",
      description: "Nested support fixture",
      dependencies: []
    }, null, 2)}\n`);
    await fs.writeFile(path.join(packageRoot, "payload", "nested-marker.txt"), "nested package\n");

    const listed = await runCliWithEnv("extend", ["--list"], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });
    const installed = await runCliWithEnv("extend", ["stable-package-id", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });

    expect(listed.exitCode).toBe(0);
    expect(listed.stdout).toContain("Support:");
    expect(listed.stdout).toContain("- stable-package-id - Nested support fixture (contents: other)");
    expect(listed.stdout).not.toContain("physical-folder -");
    expect(installed.exitCode).toBe(0);
    expect(await fs.readFile(path.join(root, "nested-marker.txt"), "utf8")).toBe("nested package\n");
  });

  test("rejects nested bundled packages without stable manifest ids", async () => {
    const extensionsRoot = await createRoot();
    const packageRoot = path.join(extensionsRoot, "packs", "missing-id");
    await fs.mkdir(path.join(packageRoot, "payload"), { recursive: true });
    await fs.writeFile(path.join(packageRoot, "extension.json"), JSON.stringify({ name: "Missing Id", dependencies: [] }));
    await fs.writeFile(path.join(packageRoot, "payload", "marker.txt"), "marker\n");

    const result = await runCliWithEnv("extend", ["--list"], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("must declare a stable id");
  });

  test("rejects duplicate bundled manifest ids across nested organization folders", async () => {
    const extensionsRoot = await createRoot();
    for (const group of ["skills", "support"]) {
      const packageRoot = path.join(extensionsRoot, group, `${group}-folder`);
      await fs.mkdir(path.join(packageRoot, "payload"), { recursive: true });
      await fs.writeFile(path.join(packageRoot, "extension.json"), JSON.stringify({
        id: "duplicate-id",
        name: group,
        dependencies: []
      }));
      await fs.writeFile(path.join(packageRoot, "payload", `${group}.txt`), `${group}\n`);
    }

    const result = await runCliWithEnv("extend", ["--list"], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("Bundled extension id duplicate-id is duplicated");
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
      path.join(agents, "templates", "mixed.md"),
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
    expect(result.stdout).toContain("(contents: skill, workflow, directive, guidance, pattern, template, workspace, memory, other)");
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
    expect(patterns).toContain("- [Alpha extension](alpha-pack/_alpha-pack.md) - #Extension #Core #Pattern");
    expect(patterns).toContain("- [Beta extension](beta-pack/_beta-pack.md) - #Extension #Core #Pattern");
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
    await fs.writeFile(path.join(extensionsRoot, "alpha-pack", "extension.json"), JSON.stringify({ id: "alpha-pack", name: "Alpha" }));
    await fs.writeFile(path.join(extensionsRoot, "beta-pack", "extension.json"), JSON.stringify({ id: "beta-pack", name: "Beta" }));
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
    await fs.writeFile(path.join(extensionsRoot, "alpha-pack", "extension.json"), JSON.stringify({ id: "alpha-pack", name: "Alpha" }));
    await fs.writeFile(path.join(extensionsRoot, "beta-pack", "extension.json"), JSON.stringify({ id: "beta-pack", name: "Beta" }));
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

    await fs.writeFile(alphaShared, "updated together\n");
    await fs.writeFile(betaShared, "updated together\n");
    const partialUpdate = await runCliWithEnv("extend", ["alpha-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });
    expect(partialUpdate.exitCode).toBe(1);
    expect(partialUpdate.stderr).toContain("unless every existing owner participates");
    expect(partialUpdate.stderr).toContain("beta-pack");
    expect(await fs.readFile(path.join(root, ".agents", "shared.md"), "utf8")).toBe("same\n");

    expect((await runCliWithEnv("extend", ["--ids", "alpha-pack,beta-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    })).exitCode).toBe(0);
    expect(await fs.readFile(path.join(root, ".agents", "shared.md"), "utf8")).toBe("updated together\n");

    await fs.rm(alphaShared);
    expect((await runCliWithEnv("extend", ["alpha-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    })).exitCode).toBe(0);
    const receipt = JSON.parse(await fs.readFile(path.join(root, "open-forge.extensions.json"), "utf8")) as {
      files: Record<string, { owners: string[] }>;
    };
    expect(receipt.files[".agents/shared.md"].owners).toEqual(["beta-pack"]);
    expect(await fs.readFile(path.join(root, ".agents", "shared.md"), "utf8")).toBe("updated together\n");
  });

  test("rejects even byte-identical unowned files instead of silently adopting them", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const sourcePatterns = await createBundledExtension(extensionsRoot, "managed-pack", "Managed Pack", "Managed extension");
    const sourceFile = path.join(sourcePatterns, "_managed-pack.md");
    const targetFile = path.join(root, ".agents", "patterns", "managed-pack", "_managed-pack.md");
    await fs.mkdir(path.dirname(targetFile), { recursive: true });
    await fs.copyFile(sourceFile, targetFile);

    const result = await runCliWithEnv("extend", ["managed-pack", root], {
      OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot
    });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("may not claim or replace unowned existing file");
    expect(await exists(path.join(root, "open-forge.extensions.json"))).toBe(false);
    expect(await fs.readFile(targetFile)).toEqual(await fs.readFile(sourceFile));
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
      id: "copied-workflow-suite",
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

  test("reconciles whole-file payload paths dropped by a managed reinstall", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const packageRoot = path.join(extensionsRoot, "evolving-pack");
    const payloadRoot = path.join(packageRoot, "payload");
    await fs.mkdir(payloadRoot, { recursive: true });
    const manifest = `${JSON.stringify({
      id: "evolving-pack",
      name: "Evolving Pack",
      version: "0.2.0",
      dependencies: []
    }, null, 2)}\n`;
    await fs.writeFile(path.join(packageRoot, "extension.json"), manifest);
    await fs.writeFile(path.join(payloadRoot, "current.txt"), "current\n");
    await fs.writeFile(path.join(payloadRoot, "legacy.txt"), "legacy\n");

    expect((await runCli("install", root)).exitCode).toBe(0);
    expect((await runCliWithEnv("extend", ["evolving-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot })).exitCode).toBe(0);
    expect(await exists(path.join(root, "legacy.txt"))).toBe(true);

    await fs.rm(path.join(payloadRoot, "legacy.txt"));
    const updated = await runCliWithEnv("extend", ["evolving-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });

    expect(updated.exitCode).toBe(0);
    expect(updated.stdout).toContain("deleted 1");
    expect(await exists(path.join(root, "legacy.txt"))).toBe(false);
    expect(await fs.readFile(path.join(root, "current.txt"), "utf8")).toBe("current\n");
    const receipt = JSON.parse(await fs.readFile(path.join(root, "open-forge.extensions.json"), "utf8")) as {
      schema: number;
      extensions: Record<string, { files: string[] }>;
      files: Record<string, unknown>;
    };
    expect(receipt.schema).toBe(2);
    expect(receipt.extensions["evolving-pack"].files).toEqual(["current.txt"]);
    expect(receipt.extensions["evolving-pack"]).not.toHaveProperty("augmentations");
    expect(receipt.files["legacy.txt"]).toBeUndefined();
  });

  test("blocks dependent removal and locally modified owned files", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "base-routing", "Base Routing", "Base routing extension");
    await createDependencyPack(extensionsRoot, "dependent-pack", "Dependent Pack", "Needs base routing", ["base-routing"]);
    expect((await runCli("install", root)).exitCode).toBe(0);
    expect((await runCliWithEnv("extend", ["dependent-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot })).exitCode).toBe(0);

    const blocked = await runCliWithEnv("extend", ["--remove", "base-routing", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });
    expect(blocked.exitCode).toBe(1);
    expect(blocked.stderr).toContain("dependent-pack still depends");

    expect((await runCliWithEnv("extend", ["--remove", "dependent-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot })).exitCode).toBe(0);
    const ownedFile = path.join(root, ".agents", "patterns", "base-routing", "_base-routing.md");
    await fs.appendFile(ownedFile, "\nLocally changed.\n");
    const modified = await runCliWithEnv("extend", ["--remove", "base-routing", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });
    expect(modified.exitCode).toBe(1);
    expect(modified.stderr).toContain("was modified or removed outside Open Forge");
    expect(await exists(path.join(root, "open-forge.extensions.json"))).toBe(true);

    await createBundledExtension(extensionsRoot, "unrelated-pack", "Unrelated Pack", "Unrelated extension");
    const unrelated = await runCliWithEnv("extend", ["unrelated-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });
    expect(unrelated.exitCode).toBe(1);
    expect(unrelated.stderr).toContain("Owned extension file .agents/patterns/base-routing/_base-routing.md was modified");
    expect(await exists(path.join(root, ".agents", "patterns", "unrelated-pack", "_unrelated-pack.md"))).toBe(false);
  });

  test("tracks shared file owners and refuses removal after an owned file is modified", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    const sharedRelative = path.join("payload", ".agents", "patterns", "shared", "rule.md");
    for (const id of ["alpha-owner", "beta-owner"]) {
      const packageRoot = path.join(extensionsRoot, id);
      await fs.mkdir(path.dirname(path.join(packageRoot, sharedRelative)), { recursive: true });
      await fs.writeFile(path.join(packageRoot, "extension.json"), JSON.stringify({ id, name: id, dependencies: [] }));
      await fs.writeFile(path.join(packageRoot, sharedRelative), "# Shared Rule\n");
    }
    expect((await runCli("install", root)).exitCode).toBe(0);
    expect((await runCliWithEnv("extend", ["--ids=alpha-owner,beta-owner", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot })).exitCode).toBe(0);
    const sharedTarget = path.join(root, ".agents", "patterns", "shared", "rule.md");
    let receipt = JSON.parse(await fs.readFile(path.join(root, "open-forge.extensions.json"), "utf8")) as { files: Record<string, { owners: string[] }> };
    expect(receipt.files[".agents/patterns/shared/rule.md"].owners).toEqual(["alpha-owner", "beta-owner"]);

    expect((await runCliWithEnv("extend", ["--remove", "alpha-owner", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot })).exitCode).toBe(0);
    expect(await exists(sharedTarget)).toBe(true);
    receipt = JSON.parse(await fs.readFile(path.join(root, "open-forge.extensions.json"), "utf8")) as { files: Record<string, { owners: string[] }> };
    expect(receipt.files[".agents/patterns/shared/rule.md"].owners).toEqual(["beta-owner"]);

    await fs.writeFile(sharedTarget, "# Locally Modified Shared Rule\n");
    const modified = await runCliWithEnv("extend", ["--remove", "beta-owner", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });
    expect(modified.exitCode).toBe(1);
    expect(modified.stderr).toContain("modified or removed outside Open Forge");
    expect(await exists(sharedTarget)).toBe(true);
  });

  test("fails closed on duplicate and cross-inconsistent ownership receipts", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "managed-pack", "Managed Pack", "Managed extension");
    expect((await runCliWithEnv("extend", ["managed-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot })).exitCode).toBe(0);
    const receiptFile = path.join(root, "open-forge.extensions.json");
    const validReceipt = JSON.parse(await fs.readFile(receiptFile, "utf8")) as {
      roots: string[];
      files: Record<string, { owners: string[] }>;
    };
    const managedTarget = path.join(root, ".agents", "patterns", "managed-pack", "_managed-pack.md");

    validReceipt.roots.push("managed-pack");
    await fs.writeFile(receiptFile, `${JSON.stringify(validReceipt, null, 2)}\n`);
    const duplicate = await runCliWithEnv("extend", ["--remove", "managed-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });
    expect(duplicate.exitCode).toBe(1);
    expect(duplicate.stderr).toContain("roots contains duplicate managed-pack");
    expect(await exists(managedTarget)).toBe(true);

    validReceipt.roots = ["managed-pack"];
    validReceipt.files[".agents/patterns/managed-pack/_managed-pack.md"].owners.push("ghost-owner");
    await fs.writeFile(receiptFile, `${JSON.stringify(validReceipt, null, 2)}\n`);
    const inconsistent = await runCliWithEnv("extend", ["--remove", "managed-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });
    expect(inconsistent.exitCode).toBe(1);
    expect(inconsistent.stderr).toContain("owner ghost-owner is not recorded");
    expect(await exists(managedTarget)).toBe(true);
  });

  test("migrates empty schema-1 augmentation arrays but rejects retained legacy augmentation state", async () => {
    const root = await createRoot();
    const extensionsRoot = await createRoot();
    await createBundledExtension(extensionsRoot, "legacy-pack", "Legacy Pack", "Legacy receipt fixture");
    const receiptFile = path.join(root, "open-forge.extensions.json");

    expect((await runCliWithEnv("extend", ["legacy-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot })).exitCode).toBe(0);
    let receipt = JSON.parse(await fs.readFile(receiptFile, "utf8")) as {
      schema: number;
      extensions: Record<string, Record<string, unknown>>;
    };
    receipt.schema = 1;
    receipt.extensions["legacy-pack"].augmentations = [];
    await fs.writeFile(receiptFile, `${JSON.stringify(receipt, null, 2)}\n`);

    const removed = await runCliWithEnv("extend", ["--remove", "legacy-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });
    expect(removed.exitCode).toBe(0);
    expect(await exists(receiptFile)).toBe(false);

    expect((await runCliWithEnv("extend", ["legacy-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot })).exitCode).toBe(0);
    receipt = JSON.parse(await fs.readFile(receiptFile, "utf8"));
    receipt.schema = 1;
    receipt.extensions["legacy-pack"].augmentations = [{
      target: ".agents/loader.md",
      sha256: "a".repeat(64)
    }];
    await fs.writeFile(receiptFile, `${JSON.stringify(receipt, null, 2)}\n`);

    const rejected = await runCliWithEnv("extend", ["--remove", "legacy-pack", root], { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot });
    expect(rejected.exitCode).toBe(1);
    expect(rejected.stderr).toContain("Legacy extension receipt contains augmentation state for legacy-pack");
    expect(await exists(path.join(root, ".agents", "patterns", "legacy-pack", "_legacy-pack.md"))).toBe(true);
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
    await fs.writeFile(path.join(extensionsRoot, "feature-pack", "extension.json"), JSON.stringify({ id: "feature-pack", name: "Feature", dependencies: "shared-pack" }));

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
    await fs.writeFile(path.join(extensionsRoot, "feature-pack", "extension.json"), JSON.stringify({ id: "feature-pack", name: null, dependencies: null }));

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

  test("classifies Windows case-folded root entry files as baseline-loading", async () => {
    if (process.platform !== "win32") {
      return;
    }

    const root = await createRoot();
    const extension = await createRoot();
    const payload = path.join(extension, "payload");
    await fs.mkdir(payload, { recursive: true });
    await fs.writeFile(path.join(payload, "agents.md"), "new entrypoint\n");
    await fs.writeFile(path.join(payload, "claude.md"), "new bridge\n");

    const result = await runCli("extend", extension, root, "--dry-run");

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("Scope review: 0 routed, 2 baseline-loading, 0 skill-executable, 0 outside-.agents files.");
    expect(result.stdout).toContain("- create agents.md");
    expect(result.stdout).toContain("- create claude.md");
    expect(await exists(path.join(root, "agents.md"))).toBe(false);
    expect(await exists(path.join(root, "claude.md"))).toBe(false);
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

  test("patches canonical and bridged root entries without replacing workspace instructions", async () => {
    const root = await createRoot();
    const originalAgents = `# Workspace Agents

Keep this agent instruction.

<!-- open-forge:start -->
Old Open Forge entry.
<!-- open-forge:end -->
`;
    const originalClaude = `# Claude Code

Keep this Claude-specific instruction.
`;
    await fs.writeFile(path.join(root, "AGENTS.md"), originalAgents);
    await fs.writeFile(path.join(root, "CLAUDE.md"), originalClaude);
    await initializeGitRepository(root);
    await commitAll(root, "Workspace instructions");

    const installed = await runCliDefault("install", root);

    expect(installed.exitCode).toBe(0);
    const agents = await fs.readFile(path.join(root, "AGENTS.md"), "utf8");
    const claude = await fs.readFile(path.join(root, "CLAUDE.md"), "utf8");
    const sourceAgents = await fs.readFile(repoPath("src", "open-forge", "AGENTS.md"), "utf8");
    const dogfoodAgents = await fs.readFile(repoPath("AGENTS.md"), "utf8");
    const sourceClaude = await fs.readFile(repoPath("src", "open-forge", "CLAUDE.md"), "utf8");
    const dogfoodClaude = await fs.readFile(repoPath("CLAUDE.md"), "utf8");
    const managedBlockPattern = /<!--\s*open-forge:start\s*-->[\s\S]*?<!--\s*open-forge:end\s*-->/;
    const sourceAgentsBlock = sourceAgents.match(managedBlockPattern)?.[0];
    const dogfoodAgentsBlock = dogfoodAgents.match(managedBlockPattern)?.[0];
    const sourceClaudeBlock = sourceClaude.match(managedBlockPattern)?.[0];
    const dogfoodClaudeBlock = dogfoodClaude.match(managedBlockPattern)?.[0];
    expect(agents).toContain("Keep this agent instruction.");
    expect(agents).toContain("Open Forge is the operating contract for this workspace.");
    expect(agents).toContain(
      "Before acting on any task, you must read `.agents/loader.md` and follow all applicable Open Forge rules and conventions throughout the task.",
    );
    expect(agents).not.toContain("Old Open Forge entry.");
    expect(dogfoodAgentsBlock).toBe(sourceAgentsBlock);
    expect(dogfoodClaudeBlock).toBe(sourceClaudeBlock);
    expect(claude.startsWith(originalClaude)).toBe(true);
    expect(claude.match(managedBlockPattern)?.[0]).toBe(sourceClaudeBlock);
    expect(claude.match(/@AGENTS\.md/g) ?? []).toHaveLength(1);
    expect(agents.match(/<!-- open-forge:start -->/g) ?? []).toHaveLength(1);
    expect(claude.match(/<!-- open-forge:start -->/g) ?? []).toHaveLength(1);
    expect(claude.match(/<!-- open-forge:end -->/g) ?? []).toHaveLength(1);

    await commitAll(root, "Install Core");
    const reinstalled = await runCliDefault("install", root);

    expect(reinstalled.exitCode).toBe(0);
    expect(reinstalled.stdout).toContain("Git reports no target changes; no new commit is needed");
    expect(await fs.readFile(path.join(root, "CLAUDE.md"), "utf8")).toBe(claude);
  });

  test("rejects malformed or duplicate managed root entry markers before mutation", async () => {
    const invalidEntries = [
      ["AGENTS.md", "<!-- open-forge:start -->\nIncomplete block.\n"],
      ["CLAUDE.md", "<!-- open-forge:end -->\n@AGENTS.md\n<!-- open-forge:start -->\n"],
      [
        "CLAUDE.md",
        "<!-- open-forge:start -->\n@AGENTS.md\n<!-- open-forge:end -->\n<!-- open-forge:start -->\n@AGENTS.md\n<!-- open-forge:end -->\n",
      ],
    ] as const;

    for (const [fileName, original] of invalidEntries) {
      const root = await createRoot();
      await fs.writeFile(path.join(root, fileName), original);
      await initializeGitRepository(root);
      await commitAll(root, `Malformed ${fileName}`);

      const result = await runCliDefault("install", root);

      expect(result.exitCode).toBe(1);
      expect(result.stderr).toContain(`Target ${fileName} must contain either no open-forge markers or exactly one complete ordered marker pair`);
      expect(await fs.readFile(path.join(root, fileName), "utf8")).toBe(original);
      expect(await exists(path.join(root, ".agents"))).toBe(false);
      expect((await gitCommand(root, "status", "--porcelain=v1")).stdout).toBe("");
    }
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
    expect(await fs.readFile(path.join(target, "CLAUDE.md"), "utf8")).toContain("@AGENTS.md");
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
    expect(result.stdout).toContain("- [Alpha guide](guides/alpha.md) - #Guide #KeepInMind");
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

  test("follows legacy workspace-relative required routes and reports missing ones as blockers", async () => {
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

  test("follows canonical Required Route links relative to their Markdown owner", async () => {
    const root = await createFindRoot();
    const workflows = await createCategory(root, "workflows", "# Workflows\n");
    const delivery = path.join(workflows, "delivery");
    const skillFolder = path.join(root, "skills", "helper kit");
    const balancedSkillFolder = path.join(root, "skills", "helper(v2)");
    const escapedSkillFolder = path.join(root, "skills", "helper(v3)");
    const escapedHashFile = path.join(root, "skills", "helper#draft.md");
    await fs.mkdir(delivery, { recursive: true });
    await fs.mkdir(skillFolder, { recursive: true });
    await fs.mkdir(balancedSkillFolder, { recursive: true });
    await fs.mkdir(escapedSkillFolder, { recursive: true });
    await createCategory(root, "skills", "# Skills\n");
    await fs.writeFile(path.join(skillFolder, "SKILL.md"), "# Helper kit\n");
    await fs.writeFile(path.join(balancedSkillFolder, "SKILL.md"), "# Balanced helper\n");
    await fs.writeFile(path.join(escapedSkillFolder, "SKILL.md"), "# Escaped helper\n");
    await fs.writeFile(escapedHashFile, "# Escaped hash helper\n");
    await fs.writeFile(path.join(delivery, "ship.md"), `# Ship

## Required Routes

- [Helper kit](../../skills/helper%20kit/SKILL.md) - #Skill #Required
- [Use [draft] helper](../../skills/helper(v2)/SKILL.md) - #Skill #Required
- [Use escaped helper](../../skills/helper\\(v3\\)/SKILL.md) - #Skill #Required
- [Use escaped hash helper](../../skills/helper\\#draft.md) - #Document #Required

## Steps

1. Ship it.
`);
    await runCli("index", root);

    const followed = await runCli("find", "--route", "workflows/delivery/ship.md", "--follow-required", "--paths", root);
    expect(followed.stderr).toBe("");
    expect(followed.exitCode).toBe(0);
    expect(followed.stdout).toContain("workflows/delivery/ship.md");
    expect(followed.stdout).toContain("skills/helper kit/SKILL.md");
    expect(followed.stdout).toContain("skills/helper(v2)/SKILL.md");
    expect(followed.stdout).toContain("skills/helper(v3)/SKILL.md");
    expect(followed.stdout).toContain("skills/helper#draft.md");

    const escapedCliRoute = await runCli("find", "--route", "../outside.md", "--paths", root);
    expect(escapedCliRoute.exitCode).toBe(1);
    expect(escapedCliRoute.stderr).toContain("must not escape or change the workspace path");
  });

  test("blocks invalid and empty Required Routes sections instead of silently skipping them", async () => {
    const root = await createFindRoot();
    const workflows = await createCategory(root, "workflows", "# Workflows\n");
    await fs.writeFile(path.join(workflows, "invalid.md"), `# Invalid

## Required Routes

- [Missing tags](../skills/helper/SKILL.md)
`);
    await fs.writeFile(path.join(workflows, "empty.md"), `# Empty

## Required Routes

Routes will be selected later.
`);

    const invalid = await runCli("find", "--route", "workflows/invalid.md", "--follow-required", "--paths", root);
    expect(invalid.exitCode).toBe(1);
    expect(invalid.stderr).toContain("blocker, not a skip");
    expect(invalid.stderr).toContain("invalid Required Routes line");

    const empty = await runCli("find", "--route", "workflows/empty.md", "--follow-required", "--paths", root);
    expect(empty.exitCode).toBe(1);
    expect(empty.stderr).toContain("blocker, not a skip");
    expect(empty.stderr).toContain("has no parseable routes and does not state none");
  });

  test("blocks encoded parent traversal outside the target from Required Route links", async () => {
    const root = await createFindRoot();
    const workflows = await createCategory(root, "workflows", "# Workflows\n");
    await fs.writeFile(path.join(workflows, "escape.md"), `# Escape

## Required Routes

- [Outside](%2E%2E/%2E%2E/outside.md) - #Required

## Steps

1. Do not escape.
`);
    await runCli("index", root);

    const followed = await runCli("find", "--route", "workflows/escape.md", "--follow-required", "--paths", root);
    expect(followed.exitCode).toBe(1);
    expect(followed.stderr).toContain("blocker");

    const diagnosed = await runCli("doctor", root);
    expect(diagnosed.exitCode).toBe(1);
    expect(diagnosed.stdout).toContain("resolves outside the target workspace");
  });

  test("blocks a Required Route link that escapes physically through a junction or symlink", async () => {
    const root = await createFindRoot();
    const outside = await createRoot();
    const workflows = await createCategory(root, "workflows", "# Workflows\n");
    await fs.writeFile(path.join(outside, "outside.md"), "# Outside\n");
    try {
      await fs.symlink(outside, path.join(root, "linked-content"), process.platform === "win32" ? "junction" : "dir");
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "EPERM" || (error as NodeJS.ErrnoException).code === "EACCES") return;
      throw error;
    }
    await fs.writeFile(path.join(workflows, "linked.md"), `# Linked

## Required Routes

- [Outside](../linked-content/outside.md) - #Required

## Steps

1. Stay inside.
`);

    const followed = await runCli("find", "--route", "workflows/linked.md", "--follow-required", "--paths", root);
    expect(followed.exitCode).toBe(1);
    expect(followed.stderr).toContain("blocker");
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
      "- [Fenced example only](../skills/missing/SKILL.md) - #Skill",
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

describe("load command", () => {
  const routedDocument = (title: string, tags: string[], entries: string[] = []): string => `---
open-forge:
  description: ${title} fixture
  tags: [${tags.join(", ")}]
---

# ${title}

## Axioms

- ${title} axiom.

## Entries

<!-- open-forge:generated-index:start -->
${entries.length > 0 ? entries.join("\n") : "- none - No entries - #Empty"}
<!-- open-forge:generated-index:end -->
`;

  test("keeps legacy .agents-prefixed generated routes readable during migration", async () => {
    const root = await createRoot();
    const agents = path.join(root, ".agents");
    const directives = path.join(agents, "directives");
    await fs.mkdir(directives, { recursive: true });
    await fs.writeFile(path.join(agents, "loader.md"), routedDocument("Loader", ["Core"], [
      "- `.agents/directives/_directives.md` - Legacy root route - #LoadNow #Directive"
    ]));
    await fs.writeFile(path.join(directives, "_directives.md"), routedDocument("Directives", ["LoadNow", "Directive"]));

    const loaded = await runCli("load", "--paths", root);
    expect(loaded.exitCode).toBe(0);
    expect(loaded.stdout.trim().split(/\r?\n/)).toEqual([
      ".agents/loader.md",
      ".agents/directives/_directives.md"
    ]);
  });

  test("emits loader-first visible LoadNow routes, adjacent overwrites, and complete KeepInMind context", async () => {
    const root = await createRoot();
    const agents = path.join(root, ".agents");
    const directives = path.join(agents, "directives");
    const nested = path.join(directives, "nested");
    const memory = path.join(agents, "memory");
    await fs.mkdir(nested, { recursive: true });
    await fs.mkdir(memory, { recursive: true });

    const loader = path.join(agents, "loader.md");
    const directivesIndex = path.join(directives, "_directives.md");
    await fs.writeFile(loader, routedDocument("Loader", ["Core"], [
      "- [Directives fixture](directives/_directives.md) - #LoadNow #Directive",
      "- [Evergreen current view](current-view.md) - #CurrentTruth #Evergreen"
    ]));
    await fs.writeFile(path.join(agents, "loader.overwrite.md"), "# Loader local overwrite\n");
    await fs.writeFile(path.join(agents, "current-view.md"), routedDocument("Current View", ["CurrentTruth", "Evergreen"]));
    await fs.writeFile(directivesIndex, routedDocument("Directives", ["LoadNow", "Directive"], [
      "- [Root directive](root-rule.md) - #LoadNow #Directive",
      "- [Visible and remembered directive](duplicate.md) - #LoadNow #KeepInMind #Directive",
      "- [On-demand nested directives](nested/_nested.md) - #Directive"
    ]));
    await fs.writeFile(path.join(directives, "root-rule.md"), routedDocument("Root Rule", ["LoadNow", "Directive"]));
    await fs.writeFile(path.join(directives, "root-rule.overwrite.md"), "# Root rule local overwrite\n");
    await fs.writeFile(path.join(directives, "duplicate.md"), routedDocument("Duplicate", ["LoadNow", "KeepInMind", "Directive"]));
    await fs.writeFile(path.join(nested, "_nested.md"), routedDocument("Nested Directives", ["Directive"], [
      "- [Nested direct directive](child-rule.md) - #LoadNow #Directive"
    ]));
    await fs.writeFile(path.join(nested, "child-rule.md"), routedDocument("Child Rule", ["LoadNow", "Directive"]));
    await fs.writeFile(path.join(memory, "_memory.md"), routedDocument("Memory", ["KeepInMind", "Memory"], [
      "- [Remembered detail](detail.md) - #LoadNow #Memory"
    ]));
    await fs.writeFile(path.join(memory, "detail.md"), routedDocument("Memory Detail", ["LoadNow", "Memory"]));

    const initial = await runCli("load", "--paths", root);
    expect(initial.exitCode).toBe(0);
    expect(initial.stdout.trim().split(/\r?\n/)).toEqual([
      ".agents/loader.md",
      ".agents/loader.overwrite.md",
      ".agents/directives/_directives.md",
      ".agents/directives/root-rule.md",
      ".agents/directives/root-rule.overwrite.md",
      ".agents/directives/duplicate.md",
      ".agents/memory/_memory.md",
      ".agents/memory/detail.md"
    ]);
    expect(initial.stdout).not.toContain("nested/_nested.md");
    expect(initial.stdout).not.toContain("current-view.md");
    expect(initial.stdout.match(/duplicate\.md/g)).toHaveLength(1);

    const exposedParent = (await fs.readFile(directivesIndex, "utf8"))
      .replace("#Directive\n<!-- open-forge:generated-index:end -->", "#LoadNow #Directive\n<!-- open-forge:generated-index:end -->");
    await fs.writeFile(directivesIndex, exposedParent);
    const withNestedParent = await runCli("load", "--json", root);
    const items = JSON.parse(withNestedParent.stdout) as Array<{ route: string; kind: string; companionOf?: string }>;
    const routes = items.map((item) => item.route);
    expect(withNestedParent.exitCode).toBe(0);
    expect(routes.indexOf(".agents/directives/nested/_nested.md")).toBeGreaterThan(routes.indexOf(".agents/directives/duplicate.md"));
    expect(routes.indexOf(".agents/directives/nested/child-rule.md")).toBe(routes.indexOf(".agents/directives/nested/_nested.md") + 1);
    expect(items.find((item) => item.route === ".agents/loader.overwrite.md")).toMatchObject({
      kind: "overwrite",
      companionOf: ".agents/loader.md"
    });
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

  test("warns when a linked Required Route omits its canonical tag suffix", async () => {
    const root = await createRoot();
    const guides = await createCategory(root, "guides", "# Guides\n");
    await fs.writeFile(path.join(guides, "guide.md"), `# Guide

## Required Routes

- [Incomplete](other.md)
`);
    await fs.writeFile(path.join(guides, "other.md"), "# Other\n");
    await runCli("index", root);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("Required Routes line is not in entry format: - [Incomplete](other.md)");
  });

  test("enforces ordered workflow Mode and always-present Constraints", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const workflow = path.join(root, ".agents", "workflows", "invalid.md");
    await fs.writeFile(workflow, `---
open-forge:
  description: Invalid workflow fixture
  tags: [Workflow, PhaseDelivery]
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
  tags: [Workflow, PhaseDelivery]
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

  test("requires exactly one known primary phase on every workflow recipe", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const workflows = path.join(root, ".agents", "workflows");
    const recipe = (tags: string) => `---
open-forge:
  description: Workflow phase fixture
  tags: [${tags}]
---

# Phase Fixture

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

Execute once.

## Outputs

- result

## Completion

- [ ] result exists
`;
    await fs.writeFile(path.join(workflows, "missing-phase.md"), recipe("Workflow"));
    await fs.writeFile(path.join(workflows, "multiple-phase.md"), recipe("Workflow, PhasePlanning, PhaseDelivery, PhaseUnknown"));
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout.match(/workflow recipe must declare exactly one phase tag/g)).toHaveLength(2);
    expect(result.stdout).toContain("workflow recipe has unknown phase tag(s): PhaseUnknown");
  });

  test("rejects workflow contract sections that are not level-2 headings", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const workflow = path.join(root, ".agents", "workflows", "wrong-level.md");
    await fs.writeFile(workflow, [
      "---",
      "open-forge:",
      "  description: Wrong heading level workflow fixture",
      "  tags: [Workflow, PhaseDelivery]",
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
  tags: [Workflow, PhaseDelivery]
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

  test("rejects legacy directive applicability gates", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const directive = path.join(root, ".agents", "directives", "fixture.md");
    await fs.writeFile(directive, `---
open-forge:
  description: Directive with a legacy second scope decision
  tags: [Directive]
---

# Fixture

## Applies To

- workspace-wide

## Axioms

- Do the thing.
`);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("directive scope belongs to routing; remove the legacy Applies To section");
  });

  test("requires direct directive files to declare LoadNow for parent-relative activation", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const directive = path.join(root, ".agents", "directives", "activation.md");
    await fs.writeFile(directive, `---
open-forge:
  description: Explicit direct directive activation fixture
  tags: [Directive]
---

# Activation

## Axioms

- Keep direct activation explicit.
`);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const missing = await runCli("doctor", root);
    expect(missing.exitCode).toBe(1);
    expect(missing.stdout).toContain("direct directive must declare #LoadNow so its loaded parent activates it explicitly");

    const activated = (await fs.readFile(directive, "utf8")).replace("tags: [Directive]", "tags: [LoadNow, Directive]");
    await fs.writeFile(directive, activated);
    expect((await runCli("index", root)).exitCode).toBe(0);
    const healthy = await runCli("doctor", "--json", root);
    expect(healthy.exitCode).toBe(0);
    expect(JSON.parse(healthy.stdout)).toMatchObject({ errors: 0, warnings: 0 });
  });

  test("requires substantive level-2 Axioms on direct directive files", async () => {
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
## Axioms

- This fenced example is not a directive contract.
\`\`\`
~~~
`);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("directive must declare exactly one non-empty Axioms section");
  });

  test("infers workflow and directive validation from routed category ancestry", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    await fs.writeFile(path.join(root, ".agents", "workflows", "untagged.md"), "# Untagged Workflow\n\n## Steps\n\n1. Run.\n");
    await fs.writeFile(path.join(root, ".agents", "directives", "tag-bypass.md"), `---
open-forge:
  description: A direct directive whose topical tag cannot bypass route ownership
  tags: [Pattern]
---

# Route-owned Directive

No binding axioms were declared.
`);
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("workflow must define exactly one Mode section");
    expect(result.stdout).toContain("directive must declare exactly one non-empty Axioms section");
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
      "  tags: [Workflow, PhaseDelivery]",
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
      "No binding axioms were declared.",
      ""
    ].join("\n"));
    expect((await runCli("index", root)).exitCode).toBe(0);

    const result = await runCli("doctor", root);

    expect(result.exitCode).toBe(1);
    expect(result.stdout).toContain("workflow must define exactly one Mode section");
    expect(result.stdout).toContain("directive must declare exactly one non-empty Axioms section");
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

  test("treats Workflow and Directive as topical tags inside Templates", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    const topical = path.join(root, ".agents", "templates", "workflow.md");
    await fs.writeFile(topical, [
      "---",
      "open-forge:",
      "  description: Copy-ready source for a workflow document",
      "  tags: [Template, Workflow, Directive]",
      "---",
      "",
      "# Workflow Template",
      "",
      "This is source content, not an active workflow or directive.",
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
    expect(parent).toContain("- [TODO - when to select this route and what it provides](react/_react.md) - #Pattern");

    const doctorResult = await runCli("doctor", root);
    expect(doctorResult.exitCode).toBe(0);
  });

  test("classifies scoped Template categories", async () => {
    const root = await createRoot();
    await runCli("install", root);

    const result = await runCli("create", "category", "templates/documents", root);
    const documents = await fs.readFile(path.join(root, ".agents", "templates", "documents", "_documents.md"), "utf8");

    expect(result.exitCode).toBe(0);
    expect(documents).toContain("tags: [Template]");
  });

  test("refuses an already routable category", async () => {
    const root = await createRoot();
    await runCli("install", root);

    const result = await runCli("create", "category", "patterns", root);

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("already routable");
  });

  test("scaffolds an extension package", async () => {
    const scaffoldRoot = await createRoot();

    const result = await runCli("create", "extension", "my-pack", scaffoldRoot);
    const packageRoot = path.join(scaffoldRoot, "my-pack");
    const metadata = JSON.parse(await fs.readFile(path.join(packageRoot, "extension.json"), "utf8")) as Record<string, unknown>;

    expect(result.exitCode).toBe(0);
    expect(metadata).toEqual({
      id: "my-pack",
      name: "My Pack",
      description: "TODO - one line shown by open-forge extend --list",
      version: "0.1.0",
      dependencies: []
    });
    expect(await fs.readFile(path.join(packageRoot, "README.md"), "utf8")).toContain("open-forge extend my-pack <target>");

    const routeRoot = path.join(packageRoot, "payload", ".agents", "patterns", "my-pack");
    await fs.mkdir(routeRoot, { recursive: true });
    await fs.writeFile(path.join(routeRoot, "_my-pack.md"), `---
open-forge:
  description: Semantic scaffold installation fixture
  tags: [Extension, Pattern]
---

# My Pack

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
`);

    const target = await createRoot();
    expect((await runCli("install", target)).exitCode).toBe(0);
    expect((await runCli("extend", packageRoot, target)).exitCode).toBe(0);
    const doctorResult = await runCli("doctor", "--json", target);
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });
    expect(await fs.readFile(path.join(target, ".agents", "patterns", "_patterns.md"), "utf8")).toContain(
      "[Semantic scaffold installation fixture](my-pack/_my-pack.md) - #Extension #Pattern"
    );
  });
});

async function createRoot(): Promise<string> {
  return sandbox.createDirectory("case");
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
  await fs.writeFile(path.join(root, id, "extension.json"), `${JSON.stringify({ id, name, description, dependencies }, null, 2)}\n`);
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
  await fs.writeFile(path.join(packageRoot, "extension.json"), `${JSON.stringify({ id, name, description, dependencies }, null, 2)}\n`);
  await fs.writeFile(path.join(packageRoot, "README.md"), "Authoring documentation that must not be installed.\n");
}

type CliCommand = "index" | "install" | "extend" | "load" | "find" | "chain" | "doctor" | "create";

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
  return executeCli(cliFile, [command, ...args], { env });
}

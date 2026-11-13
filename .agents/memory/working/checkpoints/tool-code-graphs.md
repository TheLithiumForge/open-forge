---
open-forge:
  description: Resume CodeGraph-only V1 after final decisions, waiting for core writer integration
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Tool, Contextual]
---

# CodeGraph V1

CodeGraph V1 is the issue 23 task that prepares the verified CodeGraph pin, required Skill, consent and agent integration for Open Forge.

The maintainer's final 2026-11-13 decisions replace all earlier graph-installer decisions. Codebase Memory is unsupported in V1. No installer is applied before the incoming core writers are rebased.

Status: Definitions, origin readers, contracts and guidance prepared. Core writer integration and native qualification pending.

## Current scope

Work uses `feature/tool-code-graphs` from `feature/tool-command`. This base has List, Inspect and Status, lifecycle models and an empty production catalogue. It lacks the five writers, Exec and concrete production adapters. We don't create competing implementations while waiting for the rebase. Local commits are authorized. Pushes, merges, upstream contact, full local suites and native builds aren't authorized. CI is the full-suite gate.

Commit `e4534e27a` records CodeGraph-only V1, its draft, required Skill, consent, maintenance research, origin schema and read output, contracts and site pages. The documentation step updates both CLI Skill copies, `docs/cli.md` and the affected Status snapshots. Earlier commits `e6ad8d24a` and `7c76ca5fb` keep the original twelve-asset verification history.

This is local developer tooling that can change project agent files and account-local tool state after consent. Git and sibling backups follow the maintainer's policy. Ordinary defects, interruption and accidental concurrency are in scope. A hostile same-account process is outside the supported limit. We use the existing JSON serializer, static models, ordinary BCL and compiled adapter structure. The accepted exception is exact Codex command-line text edits without a TOML parser. Writers aren't implemented here. Focused Unit and real-filesystem Integration evidence is sufficient for this draft and origin increment. Full managed and native gates run in CI.

## Sources

The [maintenance checklist](../../../../docs/tool-maintenance.md) defines pin review, process settings, verification and native gates. The [CodeGraph draft](../../../../src/tools/drafts/codegraph-1.6.2.json) records all six official archive sizes, SHA-256 values, verified attestation identity, source commits, own-launcher lifecycle arguments and four bounded rewrite locations. Native qualifications are empty and admission stays blocked. The production reader rejects the wrapper.

The [consent inventory](../../../../docs/tools/codegraph-consent.md) lists files, blocks, optional prompt hook, telemetry account state, legacy cleanup and removal. The [CodeGraph V1 design](../../crystallized/documents/cli/technical-designs/codegraph-v1.md) defines command resolution, rewrites and conditional role guidance. The [Exec contract](../../crystallized/documents/cli/contracts/tool/exec/interface.md) defines workspace selection and transparent streams, arguments, exit and signals.

The required Skill is 228 words including metadata. It gives MCP and shell use, first init, rebuild, sync and the C# recall warning. Installation internals moved to contracts and maintenance docs. Codebase Memory has no packaged definition, Skill or public tools page. Its [maintenance research](../../../../docs/tools/codebase-memory-research.md) keeps the six verified archives, Sigstore evidence, account write inventory and missing project-scope and hook controls.

## Verification

The managed Release build passes with zero warnings and errors. Focused evidence passes:

- 32 Unit cases in the touched draft and manifest classes.
- 25 Tool Integration cases in the touched read and draft-isolation classes.
- 4 affected Status cases for the shipped CLI Skill payload.
- Docusaurus production build, with the existing Node localStorage experimental warning.
- Offline schema validation and four rejected mutations for admission, origin, rewrite count and hook shape.
- Skill validator, exact CLI-copy parity, changed Markdown file links and Git whitespace.

Accepted 19 Tool snapshots for origin, wrapped columns and CodeGraph-only help. Accepted 49 Status snapshots because the shipped CLI Skill package shrank by 125 bytes. Only routed byte, character, token and startup-share values changed. Startup counts and behavior stayed unchanged. Rejected the initial stacked table captures and accepted the wrapped-column form after review. The final selected runs have no skips or flakes.

Both required .NET formatting checks pass before each commit. Three test-initializer layout fixes corrected the first failed checks. The earlier restore used the base lane's local `-p:NuGetAudit=false` workaround for the existing ILCompiler 10.0.1 NU1903 advisory. No package pin changed.

The verified Windows archive launcher ran only help for Install, Uninstall, Init, Index, Sync, Uninit, Telemetry and Explore, plus local `--print-config claude` and `--print-config codex`. The initial print-config call omitted its required target argument and was corrected. No installer, index builder or telemetry-setting command was applied. No temporary native journey repository was created. PATH and registry were unchanged.

## Reproduce checks

From the repository root, run these focused commands:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.ToolDraftDefinitionTests OpenForge.Cli.Core.UnitTests.Framework.Tools.ToolManifestTests --minimum-expected-tests 32 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.ToolDraftIsolationTests OpenForge.Cli.IntegrationTests.Commands.Tool.ToolReadCommandsTests --minimum-expected-tests 25 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-method OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests.HealthyWithExtension OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests.InstalledWorkspace OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests.RecoveryBundlePresent --minimum-expected-tests 4 --fail-warns on --fail-skips on --no-ansi --progress off
npm --prefix src/docusaurus run build
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

The offline schema check uses the site's installed Ajv package after `npm ci --prefix src/docusaurus`:

```javascript
import fs from "node:fs";
import assert from "node:assert/strict";
import Ajv from "./src/docusaurus/node_modules/ajv/dist/2020.js";
const read = (path) => JSON.parse(fs.readFileSync(path, "utf8"));
const ajv = new Ajv({ strict: false });
ajv.addSchema(read("schemas/tools/v1/manifest.schema.json"));
const validate = ajv.compile(read("schemas/tools/drafts/v1/definition.schema.json"));
const draft = read("src/tools/drafts/codegraph-1.6.2.json");
assert.equal(validate(draft), true);
for (const change of [
  (value) => (value.definition.admission = "enabled"),
  (value) => (value.definition.origin = "other"),
  (value) => value.registrationDraft.rewrites.pop(),
  (value) => (value.registrationDraft.promptHook.default = false),
]) {
  const invalid = structuredClone(draft);
  change(invalid);
  assert.equal(validate(invalid), false);
}
```

This prints no output on success and rejects the four invalid mutations. Local disposable reports are under `artifacts/tool-code-graphs/`. They aren't required shipped inputs.

## Next work

After the maintainer rebases the core writers, port the reviewed draft metadata into their typed definition model and compiled adapter. Implement the one launch-command resolver, exact command rewrites, hook choice, telemetry command, required Skill and conditional active-role line. Use native server name `codegraph` for passive checks. Keep all unrelated members and markers.

Then choose bounded timeouts from native evidence and run the temporary Windows x64 Git journey under D:, with project-only Claude Code and Codex. Verify download, hashes, installation, Status, first Init, refresh and removal. Check that native uninstall recognizes the rewritten hook. Inventory every agreed leftover. The other five targets need CI journeys before activation. All product decisions are resolved. No new maintainer choice is requested.

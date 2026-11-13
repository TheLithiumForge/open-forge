---
open-forge:
  description: Resume issue 23 graph tools from verified drafts after installer decisions
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Tool, Contextual]
---

# Graph tools

Graph tools is the issue 23 task that prepares CodeGraph and Codebase Memory definitions, required Skills, consent and public documentation.

The user accepted the R1 and R2 stop, took both questions to the maintainer and authorized independent draft preparation. No tool installer, including dry-run installation, is allowed in this continuation.

Status: Draft preparation complete. R1 and R2 pending.

## Current scope

Work uses `feature/tool-code-graphs`, based on `489e785d6` from `feature/tool-command`. The base has read commands and lifecycle models. Its five writers and production adapters are pending. This task keeps their contracts. Local commits are authorized. Pushes, merges, full local suites and native builds aren't authorized. CI is the full-suite gate.

The [maintenance checklist](../../../../docs/tool-maintenance.md) carries admission steps, both worked examples and the passive proposal. The [draft definitions](../../../../src/tools/README.md) keep pending lifecycle commands outside the production manifest. The production catalogue and allow-list stay empty. Production input rejects the wrappers. All twelve targets stay blocked, with empty native qualification.

Commit `e6ad8d24a` covers definitions, schema, checksum metadata, required Skills, consent, maintenance and focused tests. The second step covers public pages, help examples, both CLI Skill copies and this checkpoint. No upstream program bytes are redistributed.

## Release evidence

All twelve selected official archives pass byte-size and published SHA-256 checks. All six CodeGraph build attestations verify against the exact `release.yml@refs/heads/main` identity. All six Codebase Memory individual Sigstore bundles and its checksum bundle verify. Both issuers are `https://token.actions.githubusercontent.com`. Exact URLs, sizes, hashes, identities and verification command arrays are kept in the definitions.

CodeGraph pins `v1.6.2` at `6560052a6f856855d3f71eee838fd66ccfa4285d`. Its attested workflow source is `0f0af7bf22a5083a47e34fcb1e1bf67355174ef8`. The workflow makes commits before publishing. The draft keeps both commits and asserts no equivalence. Codebase Memory pins `v0.12.1` at `a7158ac2f48ca646b4527341b76f930f2aeff87e`. Its blob signatures don't provide source-build provenance.

Raw reports and downloads are disposable under `artifacts/tool-code-graphs/`. Tests use tracked checksum metadata and packaged drafts without network. No temporary journey repository was created. No installer was applied. Host configuration, PATH and registry are unchanged.

## Installer decisions

R1 is the verified Codebase Memory rejection of `install --no-hooks --dry-run -y`, exit 1. Its source has no project-scope, hook-disable or instruction-file opt-out flag. `--skip-config` skips setup. `--clients=claude,codex` limits clients. `--skip-binary` skips placement and installation's PATH edit. Uninstall has no client selection and can edit the Windows user PATH. `--delete-indexes` deletes every project index in the selected cache. Install and Uninstall argument arrays stay null.

R2 is CodeGraph's noninteractive Claude prompt hook. `--no-permissions` doesn't disable it. Its installer writes a bare `codegraph` launcher without the declared child environment. Install arguments stay null. Uninstall with `--keep-cli` and exact-index `uninit` have source-reviewed forms, with timeouts and native tests pending.

The consent inventories list the requested clients' files, hooks, Skills, profiles, config entries, legacy cleanup, temporary files and account effects. Codebase Memory's wider cleanup still needs actual resolved destinations before consent can be actionable. Required Open Forge Skills are drafted outside active native Skills. Tool-owned delivery and the private CodeGraph runtime policy need decisions.

The passive proposal uses native names `codegraph` and `codebase-memory-mcp`. JSON uses the standard reader. Unparsed Codex TOML header presence means only `present-unverified`. No host writer or TOML parser is added. The current core selector validator is unchanged.

## Verification

The final managed Release solution build passes with zero warnings and errors. Unit passes 6/6. Integration passes 7/7, consisting of 2 draft-isolation cases, 1 help case and 4 installed-workspace Status cases. All have zero skips. Run the class and method filters separately because combining them intersects the selection. No EndToEnd, complete managed or native suite ran.

The accepted snapshot is `ToolReadCommandsTests/HelpStopsBeforeWorkspace/inspect.help.txt`. Its only change replaces generic examples with both pinned graph inspection forms. The original received file is under `artifacts/command-output-snapshots/2643577d482a4fb6a1b5da774ab4d90f/ToolReadCommandsTests/HelpStopsBeforeWorkspace/`.

Review also accepts 56 Status captures from `StatusBeforeOutputSnapshotTests.HealthyWithExtension`, `InstalledWorkspace` and `RecoveryBundlePresent`. The changes are only routed byte, character and token measurements and the resulting startup share. The shipped CLI Skill package grows by 1344 bytes and 336 estimated tokens. Duplicate captures agree. The source run is `335b179f20ca4d9bb02b6f3046c927fc`. Verification passes all four cases. Together with Inspect help, 57 snapshots change.

Ajv 2020 compiles the draft schema. Both drafts validate. Supplying runnable pending arguments or enabling admission fails. Both required Skills pass the skill-creator validator. Docusaurus builds successfully, with its existing Node localStorage warning. The CLI Skill copies match. Prettier, Git whitespace and scoped Index complete the document checks. Both required .NET checks pass before each commit.

The initial restore hit the existing ILCompiler 10.0.1 NU1903 advisory. Local managed restore uses `-p:NuGetAudit=false`, as in the base lane, without package changes. Initial development failures came from a build that read support source before its edit, Windows resource separators and an intersected test filter. The final build and selections above replace those results. No flaky test was found.

Reproduce the managed gates from the repository root:

```powershell
dotnet restore OpenForge.Cli.slnx -p:NuGetAudit=false
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.ToolDraftDefinitionTests --minimum-expected-tests 6 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Framework.Tools.ToolDraftIsolationTests --minimum-expected-tests 2 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-method OpenForge.Cli.IntegrationTests.Commands.Tool.ToolReadCommandsTests.HelpStopsBeforeWorkspace --minimum-expected-tests 1 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-method OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests.HealthyWithExtension OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests.InstalledWorkspace OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests.RecoveryBundlePresent --minimum-expected-tests 4 --fail-warns on --fail-skips on --no-ansi --progress off
npm --prefix src/docusaurus run build
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

To repeat the offline schema checks after `npm ci --prefix src/docusaurus`, use this standalone check. It validates both documents and checks the two admission guards.

Illustrative:

```powershell
@'
import fs from 'node:fs';
import assert from 'node:assert/strict';
import Ajv from './src/docusaurus/node_modules/ajv/dist/2020.js';
const read = path => JSON.parse(fs.readFileSync(path, 'utf8'));
const ajv = new Ajv({strict: false});
ajv.addSchema(read('schemas/tools/v1/manifest.schema.json'));
const validate = ajv.compile(read('schemas/tools/drafts/v1/definition.schema.json'));
for (const pin of ['codegraph-1.6.2', 'codebase-memory-0.12.1']) {
  const draft = read(`src/tools/drafts/${pin}.json`);
  assert.equal(validate(draft), true);
  const command = structuredClone(draft);
  command.lifecycleDraft.install.args = ['install'];
  assert.equal(validate(command), false);
  const enabled = structuredClone(draft);
  enabled.definition.admission = 'enabled';
  assert.equal(validate(enabled), false);
}
// Both valid drafts pass. Both unsafe mutations fail.
'@ | node --input-type=module
```

## Not decided yet

- Resolve R1 and R2 with the maintainer. Keep both drafts blocked until reviewed commands reflect the decisions.
- Resolve CodeGraph's private launcher and Node policy, tool-owned Skill delivery and complete account activation destinations.
- Propose native MCP selector support in the core without host writers or a TOML parser.
- Integrate the completed core before the Windows download, install, Status, external index and Remove journeys. Other native targets come through CI.

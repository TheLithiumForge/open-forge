---
open-forge:
  description: Resume CodeGraph shared-type integration after rebase onto the final Tool core
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Tool, Contextual]
---

# CodeGraph V1

CodeGraph V1 is the issue 23 task that prepares the verified CodeGraph pin, required Skill, consent and agent integration for Open Forge.

This increment rebases onto `feature/tool-command` at `3d9df832726aacd4a1a4d3108e7b1796b74b1fae` and replaces prototype metadata with the final shared types. Writers, Dispatch and Registration are separate pending lanes. The real Windows journey waits for their integration.

Status: Rebase, typed draft and focused verification complete. Core lane integration and native qualification pending.

## Branch state

The backup branch is `backup/tool-code-graphs-pre-3d9df8327-20261113` at `2bc81a87978ed4097853208feaedad4355d90f04`. Five existing commits replayed as `5b3694d0a`, `7b5e376a7`, `31eacfc6d`, `260114a83` and `7fb883876`. The final shared contracts, schemas, canonical manifest fixture and Presentation dependency paths come from the new base. Snapshot expectations are regenerated from fresh output after review.

The production catalogue stays empty. Every CodeGraph target stays blocked with empty native qualification. The review wrapper isn't a production `--source` input. No installer, index command, telemetry-setting command, PATH change or registry change runs in this increment. Local commits are authorized. Pushes, merges and upstream contact aren't.

## Definition

The [CodeGraph draft](../../../../src/tools/drafts/codegraph-1.6.2.json) records all six official archive sizes, published SHA-256, verified build-attestation identity and distinct source commits. It now uses `ToolRegistration`, all four `ToolEntryRewrite` forms, `ToolOptionalComponent`, `ToolRoleGuidance`, `ToolOrigin.ThirdParty`, `ToolTarget.Launcher`, removal order and both native telemetry commands. Two instruction files need separate marked-line records, giving five rewrites.

The launcher runs contained Node with the official archive's fixed flags and script path without a shell. Lifecycle caps are 120 seconds for installation, removal, index cleanup and role commands and 30 seconds for telemetry. These are policy caps. Native qualification must check them.

The [maintenance example](../../../../docs/tool-maintenance.md) is the source for exact commands, expected entries, rewrites, hook choice, telemetry, removal and upstream requests. The [consent inventory](../../../../docs/tools/codegraph-consent.md) lists native files, legacy cleanup, account telemetry state and agreed leftovers to verify. The public CodeGraph page gives setup and practical use. The required Skill stays short and tells teammates to use bare `open-forge tool install` for missing declared pins. The native MCP name stays `codegraph`.

Codebase Memory has no definition, Skill or public page here. The maintainer preserved its research and missing upstream controls on `feature/tool-codebase-memory` at `7c76ca5fb`.

## Execution profile

This is a reversible draft, read-output and documentation migration for local developer tooling. Git keeps prior bytes. The supported failure limit covers normal edits, malformed inputs, interruption and accidental concurrency. A hostile same-account process is outside this limit. We reuse the final shared contracts, System.Text.Json source generation, the existing serializer and the accepted exact TOML line-edit exception. No new parser, dependency or shared API is introduced.

Focused Unit tests cover typed decode and round trip, lifecycle arrays, targets, launcher flags, hashes and admission. Integration covers origin grouping, affected read output and draft isolation. Shipped CLI Skill changes need affected Status snapshots. CI runs full managed and native suites. The user's lane rule prohibits those full local suites and native builds.

## Verification

The final Release managed build passes with zero warnings and errors. The six touched Unit cases pass. The focused Tool Integration selection passes 12 cases, including the telemetry constructor consumer from the new base. The affected Status selection passes four cases. No selected case skips or flakes. An initial build found the new telemetry test's missing origin constructor argument. It was corrected before the final build.

Accepted 72 fresh snapshots after reviewing every change. Sixteen Tool snapshots cover the CodeGraph help examples, origin column and Inspect origin line. Fifty-six Status snapshots cover only the shipped CLI Skill's 916-byte increase, routed character and token totals and startup-share ratios. Startup counts, findings and other behavior stay unchanged. The first capture runs failed only on those expected snapshot differences. Both final selections pass.

Both required .NET formatting checks pass. The site production build passes with its existing Node localStorage experimental warning. Offline schema validation passes the draft and rejects seven mutations. The practical Skill validator, exact CLI-copy parity, 62 changed-file Markdown links, scoped Index, Prettier and LF checks pass. Both hook spelling and array-position limits stay recorded. Reading the launcher member in each of the six verified archives confirms its executable, fixed flags and script.

Disposable build, schema and snapshot review reports are under `artifacts/tool-code-graphs/`. Required inputs live in tracked fixtures and source.

From the repository root, reproduce the focused managed evidence:

```powershell
dotnet build OpenForge.Cli.slnx --configuration Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-class OpenForge.Cli.Core.UnitTests.Framework.Tools.ToolDraftDefinitionTests --minimum-expected-tests 6 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-method OpenForge.Cli.IntegrationTests.Commands.Tool.ToolReadCommandsTests.HelpStopsBeforeWorkspace OpenForge.Cli.IntegrationTests.Commands.Tool.ToolReadCommandsTests.GroupsOrigins OpenForge.Cli.IntegrationTests.Commands.Tool.ToolReadCommandsTests.KeepsIndependentInstalledRows OpenForge.Cli.IntegrationTests.Commands.Tool.ToolReadCommandsTests.InspectKeepsCandidateFacts OpenForge.Cli.IntegrationTests.Commands.Tool.ToolReadCommandsTests.ReadOutput OpenForge.Cli.IntegrationTests.Framework.Tools.ToolDraftIsolationTests.DraftSourcesCannotActivate OpenForge.Cli.IntegrationTests.Commands.Tool.ToolTelemetryReadTests.ReportsConfigurationDrift OpenForge.Cli.IntegrationTests.Commands.Tool.ToolTelemetryReadTests.RendersChoice --minimum-expected-tests 12 --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-method OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests.HealthyWithExtension OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests.InstalledWorkspace OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests.RecoveryBundlePresent --minimum-expected-tests 4 --fail-warns on --fail-skips on --no-ansi --progress off
npm --prefix src/docusaurus run build
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

For offline schema validation, load `schemas/tools/v1/manifest.schema.json` into Ajv 2020, then compile `schemas/tools/drafts/v1/definition.schema.json` and validate the draft. Negative mutations cover enabled admission, unknown origin, absent registration, missing rewrite, unknown rewrite kind, reversed removal order and missing launcher executable. The exact pinned-source command and launcher oracle is `Fixtures/Tools/codegraph-installer.json` in TestSupport. The Skill hash is checked by the Unit tests.

## Next work

The shared hook form has one exact expected command and JSON Pointer. CodeGraph writes `codegraph.cmd prompt-hook` on Windows and `codegraph prompt-hook` elsewhere. It appends its group after existing `UserPromptSubmit` groups. The typed hook and decline records describe the pristine Windows fixture. Both differences are recorded in `review.registrationGaps`. A maintainer question is pending on how Registration will represent those variants and locate the appended entry. Keep admission blocked until that is resolved.

After the three core lanes arrive, bind the recipe through their generic writers and run the temporary Windows x64 Git journey under D:. Verify download, hashes, project registration, Status, first Init, refresh and removal. Check that native uninstall recognizes the rewritten hook. Inventory every agreed leftover. The other five targets need CI journeys before activation.

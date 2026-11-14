---
open-forge:
  description: Implement Task 90 lane B additive config and backups
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Additive config and backups

Lane B is the implementation that adds profile configuration and removes only recorded project additions while keeping account-wide settings.
The coordinator supplies the exact execution base in the dispatch prompt.

Status: Prepared for Phase 2. Work starts only through the assigned Worker Watch run.

## Read first

Read AGENTS.md and .agents/loader.md in your worktree and load applicable Open Forge scopes before changing files. Independently read the complete current .agents/directives/csharp/_csharp.md, design.md and style.md, plus CLI, software, Open Forge CLI, testing and work-planning scopes. Read the current CLI architecture, selected Task 90 and these exact spec files:

- .agents/memory/crystallized/documents/cli/technical-designs/tool-setup.md
- .agents/memory/crystallized/documents/cli/technical-designs/tool-setup-lanes.md
- .agents/memory/crystallized/documents/cli/contracts/tool/_tool.md, interface.md and behavior.md, then your leaf contracts
- .agents/memory/crystallized/documents/cli/contracts/shared/tool-records/interface.md and behavior.md
- .agents/memory/crystallized/documents/maintenance/tools.md
- D:/Repositories/open-forge-worktrees/of-lane-rules.md
- D:/Repositories/open-forge-worktrees/of-tool-setup-decisions-2.md

The maintainer's C1 through C7 decisions in the design override the older round-2 file. CLI selectors are bare ID, ID@major.minor or ID@exact stable SemVer. No CLI minor.x, dist-tags or full npm range expressions. Bare Install restores exact lock versions. A named bare ID selects latest support. Use --yes, no --automatic or --allow-download. No Open Forge per-tool Skill or managed-block guidance.

Codebase Memory uninstall is account-wide, including the binary and all agent entries. It needs live interactive confirmation naming that effect. With --yes, no terminal or a declined confirmation, skip it, remove only Open Forge project additions and print its native command. Project Remove never restores account-wide native settings. Keep them, say so and print native manual change commands. No cross-project setting registry. These intentional kept effects do not alone make project removal incomplete.

## Shared limits

You are not alone in the codebase. Keep other lanes' work and never revert their edits. Work only in your assigned worktree and writable paths below. The master worktree and maintainer checkout are read-only to you. No push, PR, GitHub mutation or local integration merge. Use local Git objects for old code. Public upstream research and dependency restore are authorized for the assigned task. Don't inspect credentials or unrelated private stores.

The user chooses gpt-6.1-sol with xhigh effort, overriding the older Worker Watch roster. Don't spawn more agents. Start your response with your agent name, model and effort from the invocation.

Never change real persistent user PATH, registries or real shell profiles. Root PATH capabilities must be testable through owned test paths or a fake host. No real tool installation in this first implementation pass. E handles real journeys later in owned temporary homes and Git repos under D:.

Models under src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Models/, schemas, actual .agents/open-forge.json and .agents/open-forge.lock.json, parent Task 90, shared contracts, all other packets and central composition are protected. A required shared change is a separate proposed contract(tool): ... increment. Report CONTRACT_REQUEST with exact changes and affected consumers before implementing against it. The mastermind integrates the contract first. Do not create private substitutes or copy shared contracts into your lane.

Tool operations add no state, lease, journal or recovery bundle under .open-forge-state/. Repository facts and additive receipts go in the committed lock. Native previous settings and machine facts stay in per-user state. Tool commands do not run concurrently by design. Keep expected-state checks, physical containment, unchanged unrelated data, atomic replacement and Git-aware sibling backups. No automatic rollback.

Use the existing layer graph: Framework mechanisms and data, Operations command behavior, Rendering presentation, Root privileged process/network/platform hosts and wiring. Match physical paths and namespaces. Use the BCL and existing packages, source-generated concrete JSON and existing command result conventions. A new dependency, project or exceptional platform mechanism needs an upward request.

## Verification

Run npm ci in your worktree. For the later site pass also run npm ci in src/docusaurus. Keep LF. Do not run Doctor in this repository workspace.

Use a managed solution build, then only tests you touch or create. Never run complete Unit, Integration, EndToEnd or native suites, unrelated test classes or build:native. CI is the full gate. Before a commit run npm run check:dotnet:whitespace and npm run check:dotnet:style.

A local build can use:
dotnet build OpenForge.Cli.slnx --configuration Release -p:NuGetAudit=false -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
Never commit that local NU1903 override. Never change PATH to make builds pass.

Use independently selectable tests with the right tier. For each touched class run the corresponding managed test project with --filter-class <qualified-class>, --minimum-expected-tests <actual-count>, --fail-warns on, --fail-skips on, --no-ansi and --progress off. Name the exact command, counts, artifact and result in your receipt. New tests use synthetic entries and owned temporary files, homes and processes. Never use a real per-user store. Avoid production test switches that alter semantics.

No literal assertions on user-facing text, help or rendered output. Use reviewed Imprint snapshots. Semantic assertions use the production owner for finding codes, environment names, paths and schema fields. Do not copy shipped payload strings into fixtures. Regeneration of payload-driven snapshots and fixtures is reserved for the integrator once, after all implementation and E's docs. Report any such pending snapshot impact.

## Return and commit

Update only your own packet's Progress and Evidence sections with the observed steps and exact checks. Do not edit parent state or common catalogues. Report new routed files so the integrator can Index once.

First finish the implementation and focused checks, then return READY_FOR_REVIEW with the changed files, test evidence, contract requests and remaining gaps. Leave the changes uncommitted for the coordinator's grouped review and commit continuation. Do not wait inside a running shell.

On the coordinator's continuation, apply the accepted grouped fixes if any, repeat only affected checks, make a local conventional commit with no attribution and report its full SHA. This packet includes that final commit-and-report assignment. Do not push or merge. No fabricated timestamps.

If a blocker affects only part of the assignment, finish independent work and report the exact blocked requirement. Never silently omit a requirement or call an unimplemented path complete.

## File assignment

You alone edit Framework/Tools/Shared/{ConfigChanges,Backups,Filesystem,NativeSettings}/** below src/cli/framework/OpenForge.Cli.Framework/. You own matching Unit and Integration tests and this lane-b.md packet. You implement IToolConfigService and IToolBackupService. C alone writes lock and machine JSON files. D sequences operations and calls C after each verified addition. A owns process hosts. Shared Models are protected.

## Steps

Follow this ordered implementation plan:

1. Check the exact base and read all Configuration, Records and Services models, Tool record contracts and the full lane-B copy table. Copy the Git-aware backup helpers, JSON Pointer editor and marked-block renderer through git show. Reuse the original-backup continuation fix from c2e7d119d. Strip leases, recovery bundles, roles, guidance, MCP key policies, TOML key rewrites and old registries.
2. Implement safe JSON Pointer edits using the existing byte/span editor. Array union appends missing values and records only added items. A set requires member absence, including when a pre-existing value already equals desired. Null is present. A non-array union conflicts. Keep unrelated bytes and values. Write pure parser/planner tests and real-file integration tests for effects.
3. Implement unique marked blocks and absent-only managed files. Duplicate or malformed markers conflict. Remove only the recorded managed span. Guard file removal by its hash. Reapply is a no-op when matching. Migrate a recorded set, file or block only when current equals recorded applied content or digest. Other drift conflicts. Removed profile recipes use guarded removal.
4. Implement allow-listed native config calls through the request's resolved invocation and IToolProcessHost. Probe stored native values independently of inherited telemetry override environment. Record previous/applied account settings only in returned machine facts. CodeGraph telemetry off is disclosed, including identity/queue deletion. Block before effects if previous state cannot be read.
5. On every project Remove, keep account-wide native settings, return KeptNativeSettings with ManualRestoreArguments and never execute a restore. Drop the project native receipt only as planned cleanup, while keeping machine facts needed for the report. No cross-project registry. Verify with a fake process host that removal invokes no native restore even when current matches applied.
6. Implement IToolBackupService and use the same backup mechanics for native setup targets and config edits. Git covers only tracked bytes matching HEAD. Dirty tracked and untracked files get sibling .bak. Keep one original across sequential edits. Occupied unrelated backup destinations block. Verified completion discards only an unchanged operation backup. Failure keeps it and exposes guarded manual instructions. No automatic rollback or Tool Repair action.
7. Implement ComputeProfileDigest using the fixed projection in the record Interface. It includes profile identity, ranges, minimums, native vectors/effects and recipes/template hashes. Exclude observed values, paths and timestamps. Keep authored array order and ordinal object keys. New templates or token semantics needed across B/D/E are contract requests, not local divergent formats.
8. Build, run touched/created tests and both .NET checks. Return READY_FOR_REVIEW with the service construction entry, all effect and receipt cases, exact tests and any blocked contract needs. The coordinator handles the commit continuation.

## Acceptance

Test fresh apply, no-op, migration, drift and removal for all five change kinds, union accounting, absence/null, unrelated byte preservation, duplicate markers, changed managed files, dropped recipes, unreadable native prior state, kept account-wide settings, failed native calls, backup continuation and partial completion. D can call ApplyAsync with one planned observation at a time and persist receipts before the next. Return completed receipts on later failure. No lock serialization, workspace state, leases, journals or fake filesystem. Synthetic entries and owned temporary homes only.

## Progress

Done: Packet prepared from the accepted design.
Now: Awaiting the assigned Worker Watch run.
Next: Verify the execution base and complete the ordered steps.
Blocker: None identified before implementation.

## Evidence

No implementation or focused tests have run for this lane yet.

## Divergences

None recorded before implementation.

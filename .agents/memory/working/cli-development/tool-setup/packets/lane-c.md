---
open-forge:
  description: Implement Task 90 lane C records and read commands
  tags: [Memory, Working, CLI, Tool, Task, Contextual, Active]
---

# Records and read commands

Lane C is the implementation that persists portable and machine tool facts and reports catalogue, status and drift through the CLI.
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

You alone edit Framework/Tools/Shared/{Records,Observation,Serialization}/**, Framework/Settings/** and Framework/Ownership/** below src/cli/framework/OpenForge.Cli.Framework/. This includes their codecs and schema constants. Do not edit schema JSON or actual workspace settings/lock.

You own Operations Commands/Tool/{List,Inspect,Status}/**, Commands/Tool/Shared/Observation/** and Commands/Tool/Models/Read/**, plus corresponding leaf Rendering, OutputText and tests. You also own tool findings and their required call sites/models/presentation within existing workspace Commands/Status/** and Commands/Doctor/**. Keep these modifications confined to tool observations. All Root command composition is protected.

You own this lane-c.md packet. The integrator owns group registration, common Tool group types and central rendering wiring. Each leaf binding accepts its parent Command. Do not duplicate a group binding or replace existing shared result policy.

## Steps

Follow this ordered implementation plan:

1. Check exact base, all Models/Records and Services contracts and existing production Settings/Ownership readers, codecs and writers. Read Tool list/inspect/status and workspace Status/Doctor contracts. Reuse only the missing-tool appender sources in the copy table, adapting their facts and tests.
2. Implement IToolRecordService. Settings v6 and lock v2 derive from current released v5/v1. Preserve unrelated and unknown fields. Settings declare ID/range. Lock requires exact version, channel and additive changes, with optional digest. No local paths or config preimages in the lock. Keep the existing ownership/Library facts.
3. Persist per-user tools, runtimes/node, path and workspace-hash files using the accepted filenames. Validate physical workspace-path binding. Native previous settings stay in machine state. No .open-forge-state/ writes or workspace lease/recovery dependency. Use expected file SHA-256/absence facts and atomic replacement. A write failure reports actual completed paths and unfinished work.
4. Implement List, Inspect and Tool Status operations against injected catalogue, IToolChannelService, IToolConfigService and IToolRecordService where needed. Root supplies home paths and real service instances. Each leaf provides its typed binding, concrete result/payload, source-generated JSON, presentation and output text. Use schema 3 with data.tools in ID order and existing shared statuses/exits. Do not depend directly on E's still-unimplemented concrete catalogue.
5. List can show the catalogue without an installed workspace. Inspect reports identity, every channel requirement, profiles, native setup/uninstall writes, Open Forge additions, telemetry, bin, tool/runtime paths and PATH readiness. Tool Status checks exact lock versus detected self-updates, supported/unsupported state, config drift and missing machine facts. Both show bin and manager-global-bin readiness. Reads never install, resolve latest or write state.
6. Add workspace Status/Doctor missing-tool and drift findings with actionable bare Install or named reapply advice. Use bounded detection, including declared CodeGraph no-download environment. Keep findings and observed facts separate from Repair. No new Tool Repair action. Tests exercise Doctor only on owned fixtures, never the repository workspace.
7. Build and run only touched or created classes. Add codec tests for unknown keys/version shapes, all five receipt kinds, exact lock restore facts, absent machine state, expectation mismatch and failed writes. Use Imprint for rendered output. Do not regenerate existing payload-dependent fixtures/snapshots now. Report that impact for the final integrator pass.
8. Run both .NET checks, then return READY_FOR_REVIEW with service constructors, leaf binding/presentation signatures, workspace contributor wiring, exact tests and contract requests. The coordinator handles the commit continuation.

## Acceptance

No machine facts or prior repository values leak into settings/lock. Unknown keys survive writes. No old tool manifest/registry/roles/guidance is copied. Missing versus unavailable and current versus unsupported facts stay distinct. Reports share text/JSON facts, stdout stays one JSON document and existing commands keep their semantics. Account-wide native settings are kept on Remove; readers describe their state without restoring them. The global version flag still refers to Open Forge.

## Progress

Done: Packet prepared from the accepted design.
Now: Awaiting the assigned Worker Watch run.
Next: Verify the execution base and complete the ordered steps.
Blocker: None identified before implementation.

## Evidence

No implementation or focused tests have run for this lane yet.

## Divergences

None recorded before implementation.

---
open-forge:
  description: Resume the CLI program after the Astra transfer with re-enabled tasks and preserved unfinished worktrees
  tags: [Memory, Archived, Contextual, Historical, Handoff, CLI]
---

# CLI Astra Restart Handoff

## Transfer Boundary

Sealed on 2026-09-07 for the next user-facing Open Forge Overseer. The user
requested the transfer after switching to Astra and explicitly re-enabled all
pending tasks. Resume from live Git and files, not old agent handles.

Read this file completely before acting. The accompanying
[dirty inventory](2026-09-07_cli-astra-restart-inventory.md) records every dirty
path and SHA-256 in the six relevant worktrees before this transfer's own
documentation changes. No implementation was resumed to prepare this handoff.

All former children stopped. Repair and Library Masterminds performed final
read-only inspection and returned their state. A host process inspection found
no running dotnet, compiler, test, or CLI process at sealing. Recheck processes
and files in the next chat before assigning new writers. No staging or commit
was performed at this transfer.

## Latest User Direction

All pending tasks are re-enabled. The earlier postponement pending Astra is
lifted, but dependencies and explicit contract decisions remain. Finish command
implementations before pure refactoring. Continue strictly following current
C# design, style, and applicable Patterns during every command.

The retained order is:

1. Task 19 “Repair”, currently phase 4/5, milestone 3/8.
2. Task 20 “Cleanup”, currently phase 4/5, milestone 3/8 with accepted Red.
3. Task 23 “Workspace Libraries”, currently phase 2/5, milestone 1/8.
4. Task 24 “Extensions Evolution”: functional package vocabulary and Extension
   destination permissions.
5. Task 25 “Workspace Library Destination Projections”: functional relative-link
   destinations beyond the first release's boundary.
6. Task 26 “Extension Internal Consolidation”: pure six-command refactoring and
   test streamlining after functional decisions are accepted or declined.
7. Task 10 “CLI Command Surface Audit”.
8. Conditional Task 21 “CLI Command Surface Remediation”, only for accepted
   Task 10 findings.
9. Task 13 “Native linux-x64 CI and Reproducible Artifacts”.
10. Task 22 “Final Documentation, Acceptance, and Release”.

Tasks 24–26 have no accepted active phase or milestone horizon. Re-enabling
them does not accept a naming choice, destination schema, new evidence claim,
or publication. Their final product draft still goes to the user for comments.
Task 21 remains conditional. Reconcile live ledger/plan/task rows before the
next implementation wave: many older rows and the dirty queue draft still say
postponed or incorrectly show Repair before Green.

## Required Loading And Working Rules

Start with `.apm/agents/overseer.agent.md`, `AGENTS.md`, and
`.agents/loader.md`. Follow all relevant entrypoints, overwrites, LoadNow,
KeepInMind, directives, and patterns. Read the
[project ledger](../cli-development/project-control.md),
[checkpoint](../checkpoints/cli-development.md),
[plan](../cli-development/plan.md),
[Overseer memory](../cli-development/overseer-memory.md), parent Task, and the
current task records from each lane. Also load the CLI Architecture, relevant
command Interface/Behavior contracts, technical designs, CLI implementation,
testing/evidence integrity, and selected development workflows.

Use the accepted streamlined assured flow: Task Mastermind supervision,
explicit Preflight, Gray, Red, one coherent Brilliant Implementer for Green
and verification, one fresh whole-task review and a grouped correction as
allocated by each Task. Consume and preserve stable review/correction IDs;
do not restart spent budgets. Load Program Development, Worktree Program
Development, Adaptive Development, and the applicable Development phase
recipes as the recorded capsules require.

Every C# author and reviewer must personally read the complete current files
and report their own fingerprints:

| File                                   | SHA-256 at transfer                                                |
| -------------------------------------- | ------------------------------------------------------------------ |
| `.agents/directives/csharp/_csharp.md` | `31045ebcb02d5bfeee8ba9f3112d307b1f72a2186cda618fbf7d22e7d1d90b53` |
| `.agents/directives/csharp/design.md`  | `76aa8fc7aaaa79d9535998f5557150f3754e3d80520a7a06864b61659373c1a9` |
| `.agents/directives/csharp/style.md`   | `c3fa9d31575e77fedb103ca397f0ccf10ab7236e6edbef1658c7fe36138457cb` |

Also select `.agents/patterns/software/contract-ownership.md`,
`exhaustive-csharp-enum-switch.md`,
`source-locality/nearest-shared-scope.md`, and
`.agents/patterns/testing/evidence-tiers.md` through their entrypoints.
A copied fingerprint or an orchestrator summary does not replace personal
reading. In particular:

- Keep cohesive typed inputs, truthful nullability, compiler-proven flow,
  symbolic constants, readable construction, and exhaustive enum rejection.
- Keep state-only types in the appropriate Models scope and implementation
  support in the nearest `Shared/<Capability>` scope.
- Promote only meaning demonstrated by real consumers or accepted neutral
  foundations. Do not create speculative global mechanisms.
- No DI, runtime registry, reflection/string dispatch, compatibility machinery,
  JavaScript/MJS/CJS implementation, or null suppression to silence warnings.
- Tests prove Open Forge-owned behavior. Retain exactly three simple public
  EndToEnd journeys per command. Library has five commands, hence fifteen.
- Run formatting at the correct severity, static/protected-path/callable-shape/
  prohibited-pattern/line-length checks, focused evidence, and required fresh
  full managed/public and supported linux-x64 Native AOT gates before
  acceptance. Never use stale artifacts or a different worktree's CLI.

Preserve every dirty file. Do not reset, restore, clean, stash, or rebase dirty
lanes. Do not amend immutable commits. Use natural, concise past-tense commit
subjects without conventional prefixes. Commit coherent inspected changes,
account for every untracked file, refreeze before staging, and stage exact
paths. Squash accepted tasks onto current local develop after the required
gates; preserve provenance and avoid duplicate already-integrated deltas.
No remote action, publication, destructive external action, or new global
installation is authorized.

The user authorized safe parallelism and wants speed. Masterminds supervise
their own children. Parallelize independent preparation, command-private work,
and exact verification where useful; serialize shared/public Green, composition,
artifacts in one worktree, and integration. Use Luna for bounded prose and
routine exact command execution when suitable. Use the selected capable model
for consequential architecture, implementation, and integration. The model
switch did not authorize rewriting agent configuration or weakening directives.

Progress messages use Active / Recently completed / Queued, permanent task ID
and actual name, phase A/B, milestone C/D, a short bar such as `###-----`,
and each active agent's name, role basename, actual model, and reasoning.
Do not invent phase zero for tasks without a horizon. Masterminds use terse
Done / Now / Next / Blocker. Report unknown runtime fields as unreported.

## Live Git Map

Paths below are relative to the main repository. All six indexes were empty
at capture. The HEAD tree is committed state, not an acceptance claim about
the dirty working tree.

- `.`: `develop`; HEAD `2c62f59aff8d0992b81621c298c4aabc9ab72c9a`;
  tree `fa607717e461f4e6092d38b69e566ff82a6b963a`; 2 dirty paths.
- `../open-forge-worktree/repair-implementation`: `codex/repair-implementation`; HEAD `eae8eb366e8ad50d8c18cca6a4e08e9d3b6d22bb`;
  tree `4b2523e8816bffcbb24e28ae6ebd230fcfa73aca`; 28 dirty paths.
- `../open-forge-worktree/cleanup-implementation`: `codex/cleanup-implementation`; HEAD `96ed0aa35b7e092aa78e5bb7249ce71c5598b2c9`;
  tree `be13c85b4cb26271c0770b880a094f125c0a7f7c`; 0 dirty paths.
- `../open-forge-worktree/workspace-libraries-contracts`: `codex/workspace-libraries-contracts`; HEAD `c3f01acb76c572ee486fdc24c6a2379b27459391`;
  tree `5ccffb7156f73aac3c15a49588b7eafcfa661a0a`; 102 dirty paths.
- `../open-forge-worktree/extension-queue-realignment`: `codex/extension-queue-realignment`; HEAD `2c62f59aff8d0992b81621c298c4aabc9ab72c9a`;
  tree `fa607717e461f4e6092d38b69e566ff82a6b963a`; 11 dirty paths.
- `../open-forge-worktree/extensions-evolution-preparation`: `codex/extensions-evolution-preparation`; HEAD `8a153f23dabb05019eae2c15fae51331b9e88335`;
  tree `bafd3d1e3173ad9342bd25a6086330dcfbe5ee16`; 0 dirty paths.

Main develop already had two dirty files before this handover:
`.agents/memory/emerging/ideas/extensions-overhaul.md` and
`.agents/memory/working/checkpoints/cli-development.md`. Their exact earlier
hashes are in the inventory. They contain a separate queue draft; the previous
claim that develop was clean is stale. Preserve these bytes while reconciling
overlap with the eleven-path queue lane. This handover adds a latest-direction
entry to the ledger and checkpoint and adds its routed transfer documents.
It does not integrate or accept the queue draft.

## Task 19: Repair

Lane: `../open-forge-worktree/repair-implementation`.
Former Mastermind: Ampere II, canonical
`/root/kepler_ii_repair_preflight`, `task-mastermind.agent.md`,
GPT-5.6 Sol/xhigh. Some records call that continuing role Kepler II.
Former Green author: Curie IV, `brilliant-implementer.agent.md`,
GPT-5.6 Sol/xhigh. Noether II, Luna/high explorer, completed the Doctor seam
review. Re-establish equivalent supervision only after live inspection.

Accepted Gray `140920d3`, Red `af59c957`, record `fb8ce672`.
The integrated Task 18 baseline was merged at `f0437512`, tree
`25d88102be8456c0004b2a039f690145960e7c3a`; fifteen prose conflicts were
resolved and the frozen twenty Gray/twelve Red path sets preserved.
Transition `eae8eb36` opened Green. Current state is four modified and
twenty-four untracked paths, zero staged.

Last proved evidence precedes the latest operation/application edits:

- Repair planner Unit: 51/51.
- Doctor seam Core Release: zero warnings/errors.
- Doctor Unit: 21 selected/discovered/executed, all passed, minimum 20.
- Doctor Integration: exactly 3/3 passed.
- Current diff check passes. The final 28-path dirty tree is not yet compiled.

The accepted Doctor seam consists of exactly
`Commands/Doctor/DoctorDiagnosisReader.cs`,
`Commands/Doctor/Models/Observation/DoctorDiagnosisRead.cs`, and delegation in
`Commands/Doctor/DoctorOperation.cs`, under the Core project. It retains the
same six reads and one observation/result pair. Reader catches nothing;
DoctorOperation retains its existing event/error mapping. Raw source and local
reference facts stay available to Repair.

Drafts exist for catalogue/result mapping, operation/components/factory/default
contributors, application/recovery/post-verification. The accepted interactive
session is not yet consumed, selection/final confirmation are absent, and
static root composition is untouched. Resume the same coherent slice. Do not
mistake planner or seam success for operation acceptance.

First diagnostic command, in the Repair lane:

```sh
dotnet build src/cli/core/OpenForge.Cli.Core/OpenForge.Cli.Core.csproj -c Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true
```

After coherent implementation and a fresh relevant build/publication, run:

```sh
dotnet test --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --filter-trait "Feature=repair" --minimum-expected-tests 51
dotnet test --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build --filter-trait "Feature=repair" --minimum-expected-tests 6
dotnet test --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build --filter-class "*PublishedRepairProcessTests" --minimum-expected-tests 3
dotnet test --project src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj -c Release --no-build --filter-class "*PublishedDoctorProcessTests" --minimum-expected-tests 3
```

Rerun affected Doctor Unit/Integration after seam changes. Then complete the
task-owned checks, refreeze, coherent commit, fresh full managed/public/native
gates, review/correction, and acceptance as the Task record requires.

## Task 20: Cleanup

Lane is clean at `96ed0aa3`, tree `be13c85b`. Gray `0263ae1e` and Red
`e2b7fdc0` are accepted. Former Mastermind Hubble II is inactive.
Task remains phase 4/5, milestone 3/8. Green waits for Task 19 acceptance,
integration, and a fresh refreeze against that integrated baseline.

## Task 23: Workspace Libraries

Lane: `../open-forge-worktree/workspace-libraries-contracts`.
Former Mastermind: Hegel II, canonical
`/root/noether_iii_workspace_libraries`, `task-mastermind.agent.md`,
GPT-5.6 Sol/xhigh. Older role aliases include Noether II/III.
All Gray and residual correction children stopped.

Current 102-path correction has 86 modified files, one deleted tracked file,
and fifteen untracked files. The deletion is canonical
`Framework/Recovery/Models/RecoveryBundleEntry.cs`; replacement
`RecoveryEntry.cs` is accounted for. No staged content.

This breadth came from the accepted static Reader/Store/Catalogue/DeletionGuard
call chain and direct consumers, followed by analyzer findings in forced
changed files. It is not permission for a general refactor. Review actual
necessity before acceptance.

Latest evidence and immediate defect:

- Nonincremental Core and Root Release builds passed zero warnings/errors
  before the last RecoveryEntry edit.
- Severity-info format passed Core 75, Root 1, Unit 3, Integration 22 selected
  paths before that last edit.
- Focused recovery Unit selected 20: nineteen passed and
  `TargetRejectsMismatchedPriorState` failed because delete plus intended state
  was not rejected.
- A final unverified guard was added. It currently uses forbidden
  `intended!` at `RecoveryEntry.cs:261`.
- Focused recovery Integration did not run. Current diff check passes.
- No final build, format, or green evidence covers the final edit.

First replace the equivalence guard/null suppression with explicit delete and
non-delete branches. Delete rejects non-null intended state; non-delete rejects
null; Ordinary receives compiler-proven non-null state. This is an existing
C# conformance correction, not a new policy choice.

Then serialize nonincremental Core/Root Release builds; Core, Root, Unit, and
Integration severity-info format with the current exact changed-file includes;
focused Unit classes `*RecoveryBundleContractTests` and
`*RecoveryBundleAttributionContractTests` (minimum 11; prior discovery 20);
focused Integration classes `*RecoveryBundleApplicationIntegrationTests`,
`*RecoveryBundleCatalogueIntegrationTests`,
`*RecoveryBundleStoreIntegrationTests`, and
`*RecoveryBundleDeletionAttributionIntegrationTests` (minimum 10).
Reconstruct exact includes from the live inventory and verify discovery, not
just exit codes. Follow with full scans, stale-type/static-call checks and
hash refreeze. Nothing may be staged before correction evidence passes.

Gray is not finished: the sole Gray author must then complete command/public
callable shapes. Red authors remain read-only until immutable Gray acceptance.
Green/integration waits for Task 20 and baseline refreeze.

Accepted first release: `library list`, `inspect`, `attach`, `sync`,
`detach`; strict `.agents/open-forge.libraries.json`; sources and
destinations under the accepted contained boundary; relative file symlinks;
exactly fifteen simple public journeys. The first release remains
`.agents/**`-only. Libraries retain source inventory, separate record,
source-preserving Sync/Detach, and link-aware recovery/route guards.

## Tasks 24–26 And The Unfinished Queue Draft

The exact requested idea was found in
`.agents/memory/emerging/ideas/extensions-overhaul.md`, originally
“Destinations Beyond .agents”. It proposed `content/` singular. The user
also suggested `contents/`; no naming choice is accepted.

- Task 24 preserves functional Extension package-layout analysis and
  consumer-owned exact destination permissions beyond `.agents`. Package or
  source metadata cannot grant permission. The old six-command refactor draft
  did not answer the user's intended question.
- Task 25 preserves Library relative-symlink destination projections separately
  from copied Extension files, ownership, update/remove, and recovery.
- Task 26 contains the pure six-command internal consolidation. Earlier
  `8a153f23` preparation is non-authoritative input here. Its source-reader
  fail-closed and Update parent-catalogue behavior candidates return to Task 24
  contract review; they cannot be slipped into a pure refactor.

Library naming was explicitly selected over attach/attachment/source/lib.
Keep `library` with attach/detach verbs. A shared .agents Git submodule/live
source motivated Libraries; extension packaging remains a distinct model.
Externalizing repository-local agent extensions was discussed and deferred;
it is not independently authorized by the global installation task.

Queue lane: eleven dirty Markdown paths, including two new Task records.
Its own Release root build passed zero warnings/errors; its own published
`artifacts/publish/open-forge-dev/Release/open-forge-dev` regenerated exactly
one task Entries region (24 to 26 entries), then a dry run confirmed current.
Prettier passed for authored prose. Prettier adds whitespace inside generated
regions, so rerun Index last and preserve its bytes; do not hand-edit generated
Entries. One attempted broader dry run exposed unrelated stale indices and
was never applied.

The queue prose review was interrupted before any coverage or findings.
Do not claim PASS. Three earlier Luna prose assignments were interrupted;
root took over the queue lane. Main now also has two dirty queue-related files,
so zero-byte/clean-main assumptions from earlier commentary are unsafe.
The queue draft's “postponed” wording is superseded by this transfer's user
direction. Reconcile it and preserve main's existing edits before acceptance
or integration. No current task's product contract changes through this prose.

## Completed Context And Local Installation

Doctor (Task 16), Route Remove (Task 5), Root Update (Task 6), Extension Update
(Task 17), Extension Remove (Task 18), and provisional installation (Task 7)
are complete. Do not resume the obsolete Doctor restart instructions.
Task 18 integration is `f445a55a`; its control closeout is main HEAD
`2c62f59a`. Its exact 74-path candidate overlay and focused 55/27/3/3
post-integration receipts passed. Completion grace is consumed.

Task 7's reversible global local-link installation was accepted and completed.
Ordinary projects use globally installed `open-forge`. Development/testing
must use the binary built/published in the same workspace. Global installation
does not automatically adopt later branch changes; no refresh was requested
during this handover. Loader command documentation was aligned for
provisional use. Do not publish packages or use another worktree/global binary
as acceptance evidence.

The earlier packed-stage type-closure issue involved the declared Bun lock's
`@types/node 26.1.2` and `undici-types 8.3.0` and exact reviewed cache
copies. It was dependency preparation for local packed evidence, not a product
feature or general permission to install arbitrary dependencies.

## Flow Observations And First Actions

The user asked to observe the experimental parallel flow and keep useful
lessons. Proven observations: independent preparation can overlap Green;
shared recovery signatures create large caller fan-out; format must include
the right severity and actual changed files; stale builds cannot prove newer
edits; prose delegation can stall and must be verified by live state. Record
these proportionately in the existing flow observation when resuming, without
claiming measured speed or token savings that were never measured.

At restart:

1. Inspect live Git, every dirty/untracked path, staged state, and processes.
   Compare inventory identities; investigate differences without overwriting.
2. Load authority, current lane records, and C# fingerprints. Re-establish
   Mastermind supervision and ownership only after inspection.
3. Reconcile the unfinished queue draft and main's overlapping edits with the
   re-enabled task order. Preserve the requested final user review for T24/T25.
4. Resume Repair Green and the bounded Library Gray correction in parallel.
5. Complete commands in dependency order, then accepted functional improvements,
   refactoring, audit/remediation, CI/artifacts, and final acceptance.
6. Keep external actions and publication subject to their existing explicit
   authorization boundaries.

The permission profile changed during transfer: main `.agents`, Git metadata,
and sibling worktrees may require tool approval for writes. Use the available
approval mechanism for already-authorized exact operations; do not bypass it.

The main-worktree Index dry run for the Handoffs parent returned
`index.metadata-incomplete` and made no changes. Several historical handoffs
retain older unscoped metadata. Their sealed bytes were preserved; the parent
has explicit authored links to this transfer and inventory. Generated Entries
were not edited by hand. Resolve the historical metadata/navigation issue
separately; it does not prevent direct loading of this handoff.

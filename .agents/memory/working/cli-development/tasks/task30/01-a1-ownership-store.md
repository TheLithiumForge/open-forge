---
open-forge:
  description: Task 30 G1 step 4 slice A1 execution plan for the ownership write planner and the Framework dual-write sites
  tags: [Memory, Working, CLI, Task, Plan, Contextual, Active]
---

# A1 — Ownership store and Framework dual-write

> Read [00 — Slice conventions](00-conventions.md) first: verification
> commands, pass conditions, the recurring composition pattern, and the rules
> every slice shares. This plan does not repeat them.

## Goal

`install`, `route init`, and `update` write `.agents/open-forge.lock.json`
alongside the existing lifecycle record. No command reads the lock yet, so no
observable behaviour changes.

## Depends on / Blocks

- Depends on: nothing. `Framework/Ownership/` already exists and compiles.
- Blocks: A2, A3, A4.

## References

Authorizing decision: [Workspace State Files](../../../../crystallized/decisions/framework/workspace-state-files.md),
refinement 7 in [phase-4a-g1](phase-4a-g1.md) (the lock is a write-time receipt).

Existing code to mirror:

- `src/cli/core/OpenForge.Cli.Core/Framework/Lifecycle/LifecycleStore.Planning.cs:13`
  `PlanFrameworkUpdate` — the shape to copy.
- `src/cli/core/OpenForge.Cli.Core/Framework/Lifecycle/LifecycleStore.Planning.Support.cs:173`
  — how the basis becomes `PlannedFileChange.Create` or `.Replace`.
- `src/cli/core/OpenForge.Cli.Core/Framework/Ownership/Shared/Serialization/WorkspaceOwnershipCodec.cs`
  — `Write(document)` already renders the bytes.

`WorkspaceOwnershipRead` carries `FileStateSnapshot? Snapshot`, added 2026-09-12
after this plan was first written. It is the exact bytes and resolved physical
path the mutation layer needs; re-rendering the decoded document is **not** a
substitute, because writing normalizes the document and drops members this
release does not know, so it cannot describe what is on disk.

**The lock is written wholesale and does not preserve unknown members.** It is
machine-owned and regenerable, `schemaVersion` records which shape produced it,
and preserving fields whose meaning the writer cannot validate would be worse
than rewriting them. This is deliberately the opposite of the authored
`open-forge.json`, which round-trips through the JSON object model precisely
because a person owns it.

Sites to change in this slice:

- `src/cli/core/OpenForge.Cli.Core/Commands/Install/Shared/Planning/InstallEstablishmentPlanner.cs:169`
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitFrameworkLifecycleBuilder.cs:116`
- `src/cli/core/OpenForge.Cli.Core/Commands/Route/Init/Shared/Planning/RouteInitFrameworkLifecycleBuilder.cs:220`
- `src/cli/core/OpenForge.Cli.Core/Commands/Update/Shared/Planning/UpdatePlanBuilder.cs:139`

## Preconditions

- [x] `npm run build` succeeds.
- [x] Three suites green by the conventions' pass conditions.

## Steps

1. [x] Add `Framework/Ownership/Models/OwnershipWritePlanResult.cs`:
       `Planned(PlannedFileChange)`, `Unchanged()`, `Skipped(cause)`.

   **Not `Blocked`.** The lock never gates a command, so there is no state in
   which it stops one. `Skipped` means the write did not happen and the command
   continues and reports. Naming it `Blocked` invites an executor to fail the
   command, contradicting the accepted decision. _(Renamed 2026-09-12 after the
   first version shipped as `Blocked`.)_

2. [x] `WorkspaceOwnershipRead` carries `FileStateSnapshot? Snapshot`, and the
       reader populates it. Done ahead of step 3 in response to the divergence
       below. Verify: integration suite green.

3. [x] Add `Framework/Ownership/WorkspaceOwnershipStore.cs` with
       `PlanFrameworkOwnership(WorkspaceOwnershipRead current, FrameworkOwnership intended)`.
       It replaces only the `framework` section, renders with
       `WorkspaceOwnershipCodec.Write`, and builds the change from `current.Snapshot`
       — `PlannedFileChange.Create` when the snapshot kind is `Missing`, `Replace`
       otherwise, as `LifecycleStore.Planning.Support` does at its own basis. Return
       `Unchanged()` when the rendered bytes equal the snapshot's bytes.

   The four cases, decided:

   | Current lock             | `Snapshot`        | Result                                         |
   | ------------------------ | ----------------- | ---------------------------------------------- |
   | absent                   | `Missing`         | `Planned` with `Create`                        |
   | readable, understood     | `File` with bytes | `Planned` with `Replace`                       |
   | readable, not understood | `File` with bytes | `Planned` with `Replace`, rebuilt from `Empty` |
   | bytes unreadable         | `null`            | `Skipped`, command proceeds                    |

   Verify: `npm run build`.

4. [x] Add unit tests `Framework/Ownership/WorkspaceOwnershipStoreTests.cs`, one per
       row of that table, plus: writing one section preserves the others, and a
       rendered no-op yields `Unchanged` rather than a rewrite.
       Verify: unit suite green.

5. [x] **Install composition.** The ownership effect mirrors `LifecycleEffect`
       exactly, in its own slot. Do these in order; each is one edit.

   1. **Read.** `InstallPlanningInspector.cs:72` awaits
      `_lifecycleStore.ReadAsync`. Await `WorkspaceOwnershipReader.ReadAsync`
      beside it and carry the result outward.
   2. **Thread.** `InstallPlanningStages.cs:21` and `:35` each carry
      `LifecycleStoreReadResult Lifecycle`. Add
      `WorkspaceOwnershipRead Ownership` next to it in both records.
   3. **Plan.** Add `InstallFileEffect? OwnershipEffect` to `InstallPlanEffects`
      (`InstallPlan.cs:236`) and to `InstallPlan` (`InstallPlan.cs:41`).
   4. **Fold into the derived members** in `InstallPlan.cs:46-70`, treating it
      the same way `LifecycleEffect` is treated in each: `FileChanges`,
      `RecoveryTargets`, `PlannedFileCount`, `IsNoOp`, `RequiresRecovery`.
      Folding it in is what makes preflight, revalidation, recovery
      preparation and application cover it, because all four read these
      members. There is no separate wiring for those stages.
   5. **Build the effect** at `InstallEstablishmentPlanner.cs:169`, beside the
      lifecycle effect and in the same shape: `Identity` with
      `WorkspaceOwnershipDefinitions.RelativePath`, `Change`, and
      `RecoveryTarget = RecoveryBundleTarget.Create(change, snapshot)` where
      the snapshot is `input.Ownership.Snapshot`. Keep the lifecycle call.

      A `Skipped` result yields a null effect and **no finding**. Unlike
      `LifecycleWritePlanState.Blocked` directly above it, which returns
      `InstallManagementState.Blocked`, a skipped lock must not block install.
      Copy the lifecycle branch's structure, not its blocking behaviour.

   6. **Apply** at `InstallApplicationOperation.cs:348`, which handles
      `plan.LifecycleEffect`. Handle `plan.OwnershipEffect` the same way.
   7. **Keep it out of public output.** There is exactly one place the lifecycle
      effect becomes public: `InstallPlanResultProjector.cs:134` appends its
      `Identity` to the returned identities. **Do not add the ownership
      identity there**, and do not add an ownership outcome to
      `InstallResultFactsFactory`. Everything else the effect touches is
      internal: `PlannedFileCount` reaches no renderer and appears in no
      contract.

   Verify: `npm run build`, then integration suite green.

   **Why the lock is unreported here.** A1 promises no output change, and during
   dual-write the lock duplicates what the lifecycle outcome already reports. At
   A6 the existing lifecycle outcome is repointed to the lock rather than a
   second one being added, so the public result carries exactly one state-file
   outcome before, during, and after the migration.

6. [x] Repeat step 5 at `RouteInitFrameworkLifecycleBuilder.cs:116` and `:220`.
       Verify: integration suite green.

7. [x] Repeat step 5 at `UpdatePlanBuilder.cs:139`. Verify: integration suite green.

8. [x] Add an integration test asserting that after `install` the workspace contains
       both `.agents/open-forge.lifecycle.json` and `.agents/open-forge.lock.json`,
       and that the lock's `framework.paths` equals the set of files install
       actually wrote. Verify: integration suite green.

## Expected result

After `open-forge install` in a fresh workspace both files exist. The lock
contains `schemaVersion`, `$schema`, and a `framework` section whose `paths` are
the files install wrote and whose `regions` are the generated regions it
produced. No command output changes.

## Acceptance

- [x] Both files written by `install`, `route init`, and `update`.
- [x] Lock paths match what the command actually wrote, taken from the verified
      receipts, not from the payload inventory.
- [x] Three suites green.
- [x] `npm run check:dotnet` shows no new whitespace errors (5 pre-existing).
- [x] No command output changed: no contract file needs editing in this slice.

## Divergences observed

- **Pre-existing unrelated worktree change.** The worktree carried an
  uncommitted `scripts/delivery/test-suites.ts` change (`--fail-skips` from `on`
  to `off`). It was committed separately as `3ba84f3d` and is not part of A1.
  _(Corrected: this entry originally said the file was left untouched, which
  contradicted the same run's report. It was committed.)_

- **Read model carried no write basis.** Raised during step 2. The plan said to
  mirror `LifecycleStore`, but `WorkspaceOwnershipRead` returned only the decoded
  document, logical path, state and cause. `PlannedFileChange.Replace` needs the
  exact current bytes and the resolved physical path, and re-rendering the decoded
  document cannot supply them: the write normalizes and drops members this release
  does not know, so it cannot describe what is on disk.

  Both existing state readers already solve this and the new one had not copied
  them — `LifecycleStoreReadResult` carries `FileStateSnapshot? File`, and
  `LibrariesRecordRead` carries `FileStateSnapshot? Snapshot`.

  Resolved by adding `FileStateSnapshot? Snapshot` to `WorkspaceOwnershipRead`
  and populating it in the reader. Its integration tests now assert the snapshot
  for every read state. Steps 1 to 3 were rewritten with the four decided cases.
  The accepted decision did not change; the plan was under-specified.

- **Install composition was unspecified.** Raised at step 5. The plan said
  "append the change to the same plan", but Install has no ownership read input
  and no private effect slot, and the plan did not say how the change reaches
  preflight, recovery, revalidation and application, nor how it stays out of
  public output.

  Resolved by reading the Install path and writing step 5 as seven ordered edits.
  The key facts: `InstallPlanEffects` already has a dedicated `LifecycleEffect`
  slot to mirror; folding the new slot into the four derived members on
  `InstallPlan` is what makes every downstream stage cover it, with no separate
  wiring; and `InstallPlanResultProjector.cs:134` is the single place an effect
  becomes public, so omitting it there is the whole exclusion.

  Also caught while specifying: the lifecycle branch returns
  `InstallManagementState.Blocked` when its write plan is blocked. Copying that
  branch wholesale would make a skipped lock block install, contradicting the
  decision. Step 5 now says to copy its structure and not its blocking.

- **`Blocked` renamed to `Skipped`.** The first plan named the third write state
  `Blocked`, which invites an executor to fail the command. The lock never gates,
  so no such state exists: the write is skipped and the command proceeds.

- **Install has no ownership-read or hidden-effect slot.** Step 5 says to call
  `PlanFrameworkOwnership` at `InstallEstablishmentPlanner.cs:169` and append its
  change to the same plan as the lifecycle change. The current
  `InstallEstablishmentPlanInput` carries lifecycle state and current targets,
  but no `WorkspaceOwnershipRead`, and the Install plan/application models only
  one lifecycle effect plus public target effects. Threading the asynchronous
  ownership read through planning and applying/recovering its change without
  changing Install's public effect output requires a scope and composition
  decision that Step 5 does not specify. I made no Step 5 source changes and
  stopped here pending that plan clarification. Resolved by the amended Step 5
  in commit `b4158d4e`, which specified the read threading, private effect slot,
  derived-plan folding, application point, and public-output exclusion.

- **Integration cleanup did not know the new lock file.** After Step 5 was
  implemented, the direct integration suite failed during temporary-workspace
  disposal because Install-created `.agents/open-forge.lock.json` was not in the
  Install and Status cleanup allowlists. The same new file also needed to be
  excluded from the existing helper that enumerates installed payload paths.
  Added that exact file to those existing test expectations; no product or
  public-output behavior was changed.

- **Published end-to-end cleanup did not know the new lock file.** The final
  end-to-end suite found twelve cleanup-only failures because its published
  temporary-workspace allowlist also did not include Install-created
  `.agents/open-forge.lock.json`. Added the same ownership path to that fixture
  cleanup; no product or public-output behavior was changed.

- **Initial rebuild overlapped a still-running failed test process.** The first
  end-to-end invocation had reported failures but still held its test DLL while
  the rebuild started, so the build exhausted its copy retries. Waited for that
  process to exit before rebuilding; this was execution ordering only, not a
  source failure.

- **Route Move and Route Remove have separate published cleanup fixtures.**
  After the shared published Install cleanup was fixed, the end-to-end suite
  still found seven cleanup-only failures in those two workspaces because they
  each maintain their own allowlist. Added the ownership path to both fixture
  cleanups; no product or public-output behavior was changed.

- **The whitespace baseline included new-file errors.** `check:dotnet` reported
  nine errors rather than the plan's five: the five expected unrelated errors
  plus four formatting errors in the new `OwnershipWritePlanResult.cs`. Fixed
  those four touched-file errors; the remaining five are in untouched files.

- **Route Init has a different effect composition boundary.** The plan said to
  repeat the Install composition at the two `PlanFrameworkUpdate` sites in
  `RouteInitFrameworkLifecycleBuilder`, but Route Init has no private
  `LifecycleEffect` slot: lifecycle and filesystem effects share its internal
  `FileChanges` collection, and `RouteInitPlanResultProjector` filters the
  lifecycle path from public effects. Carried the ownership read through the
  trusted Framework basis, added the ownership change and recovery target to
  that same internal collection, and filtered the lock path alongside the
  lifecycle path. Revalidation, preflight, recovery, application, and final
  verification already consume those collections; public output remains
  unchanged.

- **Fresh Install classifies generated content in new files as whole-file
  creates.** The initial ownership composition derived lock regions only from
  `GeneratedRegion` effects, so a fresh Install lock had no regions even though
  the lifecycle record contained the generated regions written into those new
  files. Route Init exposed this by repairing the lock during an otherwise
  verified no-op. Changed Install to derive ownership regions from the intended
  lifecycle generated-region set, which covers both new-file creates and
  existing-file region replacements.

- **Route Init's untracked-source fixture intentionally starts with stale lock
  ownership.** After the Install region correction, the only remaining Route
  Init failures were the two canonical/compatibility cases that remove a
  lifecycle target before invoking Route Init. The command correctly rewrites
  the lock to stop claiming that user-owned target, so the fixture's full
  snapshot changes even though its payload and lifecycle remain unchanged.
  Updated that assertion to compare all workspace entries except the expected
  ownership-lock reconciliation; the test still asserts the lifecycle bytes
  and public effects.

- **Review finding: a weakened assertion left the idempotent write unproven.**
  `UntrackedUnchangedSourceRemainsUserOwned` asserted the whole workspace was
  unchanged. Route Init now legitimately creates the lock there, so the check was
  narrowed to `SnapshotHashesWithoutOwnership`. That is the right call for that
  test, but the exclusion would equally hide a lock rewritten on every run, and
  nothing else proved otherwise.

  Closed by adding `OwnershipLockIsCreatedOnceAndIsStableOnRepeat`, which asserts
  the lock is created and that a second run leaves it byte-identical. It passes,
  so the `Unchanged()` path works end to end and the implementation was correct;
  only the evidence was missing.

- **Flaky test, unrelated to this slice.** `Route Update cancellation after the
first effect retains its receipt and stops later effects` fails intermittently
  and passes on rerun. Seen twice on different days, before and after this slice.
  Not caused by A1; worth its own investigation.

## Rollback

Revert the slice's commits. The lock file is additive and unread, so a workspace
written by the partial slice stays valid.

## Continued Task 30 review

The A5 preparation review identified R6: root managed-block receipts and retention of unchanged whole-file receipts.
The correction and fresh verification are recorded in
[A5 prerequisite review verification](05-a5-status-doctor-library-readers.md#prerequisite-review-verification).
The original execution evidence above describes its original committed baseline.

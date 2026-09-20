---
open-forge:
  description: Task 30 G1 step 4 slice A3 execution plan for Library dual-write into the lock
  tags: [Memory, Working, CLI, Task, Plan, Contextual, Active]
---

# A3 — Library dual-write

> Read [00 — Slice conventions](00-conventions.md) first: verification
> commands, pass conditions, the recurring composition pattern, and the rules
> every slice shares. This plan does not repeat them.

## Goal

`library attach`, `detach`, and `sync` write their ownership into
`.agents/open-forge.lock.json` alongside `.agents/open-forge.libraries.json`.
Nothing reads the lock yet.

## Depends on / Blocks

- Depends on: A1.
- Blocks: A4.

## References

Sites, all routed through one policy:

- `src/cli/core/OpenForge.Cli.Core/Commands/Library/Shared/Planning/LibraryMutationPlanningPolicy.cs:127`
  `CreateRecordChange` — the single place that builds the record change.
- Callers: `Library/Attach/Shared/Planning/LibraryAttachPlanner.cs:148`,
  `Library/Detach/Shared/Planning/LibraryDetachPlanner.cs:103` and `:104`,
  `Library/Sync/Shared/Planning/LibrarySyncPlanner.cs:125`.

Model to record: `LibraryOwnership(Id, SourceRoot, DestinationRoot, Paths)` in
`Framework/Ownership/Models/Document/WorkspaceOwnershipDocument.cs`.

Recorded constraint: **directories are not recorded.** `LibraryDetachPlanner`
plans `Directories = []`, so detach never removes a directory attach created.
Only the leaf links go in `paths`.

## Preconditions

- [x] A1 committed; three suites green.

## Steps

1. [x] Add `PlanLibraryOwnership(WorkspaceOwnershipRead current, ImmutableArray<LibraryOwnership> intended)`
       to `WorkspaceOwnershipStore`, replacing only the `libraries` section.
       Verify: `npm run build`, unit suite green.

2. [x] Add a sibling `CreateOwnershipChange(...)` to
       `LibraryMutationPlanningPolicy`, returning the lock's `PlannedFileChange?`
       from the same intended library set that produces `intendedBytes`.
       Verify: `npm run build`.

3. [x] **Resolve the property-name collision first.** `ILibraryMutationObservation`
       already has `LifecycleOwnershipReadResult Ownership` — the lifecycle's claims
       read, a different concept — with 21 call sites.

   Rename that existing property to `LifecycleOwnership`, then use `Ownership`
   for the new `WorkspaceOwnershipRead`. Do not name the new one
   `WorkspaceOwnership`: Install, Route Init and Extension Install all call
   theirs `Ownership`, and Library should not be the exception.

   The rename is mechanical and the compiler finds every site. It also earns its
   keep at A6, where `LifecycleOwnershipReadResult` is deleted and the clearer
   name makes its call sites obvious.

   Verify: `npm run build`, three suites green. Commit this rename on its own,
   before any ownership work, so the behaviour-preserving rename is reviewable
   separately.

4. [x] **Library composition.** Follow the conventions' six-step pattern. The
       Library carriers, named:

   | Pattern step | Library site                                                                                                                         |
   | ------------ | ------------------------------------------------------------------------------------------------------------------------------------ |
   | Slot         | `PlannedFileChange? RecordChange` on `Models/Planning/LibraryMutationEffects.cs:13`                                                  |
   | Thread       | `PlannedFileChange? RecordChange` on `Models/Application/LibraryMutationApplicationRequest.cs:20`                                    |
   | Apply        | `LibraryMutationApplicationRunner`, where it applies `RecordChange` under the lease                                                  |
   | Exclude      | wherever the record's effect becomes a public completion fact; find it in `Shared/Completion/LibraryMutationCompletionProjection.cs` |

   Add `OwnershipChange` beside `RecordChange` in both models and apply it in the
   same runner, under the same lease, revalidation and recovery preparation.
   **Do not apply it outside that runner.**

   Verify: `npm run build`.

5. [x] Wire the three planners at the call sites listed above, deriving
       `LibraryOwnership` from the same intended library set that produces
       `intendedBytes`. Detach with `delete: true` removes that library's entry from
       `libraries[]`; it does not delete the lock.

   **Only leaf links go in `paths`.** `LibraryDetachPlanner` plans
   `Directories = []`, so detach never removes a directory attach created, and
   recording directories would imply a cleanup the CLI does not perform.

   Verify: integration suite green.

6. [x] Cover these cases, extending an existing fixture where one fits:
   - Attach two libraries, detach one; the lock retains the other with its exact
     leaf paths and no directory appears in any `paths`.
   - Attach, then attach again with no change; the lock is byte-identical.
   - A failed link effect leaves no lock write.

   Verify: integration suite green.

## Expected result

The lock's `libraries[]` mirrors `.agents/open-forge.libraries.json` after every
library mutation, with leaf links only. No command output changes.

## Acceptance

- [x] All three commands dual-write through the existing mutation runner.
- [x] Detach removes one entry, never the file.
- [x] No directory recorded in any `paths`.
- [x] Three suites green; no contract file edited.

## Divergences observed

- **Step 3's Thread instruction conflicts with the existing Library ownership carrier.**
  The plan says to add `WorkspaceOwnershipRead Ownership` to each Library
  planning input beside the existing record read. The current
  `LibraryAttachPlanningInput`, `LibraryDetachPlanningInput`, and
  `LibrarySyncPlanningInput` already define `Ownership` as
  `LifecycleOwnershipReadResult`, and the three planners pass that value to
  `TryFindOwnershipConflict` for the existing lifecycle ownership boundary.
  Replacing it would remove that behavior; adding a second carrier requires a
  property name and ownership decision that the plan does not specify. No Step
  3 source changes were made; execution stopped at this boundary.

- **Property-name collision.** Raised at step 3. `ILibraryMutationObservation`
  already has `LifecycleOwnershipReadResult Ownership` — the lifecycle's claims
  read, an unrelated concept — across 21 call sites, so the conventions' name for
  the new read was taken.

  Resolved by renaming the existing property to `LifecycleOwnership` and keeping
  `Ownership` for the new `WorkspaceOwnershipRead`, rather than making Library
  the one command whose property is spelled differently. Install, Route Init and
  Extension Install already use `Ownership`.

  The rename is behaviour-preserving and compiler-verified, and it pays off at
  A6, where `LifecycleOwnershipReadResult` is deleted and the explicit name makes
  its call sites obvious. It is committed on its own, before any ownership work,
  so it reviews as the pure rename it is.

  The conventions gained the general rule, so A4 and A5 do not have to ask again.

- **The initial rename missed one interface consumer.** The first Step 3 build
  found `LibraryMutationCompletionProjection.cs:181` still reading
  `observation.Ownership`, even though `observation` is an
  `ILibraryMutationObservation`. Changed it to `observation.LifecycleOwnership`;
  no behavior or output changed.

- **Step 5 integration exposed a Library fixture cleanup gap.** The plan
  expected the existing integration suite to remain green after the Library
  mutation runner began creating `.agents/open-forge.lock.json`, but
  `LibraryMutationWorkspace` delegates teardown to `TemporaryWorkspace`, whose
  ownership ledger correctly rejects effect-created files it did not create.
  Added explicit ordinary-file cleanup for the Library ownership path before
  delegating teardown; the lock remains included in workspace snapshots and
  assertions.

- **Step 5's ownership no-op fold exposed one stale pre-A3 fixture.** The plan
  requires the ownership change to participate in the Sync no-op test. The
  existing source-byte regression seeded the libraries record without its new
  matching lock, so the intended ownership reconciliation was correctly an
  application effect and the old `NoOp` assertion failed. Seeded the matching
  lock in that test so it continues to exercise the unchanged-projection
  no-op case.

- **Step 6's repeat wording meets a pre-existing duplicate-ID guard.** The plan
  says to attach again with no change, but `LibraryAttachPlanner` explicitly
  blocks an already-registered library ID before application. Re-ran the same
  attach request and asserted that this blocked no-change attempt leaves the
  first lock byte-identical; no production duplicate-ID behavior was changed.

- **Step 6's two-library case initially crossed the permission boundary.** The
  first test setup put the second library under `docs/.agents`, and the actual
  permission planner correctly returned `PermissionRequired` before the second
  attach. Extended the existing fixture for a distinct source root and placed
  the second leaf under the already-covered `.agents` consumer boundary, so the
  test exercises two-library ownership retention without adding permission
  behavior to the case.

- **Final e2e verification exposed a published-fixture cleanup gap.** The
  ownership writer now creates `.agents/open-forge.lock.json`, but
  `PublishedLibraryWorkspace` delegated teardown to `TemporaryWorkspace`, whose
  ownership ledger rejected that effect-created file and left `.agents`
  non-empty in four existing Library process tests (two Sync, one Attach, and
  one Detach). Added the same guarded ordinary-file cleanup for the published
  fixture before teardown; no production behavior or output changed.

- **The published unchanged-Sync fixture lacked its matching ownership state.**
  After the cleanup fix, the read-only snapshot in the existing no-op test
  showed the actual Sync application adding `.agents/open-forge.lock.json` to a
  record that had no lock entry. Seeded the fixture with the canonical matching
  Library ownership bytes so the test remains an effect-free no-op.

## Rollback

Revert the slice. The lock stays unread.

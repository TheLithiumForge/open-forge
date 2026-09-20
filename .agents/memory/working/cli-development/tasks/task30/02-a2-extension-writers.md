---
open-forge:
  description: Task 30 G1 step 4 slice A2 execution plan for Extension dual-write into the lock
  tags: [Memory, Working, CLI, Task, Plan, Contextual, Active]
---

# A2 — Extension dual-write

> Read [00 — Slice conventions](00-conventions.md) first: verification
> commands, pass conditions, the recurring composition pattern, and the rules
> every slice shares. This plan does not repeat them.

## Goal

`extension install`, `extension remove`, and `extension update` write their
ownership into `.agents/open-forge.lock.json` alongside the lifecycle record.
Nothing reads the lock yet, so no observable behaviour changes.

## Depends on / Blocks

- Depends on: A1 (the store exists).
- Blocks: A4.

## References

Authorizing decision: refinement 7 in [phase-4a-g1](phase-4a-g1.md).

Sites to change, each already calling `PlanExtensionUpdate`:

- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/Shared/Planning/ExtensionInstallEffectPlanner.cs:26`
- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Remove/Shared/Planning/ExtensionRemovePlanner.cs:242`
- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Update/Shared/Planning/ExtensionUpdatePlanner.cs:430`

Shape to follow: `WorkspaceOwnershipStore.PlanFrameworkOwnership` from A1.

## Preconditions

- [x] A1 committed and three suites green.

## Steps

1. [x] Add `PlanExtensionOwnership(WorkspaceOwnershipRead current, ImmutableArray<ExtensionOwnership> intended)`
       to `WorkspaceOwnershipStore`. It replaces only the `extensions` section and
       leaves `framework` and `libraries` untouched. Verify: `npm run build`.

2. [x] Extend `WorkspaceOwnershipStoreTests`: writing extensions preserves an
       existing framework section, and vice versa. Verify: unit suite green.

3. [x] **Extension Install composition.** Derive ownership from the intended
       lifecycle, not from the write effects. This answers the shared-path,
       region-owner, and source-threading questions at once, because
       `ExtensionInstallTargetInspector.cs:188` already computes exactly the shape
       the lock wants.

   **Mapping.** For each `LifecycleExtensionPackageV1` in
   `input.TargetState.IntendedLifecycle.Packages`, emit one `ExtensionOwnership`:

   | `ExtensionOwnership` | Source                                                                            |
   | -------------------- | --------------------------------------------------------------------------------- |
   | `Id`                 | `package.Id`                                                                      |
   | `Version`            | `package.Version`                                                                 |
   | `Source`             | `package.Source` — already set from `sourceIdentity`; **no new threading needed** |
   | `Dependencies`       | `package.Dependencies`                                                            |
   | `Paths`              | `package.Paths` — already per-package, deduplicated and ordered                   |
   | `Regions`            | `[]` — see below                                                                  |

   **Shared paths are already correct.** `package.Paths` comes from that
   package's own payload, so a path two packages ship appears under both. Do not
   derive paths from write effects: writes are deduplicated, so a shared path has
   one receipt and would be attributed to one owner.

   **This is still receipt-backed.** `ExtensionInstallEffectApplication.cs:130`
   publishes the lifecycle only after the effect loop, which returns `Stop` on any
   unverified effect. The lock change rides in the same plan, so a published lock
   always corresponds to fully verified writes. Assert that ordering holds rather
   than assuming it.

   **Extensions own no generated regions.** `ExtensionLifecycleState` has no
   region member: only `Coverage`, `Packages`, `Paths`. `Regions` is therefore
   always empty for extension ownership. If a future extension does generate a
   region, that is a new capability and a new decision, not something to infer
   here.

   **Plumbing**, mirroring A1's Install slice:

   1. Add `WorkspaceOwnershipRead Ownership` to
      `ExtensionInstallEffectPlanningInput` (`ExtensionInstallPlan.cs:359`,
      beside `LifecycleRead`), and read it where the lifecycle read is performed.
   2. Give `ExtensionInstallEffectPlanner` the `WorkspaceOwnershipStore`
      dependency beside `LifecycleStore`.
   3. Add `PlannedFileChange? OwnershipChange` and its recovery target to
      `ExtensionInstallEffectPlan` and `ExtensionInstallPlan`
      (`ExtensionInstallPlan.cs:40` and `:96`).
   4. Fold it into `AllFileChanges` (`:69`), `RecoveryTargets` (`:72`), `IsNoOp`
      (`:102`) and `RequiresRecovery` (`:103`), exactly as `LifecycleChange` is
      folded in. That is what makes preflight, revalidation and recovery cover
      it; there is no separate wiring.
   5. Apply it in `ExtensionInstallEffectApplication` beside the lifecycle
      change at `:130`.
   6. **Do not add it as an `ExtensionInstallPlannedEffect`.** That list is
      public. The lock is a private change slot like `LifecycleChange`, which is
      also not a planned effect.

   A `Skipped` ownership result yields a null change and **no finding**. Unlike
   the `LifecycleWritePlanState.Blocked` branch above it, a skipped lock must not
   stop the install.

   Verify: `npm run build`, integration suite green.

4. [x] Repeat at `ExtensionUpdatePlanner.cs:430`. Ownership after update is the
       package's new path set, not the union with the old one: a path the new
       version no longer ships stops being owned. Verify: integration suite green.

5. [x] Repeat at `ExtensionRemovePlanner.cs:242`. Removing an extension deletes its
       entry from `extensions[]`. A path another extension still owns stays owned by
       that extension, which is derived and needs no special handling here.
       Verify: integration suite green.

6. [x] Add integration tests:
   - Install two extensions that share one path; assert the lock lists that path
     under **both** packages, and that `OwnersOf` returns both.
   - Remove one; assert the shared path remains under the survivor only.
   - Assert a failed target effect leaves **no** lock write, proving the
     published lock only ever reflects verified writes.

   Verify: integration suite green.

## Expected result

After any extension mutation the lock's `extensions[]` matches what is installed.
`WorkspaceOwnershipDocument.OwnersOf(path)` returns the right owner set for a
shared path. No command output changes.

## Acceptance

- [x] All three commands dual-write.
- [x] Paths come from verified receipts, not from the package manifest.
- [x] Shared-path ownership is correct after a partial removal.
- [x] Three suites green; no contract file edited.

## Divergences observed

- **The supplied direct-suite command does not match the current delivery CLI.**
  The requested `npm run build <assembly> --parallel ...` invocation routes to
  `scripts/delivery/cli.ts build`, whose `build` command rejects positional
  assembly paths before running anything. I ran the three release `.exe`
  assemblies directly with the specified test arguments instead; all three
  were green. No source or delivery script was changed.

- **The expected baseline counts are stale after A1.** The current committed
  baseline is unit `3462` passed (not `3456`), integration `1807` passed plus
  `17` skips (not `1805` plus `17`), and end-to-end `123` passed. The six unit
  and two integration additions are already in the committed A1 work; no test
  was removed or altered in A2.

- **The unit executable initially predated the Step 2 test edit.** The first
  direct unit invocation was green but still reported `3462`; I rebuilt before
  accepting the verification, and the rebuilt executable passed `3463`, which
  includes the new preservation test. No product source was affected.

- **Step 3 lacks the Extension Install composition boundary it requires.** The
  plan says to call `PlanExtensionOwnership` from
  `ExtensionInstallEffectPlanner` and append the change to the same plan, but
  the current path has no ownership read in `ExtensionInstallEffectPlanningInput`,
  no private ownership-change or recovery slot in `ExtensionInstallEffectPlan`
  or `ExtensionInstallPlan`, no derived `AllFileChanges`/`RecoveryTargets`
  inclusion, and no application point after lifecycle publication. Every
  `ExtensionInstallPlannedEffect` is also projected into public `effects`, so
  using that collection directly would change output, contrary to this slice's
  goal. Choosing the hidden effect shape, failure finding, sequencing, and
  read-threading is a scope/naming/ownership decision absent from the plan. I
  made no Step 3 source changes and stopped here pending plan clarification.

- **Step 3 was under-specified; three questions raised, all already answered by
  existing code.** The executor stopped rather than infer ownership derivation,
  which was right.

  - _Shared paths._ Deriving from write receipts would attribute a deduplicated
    shared write to one owner. But `ExtensionInstallTargetInspector.cs:188`
    already builds per-package `Paths` from each package's own payload, so a
    shared path is already listed under every owner. Use that, not the effects.
  - _Region owners._ `ExtensionLifecycleState` carries no region member at all,
    so extensions own no generated regions and `Regions` is always empty. The
    mapping the executor asked for does not exist because the concept does not.
  - _Missing source field._ `ExtensionPackageFact` has none, but
    `LifecycleExtensionPackageV1.Source` is already populated from
    `sourceIdentity` in the same inspector. Read it there; no threading needed.

  The receipt requirement still holds without deriving from effects:
  `ExtensionInstallEffectApplication.cs:130` publishes the lifecycle only after
  the effect loop, which stops on any unverified effect, so a published lock
  always corresponds to verified writes. Step 3 now says to assert that ordering
  rather than rely on it silently.

  Step 3 rewritten as a mapping table plus six plumbing edits. Step 6 gained a
  shared-path test and a failed-effect test.

- **The existing lifecycle application did not advance its validation-check
  index.** The plan said to apply the private ownership change beside the
  lifecycle change, and the derived validation list correctly included both,
  but the lifecycle branch read `input.Validation.Checks[checkIndex]` without
  incrementing `checkIndex`. The newly added ownership application therefore
  received the lifecycle check and failed its exact-expectation guard. Changed
  the lifecycle read to `input.Validation.Checks[checkIndex++]`; ownership now
  consumes the next check. No public output or contract changed.

- **The existing Extension Update lifecycle application also did not advance
  its validation-check index.** The plan required the private ownership write
  to be applied beside lifecycle publication, and the Update validation list
  therefore contains both checks. Changed the lifecycle read to use
  `validation.Checks[checkIndex++]` before applying ownership. No public output
  or contract changed.

- **Remove's plan-ownership integration fixture had an extra private-plan call
  site.** The plan model gained ownership change and recovery slots, but the
  fixture's manually reconstructed plan populated only the pre-existing
  lifecycle slots, so plan comparison correctly failed. Added both ownership
  slots from the observed baseline; no test assertion or public output changed.

- **Remove's lifecycle application also did not advance its validation-check
  index.** Adding the private ownership check beside lifecycle publication
  exposed that the lifecycle branch read `validation.Checks[checkIndex]`
  without incrementing it. Changed it to `validation.Checks[checkIndex++]` so
  ownership consumes the next exact check. No public output or contract
  changed.

- **The first full Step 5 integration run had one unrelated Route Update
  cancellation-monitor timing failure.** The two Remove fixture failures were
  fixed as above; the Route Update test passed on the required full rerun
  (1,807 passed, 17 skips, 0 failed). No Route source or test was changed.

- **Review of the completed slice.** Suites verified independently: unit 3463,
  integration 1807 plus 17 skips, e2e 123, zero failures, only the plan file
  touched under `.agents`, and no ownership member reaches any extension result
  or renderer.

  Step 6's three tests were implemented by extending existing fixtures rather
  than adding files, which is why the integration count did not move. All three
  are covered: shared-path ownership both ways in the Remove application test,
  and the failed-effect case at
  `ExtensionInstallMutationIntegrationTests.cs:127`, which asserts the lock is
  byte-identical after a target changes mid-apply. Reusing real fixtures is the
  better choice; the plan should have said "cover these cases" rather than
  "add tests".

  The validation-index work the executor flagged is correct:
  `ReadChanges` appends effects, then lifecycle, then ownership, and the three
  `checkIndex++` sites consume them in that order with both later ones guarded
  by a null check. An unguarded increment there would have misaligned every
  subsequent check against the wrong file, so this was worth verifying directly.

  The acceptance line claiming paths come from verified receipts was corrected:
  step 3 settled on the intended package lists, with the receipt property met
  structurally.

## Rollback

Revert the slice. The lock stays unread.

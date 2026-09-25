---
open-forge:
  description: Task 30 G1 step 4 slice A4 execution plan for flipping Extension readers to the lock and collapsing the remove classification
  tags: [Memory, CLI, Task, Plan, Contextual, Archived, Historical]
---

# A4 — Extension readers flip

> Read [00 — Slice conventions](../../../../working/cli-development/tasks/task30/00-conventions.md) first: verification
> commands, pass conditions, the recurring composition pattern, and the rules
> every slice shares. This plan does not repeat them.

> Historical G1 record: its output examples and snapshots predate Task 30 G4.
> Current CLI output uses one schema-3 report with `--format`, `--detail`, and
> `--detail-filter`; the older flag and status vocabulary below is retained as
> evidence of what was true when A4 ran. Its serial qualification commands are
> also superseded by the six-suite `--parallel collections` delivery setting.

## Goal

`extension install`, `remove`, and `update` read ownership from the lock instead
of the lifecycle record. `extension remove` loses the changed-versus-unchanged
distinction and always deletes a path it solely owns.

**First behaviour-changing slice.** Output changes; contracts change with it.

## Depends on / Blocks

- Depends on: A1, A2. Dual-write keeps both files current, so this flip is safe
  without flipping every other reader at the same time.
- Blocks: A6.

## References

Authorizing decision: refinement 8 in [phase-4a-g1](phase-4a-g1.md) — the
baseline was the only input to the classification, so it collapses.

Sites:

- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Remove/Shared/Planning/ExtensionRemovePathInspector.cs`
  — `ReadClassification` and `Classify` are deleted; owners come from
  `WorkspaceOwnershipDocument.OwnersOf(path)`.
- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Remove/Models/Planning/ExtensionRemovePathObservation.cs`
  — `ToPathPlan` collapses.
- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Remove/Models/Planning/ExtensionRemovePathPlan.cs`
  — `ValidateAction` collapses.
- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Remove/ExtensionRemoveDefinitions.cs:213`
  — `ReadMachineName(ExtensionRemoveChangedContentPolicy)` is deleted.
- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/Install/Shared/Planning/ExtensionInstallFoundationReader.cs`
  — reads installed ownership.

Contract to update **in this slice**:

- `.agents/memory/crystallized/documents/cli/contracts/extension/remove/interface.md`
  — the `--prune` row at line 177 and the "Keep-as-unmanaged and Delete"
  section at line 181.
- the matching `behavior.md`.

## Preconditions

### Execution capsule

- Baseline: `7fb104e468381b7cb69895899fca99debe183ba0`. Work is isolated on
  `task30-a4-extension-readers`; the source checkout was clean.
- Profile: direct sequential work. Review covers the completed A1–A3 writers
  and the A4 deletion boundary. No delegated execution is selected.
- Consequence: removal changes ordinary user files. Recovery must preserve
  their bytes before deletion. Stable directory mappings and cooperating
  processes remain the supported boundary.
- Reuse: the ownership codec and reader, existing destination checks, mutation
  lease, expected-state validation, recovery bundle, and Imprint snapshots.
  Exceptional machinery: none.
- Evidence: Unit snapshots cover human and JSON output in both views;
  Integration must prove recovery ordering and stale-claim containment;
  EndToEnd must prove the removed option diagnostic. The three complete managed
  suites and formatting check are required. A behavior change also triggers
  the supported Native AOT gate.
- Verify the recorded paths against the shared allow list, reserved targets,
  and physical containment required by refinement 7. The maintainer resolved
  the original stronger interpretation under A4-R1.

- [x] A1 and A2 committed; three suites green.
- [x] A snapshot baseline exists for `extension remove` output. If none, capture
      it first with an Imprint snapshot test, review it, and commit it **before**
      changing behaviour. Capturing after the change proves nothing.

## Steps

1. [x] Capture the `extension remove` output snapshot baseline if absent. Commit it
       as its own commit. Verify: unit suite green, snapshot file present.

2. [x] Replace the ownership source in `ExtensionRemovePathInspector`: owners come
       from the lock via `OwnersOf(path)`; delete `ReadClassification` and
       `Classify`. Verify: `npm run build`.

3. [x] Collapse `ExtensionRemovePathClassification` to `Shared`, `FinalOwner`,
       `Missing`. Update `ToPathPlan` and `ValidateAction`: `FinalOwner` maps to
       `Delete`, always. Delete the `KeepAsUnmanaged` action and the
       `ExtensionRemoveChangedContentPolicy` enum. Verify: `npm run build`.

4. [x] Remove the `--prune` option from `ExtensionRemoveDefinitions` and its
       binding. **A removed option must be reported, not silently ignored**: confirm
       the parser's unknown-option diagnostic fires for `--prune` and add an e2e
       case asserting it. Verify: e2e suite green.

5. [x] Constrain deletion: delete only a path that still exists and still resolves
       inside the extension's declared destination boundary. The existing
       `ExtensionDestinationPolicy.IsAllowed` and `HasOrdinaryAncestors` checks stay.
       Verify: integration suite green.

6. [x] Flip `ExtensionInstallFoundationReader` to the lock. Verify: integration green.

7. [x] Update the two `extension/remove` contract files: delete the `--prune` row
       and the Keep-as-unmanaged section, and state that a solely owned path is
       deleted with the recovery bundle as the protection. Verify:
       `npx prettier --check` on the changed files.

8. [x] Review the snapshot diff. It must show only the removal of the
       keep-as-unmanaged outcome and the changed/unchanged distinction. Anything
       else is a divergence — record it below and stop.

## Expected result

`extension remove` deletes every path it solely owns. A user-edited file is
deleted; the recovery bundle written before deletion is the protection, and
`git diff` is the other. `--prune` is rejected as an unknown option.

## Acceptance

- [x] Snapshot baseline captured before the change, diff reviewed after.
- [x] Recovery bundle still written before any deletion — assert it directly.
- [x] A stale lock cannot over-delete: a path absent from disk or outside the
      destination boundary is never deleted.
- [x] Both `extension/remove` contract files updated in the same commit as the
      behaviour.
- [x] Three suites green.

## Divergences observed

- **A4-R1: the declared destination boundary has no independent representation.**
  Severity: high. Category: deletion safety / accepted-contract readiness.
  Step 5 and accepted refinement 7 require a path to resolve inside the
  extension's declared destination boundary. `ExtensionOwnership` in
  `Framework/Ownership/Models/Document/WorkspaceOwnershipDocument.cs` records
  ID, version, source, dependencies, paths, and regions. It has no destination
  root or separate destination declarations. `ExtensionManifestDocument` in
  `Framework/Extensions/Models/ExtensionPackageJsonModels.cs` has no such field
  either. `Commands/Extension/Shared/Permissions/ExtensionDestinationPolicy.cs`
  checks portable workspace paths, reserved targets, and recovery storage;
  `HasOrdinaryAncestors` checks physical traversal. Neither establishes a
  package-specific boundary for an otherwise ordinary `.agents/unrelated.md`
  claim. `OwnersOf` derives owners from the same path list, so it cannot supply
  an independent check. An explicit package source may be gone at removal time,
  and refinement 7 rejects checking against a later payload.

  Initial disposition: clarification pending before the reader/deletion change. Asked
  whether the existing workspace destination policy is the intended boundary,
  or whether a separate per-extension boundary must first be defined and
  recorded. The smallest correction depends on that meaning: use and assert the
  existing checks if they define the boundary; otherwise accept the retained
  boundary shape before enabling deletion. No new schema or inferred boundary
  has been introduced. Earliest affected boundary: A4 acceptance readiness.

  Follow-up clarification: the missing separate boundary is an ambiguity in the
  interpretation of the plan, not proof that the accepted receipt design needs
  another field. The plan explicitly names the existing destination checks.
  The recommendation is to use those checks as the boundary, trust the exact
  verified ownership receipts inside it, and preserve recovery for deleted
  content. Do not introduce a separate per-extension root without an explicit
  requirement for one.

  Resolved by the maintainer: the allow list is shared by all owners. Use the
  existing allow-list, reserved-path, and physical destination checks. The
  plan's wording did not require an independent per-extension declaration;
  that interpretation was unnecessarily stronger than its named checks.
  Recorded the clarification in refinement 7 and Workspace State Files.
  Continue A4 without changing the ownership schema.

- **A4-R2: successful removal deletes the proposed protection.** Severity:
  high. Category: recovery / contract alignment. The plan treats preparation
  before deletion as sufficient protection for edited files. The current
  `ExtensionRemoveApplicationOperation.ExecuteUnderLeaseAsync` calls
  `ExtensionRemoveRecoveryApplication.CleanupAsync` after final verification;
  that method calls `RecoveryBundleDeletionGuard.DeleteAsync`. The Remove
  Interface and Behavior contracts explicitly require this deletion, and
  `AppliesRemovalAndPreservesUnownedContent` asserts recovery state `removed`.
  Thus the prepared bundle protects interrupted operations but does not retain
  edited bytes after successful removal. Git cannot recover uncommitted bytes
  that were never recorded there.

  Disposition: the accepted promise to protect edited deletions logically
  requires retaining their recovery bundle after success. Implement that
  change, its exact recoverable path, and direct byte assertions with A4 once
  R1 is resolved. Check the next-command recovery-catalogue behavior, which
  currently treats retained bundles as a conflict. This is an additional
  snapshot/contract change beyond Step 8's classification-only description;
  the accepted protection takes precedence. Earliest affected boundary: A4's
  behavior and recovery acceptance, not the A1–A3 dual-write contract.

- **The Goal includes Extension Update, but the Steps omit its reader flip.**
  `ExtensionUpdatePlanner` still selects installed packages from lifecycle
  state. A4 must follow the Goal and migrate that reader along with Install and
  Remove. Merely switching `ExtensionRemovePathInspector` would also leave
  Remove's selection, dependencies, and early lifecycle gates authoritative.
  These directly related consumers must be handled before A4 is complete.

- **Snapshot coverage was absent.** Added four Imprint output snapshots in the
  mirrored `Commands/Extension/Remove/Shared/Rendering` test scope, covering
  human/JSON and compact/expanded views. The fixture exposes changed, unchanged,
  shared, and missing paths. Explicit capture ran only those four tests; the
  full Unit verification ran with `CI=true` to disable baseline creation.
  Commit `b56e3d9c` contains only this pre-change test and its snapshots.

- **Isolated build dependencies were not linked.** The first offline build
  failed before compilation because the new worktree could not resolve the
  already-installed `semver` package. Linked its `node_modules` to the prepared
  local dependencies and used `npm run build -- --offline`, then
  `npm run build -- --no-restore`. No dependencies were downloaded. Offline
  restore does not provide a vulnerability audit.

- **The managed suites cannot run concurrently with the current Cleanup
  fixture.** Integration passed, but the concurrent EndToEnd run had two
  Cleanup snapshot failures: another suite added workspace directories under
  the shared recovery store between snapshots. Rerun EndToEnd only after
  Integration exits. The serial rerun passed all 123 cases with zero failures
  or skips. Keep these suites sequential for subsequent slices.
  This was verification scheduling, not a product regression; the delivery
  script remains untouched.

- **Recovery retention uses the existing catalogue contract.** The old Steps
  assumed successful cleanup could coexist with edited-byte protection. It
  cannot. Remove now verifies and retains the final bundle, reports its exact
  path, and leaves subsequent mutation conflicts and explicit Cleanup under
  their existing rules. An absent-ID Remove no-op performs no mutation and may
  complete before the recovery-conflict check. No new archive kind, completion
  marker, or recovery schema was introduced.

- **The reader flip reaches planning and revalidation.** Selection, dependency
  planning, current package/path ownership, cross-manager checks in Remove,
  and intended lock composition now use lock facts. Install revalidation also
  compares its observed lock even when the intended lock write is unchanged.
  Legacy data remains only for transitional writes and the comparison and
  Framework-currentness removal already assigned to A6. No synthetic baseline
  fingerprints were introduced to make lock facts fit a legacy reader.

- **Old fixtures manufactured ownership in the lifecycle record.** Changed
  Remove Framework/Library collision and shared permission-revocation fixtures
  to modify the lock they are testing. Removed duplicate Keep/prune tests and
  retained their edited/binary file scenarios under final-owner deletion.

- **Dependency planning assumed validated lifecycle data.** Lock records may
  contain duplicate identities or cyclic dependency claims. The existing plan
  contract cannot interpret them as a safe dependency order. Remove now reports
  that observation and completes without deletion, preserving the accepted
  failure direction. It does not manufacture an ordering or add a schema.
  Direct request-level assertions cover both cases.

- **Snapshot consequences include derived facts.** Removing Keep-as-unmanaged
  also removes its managed-divergence finding, changes the representative
  status from attention to complete, adds the edited file's delete effect and
  recovery path, and removes prune from both outputs. These follow the accepted
  behavior; a classification-only textual diff would leave an inconsistent
  fixture. The reviewed pre-change baseline remains committed separately.

- **Update still tied current-byte computation to a legacy baseline.** The new
  three-command disagreement test exposed an attempted replacement of identical
  bytes when the lock recorded a package and the legacy section did not. Compute
  current hashes directly from observed bytes when no legacy comparison fact
  exists. Final Update verification compares legacy state only when that plan
  actually published a legacy change; skipped legacy writes cannot gate the
  lock-backed operation. The new test passes for Install, Update, and Remove.

- **Variadic IDs consumed the removed option.** Removing the option symbol alone
  caused System.CommandLine to consume `--prune` as a stable-ID operand, reaching
  the identity-grammar error instead of its unknown-option boundary. Added an
  argument validator for unrecognized option-shaped tokens. Unit cases cover
  the removed option before and after an ID, alone, and another unknown option.
  The published-process assertion requires exit 4, the exact unknown-option
  stderr diagnostic, empty stdout, and no writes. The shell's existing parser
  diagnostic stream applies even with `--json`.

- **A4-R4: portable aliases bypass exact shared-owner lookup.** The plan says
  to use `OwnersOf(path)`, which intentionally matches exact recorded strings.
  The legacy ownership reader had additionally rejected aliases. The lock reader
  does not, so differently cased receipts for two owners could be misclassified
  as final-owner content on Windows. A real request-level test failed because
  the supposedly shared file was deleted. Before classifying, Remove now rejects
  ambiguous spellings using the existing portable path key and reports an
  ownership observation without effects. The assertion covers preserved bytes
  and all workspace hashes. This adds no schema or new path policy. The earlier
  native candidate was superseded and the safety change requires a fresh build
  and verification.

- **Rollback cannot promise that an optional legacy write stayed current.**
  The old rollback text assumed both files always remained current. A4's
  best-effort transitional writes can be skipped. The rollback description now
  preserves the independent reservation fix and states the legacy-reader
  limitation instead of promising a state the command does not guarantee.

## Review of completed Task 30 work

The reviewed production baseline is `7fb104e468381b7cb69895899fca99debe183ba0`.
The source review focused on A1–A3 and their immediate A4 consumers: ownership
store/codec, Framework writer composition, Extension intended ownership and
validation-check ordering, Library mutation ordering and recovery matching,
and the existing focused ownership evidence. The earlier phase 0–3 receipt was
read as provenance; its disposable 297-output harness was not recreated, so
this review does not claim a new byte-equivalence proof for those old changes.

The reviewed A1–A3 structure preserves the other lock sections, includes private
ownership effects in validation and recovery, and only publishes ownership
after the target effects verify. Library publishes ownership before its legacy
record, preserving the existing record-last contract. Existing shared-owner,
failed-effect, retained-library, and no-op evidence was inspected. No additional
regression was found in those inspected composition paths.

**A4-R3: new state-file names are absent from destination reservations.**
Severity: high. Category: state ownership / migration integration.
`Commands/Extension/Shared/Permissions/ExtensionDestinationPolicy.cs` reserves
the lifecycle, libraries, and permissions records and the old workspace lease
path, but neither `.agents/open-forge.json` nor
`.agents/open-forge.lock.json`. Its pure predicate therefore admits these
paths and their descendants as package destinations. The corresponding
`LibraryDestinationPolicy.IsAllowed` has the same omission. A published-process
reproduction in an owned disposable workspace confirmed that an explicit
`state-collision` package containing `content/.agents/open-forge.json` installs
successfully and records that authored settings file in its lock paths. A
subsequent `extension remove state-collision --automatic --json` returned
`complete`, deleted the settings file, and reported recovery `removed`.
No real user settings were touched. The generated-lock collision and Library
journey were inspected statically, not exercised end to end.

The smallest correction is to reserve both new filenames and their descendants
in the existing destination policies with direct rejection evidence. This
follows the accepted state-file ownership model and needs no new design.
Disposition: recorded for the migration correction; the reservation correction was subsequently implemented independently of R1. Earliest affected boundary: destination admission after
the introduction of the two state-file names.

Follow-up disposition: the maintainer explicitly confirmed that the authored
settings file must not be admitted. Applied the existing reserved-path rule to
both authored settings and generated ownership state, including descendants,
in the Extension and Library destination policies. This bounded correction is
independent of R1 and precedes the reader flip. The Remove Interface and
Behavior contracts now state the reservation. Direct policy tests cover the
reserved names and ordinary neighboring names; the earlier owned-workspace
reproduction is rerun against the fresh executable to verify rejection.

This correction uses the existing path comparison and symbolic state-file
definitions. It introduces no new safety mechanism, schema, serializer, or
platform capability. Focused evidence covers destination admission and the
published reproduction; the full managed and Native AOT gates for the reader
and deletion change remain due at A4 closeout.

Correction verification: `npm run build -- --no-restore` passed with zero
warnings/errors. The Unit executable with
`--filter-class '*ExtensionDestinationPolicyTests' '*LibraryDestinationPolicyTests'
'*ExtensionRemoveOutputSnapshotTests'` and the conventions' other arguments
passed 30 cases, with zero failures/skips and `CI=true`. The published
reproduction now returns exit 5 and `extension-install.target-unsafe` for
`.agents/open-forge.json`; all workspace file hashes are unchanged and no
settings file is created. `npm run check:dotnet` still reports exactly the
five known unrelated whitespace errors. All four Remove snapshots remain
byte-identical to the pre-change commit. Full-suite results below remain the
earlier preparation baseline, not new full-suite evidence for this correction.

Reproduction inputs: create an empty workspace; run `install --automatic
--json`; create an explicit catalogue package with ID `state-collision`, version
`1.0.0`, no dependencies, and one payload file
`content/.agents/open-forge.json` containing
`{"allowInstallPaths":["docs"]}`. Run `extension install state-collision
--source <catalogue> --automatic --json`, then `extension remove state-collision
--automatic --json`, both against that owned workspace. Both commands returned
exit 0. The source and raw output are disposable artifacts; these inputs and
observed outcomes are the durable evidence.

## Reviewed snapshot diff

`b56e3d9c` adds one test file and four snapshots, 301 lines total. No production
or contract file changed. The snapshots preserve the existing `prune` field,
`keep-as-unmanaged`, both final-owner classifications, shared-owner retention,
missing-path ownership release, and recovery planning fields. Expanded JSON
uses the existing schema version 1, while compact JSON uses the existing schema
version 2; this difference predates A4. No normalization was added; files were
written as LF. These are presentation fixtures, not deletion-safety evidence.

The post-behavior diff was reviewed against the separately committed baseline.
It removes `prune`, replaces changed/unchanged final-owner classifications with
`final-owner`, replaces the edited file's Keep action with Delete, and adds its
delete effect and protected recovery path. The corresponding managed-divergence
finding disappears and the representative status becomes `complete`. Shared
retention, missing-path release, field ordering, and existing schema versions
remain unchanged. These derived changes are recorded above. Dry-run snapshots
still report recovery `not-created`; Integration proves actual preparation and
retention with exact archived bytes.

## Preparation verification receipt

Verified on Windows x64 with .NET SDK `10.0.101` in the isolated
`task30-a4-extension-readers` worktree. Production/configuration baseline:
`7fb104e468381b7cb69895899fca99debe183ba0`, tree
`6bd36df3b22b906044a2795d6a281c5336e8c55b`. Snapshot candidate:
`b56e3d9c09739f66f8ec565bbf7210f35f2188df`, tree
`47fda28682d580f5599c8d981c9660cf2a5b31b1`. This identity describes the preparation snapshot only; the behavior candidate is verified separately below.

Rebuilt using `npm run build -- --offline`, then
`npm run build -- --no-restore` after adding and formatting the test. Final
build: exit 0, zero warnings and errors. These are the conventions' build
command with explicit supported local-cache options; no network restore ran.

The four verification commands from `00-conventions.md` produced:

| Verification                                                 | Result                                                                                                                                                                                              |
| ------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Core UnitTests executable, prescribed arguments              | 3,467 passed; 0 failed; 0 skipped; exit 0                                                                                                                                                           |
| IntegrationTests executable, prescribed arguments            | 1,809 passed; 0 failed; 17 skipped; exit 0                                                                                                                                                          |
| EndToEndTests executable, prescribed arguments, serial rerun | 123 passed; 0 failed; 0 skipped; exit 0                                                                                                                                                             |
| `npm run check:dotnet`                                       | Exit 1 with exactly the five documented whitespace errors in `ReferencesOperation.cs` and `ExtensionListApplicationIntegrationTests.cs`; no new errors. The chained analyzer check did not execute. |

Every executable used `--parallel none --no-ansi --progress off
--minimum-expected-tests 1`. Unit verification additionally used `CI=true` for
read-only Imprint verification. Capture used `IMPRINT_UPDATE=all`, `CI=false`,
and `--filter-class '*ExtensionRemoveOutputSnapshotTests'` on that Unit
executable, selecting and passing exactly the four new snapshot tests.

Assembly SHA-256 identities:

- Core UnitTests: `235CDE0D1CB150387A56BCB90CB10BED4A79B091BB22BC692A16858EAEA7E98E`.
- IntegrationTests: `869A603AF0DAF9823521308119AB89FEA41FAD4E66B4327D0C95B00861382D7B`.
- EndToEndTests: `5D04E76BC84DB92780D74C22ABFFBDA21C15399CC4416282B1BF09362350457A`.

The initial concurrent EndToEnd run was 121 passed, 2 failed, 0 skipped;
its two Cleanup failures are accounted for above and superseded by the serial
rerun. The known Route Update cancellation flake did not fire. Native AOT was
not rerun for this preparation-only change; it remains required for the A4
behavior change. These passing suites establish the preparation baseline,
not A4 completion. At that preparation boundary the acceptance boxes remained open.

## Behavior implementation receipt

A4 implementation and verification are complete. The behavior, contracts,
snapshot diff, and this receipt are committed together. Install, Remove, and Update select ownership from the lock.
Remove uses Shared, FinalOwner, and Missing, prepares recovery before deletion,
and retains the verified bundle after success. Unknown option rejection, stale
missing/unallowed claims, missing/malformed locks, uninterpretable dependencies,
lock/legacy disagreement, portable aliases, and exact recovery bytes have
executable evidence.

After rebuilding with `npm run build -- --no-restore` (exit 0, zero warnings and
errors), the four prescribed managed verification commands produced:

| Verification                                      | Result                                                                                         |
| ------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| Core UnitTests executable, prescribed arguments   | 3,481 passed; 0 failed; 0 skipped; exit 0                                                      |
| IntegrationTests executable, prescribed arguments | 1,817 passed; 0 failed; 17 skipped; exit 0                                                     |
| EndToEndTests executable, prescribed arguments    | 124 passed; 0 failed; 0 skipped; exit 0                                                        |
| `npm run check:dotnet`                            | Exit 1; exactly the five documented whitespace errors; the chained analyzer check did not run. |

Unit used `CI=true` for read-only snapshot verification. Counts are observations,
not pass conditions. The initial post-change EndToEnd run exposed the variadic
option diagnostic and obsolete preview assertions; both were corrected, rebuilt,
and the complete suite rerun. Final prescribed and copied managed Integration
runs both passed 1,817 cases with 0 failures and 17 skips. The preceding candidate
hit the documented Route Update cancellation flake; its one permitted focused
rerun passed. The flake did not recur after the alias safety correction.

The final `npm run build:native -- --rid win-x64 --offline` build passed with
zero warnings or errors. Supported host execution used the same prescribed
arguments throughout:

| Runtime check                               | Result                                     |
| ------------------------------------------- | ------------------------------------------ |
| Native Integration                          | 1,817 passed; 0 failed; 17 skipped; exit 0 |
| Native EndToEnd test executable             | 124 passed; 0 failed; 0 skipped; exit 0    |
| Managed EndToEnd against native CLI         | 124 passed; 0 failed; 0 skipped; exit 0    |
| Copied managed EndToEnd against managed CLI | 124 passed; 0 failed; 0 skipped; exit 0    |

The prescribed EndToEnd executable ran against the native CLI after the supported
native build configured that target. Unit and Integration also ran through their
prescribed executables. The copied managed closures provide the additional
managed-target checks. Integration and EndToEnd were kept sequential. These are
host test results, not a package publication or release qualification; the
unchanged delivery runner was not used to impose count or skip thresholds.

The verified native candidate was built from parent `7a15c689b2757fa21cce0b3a39fd7a196d9bc141` with
uncommitted source identity `b081372b5a39da91d1d988a24805585251d30b450b8cfa2b24af6b6b5d47517c`.
That identity was rechecked against the manifest before staging. Test/artifact
SHA-256 identities at acceptance:

- `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll`: `EC0DDFA23C61D0EF4385A21E12C30DA9B1A7865CD04610A3DDF2F12254CAE68F`.
- `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll`: `F67F67B1A2DFA048A2807AA8C8C8643EABF33B551D2A5869B32E4D5F4B943AC7`.
- `artifacts/bin/OpenForge.Cli.EndToEndTests/release/OpenForge.Cli.EndToEndTests.dll`: `565EDFA9E092B6B16939B9E9DA3F6BD529CBF226F772639D1462F70077A751C2`.
- `artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe`: `4B748C812B2D8017F59831016062FBEFDE0D7650733D51D679282DF4344CE67A`.
- `artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe`: `74C6C256ECAFD0AE95004FAFBAE2C6A2E2B6404AB145798BE1C278B097DC8A3D`.
- `artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe`: `A373679B7DC6C5B0DB238708FFC4A54AA78472930DE7F61740AE8A95F68001BE`.

Not done from the numbered Steps: no independent per-extension destination
schema was added, no classification-only snapshot diff was forced, and no
successful recovery-bundle deletion was retained. The reasons and replacements
are recorded under Divergences observed. Remaining legacy comparison/currentness
removal is A6's scope, not an unreported A4 omission. No payload formatting,
delivery-suite edits, push, or G4 work was performed.

## Rollback

Revert the A4 behavior commit while retaining the state-file reservation fix.
Ordinary dual-written workspaces remain compatible with the earlier readers.
If a transitional legacy write was skipped, reverting also restores the earlier
legacy-reader limitation. Existing recovery bundles remain; no automatic
migration or restoration is promised.

## Continued Task 30 review

The A5 preparation review identified R5: Library ownership paths mapped through non-root destinations.
The correction and fresh verification are recorded in
[A5 prerequisite review verification](05-a5-status-doctor-library-readers.md#prerequisite-review-verification).
The original execution evidence above describes its original committed baseline.

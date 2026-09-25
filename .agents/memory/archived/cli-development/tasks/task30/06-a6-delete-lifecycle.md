---
open-forge:
  description: Task 30 G1 step 4 slice A6 execution plan for deleting the lifecycle and libraries records and collapsing the vocabularies that named a baseline
  tags: [Memory, CLI, Task, Plan, Contextual, Archived, Historical]
---

# A6 — Delete the old records

> Read [00 — Slice conventions](../../../../working/cli-development/tasks/task30/00-conventions.md) first: verification
> commands, pass conditions, the recurring composition pattern, and the rules
> every slice shares. This plan does not repeat them.

## Goal

`.agents/open-forge.lifecycle.json` and `.agents/open-forge.libraries.json` stop
being written and stop existing. `Framework/Lifecycle` and the Libraries record
are deleted. Every vocabulary member that named a stored baseline collapses.

**Last slice of step 4.** After this the lock is the only state file.

## Depends on / Blocks

- Depends on: A4 and A5 — nothing may read either old file.
- Blocks: G1 closeout, then the structural follow-up and G4.

## References

Authorizing decision: [Workspace State Files](../../../../crystallized/decisions/framework/workspace-state-files.md)
— "no migration and no legacy reader", and the deleted-outright list.

Vocabularies that collapse here, and only here, because only now does nothing
compute a baseline:

- `ExtensionInspectPathRelation` — delete `CurrentDiverged`. `Unchanged` and
  `Changed` keep their names with two-way meaning. `Retired` stays, derived from
  lock ownership against the payload. Refinement 12.
- `ExtensionInspectComparisonMode` — `InstalledAndAvailable` keeps its name; it
  names sources, not sides, so it stays correct. Confirm `ReadMode` no longer
  takes `trustedLifecycle`.
- `ExtensionInspectFingerprintOrigin` — delete `PersistedBaseline`.
- Update's `currentState` — delete `baseline-equivalent`.

Deletion surface measured 2026-09-12: 169 files reference `Framework.Lifecycle`,
67 reference `LibraryRecord`/`LibraryPathIdentity`.

## Preconditions

### Execution capsule

Current continuation position: A6 implementation and final managed/native
qualification are complete. The reviewed before snapshots are separately
committed, and the behavior/contracts are ready for one local commit. G1 step 4
is complete; G1 steps 3a/3b remain and continue through
[P1 shared permissions](06p-p1-shared-permissions.md) before B1/M1. The former
stop-before-G4 boundary is superseded; G4 is complete and its current state is
recorded in [the G4 packet](../task30-g4/_task30-g4.md).

- Baseline: A5 commit `4368148f`, isolated on `task30-a4-extension-readers`.
  The four prescribed checks and supported Windows native gate passed. A5's
  receipt owns the exact executables, counts, results, and artifact identities.
  Direct sequential execution continues under the maintainer's authorization
  through accepted pre-G4 work.
- Consequence: remove obsolete state authority and publication across Framework,
  Extension, and Library commands. Preserve existing workspace files, recovery
  ordering, leases, revalidation, allow-list admission, reserved files, physical
  boundaries, and the distinction between whole-file and region ownership.
  Old files become unrelated user content; do not migrate or delete them.
- Reuse: the accepted ownership reader/store/codec, typed Library identities and
  mappings, existing payload/source readers, Markdown-semantic comparison,
  mutation/recovery machinery, and concrete rendering/snapshot surfaces.
  No dependency, parser, schema expansion, native bridge, or compatibility
  reader is selected. Exceptional machinery: none.
- Evidence: commit reviewed before snapshots for affected public state-file
  outcomes and comparisons before changing them. Direct tests cover ignored
  legacy content, current-versus-intended comparison, forgiving lock reads,
  real mutation receipts, and leftover-file preservation. Run the four
  convention commands after rebuilding plus the supported Windows Native AOT
  gate; this slice materially changes public and shared mutation composition.
- Deletion proceeds only after each concrete reader and writer has been moved to
  current ownership facts. Retain neutral mapping, content, and filesystem
  capabilities under their actual existing domain owners; remove their obsolete
  record models/codecs rather than introducing legacy-shaped adapters.

- [x] A4 and A5 committed; three suites green.
- [x] `git grep -l "Framework.Lifecycle" src/cli/core` returns only writers and
      the types being deleted — no readers. If a reader remains, it belongs to
      an earlier slice; go back rather than deleting under it.

## Steps

1. [x] Delete the dual-write. Remove the `PlanFrameworkUpdate` and
       `PlanExtensionUpdate` calls at the seven sites listed in A1 and A2, and the
       `CreateRecordChange` calls at the four sites in A3. Verify: `npm run build`,
       three suites green.

2. [x] Delete `Framework/Lifecycle/` and `Framework/Libraries/Models/Record/`, plus
       `LibrariesRecordCodec` and `LibrariesRecordReader`. Follow the compiler until
       it is silent. Verify: `npm run build`.

3. [x] Collapse the four vocabularies listed above. Each is an exhaustive switch, so
       the compiler names every site. Verify: `npm run build`.

4. [x] Delete the workarounds that existed only for the old shape:
       `FrameworkLifecycleCurrentnessReader`'s `continue` for
       `LifecycleSchema.GeneratedEntriesRegion` goes with its file; confirm no
       equivalent survives elsewhere. Verify: `git grep -n "GeneratedEntriesRegion"`
       returns nothing in `src/cli`.

5. [x] Update every contract naming a deleted file or member. Measured 2026-09-12:
       44 crystallized records name the old files. Archived and Emerging records keep
       the old names as provenance and are **not** rewritten.
       Verify: `git grep -l "open-forge.lifecycle.json\|open-forge.libraries.json" .agents/memory/crystallized`
       returns nothing.

6. [x] Cover this case in e2e: a workspace containing a leftover
       `.agents/open-forge.lifecycle.json` is neither read nor deleted, and every
       command succeeds. It is a stranger's file now. Verify: e2e suite green.

7. [x] Remove the `.prettierignore` fences that exist only for the old shape, if the
       structural slice B1 has already landed. If it has not, leave them and note it.

8. [x] **Repoint the state-file outcome, do not add one.** Slices A1 to A3 applied
       the lock effect without reporting it, so the public result carried one
       state-file outcome throughout. Now that the lifecycle effect is gone, point
       the existing outcome at the lock: in Install that is
       `InstallResultFactsFactory`'s `lifecycleOutcome` and the identity appended in
       `InstallPlanResultProjector`, with the equivalents in the other commands.
       Net public output across the whole migration is one outcome, renamed.
       Verify: reviewed snapshot diff shows a renamed subject, not a new field.

9. [x] Update [phase-4a-g1](phase-4a-g1.md): mark step 4 complete and record the
       final measured state.

## Expected result

A fresh `install` produces exactly one state file, `.agents/open-forge.lock.json`.
No command reads or writes the old names. A workspace carrying the old files is
unaffected by them.

## Acceptance

- [x] Only the lock is written.
- [x] `git grep "Framework.Lifecycle" src/cli/core` returns nothing.
- [x] No crystallized record names a deleted file.
- [x] A leftover old file changes no behaviour.
- [x] Three suites green; `npm run check:dotnet` still reports exactly 5 errors.

## Before-output evidence

- `b1d93575` captured eleven concrete rendering harnesses and 44 human/JSON
  snapshots against unchanged A5 production. The rebuilt Unit suite passed:
  3,547 succeeded, zero failed, zero skipped.
- `f454aab6` corrected Inspect fixture metadata before changing Inspect:
  generated ownership uses the contract's `derived-navigation-only` literal,
  and the resolved closure includes the requested package. The four recaptured
  views were reviewed; the rebuilt full Unit suite again passed with zero
  failures and zero skips. Publication, comparison and path expectations did
  not change in either baseline commit.

## Managed qualification

The final production rebuild (`artifacts/a6-final-managed-build.log`) completed
with zero warnings/errors. After the help assertion correction, the solution was
rebuilt again (`artifacts/a6-help-assertion-build.log`), also with zero
warnings/errors; only that Integration assertion changed after the Unit and
EndToEnd runs below.

| Prescribed check       | Result                                                                       | Evidence                                        |
| ---------------------- | ---------------------------------------------------------------------------- | ----------------------------------------------- |
| Unit executable        | 3,417 passed; 0 failed; 0 skipped                                            | `artifacts/a6-final-managed-unit.log`           |
| Integration executable | 1,823 passed; 0 failed; 17 expected skips                                    | `artifacts/a6-accepted-managed-integration.log` |
| EndToEnd executable    | 125 passed; 0 failed; 0 skipped                                              | `artifacts/a6-final-managed-e2e.log`            |
| `npm run check:dotnet` | Exactly 5 existing whitespace diagnostics; exit 1 stops the chained analyzer | `artifacts/a6-final-managed-check-dotnet.log`   |

The five diagnostics remain ReferencesOperation lines 459/460 and Extension
List's necessary migrated fixture at lines 83/84/85. The latter's three original
whitespace defects are preserved. No cancellation-flake rerun was needed.
Counts are recorded for traceability, never used as pass conditions.

Focused checks additionally cover unknown, stale, malformed and nonordinary
locks; retained recovery before deletion; missing and out-of-bound destinations;
whole-file versus region ownership; current/intended comparisons; removed root
categories; skipped lock publication; missing required parents; and actual
Library links and registration mappings. Retired codec/baseline tests were
removed with their contracts; surviving physical reads and source validation
were rehomed with production. The published-process leftover case executes
Install, Update, Status, Doctor, Extension List and Library List against the
same malformed old files and asserts their exact bytes after each command.
The other affected command families have direct integration leftover evidence.

## Reviewed output diff

Before production changes, `b1d93575` captured 44 views, corrected Inspect
metadata was committed in `f454aab6`, Route Move/Remove's eight views were
committed in `c8708ebf`, and List/Inspect's two product-help views were committed
in `57cad3c8`.

The combined reviewed diff is `artifacts/a6-reviewed-snapshot.diff`: **50 changed snapshots, 119 additions and 176 deletions**.
It preserves result shapes while repointing state outcomes to the ownership
lock and dropping the obsolete dual-write recovery entries. Current/intended
hashes remain, with two-way comparison verdicts. Unknown ownership yields
information and no inferred destructive effects; Route Move/Remove retain
positive unmanaged proof and truthful no-change headers. Help names the current
lock and operation-time comparison. Route Init's four original views remain
unchanged. The narrower reviewed diffs are retained under `artifacts/a6-*-reviewed-snapshot.diff`.

The supported Windows native build completed successfully with zero warnings
and errors (`artifacts/a6-accepted-native-build.log`). Its copied Unit closure
passed 3,417 with zero failures/skips (`artifacts/a6-accepted-native-unit.log`).
Native Integration passed 1,823 with zero failures and 17 expected skips
(`artifacts/a6-accepted-native-integration.log`). Native EndToEnd passed 125 with zero failures/skips
(`artifacts/a6-accepted-native-e2e.log`). The final format check again reported
exactly the five known diagnostics (`artifacts/a6-accepted-check-dotnet.log`).

## Divergences observed

- The precondition says A4/A5 leave no readers of `Framework.Lifecycle`, but
  A4's binding Goal names only Extension Install/Remove/Update and A5 names
  Status/Doctor/Library. Extension List and Inspect still read the old document;
  Framework Install, Update, and Route Init also read it while planning their
  transitional publication. A6's own References explicitly assign Inspect's
  relation/read-mode changes and Update's baseline vocabulary here. A4's receipt
  also explicitly leaves transitional comparisons and Framework currentness to
  A6. Complete those concrete reader transitions first within A6, before deleting any type
  they use. This satisfies the accepted no-legacy-reader result without
  retroactively claiming those changes belonged to A4 or A5.
- The lexical precondition also matches neutral operational, content-comparison,
  and safe target-read helpers that A5 still houses under the old namespace.
  They do not read legacy state. Preserve their accepted behavior in the
  appropriate existing capability owner while deleting the record subsystem;
  do not delete required current comparison or no-follow checks to satisfy grep.
- Step 7 is conditional: B1 has not landed. Leave the shipped-payload
  `.prettierignore` fences in place until the heading-based Entries slice.

- Snapshot preparation began as drafts under ignored artifacts while A5's
  executable source was frozen for qualification. After A5 was committed,
  promote the eleven harnesses into the existing Unit project, validate and
  capture them, and commit their reviewed before outputs separately. The
  drafts have not changed production behavior or added a dependency.
- Step 8 expects a renamed state-file subject. Existing recovery projections
  already report every protected entry, including both transitional records
  when both writes were planned. The before fixtures must show both; after
  retirement the legacy entry disappears while the lock entry remains. Keep
  that truthful recovery diff rather than inventing a second state-file outcome
  or hiding the lock from the before snapshot.

- The detailed Inspect contract review found two mistakes in the new synthetic
  before fixture: its generated ownership literal and resolved-package count.
  Corrected and separately committed those facts while Inspect production was
  unchanged. This is a baseline correction, not a product output change; use
  the corrected commit when reviewing Inspect's subsequent diff.

- The conventions say Extension List's Integration fixture file remains
  untouched, but the reader transition requires replacing its legacy seeds.
  Updated the necessary fixtures and restored its three original whitespace
  diagnostics; the final check must still report those three and References'
  two baseline diagnostics. No formatting cleanup is included.
- Inspect's old cancellation theory injected a lifecycle-document result that
  no longer exists. Removed that one obsolete case; existing operation-level
  cancellation evidence exercises the ownership reader's cancellation boundary.
  Corrected the surviving List fixture to pass a fully qualified snapshot path.
- Install's preservation verifier compared unselected scoped targets with
  stored hashes. A6 deletes those hashes, and the selected footprint is the
  closed root subset. Removed that verifier instead of reconstructing an
  integrity baseline. Preserve the scoped receipts and content; missing
  unselected content neither blocks nor gets recreated. Added direct evidence.
- Removing the old state store also removes its Framework/Extension collision
  check. Kept that safety rule in Install's concrete planning step using the
  existing portable path identity, including case aliases. The ownership store
  itself remains a forgiving receipt planner.
- Extension Install's transitional Foundation reader gated on the old Framework
  source inventory and stored target hashes. Replace that gate with the existing
  physical `.agents` container and affected topology/host safety boundaries;
  preserve Framework ownership through the single lock plan and exact final
  receipt verification. This executes A4's explicitly deferred A6 currentness
  removal without rebuilding a synthetic baseline.
- Optional cleanup of failed-test temporary workspaces and ignored snapshot
  drafts was rejected by automatic approval review with only “blocked by
  policy.” Nothing was deleted by that attempt. Leave these optional artifacts
  in place; do not retry the rejected cleanup through another route.

- The Library record directory also contained typed registration identities,
  source-to-destination validation, and a public presentation projection. Keep
  these capabilities as `LibraryRegistration`, `LibraryRegistrationSet`, and
  lock-backed `LibraryRegistrationReader` under Libraries' identity/observation
  owners. Remove stored schema state and the old serializer; the retained public
  subset projection names the lock schema and does not serialize a second file.
- Final Library detach previously deleted its separate record. With a shared
  lock it instead publishes an empty Libraries section and preserves other
  sections. Recovery attribution previously looked for prior record bytes only
  when the current record was missing; it now uses verified prior lock bytes
  when current registrations cannot attribute the recovered effects. This keeps
  final-detach recovery working without adding a legacy reader.
- Strict Library codec tests enforced the removed standalone schema, including
  rejection of missing/unknown fields that the accepted lock codec tolerates.
  Retire that obsolete decoder evidence and cover registration identity,
  canonical mapping, duplicate ownership, and forgiving lock decoding through
  the actual surviving reader.

- Extension Install's Library boundary still treated an unavailable lock as a
  blocked Library record after the direct Framework baseline gate was removed.
  It now uses the raw lock read state: unknown ownership contributes no claims;
  uninterpretable mappings in a readable section produce information with no
  inferred effects. Real tests cover safe installation with missing, malformed,
  and nonordinary lock targets plus unrelated edited Framework content.
- Library source-control fixtures named retired files. The active control paths
  are settings and lock; retired filenames become ordinary unowned content.
  Source inventory now excludes settings as well as the lock, while destination
  validation retains the existing reserved-file protection. Tests name the actual
  current controls instead of preserving obsolete exclusions.

- Route Init's scoped Framework builder mixed physical workspace admission,
  payload alignment, legacy currentness, and publication. Keep its physical
  container check and existing route/source/prospective-topology checks, but
  remove the baseline and release-metadata gate. Existing Framework membership
  comes from whole-file lock ownership; selected source alignment remains an
  operation-time fact. Publish only additions backed by actual file/region
  effects and preserve unselected receipts. A skipped lock write cannot turn
  final target verification into a claim that ownership was published.

- Extension Update's old `baseline-equivalent` current-state member has no
  two-way meaning. Use `same` for current/intended agreement and `changed` for
  disagreement; the before fixture has distinct hashes, so its after verdict
  becomes changed. Remove the stored baseline constructor/property entirely.
- Extension Update's normal/force distinction depended on stored divergence.
  Ordinary update now overwrites/restores owned current paths under the accepted
  decision; the existing force flag grants no extra bypass. Pruning still requires
  explicit `--prune`, current existence, destination admission and no retained owner.
  Retain and verify successful recovery bundles instead of deleting prior content.
- The plan ownership test included mutation of ten legacy array graphs. Those
  mutable record types no longer cross this boundary; ownership uses immutable
  receipts. Keep the eight real mutable topology-isolation cases and assert the
  retained ownership facts instead of reproducing obsolete legacy adapters.

- While tracing root Update, the accepted `removedCategories` setting was found
  decoded but unused by either Install or Update. Decision 4 and the settings
  schema example (`skills`) resolve the meaning: exclude payload paths beneath
  each named `.agents/<root-category>/` from reinstatement. Wire that selection
  into both commands, preserve existing content and receipts outside selected
  effects, and cover it directly. This closes a prior implementation gap instead
  of restoring deliberately removed defaults during A6's new normal update.
- Extension Update's rebuilt focused checks now pass: 23 Unit and 29 Integration,
  zero failures/skips. Recovery ordering, edited retirement, absent paths, shared
  allow-list rejection, duplicate/cyclic/portable-alias receipts, legacy leftovers,
  older Framework metadata and both Git/bundle review advice are covered.
  The four reviewed snapshots retain current/intended hashes, change the unequal
  fixture's current state to changed, and remove only the legacy recovery entry.

- Root Update's effect model required legacy source provenance even for a
  retired whole-file deletion. Permit null current source alignment for that
  deletion. Move the existing pure scoped-source lookup into Distribution's
  source owner rather than retaining a lifecycle dependency solely for lookup.
- Root Update's baseline-unchanged, force-only overwrite and cleanup expectations
  contradicted accepted ordinary update. Replace them with normal overwrite and
  restore, edited retirement and retained recovery. Root blocks use their actual
  `open-forge` region receipts and preserve outside host bytes. Retired-file
  discovery can also refresh navigation, so assert the retired path's effect
  without suppressing independent generated changes.
- Retain destination and cross-owner checks against actual portable paths,
  the shared allow-list and mapped Library registrations after deleting the old
  lifecycle gate. Uninterpretable destinations produce complete information with
  no inferred effects. Revalidate raw lock identity even without publication.
- Final Update verification formerly required every finding to disappear.
  Preserve-without-prune is an accepted final state. Verify an effect-free
  projection, exact intended lock bytes or its unchanged expectation, and only
  accepted retirement findings. Preserve those findings in the final result and
  keep skipped-publication outcomes truthful.

- The four root Update after snapshots were reviewed against the separately
  committed before images. The distinct current/intended hashes remain; current
  state changes from baseline-equivalent to changed and the obsolete recovery
  entry disappears. The existing lock outcome and all other fixture values stay.
  Root and Extension Update's actual successful retained-bundle advice are covered
  in integration; the historical dry-run snapshot remains a dry-run fixture.

- The final reader inventory found `LifecycleOwnershipReader` still used by
  Route Move and Route Remove, omitted from the named reader slices. Deleting
  it under those consumers would violate A6's precondition. Close that missed
  reader transition first, with separately committed before snapshots. Keep
  positive-unmanaged proof: unknown ownership authorizes neither moving nor
  deleting content. Replace the obsolete missing-record block with a complete
  informational no-effect observation, consistent with the accepted lock-never-
  gates decision. Known whole-file and region claims still protect selected
  content. This does not merge the commands' separate finding vocabularies or
  cross the unrelated M1 extraction boundary.
- Root Update focused evidence is complete: 85 Unit and 43 Integration tests,
  zero failures/skips after rebuilding. Tests cover ordinary edits/restoration,
  allow-listed and forbidden stale claims, missing retirement, recovery before
  deletion, retained prior bytes, removed categories and both review-advice
  branches. Full A6 acceptance gates remain pending the final reader retirement.

- Route Move/Remove's finding constructors rejected complete information even
  though that is the accepted unknown-lock outcome. Permit it only for their
  existing ownership-unavailable code. Snapshot review shows unchanged empty
  effects and result shape, with complete/not-established facts, truthful
  no-change headers and no obsolete blocked-next advice. Whole and region
  claims protect case-alias paths in direct integration evidence.
- The lifecycle folder also contained surviving neutral content recognition,
  current payload observations and filesystem reads. Move these to Distribution
  and Filesystem, preserving behavior; delete the unused strict legacy codecs,
  validators, read/write models and snapshot copier. Retire tests of those
  deleted contracts, retain the source-validation test and physical reader
  boundary cases at their mirrored owners. Local comparison kind now uses the
  existing Markdown fingerprint enum instead of stored-schema strings.
- Status and several Extension fixtures still constructed legacy graphs despite
  earlier reader migrations. Replace those fixtures with actual ownership
  documents; retain explicit malformed legacy leftovers as unrelated bytes.
  Do not keep legacy-shaped test adapters merely to preserve old test counts.

- Full-suite expectations still reserved retired filenames and inferred unchanged
  content without an intended source. Update those expectations: old filenames
  are unrelated content and current-only Inspect has unknown relation. Preserve
  exact current hashes and current settings/lock reserved-path coverage.
- Inspection found that final Update verification rejected an informational
  skipped-publication observation even when no lock write was planned. Permit that
  observation only with no planned lock write and unchanged raw read state and
  expectation. Exact intended lock bytes remain required when publication occurs.
- Retaining recovery exposed an existing Extension Update repeat gate: catalogue
  candidates blocked even an effect-free repeat. Defer that conflict until real
  content or ownership effects are known. Unavailable recovery inspection and
  mismatched in-flight preparation still stop; new mutations retain the existing
  residual conflict boundary. This changes no recovery schema or history claim.
- The Library technical design contained one Windows-1252 dash byte inside
  otherwise ASCII content. Normalize that affected source to UTF-8 while updating
  its retired reader/publication description; no text meaning changes.

- The skipped-lock integration fixture initially also lacked payload parent
  directories. Diagnostics separated the actual write failure from final
  verification. Keep the lock test in an installed workspace with existing
  parents, and add a separate parent-absence case. Update has no directory
  effects: reject a required missing parent through the existing target-safety
  finding before any write, instead of applying a prefix then failing. Missing
  retired paths still need no parent and never receive a deletion.

- A6's Step 9 closes G1 step 4 and the active sequence then jumps to B1,
  but `phase-4a-g1.md` explicitly records
  step 3b (the per-owner permission subsystem) as unfinished. Review finding
  **A6-R8**, high severity, acceptance/authority: `WorkspacePermissionReader`
  reads both authored settings and the retired grant file;
  `WorkspacePermissionEvaluator` still honours per-subject grants, and
  `WorkspacePermissionChangePlanner` still writes the retired file. The accepted
  Workspace State Files decision requires one shared allow list and no legacy
  reader. Finish A6's lifecycle/Library retirement as its own verified commit,
  then close the already accepted permission work and remaining four
  `--allow-path` bindings before G1 completion. Do not
  silently mark that missing work complete or merge it into the unrelated M1
  extraction. Earliest affected boundary: G1 closeout. No new schema or
  per-extension destination policy is authorised.
- The final source-reference review also found stale List/Inspect help text.
  Captured and reviewed both concrete product-help outputs before editing them,
  committed their harnesses and baselines separately in `57cad3c8`, then changed
  only the retired state and comparison claims. The reviewed help diff is
  `artifacts/a6-help-reviewed-snapshot.diff`; both focused captures passed.
- The two newly authored conditional chains in Update intended-byte selection
  and Framework target-kind selection violated the loaded C# rule. Expanded
  them into ordered branches without changing precedence or behavior.

- Final integration caught an existing help assertion requiring two words on
  the same rendered line. The intentional help correction moved the phrase
  across a wrap. Keep the exact reviewed help snapshot and make the semantic
  read-only assertion whitespace-tolerant; product behavior is unchanged.
  The queued native build was stopped before it started so the corrected
  integration evidence can be qualified first.

- Step 6 says every command succeeds with a leftover legacy file. The published
  regression runs six state entry points in one real workspace; existing focused
  integration evidence covers the remaining affected mutation/read families.
  The zero-reader inventory establishes that no command can consult either old
  file. Do not duplicate the entire CLI catalogue in EndToEnd merely to repeat
  that same read boundary; unrelated invalid inputs and safety findings retain
  their own statuses. This is the narrower evidence selected instead of a new
  all-command EndToEnd matrix.

- **A6-R9 follow-up:** P1 review found stale root Update and Extension Update
  product help describing the old force-gated preservation behavior. Their exact
  published before outputs were captured separately in `badbde7a`; the correction
  and its verification are owned by [P1](06p-p1-shared-permissions.md). A6's accepted
  behavior is unchanged. This is a review correction, not G4 presentation work.

## Final Artifact Identity And Closeout

`readBuilt` verified the current source identity and every native/managed closure
hash **before staging**. The complete record is
`artifacts/a6-accepted-evidence-identity.json`. Parent: `57cad3c869b727e35ef4cc6a4f94f8d2ae988aca`;
change hash: `14aa27870e4ec460f3b09adbf2d2521ad6402860af6dd1546a0bf20fc5700a58`.

| Native artifact                                                            | SHA-256                                                            |
| -------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/publish/win-x64/open-forge/OpenForge.Cli.exe`                   | `e1a08d654b39811797e2ee81914ecf6469119f6780db07f6c6350a15febf32d5` |
| `artifacts/publish/win-x64/integration/OpenForge.Cli.IntegrationTests.exe` | `6f77fc99f90b1c6b5464933257e1243070bcefc950c174c4d0b6b191070fea23` |
| `artifacts/publish/win-x64/end-to-end/OpenForge.Cli.EndToEndTests.exe`     | `42e6507af12daf21ac6038fcee19442efd3f09e3d531243dc639bb9812b98101` |

The manifest remains `tested: false`: these are direct suite qualifications,
not a packaging or release claim. No push or integration was performed. The
original checkout remains clean; `scripts/delivery/test-suites.ts`, shipped
payload and `.prettierignore` remain untouched.

Intentionally not done: no old user file was migrated or deleted, no full CLI
catalogue EndToEnd matrix was duplicated, and B1's conditional formatting fences
were not removed early. A6 closes G1 step 4, not the unfinished shared-permission
steps 3a/3b; P1 now owns that accepted continuation. G4 has not been entered.

## Rollback

This slice is not partially revertible. Revert the whole slice or finish it.

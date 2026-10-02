---
open-forge:
  description: Record completed Task 64 CLI defect and contract corrections with independent managed, native, package and qualification-closure evidence
  tags: [Memory, Working, Task, CLI, Defect, Contract, Wording, Contextual, Complete]
---

# Task 64 — CLI defects and contract drift

## Outcome

Recorded on 2026-09-28, when the maintainer accepted the
[open task review](../../../emerging/analysis/open-task-review/_open-task-review.md). One owner for findings that no
other open Task covers. Recommended to do next, alongside
[Task 53](task53-loading-and-scoping-audit.md).

## Scope

- **CLI behavior and wording defects:** the seven behavior findings and six
  wording findings under "Findings for follow-up" in
  [Task 61](task61-documentation-accuracy-and-voice.md). One wording defect has
  a clear root cause: `LibraryDetachWording.cs` passes "Would" where it needs
  "Would update". The wrong `Next:` line for a kept recovery bundle belongs to
  [Task 32](task32-minimal-output-sweep.md) instead.
- **Contract contradictions:** five of the seven in Task 61. The Extension
  install and create contracts on confirmation, the update contract's
  "non-shipping" wording, the retired `memory-starters` example, help wrapping
  defined only in code, and `library attach` into a Framework category. The
  two removal contradictions are decided in
  [Task 63](task63-keeping-edits-through-updates.md).
- **From the closed removal Tasks:**
  [Task 33](../../../archived/cli-development/tasks/task33-managed-content-removal.md) and
  [Task 35](../../../archived/cli-development/tasks/task35-removal-and-suppression-model.md).
  - Correct the `route remove` interface: its eligibility list, its ownership
    sentence, and the `ownership-claimed` next action.
  - Change the site's Customizing advice for hand-deleted files to
    `open-forge remove <path>`, and confirm with a scratch run that `extension
list` and `doctor` then stay quiet.
  - Write one Crystallized Decision for persistent removal intent, or extend
    the Workspace State Files decision. Record the shape, units, storage,
    consumers, the options not chosen, and the `registered-link-restored`
    split. That decision still lists only two keys for `.agents/open-forge.json`.
  - Fix the stale Task 50 path in the `WorkspaceRemovals.cs` doc comment.
- **From the closed [Task 30](../../../archived/cli-development/tasks/task30-cli-experience-remediation.md):** Phase 6,
  the gap between Doctor's `route.axioms-invalid` finding and the loader rule
  that a missing Axioms section adds no rules, and a current home for the
  shared presentation rules: the `Workspace:` echo, the `Next:` rule, and the
  shared message families. They live only in an archived convention file and
  in code, and Tasks 32 and 34 point their acceptance at the archive.

## Frozen first implementation wave

Root accepted this first implementation boundary. It narrows the execution
packet, not Task 64's complete scope or its unresolved policy questions.

- **B1 — Find metadata route facts:** `FindOperation` reuses applicability
  route facts with any selectors. Separate reads use the full catalogue
  `SelectAll`, while candidate selection and counts remain filtered. Tests
  preserve filtered projection, unavailable Loader, and empty-selection cases.
- **B2 — Update navigation refusal:** `UpdateGeneratedNavigationPlanner`
  returns `TargetUnsafe` with `read.RelativePath` and the existing cause when
  projection is refused. Linked-path and containment admission stay unchanged.
  The exact Library-link regression shows the real target and performs no
  writes. Full verified registered-Library read-only projection is now
  integrated; its exact API and source are in the candidate
  `UpdateProjectionInputReader.cs`.
- **B3 — Extension update topology and reports:**
  `ExtensionUpdateTopologyBuilder` compares generated Entries with guarded
  actual-destination observations, not package input, using
  `FileExpectationValidator` and the existing strict UTF8/Markdown spans.
  `changed` means the actual destination body differs.
  `ExtensionUpdateReportSelector` derives section claims and counts only from
  matching Planned preview or Verified apply effects, never synthetic effects.
  The shared `PlanProjectedEntries` path and generated-only reconciliation for
  package and nonpackage hosts are integrated through the existing bounded
  Entries writer. Truthful reports alone are insufficient. Two-case installed
  catalogue no-op evidence preserves all bytes, effects, and counts.
- **B4 — Route-move reference scan:** `RouteMoveReferenceScanner` exempts only
  a `TargetMissing`/`Local`/`Missing` canonical coordinate whose source is
  stationary, whose target cannot intersect old/new leaf-plus-overwrite or
  category coordinates, and whose ancestry is proven ordinary with
  `LinkTargetReader`. Preserve missing meaning, occurrence, and the document
  snapshot for post-move verification. Unproved, unsafe, or affected
  references still refuse. Cover unaffected incoming-link rewrites, old/new
  coordinates, moved-source missing references, component boundaries, encoded
  paths, and unsafe-path preservation.
- **B5 — Doctor generated-region suppression:** suppress
  `FrameworkManagedChanged` only for `Changed` `GeneratedRegion` observations.
  Route-generated stale diagnostics and authored whole-file changes remain
  visible. Test deletion of the maps entrypoint with both an untouched and a
  separately edited Loader, with no Doctor writes.

Every packet keeps wire shape, dependencies, and parser behavior unchanged and
stops on an unanticipated shared-contract or safety change. Luna max owns the
precise command-local implementation and evidence. Root supplies explicit files
and test commands per worker. Each packet uses the existing Unit or Integration
project, and no new test project is permitted.

### Additional frozen conformance boundaries

- **W1 — Extension Remove attention:** the dry-run headline uses
  `WouldRemove`. Apply and status policy are unchanged.
- **W2 — Root remove settings wording:** the planned-settings wording is
  already fixed. Existing `RemoveRootTextIntegrationTests` remains
  qualification evidence.
- **W3 — Library detach preview:** the `LibraryDetach` `SectionRow` preview
  verb is `Would update`.
- **W4 — Ownership trust:** a trustworthy ownership document maps `Trusted`
  even when `Extensions` is empty. Absent and untrustworthy meanings remain
  distinct.
- **W5 — Update force help:** both Update force-help descriptions state the
  existing eligibility and grant no extra authority.
- **Confirmation boundary:** preserve existing runtime final confirmations and
  correct the Create and Install help and contracts. This makes no runtime
  policy change.
- **Context presentation:** a pending text-only subject becomes identifier
  context through `ContextPresentation.SelectText` and `TextFindings`.
  Report and JSON workspace identity remain intact. This is not a global
  presentation change.
- **B2 conformance:** the diagnostic correction is not sufficient by itself.
  Supported healthy registered Library leaves must supply Update read-only
  navigation inputs through observed ownership mapping,
  `LibraryMappingObserver`, a safe source-root reader, unchanged no-follow
  source reads, catalogue identity agreement, destination reobservation, and
  the existing under-lease plan rebuild. Mutation destinations still reject
  links. The full verified registered-Library read-only projection is now
  integrated; its exact API and source are in the candidate
  `UpdateProjectionInputReader.cs`. The unchanged mutation no-follow guards
  remain.
- **Ownership collision boundary:** Library attach, sync, and detach checks use
  actual mapped leaves, not generated host paths. Bounded navigation validation
  remains.
- **Doctor compatibility:** local missing or empty Axioms compatibility is
  correct. Clarify the contract while retaining the Loader requirement.
- **Recovery Next:** successful retained recovery uses `git diff` for an
  ordinary `.git` directory. Otherwise the sentence directs the user to the
  exact bundle, while actual-error precedence is preserved.
- **B6 — Recovery diagnostics and coexistence:** The narrow Option A diagnostic
  correction below is accepted and active, with admission rules unchanged.
  The separate **OPTIONAL PRODUCT QUESTION** is whether `route create`,
  `route init`, `route update`, and `route move` should later permit
  verified-final recovery coexistence. That policy remains unresolved and,
  after diagnostics are corrected, is not a shipped defect. Beta4 preserves
  the current admission policy. Do not mark the diagnostic correction complete
  before its focused qualification and review receipts exist.

These are accepted conformance fixes, not new ownership authority.

## Frozen Option A diagnostic follow-up

This new narrow Task 64 horizon corrects `RecoveryConflict` reporting for
`route create`, `route init`, `route update`, and `route move`. It preserves the
current recovery-bundle admission policy and does not decide the optional
verified-final coexistence question described under B6.

- A command-local `RecoveryConflict` finding identifies the exact positively
  observed recovery candidate from the existing `Finding.Target`, rendered
  through the file subject's `path` and message. The requested route remains
  the command subject.
- Create and Init application recovery metadata may report a positively
  observed prior candidate as `Retained(path)`, consistent with Move and
  Update. Create, Init, Update, and Move continue to reject an existing
  candidate when recovery is required. Init planning also refuses a conflicting
  candidate during dry run.
- Init planning uses an optional internal `FindingTarget` to identify the
  candidate while preserving the requested route and planning recovery state.
- Each command-local `RecoveryConflict` finding action and report next action
  is `open-forge cleanup --dry-run`. The exact reason is
  `Review the recovery file and preview what cleanup would remove before deleting anything.`
- `RouteRemove` already permits verified-final coexistence and remains
  protected. This horizon adds no result schema fields. B1-B5 and B7 remain
  accepted beta4 fixes and are not reopened.
- Do not delete recovery data automatically. Do not promise Cleanup removes
  malformed or unsupported archives or guarantees that the command will be
  unblocked. Unavailable, cancellation, and other errors retain their current
  classifications and precedence.

### Horizon and execution

- Phases: (1) implementation, (2) focused qualification, and (3) fresh
  whole-task review and grouped correction.
- Milestones: (1) frozen packet, complete; (2) code and focused evidence ready,
  complete; and (3) whole-task review and grouped correction resolved,
  complete.
- Current state at this source receipt: phase 3 review and the single grouped
  correction are complete, milestone 3/3. Root acceptance was still pending at
  that checkpoint; see the integrated Root acceptance addendum below. Task 64
  completion remains unchecked.
- Execution profile: Standard.
- Whole-task review: one fresh `gpt-6.1-sol/high` review (`r_db3d1fe31440`),
  completed; 1/1 consumed. Its four finding groups have been addressed by the
  sole authorized correction, with focused evidence recorded. No second review
  is authorized.
- Grouped repair: 1/1 consumed under Root's released mutation slot.
- Councils: zero.
- The Task 64 Mastermind owns this implementation horizon. Root Overseer retains
  acceptance and shared contracts. Luna owns the assigned implementation,
  evidence, documentation and deterministic integration work.
- Focused code/evidence and grouped-correction receipts are recorded below.
  Root integration acceptance is recorded in the addendum; full managed/AOT
  and security/dependency gates remain pending. Review receipts are in
  `artifacts/task64/integration/`.

## Done when

- [x] Each defect is fixed with a capture or test that pins it, or recorded as
      accepted behavior with a reason.
- [x] Each contract contradiction is resolved in the contract, the help, or the
      code, so the three agree.
- [x] The removal Decision is recorded, and the `route remove` interface and
      the Customizing page match it.
- [x] The shared presentation rules live in a current source that Tasks 32 and
      34 can point to.
- [x] Option A passes focused qualification with evidence for candidate
      identity, cleanup guidance, unchanged admission policy, RouteRemove
      protection, and preserved error precedence.
- [x] One fresh whole-task review is recorded and any accepted findings are
      resolved within the single grouped correction budget.
- [x] The independently reconstructed candidate passes the complete final
      managed public suite. Root-authorized qualification closure resolved the
      environmental metadata race; the fresh final run passed 263/263.

## Independent completion applicability — 2026-10-01

The completion candidate reconstructs only the accepted 68-file Option A
manifest on base `2e0da10a5657c084b2847245775858123e09a5ed`, plus this
Task record. At reconstruction, all 68 files matched the frozen author and integrated hashes.
The integrated Task record adds Root's acceptance receipt and is handled
separately. The product version remains `0.9.0-beta.4`.

Execution profile: Standard. This local workspace tool affects user Markdown
and recovery diagnostics. The accepted ordinary filesystem, interruption,
malformed-input and cooperating-process boundary remains unchanged. Existing
locks, expected-state checks, atomic BCL operations and recovery evidence
provide the practical recovery boundary. Reuse the pinned runtime, parsers,
recovery catalogue and typed presentation. Exceptional machinery: none.

Focused Unit presentation and Integration coordinate/admission selections
provide the cheapest decisive evidence. Because this closes a public diagnostic
and recovery boundary, run the complete managed suite, Windows x64 Native AOT
Integration and EndToEnd suites, managed EndToEnd on the native CLI, and an
offline installed-package journey. Run whitespace/analyzer verification and
`git diff --check`. Restore only from existing caches with repository
`NuGet.Config` and an empty local feed. A fresh vulnerability database cannot
be obtained under the no-network boundary. Dependency versions remain unchanged.

Keep the one completed whole-task review and grouped correction consumed at
1/1 each, with zero councils. No additional review phase or policy change is
selected. Shared ledgers and other Tasks remain Root-owned.

Done: exact reconstruction, cached-only restores, focused qualification,
managed Unit and Integration, refreshed managed and native public suites,
package journey and formatting passed. The qualification cause and exact owned
transfer are recorded below.
Now: Task 64 “CLI defects and contract drift” (phase 3/3): milestone 3/3,
independent completion complete and ready for Root integration.
Next: Root reconciles shared ledgers and performs task integration and combined
beta5 qualification within its existing ownership.
Blocker: none at the independent Task64 boundary.

## Historical source state

**Task 64 “CLI defects and contract drift” (phase 3 review and grouped correction complete): milestone 3/3.**
The accepted B1-B5, W1/W3/W4/W5, Context, Library mapped-leaf ownership, N1,
N2, and `ExtensionUpdateMutation` fixes shipped in
beta4. B7 is also resolved: successful retained recovery remains
`completed`/exit 0. Task 69's [verified candidate and merge
receipt](task69-next-beta-stabilization-release.md#verified-candidate-and-merge-receipt)
records the clean merge and beta4 release. Existing runtime final confirmations
remain unchanged.

The frozen Option A diagnostic follow-up above has parent code and focused
qualification receipts. RecoveryConflict constructors require an observed
nonblank target, and the four selectors retain that exact file coordinate.
Stored Blocked preparation with a residual stays RecoveryConflict; the concrete
publish-IOException Blocked(null) edge now maps to the existing
Incomplete/RecoveryUnavailable flow at all four route adapters. Init preserves
its prior non-conflict headline and resolved-path finding message behavior.
Compiler-visible target narrowing and ordered test-seed branches address the
review's C# findings. The seven focused groups passed 354/354 executions with
zero skips or failures. The branch audit records the exact storage paths and
the pathless mapping. The optional verified-final coexistence policy remains
unresolved and outside this horizon. At the source-freeze checkpoint, Root
acceptance and broader managed, AOT, security, and dependency qualification
remained pending. Root has since accepted inclusion for beta5 combined
qualification after exact integration; those full gates remain pending. The
Init handoff was then ready for Root verification; Root has since accepted
corrected Init for a later Task 48 resume. Task 48 remains frozen and deferred
beyond beta5. Task 64 remains open, with completion unchecked.

### Whole-task review receipt

- Review `r_db3d1fe31440` (`gpt-6.1-sol/high`) completed with findings. Exact
  supplied finding text and dispositions are recorded in
  `artifacts/task64/integration/whole-task-review.json` and
  `artifacts/task64/integration/whole-task-review.md`.
- The original structured Worker Watch export is saved at
  `artifacts/task64/integration/whole-task-review-original-export.json`
  (SHA-256 `CB17089933F009DFB24A0489512CBBFE9DC65DA9D1952F3AE57809B34B1DF30F`).
  The findings below preserve their review-time wording; Root later released
  the sole grouped correction slot.
- R1 is corrected per Root's disposition: pathless stored Blocked results map
  to Incomplete/RecoveryUnavailable, while a known residual remains an exact
  RecoveryConflict coordinate. R2 restores Init's original non-conflict
  headline fallback and resolved-path finding message. R3 removes nullable
  suppressions and chained seed conditionals and removes Create's dead arm. R4
  uses a typed ordinal digest recipe. Grouped repair 1/1 is consumed; no second
  review is authorized. At that review checkpoint, Root acceptance remained
  pending.

### Parent focused qualification receipt

- Parent candidate base: `2e0da10a5657c084b2847245775858123e09a5ed`.
- Final Unit and Integration Release builds ran sequentially with
  `DOTNET_CLI_USE_MSBUILD_SERVER=0` and `MSBUILDDISABLENODEREUSE=1`. Both built
  with 0 warnings and 0 errors using `--no-restore
  -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1`.
- Unit presentation filter `*RecoveryConflictPresentationTests`: 29 passed,
  0 failed, 0 skipped; snapshot updates were disabled with
  `OPENFORGE_SNAPSHOT_UPDATE=0` and `IMPRINT_UPDATE=verify`.
- Integration filter `*RecoveryConflictCoordinateIntegrationTests`: 25
  passed, 0 failed, 0 skipped.
- Integration filter `*RouteRemovePriorRecoveryIntegrationTests`: 4 passed,
  0 failed, 0 skipped.
- Unit command regression filters `OpenForge.Cli.Core.UnitTests.Commands.Route.Create.*`,
  `OpenForge.Cli.Core.UnitTests.Commands.Route.Init.*`,
  `OpenForge.Cli.Core.UnitTests.Commands.Route.Update.*`, and
  `OpenForge.Cli.Core.UnitTests.Commands.Route.Move.*`: 42, 45, 108, and 101
  passed respectively; each had 0 failures and 0 skips.
- Total across the seven focused selections: 354 passed, 0 failed, 0 skipped.
  Initial build/test assertion failures and their corrections remain recorded
  in the qualification logs.
- Exact commands, exits, and logs are recorded in
  `artifacts/task64/integration/qualification.json` and its `logs` directory.
  The commands run from the parent (with DOTNET_CLI_USE_MSBUILD_SERVER=0 and
  MSBUILDDISABLENODEREUSE=1) were:

  ```text
  dotnet build src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
  dotnet build src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true --disable-build-servers -m:1
  dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --no-restore -- --filter-class '*RecoveryConflictPresentationTests' --minimum-expected-tests 1
  dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build --no-restore -- --filter-class '*RecoveryConflictCoordinateIntegrationTests' --minimum-expected-tests 1
  dotnet run --project src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj -c Release --no-build --no-restore -- --filter-class '*RouteRemovePriorRecoveryIntegrationTests' --minimum-expected-tests 1
  dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --no-restore -- --filter-class 'OpenForge.Cli.Core.UnitTests.Commands.Route.Create.*' --minimum-expected-tests 1
  dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --no-restore -- --filter-class 'OpenForge.Cli.Core.UnitTests.Commands.Route.Init.*' --minimum-expected-tests 1
  dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --no-restore -- --filter-class 'OpenForge.Cli.Core.UnitTests.Commands.Route.Update.*' --minimum-expected-tests 1
  dotnet run --project src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj -c Release --no-build --no-restore -- --filter-class 'OpenForge.Cli.Core.UnitTests.Commands.Route.Move.*' --minimum-expected-tests 1
  ```
  This is focused managed qualification only: no AOT or full security/
  dependency qualification. This pre-integration source receipt did not claim
  Root acceptance.

## Root integrated acceptance addendum — 2026-10-01

Root accepted the grouped R1-R4 closures for inclusion in beta5 combined
qualification after independently verifying 68 source hashes, the ordinal
aggregate, bounded closure, and the exact 69-file integration transfer. The
integrated run `r_315ef7828ab9` passed 29 presentation and 25 coordinate tests,
with zero failures or skips; both Release builds completed with zero warnings
and errors. The separate source qualification remains 354 focused tests passed,
zero failed or skipped. Corrected Init behavior is accepted for a later Task 48
resume; Task 48 remains frozen and deferred beyond beta5. This does not close
Task 64 or complete final combined qualification; managed, native, package,
and user-journey gates remain.

## Historical blocked completion checkpoint — 2026-10-02

**Task 64 remains incomplete and is not independently merge-ready.** The final
managed public gate failed twice with read-only directory snapshot differences.
No admission, safety, presentation or test assertion was relaxed. The smallest
remaining requirement is to resolve or establish the cause of these differences
within an authorized boundary, then pass the complete independent public suite.
No additional review or repair phase is inferred from that requirement.

The exact candidate contains 69 paths: 28 tracked modifications and 41 additions,
including all meaningful previously untracked sources, tests and 32 snapshots.
It contains no other Task's delta, shared ledger, version bump, prompt file or
ignored artifact. All 68 source/contract/test files matched both the frozen
integrated manifest and original final author tree before qualification. Original
producer and presentation patch hashes were also independently checked. The
only subsequent source deviation corrects three indentation lines in
`RouteInitPlanFinalizer.cs` after `dotnet format whitespace` reported them; a
byte/line comparison proves every line has identical non-leading-whitespace
content. The canonical Task record separately preserves Root's acceptance and
adds this current evidence. The 68-file final ordinal aggregate is
`BF07BE3A006AF2DB19DF06E1D9861B140898CCFD2CEDCD67F9990013BCD099F6`.

Independent evidence from this worktree, with snapshot updates disabled:

| Check | Result |
| --- | --- |
| Cached-only solution and three Windows x64 native restores | Exit 0 each; no new dependencies or network restore |
| Final Release solution build | Exit 0; zero warnings and errors |
| Seven focused selections | 354 passed, zero failures or skips |
| Final managed Unit | 3,904 passed, zero failures or skips |
| Final managed Integration | 2,693 passed, zero failures; 17 platform exclusions qualified by the repository Windows allowlist |
| Earlier managed public suite | 263 passed, zero failures or skips; historical passing evidence |
| Final managed public attempt | Exit 2; 262 passed, one F10 read-only snapshot failure, zero skips |
| Focused unchanged F10 reproduction | Exit 0; two passed, zero failures or skips |
| Complete unchanged managed public recheck | Exit 2; 261 passed, F03 and F18 read-only snapshot failures, zero skips |
| Native AOT Integration | Exit 0; 2,693 passed, zero failures; the same 17 qualified platform exclusions |
| Native AOT EndToEnd | Exit 0; 263 passed, zero failures or skips |
| Managed EndToEnd against the native CLI | Exit 0; 263 passed, zero failures or skips |
| Offline installed-package journey | Exit 0; one journey passed, installed native bytes matched the published executable |
| Final whitespace and full analyzer verification | Exit 0; no changes, no missing references or analyzer-load warnings |
| Git whitespace and reverse binary patch applicability | Exit 0 |

The first full Integration attempt failed nine cases with the long temporary
path inside this Git worktree. Eight diagnostic snapshots truncated raw paths
before normalization; one non-Git fallback observed the containing repository.
The same tests passed using a short isolated temporary directory outside Git.
Earlier analyzer attempts returned exit 0 but reported unloaded references;
they are explicitly unqualified. The final full formatter loaded all references
after removing extra `MSBUILDNOINPROCNODE` and `Configuration` overrides while
retaining build-server suppression.

The final public failures are unresolved. F10 reported `.agents/guidance`;
F03 and F18 reported a recovery workspace directory and its data-home directory.
The snapshot captures attributes, creation time and last-write time; failure
output omits the differing values. No particular field or cause is established,
and no product write is inferred solely from the assertion. F18 did not time
out. These failures remain in the raw reports, command history and receipt.
Doctor ran only in scratch test workspaces.

Earlier accepted scope remains covered by the beta4
[Task 69 release receipt](task69-next-beta-stabilization-release.md#verified-candidate-and-merge-receipt).
The current [Workspace State Files decision](../../../crystallized/decisions/framework/workspace-state-files.md)
records persistent removal lists and their meaning; Customizing advises
`open-forge remove <path>`, and `WorkspaceRemovals.cs` points to archived Task 50.
The [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
provides the current presentation home. These baseline documents were inspected
and require no additional Task64 delta. The original four broad checklist items
remain unchecked while independent completion is blocked. Optional verified-final
coexistence, paused Tasks 39/34, deferred Task 48 and Task 70 ownership remain
outside this completion candidate.

`artifacts/task64/sol-completion/completion-receipt.json` records every expanded
qualification command, exit, pass/fail/skip count, log hash, qualified platform
skip, binary identity, unchanged protected path and remaining requirement.
`inventory.json` records every exact path, action, base Git-blob SHA-256 and
current-byte SHA-256. `task64-owned.patch` is the full-index binary transfer,
including safe additions; `patch-identity.json` identifies its hash and checked
reverse applicability. Frozen manifests, original review/repair receipts and
patch provenance are retained beside it. The real Git index and base HEAD remain
unchanged; nothing was staged, committed, merged, pushed or released.

The beta4 version and all dependency pins/configuration remain unchanged.
A fresh vulnerability database audit was not run under the mandatory cached-only
no-network boundary; no fresh audit is claimed. Root alone owns squash integration,
shared-ledger reconciliation, combined beta5 qualification and release. This
receipt does not authorize integration while the independent public gate is red.

## Current completion state — 2026-10-02

**Task 64 “CLI defects and contract drift” (phase 3/3): milestone 3/3 complete;
independently merge-ready for Root.** Root explicitly authorized investigation
and minimal repair of the repeated read-only directory snapshot failures as
qualification closure, with the existing review budget unchanged. The earlier
blocked checkpoint remains above for provenance and is superseded by this state.

### Metadata cause and bounded closure

The original short temporary root was on ReFS. Exact assertion diagnostics
reproduced three F03 failures under focused concurrent load: only
`lastWriteUtcTicks` changed in the empty recovery directory; its attributes and
creation time were identical. No entries were added or removed. The nine focused
F03/F10/F18 cases passed in four earlier sequential diagnostic runs, while the
four concurrent ReFS runs passed 33 cases and failed three.

A no-CLI control demonstrated the underlying timing mechanism. Create a child
file, open a read handle permitting deletion, delete the child, observe an empty
directory, and then close that read handle. On ReFS, the final close changed the
parent directory's last-write time even though the child name was already absent.
The same control on NTFS kept its unlink-time directory metadata unchanged. The
controlled read handle closes before the final stable baseline is captured. This
separates delayed completion of a previous deletion from a new product write.
Simple and prepared-fixture idle controls on both volumes observed no metadata
changes; the held-reader control isolates the relevant condition.

Further controls denied Write, Delete and DeleteSubdirectoriesAndFiles to the
current user on the observed recovery bucket and guidance tree. Six Doctor and
six Context invocations on each volume retained their expected exit 2 with writes
denied: 24 successful read-only invocations. An actual RouteCreate write on each
volume failed with exit 4, confirming that denial was effective. The CLI binary
closures were preserved byte-for-byte throughout. All 36 focused cases passed
under concurrent NTFS load with unchanged assertions and parallelism.

The original read-handle holder was not instrumented; no attribution to an
antivirus or another named process is claimed. Unique owned paths and effective
write-denial controls separate this from shared current-user writes. The
ReFS delete-completion mechanism is directly demonstrated, matches every
instrumented differing field, and explains why an exact metadata baseline could
precede the physical completion of an earlier fixture mutation.

Qualification now uses a fresh short isolated NTFS temporary root outside Git,
where the control demonstrates stable directory metadata at unlink. The exact
comparison still covers path membership, types, attributes, creation and
last-write times, link identities, lengths and content hashes. Capture order,
missing/changed classification, added-path classification and the equality
predicate are unchanged. There is no retry after the observed command and no
normalization, ignored field, timing sleep or parallelism change. One existing
EndToEnd helper now prints exact before/after values when equality fails; that
is the only additional tracked source file beyond the frozen candidate.

### Final independent qualification

| Final evidence | Result |
| --- | --- |
| Fresh managed public suite, final diagnostics, preserved managed CLI | Exit 0; 263 passed, zero failures or skips |
| Republished Native AOT EndToEnd, final diagnostics, preserved native CLI | Exit 0; 263 passed, zero failures or skips |
| Rebuilt managed EndToEnd against the preserved native CLI | Exit 0; 263 passed, zero failures or skips |
| Full whitespace/analyzer verification after closure | Exit 0; no source changes, unloaded references or analyzer-load warnings |
| Exact owned Git diff whitespace and reverse binary patch check | Exit 0 each |

The final managed Unit and Integration evidence remains 3,904 and 2,693 passed
respectively, with the 17 Integration platform exclusions independently
qualified by the repository's Windows allowlist. Native Integration remains
2,693 passed with the same 17 qualified exclusions. Their exact input closures
are checked against the recorded passing executions. The additional helper
belongs only to EndToEnd, so all three dependent public runtime gates were
rebuilt or rerun. The native CLI and installed-package inputs are byte-identical
to their earlier passing independent journey; that package evidence remains
applicable. No production repair was needed for this metadata observation.

The final owned transfer contains **70 paths: 29 modifications and 41 additions**.
It comprises the accepted 68-file manifest, the canonical Task64 record, and the
one Root-authorized EndToEnd diagnostic helper. The 68-file final ordinal
aggregate remains
`BF07BE3A006AF2DB19DF06E1D9861B140898CCFD2CEDCD67F9990013BCD099F6`;
the sole deviation within that manifest remains the three proven indentation
lines. `qualification-closure-inventory.json` identifies the additional helper
and its before/after hashes. The four original broad checklist items are now
checked against the accepted beta4 Task69 release evidence and the inspected
current removal, help/contract and shared-presentation sources cited above.

`artifacts/task64/sol-completion/diagnosis-receipt.json` retains the exact differing
values, control inputs/results, preserved CLI closure and explicit causal limit.
`completion-receipt.json` records the complete final evidence, every failed
attempt, input and log hashes, precise expanded commands and exits, platform
exclusions, unchanged protected paths and independent merge readiness.
`inventory.json`, `patch-identity.json` and `task64-owned.patch` provide the exact
owned transfer. The original blocked receipt and patch are also preserved.

No independent Task64 requirement remains. Root owns shared-ledger reconciliation,
task squash integration and combined beta5 qualification/release. The base HEAD,
real Git index, beta4 version, dependency pins and shared ledgers remain unchanged.
The existing review and grouped repair budgets remain consumed at 1/1 each;
qualification closure adds no review phase. Cached-only restore and the absence
of a fresh network vulnerability audit remain explicitly recorded limits.

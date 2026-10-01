---
open-forge:
  description: Open Task 64 to fix the CLI defects, wording errors, and contract contradictions found by the documentation review, and the leftovers of the closed removal and remediation Tasks
  tags: [Memory, Working, Task, CLI, Defect, Contract, Wording, Contextual, Active]
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
- **B6:** recovery coexistence policy remains pending the user's answer.

These are accepted conformance fixes, not new ownership authority.

## Done when

- [ ] Each defect is fixed with a capture or test that pins it, or recorded as
      accepted behavior with a reason.
- [ ] Each contract contradiction is resolved in the contract, the help, or the
      code, so the three agree.
- [ ] The removal Decision is recorded, and the `route remove` interface and
      the Customizing page match it.
- [ ] The shared presentation rules live in a current source that Tasks 32 and
      34 can point to.

## Current State

**Now:** Active within the authorized bounded beta-stabilization horizon in
[Task 69](task69-next-beta-stabilization-release.md). The existing defect and
contract scope and unresolved policy questions remain visible. Root has
integrated B1–B5, W1/W3/W4/W5, Context, Library mapped-leaf ownership, N1,
N2, and the `ExtensionUpdateMutation` regression. B2 supplies the full
verified registered-Library read-only projection through
`UpdateProjectionInputReader.cs`; mutation no-follow guards are unchanged. B3
includes the shared `PlanProjectedEntries` path and generated-only
package/nonpackage reconciliation. These corrections add no new production
policy or shared contract. Version `0.9.0-beta.4` is allocated in
`package.json`, `package-lock.json`, and `Directory.Build.props`.

On beta4, `npm run verify` exited 0: `check:delivery` was green, 55 delivery
tests and 7 package-layout tests passed across all six targets (layout only,
not host execution), dotnet whitespace/analyzers were green, and 13
`RepositoryMarkdownTests` passed with 0 skipped against the current repository
inventory. The earlier 39-test result, 38 passed, 1 failed, and 0 skipped, is
resolved historical evidence. The four-class snapshot refresh passed 47/47
with 0 skipped, and Root independently reviewed its 68 changes: 32 JSON-only
top-level `next` changes and 36 human `Next` or empty-Extension-list wording
changes. The beta4 managed run passed via `npm run test -- --no-restore`:
unit 3,875 passed with 0 excluded, integration 2,668 passed with 17
documented Unix platform exclusions, public 263 passed with 0 excluded, and
zero failures. The build completed with 0 warnings/errors. Exact reports are
at `artifacts/delivery/managed-reports/{unit,integration,public}/results.json`.
Native AOT/package gates, the whole-candidate review, merge, and release remain
pending. B6 recovery coexistence is the sole pending user decision. No changed
policy or release-green claim is made.

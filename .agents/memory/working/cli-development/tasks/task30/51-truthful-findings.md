---
open-forge:
  description: Task 30 phase 5-D slice 51 making every blocking finding name its subject, state its consequence and give a usable next action, and stopping repair reporting success over a still-blocking condition
  tags: [Memory, Working, CLI, Task, Subtask, Diagnosis, Contextual, Active]
---

# 51 — Truthful blocking findings

## Outcome

A reader who hits a blocking finding learns what is wrong, where, what it costs
them, and one command that moves them forward. And a `repair` that says it
succeeded has actually unblocked the workspace.

## Depends on

[50](50-diagnosis-ownership.md). The ownership table decides which findings must
meet this bar in which command, so the bar cannot be applied before it exists.

## Current state, measured

Measured on 2026-09-16. Half of this is already enforced.

- `CliReportInvariantsTests.AssertSubject` already asserts that every listed
  subject carries **either a path or an identifier**, across the whole capture
  corpus.
- The invariants also already hold "at most one `Next:` line, and it is the last
  line", with Extension Install's catalogue-required continuation as the
  accepted exception.
- What is **not** enforced: that a blocking finding states its consequence, that
  its next action is copy-pasteable, and that `repair` never reports success
  while a corresponding `doctor` finding still blocks.

## Actionable boundary

- **Extend `CliReportInvariantsTests`; do not add a parallel test class.** It
  already walks every capture and owns this family of rules.
- "Copy-pasteable" means the next action is a command a reader can paste
  unchanged, with concrete values substituted — not a template with `<id>` left
  in it. Check what is actually rendered, not what the catalogue row shows.
- The repair-truthfulness rule is the substantive half. Prove it from a seeded
  workspace: run `doctor`, run `repair`, run `doctor` again, and assert that
  anything `repair` reported as completed is no longer blocking. Use slice 50's
  shared seeded workspaces.
- Where a finding cannot state a consequence because the result model does not
  carry the fact, **report it** rather than inventing wording. That is the same
  shape as the `reference-unsafe` and `mapping-blocked` contract decisions, and
  it is a maintainer decision.
- A situation with **no capture** is invisible to the corpus invariants. Record
  which blocking findings have no fixture rather than assuming they pass.

## Acceptance

- Every blocking finding names its leaf subject, carries a non-null target,
  states the consequence, and gives a copy-pasteable next action — enforced in
  `CliReportInvariantsTests` where the corpus can see it, and recorded as a gap
  where it cannot.
- A test proves `repair` does not report success over a condition `doctor` still
  reports as blocking.
- Every finding that cannot meet the bar without a new result fact is recorded
  as an open decision, not reworded around.
- All four gates green, with every regenerated capture reviewed per situation.

## Changes ledger

- invariant: blocking findings had no rendered-command assertion -> `CliReportInvariantsTests` now requires every captured `blocked-repair` finding to expose a non-empty `open-forge ...` command from its finding actions or report `next`, with no `<...>` placeholder; the existing `AssertSubject` path-or-identifier rule remains the subject check.
- repair: ordinary Repair mapped only operational coverage -> it now preserves the three observed Repair states, maps blocking `Workspace*`, `Route*`, and `Reference*` Doctor finding kinds to `WorkspaceContainment`, `RouteAndHeading`, and `LocalReference` respectively, recomputes `SelectedScope`, and keeps the top-level blocked Doctor status broad; Library, Framework, Extension, and Recovery per-finding blockers remain outside ordinary Repair dependencies, while Library recovery remains scoped to its selected operation.
- test: the shared handoff had no mixed blocking residual -> `DiagnosisOwnershipIntegrationTests.RepairDoesNotReportCompletionOverBlockingDoctorFinding` adds a second `Entries` section to the existing seeded route workspace for the required-domain case, then adds the same workspace shape's `shared/library` ownership record for an unrelated `library.projection-retargeted` blocker and proves required blocking, unrelated completion, and the respective link states.
- situation `diagnosis-ownership-blocked-repair`: before `repair` returned `status: "completed"` while Doctor still reported `route.generated-region-duplicate`; after it returns `status: "blocked"` and does not rewrite the link. This scenario has no output capture.
- situation `diagnosis-ownership-unrelated-library-blocked`: before `repair`, Doctor reported `library.projection-retargeted` as `blocked-repair` while the document link was `./guide.md`; after `repair`, it returns `status: "completed"`, rewrites the link to `guide.md`, and Doctor still reports the Library blocker. This scenario has no output capture.
- capture: no capture was regenerated -> the invariant and handoff tests inspect existing snapshots or structured integration output only; no frozen string, JSON shape, or snapshot file moved.
- wording: `context.closure-unavailable` changed from `The startup files could not be resolved: <reason>.` to `The startup files could not be resolved: <reason>. Some context was not included.`; the closure and followed-link paths mark the result incomplete and omit the unavailable context.
- wording: `context.layer-unavailable` changed from `<path> could not be read.` to `<path> could not be read, so it was not included.`; the context projection builder emits no usable layer content when the layer read is incomplete.
- wording: `context.fragment-missing` changed from `The link at <path>:l:c points to <file>, which has no heading <#fragment>.` to `The link at <path>:l:c points to <file>, which has no heading <#fragment>. It was not followed.`; unresolved fragment targets are not followed by `ContextLinkExpander`.
- wording: `context.target-unreadable` changed from `The link at <path>:l:c points to <file>, which could not be read.` to `The link at <path>:l:c points to <file>, which could not be read. It was not followed.`; unreadable targets remain unresolved and receive `FollowState.NotFollowed`.
- contract: the four corresponding rows in `.agents/memory/crystallized/documents/cli/contracts/context/interface.md` were synchronized with the rendered wording.
- capture: `OPENFORGE_SNAPSHOT_UPDATE=1` was exported and integration snapshot regeneration was attempted; the integration build stopped before the test executable was produced because the sandbox denied writes under `artifacts/obj/OpenForge.Cli/release`, so no capture line was regenerated or changed. Fixture inspection found no Context snapshot for the four changed codes and no Find snapshot for `find.projection-unavailable`.

## Divergences observed

- consequence: the shared `CliFinding` / schema-3 result model carries message, subject, resolution, actions, and evidence but no typed consequence fact; the invariant therefore cannot prove that a blocking message states cost/consequence. This remains an open maintainer decision; no wording or placeholder consequence was invented.
- corpus: the invariant `SnapshotCorpus` scans integration command captures and contains three blocking command-local variants (`install.recovery-unavailable`, `repair.proposal-unavailable`, and `repair.workspace-lock-unavailable`); Doctor has no integration capture in that corpus. The Doctor unit-only `workspace.unavailable` capture is also outside the corpus, so all 39 Doctor matrix rows carrying `blocked-repair` remain uncaptured here (36 errors plus 3 warnings): `workspace.unavailable`, `workspace.not-directory`, `workspace.agents-inaccessible`, `workspace.loader-missing`, `workspace.loader-unreadable`, `workspace.loader-malformed`, `workspace.entry-ambiguous`, `workspace.entry-compatibility-collision`, `workspace.path-invalid`, `workspace.path-containment`, `workspace.physical-alias`, `recovery.bundle-collision`, `recovery.provenance-unavailable`, `route.entrypoint-duplicate`, `route.escape`, `route.generated-region-malformed`, `route.generated-region-duplicate`, `route.compatibility-conflict`, `reference.fragment-unverified`, `reference.destination-encoding`, `reference.target-outside-workspace`, `reference.target-physical-escape`, `reference.target-alias`, `reference.target-unreadable`, `framework.lifecycle-evidence-unavailable`, `framework.bridge-boundary`, `framework.root-region-boundary`, `framework.partial-lifecycle`, `framework.partial-recovery`, `extension.manifest-malformed`, `extension.duplicate-id`, `extension.dependency-cycle`, `extension.catalogue-unavailable`, `extension.partial-lifecycle`, `library.source-root-invalid`, `library.source-root-aliased`, `library.projection-dangling`, `library.projection-retargeted`, and `library.link-capability-unsupported`.
- domain: `DoctorDomainReport.Domain` alone cannot distinguish Library findings because `LibraryDoctorInspector` extends the WorkspaceEntry report; the mapper therefore uses the stable `DoctorFindingKind` families. `library.source-root-invalid` intentionally remains covered by the broad top-level `status: "blocked"` rule; `library.projection-retargeted` supplies the non-required-domain proof because its Library coverage remains complete.
- ownership: Recovery, Framework, Extension, and Library per-finding blockers have no ordinary document-link Repair dependency domain in slice 50, so the mapper leaves the observed Repair states unchanged for them; the top-level blocked result and the dedicated Library-recovery path retain their existing broader or selected-scope behavior. No command, consequence, or finding code was invented.
- overload: `context.closure-unavailable` is reached both from startup/selected-source closure loading and from a followed-link target that cannot be added to the Context graph. Both paths mark the result incomplete and omit affected context, so the shared clause is deliberately generic; splitting the code or correcting the pre-existing “startup files” lead-in needs a maintainer decision.
- candidate: `context.projection-unavailable`, `context.target-case-mismatch`, `context.frontmatter-missing`, `context.section-missing`, `find.projection-missing`, and `find.projection-unavailable` were left unchanged. The projection-unavailable messages already state that the requested part could not be produced; target-case-mismatch can be corrected and followed and may co-occur with a later fragment failure; frontmatter-missing and section-missing are self-evident facts; and the two Find projection rows respectively describe a self-evident missing section or already carry the consequence in “could not be produced”.
- capture: because the integration build was blocked before tests ran, capture review is limited to the fixture inventory; no authorized capture line was available for the four changed Context situations (`closure-unavailable`, `layer-unavailable`, `fragment-missing`, or `target-unreadable`).
- gates: the post-change unit executable remained at 3,206 passed, 0 failed, 0 skipped; `dotnet format whitespace ... --verify-no-changes` still reports the five recorded whitespace errors, all in unrelated pre-existing files; integration execution remained unavailable because the sandbox denied writes under `artifacts/obj/OpenForge.Cli/release`.
- invariants: no finding code, severity, status, exit code, count, ordering, result field, or Find wording changed.

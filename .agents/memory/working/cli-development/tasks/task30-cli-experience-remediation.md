---
open-forge:
  description: Open Task 30 for CLI experience remediation, with actionable phase state and evidence carried from Emerging Analysis
  tags: [Memory, Working, CLI, Task, Remediation, Contextual, Active]
---

# Task 30 — CLI Experience Remediation

## Task state

- State: **Active**; G1, B1 and G4 are complete. G4 verification and
  documentation propagation closed after the six executable modes passed.
- Owner: Root, direct sequential implementation.
- Authority: maintainer decisions recorded in this Task and its active
  subtasks. Recommendations in Emerging Analysis are not acceptance.
- Current next step: continue with the planned Phase 5 diagnosis and
  interoperability boundary. The [G4 packet](../../../archived/cli-development/tasks/task30-g4/_task30-g4.md) is
  complete: its accepted decisions C1–C18, shared rules, six lanes, 28 command
  subtasks, verification and documentation propagation are recorded. [B1 heading-based
  Entries](../../../archived/cli-development/tasks/task30/07-b1-heading-entries.md) completed automatic old-guard migration,
  reviewed snapshots, same-commit contracts and managed/native qualification.
  Safe M1 is complete and qualified in that native integration wave. A1–A6 and
  [P1](../../../archived/cli-development/tasks/task30/06p-p1-shared-permissions.md) completed the accepted state/permission
  migration; their receipts own the deletion-safety evidence and resolved review
  findings. Structural items 2, 4 and 5 remain deferred. The [plan](../plan.md)
  owns the order and the [progression report](../../../archived/cli-development/tasks/task30/progression-report.md)
  gives the full handoff.
- Review follow-ups: [SG-R1 and SG-R2](../../../archived/cli-development/tasks/task30/review-second-gate-pre-g4.md#accepted-disposition-and-follow-ups)
  are resolved: SG-R1 contracts match current code, and the maintainer retired
  SG-R2's legacy-receipt notice requirement. The reviewed implementation was
  integrated locally at `e40492aa`. The bounded
  [G4 preparation record](../../../archived/cli-development/tasks/task30/phase-4b-g4.md#preparation-reconciliation)
  preserves obsolete assumptions without rewriting historical proposals.
- Continuation authority: the maintainer authorized consecutive pre-G4 slices
  on 2026-09-13 when implementation divergences can be resolved from the accepted
  code and documents. The G4 design gate closed on 2026-09-14; the G4 subtasks
  carry the same authority, record divergences only in their own file (C17),
  and stop at any unresolved consequential choice. A consequential unresolved
  safety meaning still requires clarification.
- Evidence baseline: 18 pre-existing unit failures of 3,365; a disposable
  characterization harness captured 297 read-only outputs across three seeded
  workspaces and remained byte-identical through the completed refactoring
  slice.

## Evidence boundary

The [CLI Experience Audit](../../../emerging/analysis/cli-experience-audit/_cli-experience-audit.md)
remains Emerging evidence about observed defects and proposed remedies. The
[CLI Design Retrospective](../../../emerging/analysis/cli-design-retrospective/_cli-design-retrospective.md)
remains Emerging evidence about their origins. This Task owns execution, decisions,
implementation discoveries, and acceptance. The phase subtasks below copy the
facts needed to execute without reconstructing the Task from the analyses.

## G4 Closeout

The merged G4 implementation uses one report model and schema-3 envelope, with
`--format text|json`, `--detail minimal|standard|full|debug`, and repeatable
`--detail-filter error|warning|info|all`. The status names are `completed`,
`completed-with-warnings`, `incomplete`, `invalid-input`, `blocked`, `failed`,
and `cancelled`. Confirmation flows render the established minimal plan before
each confirmation, and explicit `--allow-path` grants publish only during
confirmed application.

Verification ran all six test suites with `--parallel collections`. Native AOT
qualification required `vswhere` on `PATH` and was run by the overseer on an
unsandboxed host; a sandboxed worker cannot run that gate. The remaining Task
30 phases are outside G4.

## Phase map

The phase labels come from the analysis groups and do not sort. `Order` is the
sequence to execute in.

| Order | Phase | State                                                                                                    | Execution slices                                                                                                                                                                                                                                       | Task-owned route                                                                   |
| ----- | ----- | -------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------- |
| done  | 0–3   | Complete                                                                                                 | —                                                                                                                                                                                                                                                      | [Completed baseline](../../../archived/cli-development/tasks/task30/phase-0-3.md)                                          |
| done  | 3.5   | Complete for the carried refactoring slice; Task 31 remains open for its own work                        | —                                                                                                                                                                                                                                                      | [Completed baseline](../../../archived/cli-development/tasks/task30/phase-0-3.md)                                          |
| 1     | 4a    | Complete — A1–A6 and P1 qualified                                                                        | [01](../../../archived/cli-development/tasks/task30/01-a1-ownership-store.md) [02](../../../archived/cli-development/tasks/task30/02-a2-extension-writers.md) [03](../../../archived/cli-development/tasks/task30/03-a3-library-writers.md) [04](../../../archived/cli-development/tasks/task30/04-a4-extension-readers.md) [05](../../../archived/cli-development/tasks/task30/05-a5-status-doctor-library-readers.md) [06](../../../archived/cli-development/tasks/task30/06-a6-delete-lifecycle.md) | [State model and diagnosis](../../../archived/cli-development/tasks/task30/phase-4a-g1.md)                                 |
| 2     | 4a-S  | **B1 complete** — items 2, 4 and 5 deferred                                                              | [07](../../../archived/cli-development/tasks/task30/07-b1-heading-entries.md) for items 1 and 3; items 2, 4 and 5 deferred                                                                                                                                                                     | [Structural follow-up](task30/phase-4a-structural.md)                              |
| 3     | 4b    | **Complete** — G4 rendering, interaction, command lanes, verification and documentation propagation green | [G4 packet](../../../archived/cli-development/tasks/task30-g4/_task30-g4.md): 01 snapshots, 02 naming, 03 rendering system, 04 interaction, 05 index region, 10–37 one per command in six lanes, 40 verification, 41 documentation propagation | [Presentation revamp](../../../archived/cli-development/tasks/task30/phase-4b-g4.md) |
| 4     | 5-D   | **Specified, not started** — four slices, current state measured                                       | [50](task30/phase-5-diagnosis-and-interoperability.md) ownership, [51](task30/phase-5-diagnosis-and-interoperability.md) truthful findings, [52](task30/phase-5-diagnosis-and-interoperability.md) interoperability inputs, [53](task30/phase-5-diagnosis-and-interoperability.md) one normalizer                                                                                                                                                                                                                                      | [Diagnosis and interoperability](task30/phase-5-diagnosis-and-interoperability.md) |
| 5     | 5–8   | **Mostly absorbed** — only phase 6 (document ownership) remains distinct                                 | measure whether any statement is duplicated and drifting; close if not                                                                                                                                                                                                                                      | [Interaction, content, and scenarios](task30/phase-5-8.md)                         |
| 6     | 7-S   | **Specified, not started** — three slices, current state measured                                        | 70 coverage matrix, 71 dirty and relocated journeys, 72 normalization audit                                                                                                                                                                                                                                      | [Scenario evidence](task30/phase-7-scenarios.md)                                   |

Task 31's [slice 08](../../../archived/cli-development/tasks/task31/08-m1-enumerated-extractions.md) runs independently
of this order and blocks nothing.

## Completed phase 4a — G1 state model

The current actionable model is:

- Use two npm-shaped files in `.agents`: `open-forge.json` for authored
  configuration and `open-forge.lock.json` for ownership state.
- Detect changes through the existing recovery story and user-visible Git
  information when a `.git` directory is present; do not invoke Git as a
  required dependency and do not reintroduce per-file fingerprints.
- Keep `allowInstallPaths` in `open-forge.json`, with a noninteractive
  `--allow-path <path>` for the six permission-gated commands. Entries are
  paths, not patterns.
- Keep a `removed` category list so user removal is not silently restored.
- The lock is ownership-only and **accepted**: installed versions and owned
  files, written as a verified write-time receipt rather than a payload
  projection. Missing or stale lock state is best-effort and never a command
  gate, and it fails toward a missed deletion. `extension remove` no longer
  distinguishes changed from unchanged content, and `baselineFingerprint` leaves
  public output entirely. The validation evidence and the three accepted
  refinements are in the [G1 subtask](../../../archived/cli-development/tasks/task30/phase-4a-g1.md).

The following are not reopened in this Task: the maintainer's rejection of a
fingerprint-chain change detector, the no-git-cleanliness gate, and the choice
to preserve recovery bundles rather than create `.bak` files.

## Open findings

**Both findings below are settled; neither is live work.** They are kept in
full because they record what was observed and why the disposition was chosen.

- **Settled — resolved by deletion, pending item 2.** Awaiting the marker
  removal in [structural follow-up](task30/phase-4a-structural.md), which is
  deferred on a Framework decision. `.prettierignore` is the whole containment
  until then, and that is deliberate rather than provisional.
- **Settled — fixed and covered.** Decision C16 and
  [05 Index Entries region](../../../archived/cli-development/tasks/task30-g4/05-index-entries-region.md) are merged.
  `IndexEntriesPreservationTests` pins the reported scenario with the theory
  "Status and Update agree about authored Entries prose before and after Index",
  so the `status`/`update` disagreement is closed as well.

- **Prettier and `index` disagree about generated-index regions.** Prettier
  inserts a blank line after `<!-- open-forge:generated-index:start -->` and
  before the matching `end` marker; `open-forge index` writes the region without
  them. A workspace formatted by Prettier and then indexed churns on every run,
  in both directions. Measured on 2026-09-12 when formatting this repository's
  own `.agents` tree, and again as four integration failures once the shipped
  payload under `src/open-forge/` was formatted, because installed bytes must
  match what `index` regenerates.
  - Contained for now by `.prettierignore`, which fences the shipped payload,
    `apm.lock.yaml`, and `vendor/`.
  - **Resolved by deletion, not by a fix.** The maintainer confirmed on
    2026-09-12 that the comment guards are going away entirely, so making the
    writer emit Prettier-compatible blank lines would be work on a shape that is
    being removed. Keep the guards until then; the `.prettierignore` fence is the
    whole containment until they go.
  - The removal is proposed in
    [Structural requirements and markers](../../../emerging/analysis/cli-experience-audit/structural-requirements-and-markers.md),
    section 2: locate the Entries region **by heading** with the Markdig parser
    already in use, the way `## Axioms` is already located, and drop the markers.
  - It is now routed to the planned [structural follow-up](task30/phase-4a-structural.md),
    which owns the heading migration, parser disposition, and idempotence
    evidence before any marker deletion.
- **`index` deletes authored text written after `## Entries`.** Observed on
  2026-09-13 on a managed build of `d5824851` in a scratch workspace outside
  the repository, while capturing transcripts for the G4 output design. One
  prose line was appended after the Entries list of `.agents/maps/_maps.md`.
  `status` reported the file as changed and its section as stale. `index` then
  rewrote the section body to the empty placeholder and the appended line was
  gone. The recovery bundle was removed after verification, so nothing outside
  Git preserves it. The heading-based section from B1 extends to the end of the
  file, so text after the generated list is treated as generated interior.
  `update --dry-run` reported the same file as `current: same; intended: same`
  because the semantic fingerprint excludes the region interior, so `status`
  and `update` disagree about one state. They are recorded in the
  [G4 output proposal](task30/phase-4b-g4-output-proposal-fable.md#part-5--discoveries-and-deviations)
  and are not presentation questions. The maintainer decided on 2026-09-14
  that `index` rewrites only the list of entries and nothing before or after
  it (decision C16); [05 Index Entries region](../../../archived/cli-development/tasks/task30-g4/05-index-entries-region.md)
  owns the fix and gives `status` and `update` the same boundary.

## Execution rules

- Update this Task or its phase subtask when implementation changes scope,
  assumptions, evidence, or acceptance.
- Do not rewrite either Emerging Analysis folder merely to reflect code
  findings; keep current implementation reality in this Task and its subtasks.
- Do not implement an unaccepted architecture, public contract, dependency,
  lifecycle, or safety change. Record the decision frontier and stop.
- Keep Task 31's behavior-preserving refactors separate from Task 30's
  lifecycle redesign; do not deduplicate code that G1 is about to replace.

## Acceptance

Each phase must record its decisive evidence in the owning subtask. Behavior
preserving changes must retain the characterization baseline; output-changing
work must define and review its contract and snapshots before renderer rewrites.
The task is complete only when its accepted phases, affected regressions, and
integration state are recorded here.

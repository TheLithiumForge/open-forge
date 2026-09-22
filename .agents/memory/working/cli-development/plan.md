---
open-forge:
  description: Current migration selection and retained Task 30/31 sequence
  tags: [LoadNow, Memory, Working, CLI, Plan, Contextual, Active]
---

# Replacement CLI Development Plan

## Current selection

The maintainer requested the [lossless source wording proposal](../src-wording-proposal.md)
on 2026-09-22. The maintainer subsequently authorized all eight replacements; they are
applied to source and matching workspace copies for review. The [default Skill indexing follow-up](tasks/task47-default-skill-indexing.md)
is recorded for later specification under Task 47, with Task 46 sharing the
native metadata and topology boundary.

The previous D1–D6, E1–E2 and N1–N3 corrections are complete and squash-integrated
into `develop`. Their [execution record](tasks/beta-follow-ups/execution.md)
retains the independent review and six passing Windows managed/native modes.
Broader error-family corrections and historical upgrade qualification remain
separate follow-ups. Other backlog work is not activated.

## Completed migration context

The maintainer selected [Task 38's migration](tasks/task38/_task38.md) on
2026-09-19 for the project split, test boundaries and later output-text
extraction. Its completion record retains the outcome;
structural stages 0–4 are accepted and committed locally, G5 is accepted, and
approved G6 is accepted in [Task45](tasks/task45/_task45.md). The
[subsequent qualification](tasks/task45/beta-baseline-acceptance.md) resolves
all four former failures; platform exclusions remain explicitly recorded.
The Task 30/31 sequence below is retained context, not a second instruction
to dispatch overlapping work.

## Retained Task 30/31 Context

The following records preserve earlier sequencing and evidence. They are not
dispatchable instructions for the current Task 38 migration. Follow its linked
master plan for all current execution and resolve any overlap there.

### Recorded State

- [Task 30 — CLI Experience Remediation](tasks/task30-cli-experience-remediation.md)
  completed G1 through A1–A6 and P1, then
  [B1 heading-based Entries](tasks/task30/07-b1-heading-entries.md), with reviewed
  before snapshots, same-commit contracts and passing managed/native suites.
  Existing generated guards migrate automatically; payload and repository
  entrypoints use headings and Index/Prettier stability is proved. Safe M1 is
  complete and qualified in B1's native integration wave. Structural items 2,
  4 and 5 remain deferred. Integrated locally into `feature/development-2`. The
  [SG-R1/SG-R2 dispositions](tasks/task30/review-second-gate-pre-g4.md#accepted-disposition-and-follow-ups)
  record corrected contracts and the maintainer-retired legacy-receipt notice.
  **G4 is complete.** The accepted decisions, shared rules, six lanes, 28
  command subtasks, verification and documentation propagation are recorded in
  the [G4 packet](tasks/task30-g4/_task30-g4.md). All six executable modes use
  `--parallel collections`; the supported Native AOT gate was run by the
  overseer on an unsandboxed host with `vswhere` on `PATH`.

- [Task 31 — Implementation Duplication Removal](tasks/task31-implementation-duplication.md)
  remains open. M1, M2, M3 and M4 are complete within their accepted
  boundaries; Route Move/Remove stays separate, and M5 waits for the stabilized
  presentation structure.
- The reviewed analysis conclusions are routed through the Task 30/31 phase
  subtasks. Framework-level and contract-editorial follow-ups remain in the
  [candidate queue](tasks/potential/_potential.md), not in the active plan.

### Previous Execution Order

The earlier slices below have a [Work Plan](../../../patterns/work-plan.md) written
ahead of time: exact sites, ordered steps with verification commands, acceptance
checkboxes, and a Divergences section filled during implementation. The Goal, References, Expected result, Acceptance, and Divergences are the
contract. Numbered Steps guide execution and may be stale. Record each material
deviation in its slice, resolve routine choices from accepted code and documents,
and stop at unresolved consequential decisions. Tick completed acceptance boxes
as evidence becomes available. G4 slices follow the
[G4 conventions](tasks/task30-g4/00-conventions.md) instead, which replace the
same-commit contract rule with a per-slice changes ledger.

The earlier [00 — Slice conventions](tasks/task30/00-conventions.md) carries
that program's verification commands and pass conditions, the recurring
composition pattern that A1 and A2 each stalled on, and the rules every slice
shares. The plans do not repeat it.

| Order | Slice                                                                                          | Changes behaviour |
| ----- | ---------------------------------------------------------------------------------------------- | ----------------- |
| 1     | [A1 ownership store and Framework dual-write](tasks/task30/01-a1-ownership-store.md)           | no                |
| 2     | [A2 Extension dual-write](tasks/task30/02-a2-extension-writers.md)                             | no                |
| 3     | [A3 Library dual-write](tasks/task30/03-a3-library-writers.md)                                 | no                |
| 4     | [A4 Extension readers flip](tasks/task30/04-a4-extension-readers.md)                           | **yes**           |
| 5     | [A5 Status, Doctor, Library readers flip](tasks/task30/05-a5-status-doctor-library-readers.md) | **yes**           |
| 6     | [A6 delete the old records](tasks/task30/06-a6-delete-lifecycle.md)                            | **yes**           |
| 6p    | [P1 shared allow-list closeout](tasks/task30/06p-p1-shared-permissions.md)                     | **yes**           |
| 7     | [B1 heading-based Entries, guards deleted](tasks/task30/07-b1-heading-entries.md)              | **yes**           |
| 8     | [M1 enumerated extractions](tasks/task31/08-m1-enumerated-extractions.md)                      | no                |

Slices 1 to 3 dual-write: every mutating command writes both the lock and the old
record, so no reader ever sees state a writer did not produce and every commit
stays green. That is what makes slices 4 and 5 safe to flip independently.

Slices 4, 5, 6 and 7 change output. Each requires its reviewed snapshot captured
**before** the change and its contract updated in the same commit as the
behaviour. A snapshot captured after a change proves nothing.

Slice 8 is independent of the rendering work. Slices 1 to 3 and 8 need no
maintainer decision; 4 to 7 execute decisions already recorded in
[phase-4a-g1](tasks/task30/phase-4a-g1.md).

Deferred out of this sequence, each needing a Framework decision first:
the Axioms absent/empty contract, the TypeScript tooling grammar, and the
loader/AGENTS content placement. They are recorded in
[the structural subtask](tasks/task30/phase-4a-structural.md) as items 2, 4
and 5.

### Previous Dependency Order To The Rerendering Revamp

The rerendering revamp is Task 30 phase 4b, G4. “Start G4” means open its
contract and evidence gate; renderer implementation starts only after that gate
is accepted.

1. **Complete:** [Task 30 G1](tasks/task30/phase-4a-g1.md) step 4: migrate the state
   readers/writers and remove the old lifecycle/library consumers in the
   accepted order. This established which state and
   fields the later presentation contract can legitimately expose.
2. **Complete:** [P1 shared permissions](tasks/task30/06p-p1-shared-permissions.md)
   closed recorded steps 3a/3b: all six gated commands expose `--allow-path` and
   consult the shared authored allow list. Per-owner permission reads/writes
   are retired. Before output was committed separately; contracts accompanied
   behavior.
3. **Complete:** the two independent pre-G4 gates closed in the accepted order:
   - **Complete:** [Task 31's safe M1 work](tasks/task31/phase-3-5.md) extracted
     three identical mappings and retired its conditional Library group. Keep Route Move/Remove at
     its recorded contract stop, and do not pull lifecycle or permission code
     out of G1. M4 is already complete; M5 structure cleanup waits until G4
     has reduced the presentation seams.
   - **Complete:** [B1](tasks/task30/07-b1-heading-entries.md) closed structural
     items 1 and 3: heading-owned Entries and shared Markdig section lookup.
     Axioms meaning, TypeScript grammar and loader-content placement remain
     explicitly deferred Framework decisions.
4. **Complete:** the [Task 30 G4](tasks/task30/phase-4b-g4.md) decision gate
   closed on 2026-09-14. The maintainer accepted the detail levels, the
   severity filter, the format option, the single report model and envelope,
   the escaping rule, the status renames, the finding-model aggregation, the
   interaction design and the record-only implementer rule. They are recorded
   as C1–C18 in the [G4 packet](tasks/task30-g4/_task30-g4.md).
5. **Complete:** the G4 packet executed in its recorded order: 01 before snapshots, 02
   naming, 03 rendering system (which carries
   [Task 31 M3](tasks/task31/phase-escaper.md) as its escaper step), 04
   interaction system with 05 index region in parallel, then the six command
   lanes in parallel, then 40 verification and 41 documentation propagation.
   M3 is not a separate post-revamp cleanup; its escaping contract guides the
   renderer changes and both are reviewed through the same output diffs. This
   is the actual rerendering revamp. [40](tasks/task30-g4/40-verification.md)
   records all six executable modes green; [41](tasks/task30-g4/41-documentation-propagation.md)
   records the documentation closeout.
6. After G4, continue the diagnosis/interoperability, interaction/content, and
   scenario/test-architecture phases. Keep the candidate Framework and
   contract-editorial records out of execution until separately promoted.

This is historical dependency context only. Do not dispatch these completed
slices or old coordination records as current work. Recover additional archived
detail only when a concrete question needs historical evidence.

### Evidence Provenance

The two Analysis routes remain Emerging contextual evidence while Task 30 and
Task 31 are active. The active flow is:

```text
Emerging Analysis -> task-owned phase subtask -> implementation -> Task update
```

Each subtask records the measured facts it needs, its source links, its current
decision state, and its acceptance boundary. Code discoveries update the Task or
subtask; unresolved reasoning stays in the Emerging Analysis until an explicit
seal/archive decision is made.

### Earlier Scope Limits

Return to the maintainer when work would change accepted public behavior,
architecture, dependency/runtime assumptions, lifecycle meaning, safety
guarantees, or release scope. A recommendation in Analysis is not acceptance.

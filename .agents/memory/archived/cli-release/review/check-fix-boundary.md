---
open-forge:
  description: Settled historical synthesis for the accepted read-only Doctor and exact Repair boundary
  responsibility: Preserve contextual review evidence and dissent after the Doctor and Repair outcome was accepted
  tags: [Memory, Archived, CLI, Release, Review, Historical, Doctor, Repair, Safety, Mutation, Contextual]
---

# Diagnosis And Repair Boundary Review Synthesis

## Status

This file is settled historical and contextual review evidence. It is not an
authority for the current CLI. The maintainer-approved outcome is recorded in
the current [`doctor` contracts](../../../crystallized/documents/cli/contracts/doctor/_doctor.md) and
[`repair` contracts](../../../crystallized/documents/cli/contracts/repair/_repair.md). The focused council
resolution and the useful dissent below explain the boundary that led to that
outcome; they do not add examples, syntax, or implementation authority.

- **Origin:** `.agents/memory/working/cli-release/review/check-fix-boundary.md`.
- **Archived because:** Queue 18 settled and its accepted meaning moved into the
  current Doctor and Repair Interface and Behavior contracts.
- **Current authority:** the linked current Doctor and Repair contract sets.

The accepted public name is `repair`. Doctor is always read-only and runs six
fixed diagnostic domains. Repair uses one atomic plan, a wizard for the simplest
interactive invocation, explicit `--automatic` and `--relink` selection, the
sole `--dry-run` preview spelling, a strict complete-diagnosis write gate, and a
first-release catalogue limited to exact same-target local-reference repairs and
user-selected missing-target relinks. Future lifecycle syntax remains open and
is not assigned here.

## User Jobs

- Diagnose the complete known Open Forge structural surface without changing it.
- Understand which findings have one exact mechanical correction and which need
  a human decision.
- Preview or apply current exact repairs through one Repair invocation.
- Select one exact local-reference relink with three explicit values.
- Use a targeted command when the intended change is already known.

## Historical Review Evidence And Resolved Shape

Keep these boundaries:

### Human Views

The global expanded view is the default. Expanded `doctor` output includes each
finding's typed subject, evidence, resolution lane, provenance, and typed next
action. Compact view keeps one token-friendly finding row with its required next
action. Doctor has no prompt and does not apply any next action.

Expanded repair output explains selected and unselected findings, planned
effects, conflicts, verification, recovery, and fresh post-repair diagnosis.
Compact repair output keeps status, affected paths, effect counts, blocked or
manual findings, and required next actions. Structured output retains the
complete typed facts regardless of human view.

### `doctor`

- Always read-only.
- Runs every accepted diagnostic domain or reports that domain as incomplete or
  blocked.
- Returns typed findings with severity, subject, evidence, resolution lane, and
  useful next actions.
- May include an exact internal repair proposal only when the same current facts
  prove one meaning-preserving correction.
- Never accepts repair flags and never mutates through confirmation or
  interactivity.
- Reports the six domains in fixed order and retains incomplete or blocked
  dependent domains.

### Targeted Mutations

Use `index` and accepted route operations when the caller already knows the
intended generated-navigation or route result. A Doctor finding may report the
typed action, but it does not execute it. Future cleanup and lifecycle actions
remain typed future or manual actions; this record does not invent their names.

### General Repair Command

The accepted interface is the canonical `repair` operation. Its exact public
form is:

```text
open-forge repair [--automatic] [--relink <source-location> <expected-destination> <target-path>]... [--dry-run] [--skip-git-check] [global flags]
```

The human simplest invocation opens the wizard. `--automatic` suppresses it and
selects every current safe-exact proposal, never a guided candidate.
`--relink` supplies one exact source location, expected destination, and
contained target per occurrence. Interactive use presents a plan and final
confirmation with `No` as the default; JSON and other non-interactive use never
prompt. Repeated Boolean flags are idempotent, duplicate relink tuples
deduplicate, and contradictory tuples are invalid.

The operation builds one complete conflict-free plan, preflights it, previews or
applies it, verifies it, recovers when needed, and runs fresh diagnosis. No saved
report, finding text, or previous dry-run output is executable authority.

The first catalogue contains same-target canonical path, case, encoding, and
unique-fragment corrections, plus user-selected missing-target relinks from
bounded filename, title, literal-content, and route-neighborhood candidates.
Do not split preview and application into separate `plan` and `apply` child
operations; `--dry-run` expresses the preview dimension of this one write
operation.

## Finding And Proposal Boundary

A public finding should report:

- Diagnostic domain and stable finding kind.
- Severity.
- Typed subject and evidence.
- Resolution lane: safe-exact, guided-choice, targeted-operation,
  manual-decision, blocked-repair, or informational.
- Zero or more typed next actions.

An exact repair proposal remains attached to the current typed finding and
contains the expected state, resulting state, affected paths, conflict evidence,
verification condition, and recovery requirement. A code or message may identify
the finding for filtering but never dispatches repair behavior.

## Selection And Batching

The interactive wizard initially offers all current safe-exact proposals and
leaves guided candidates unselected. `--automatic` suppresses the wizard and
selects every current safe-exact proposal. Explicit `--relink` tuples select
only their addressed occurrences. Automatic and explicit selection compose as a
union; there is no generic selector, repair-kind mode, or arbitrary batch input.

Every invocation:

1. Rebuilds current diagnostic facts.
2. Resolves automatic proposals, wizard choices, or explicit relink tuples
   against current eligible facts.
3. Rejects stale, manual, ambiguous, destructive, or authority-bearing choices.
4. Detects overlapping targets, incompatible expected states, and effect-order
   conflicts.
5. Requires complete coverage for all six Doctor domains and blocks the complete
   selected batch before writes when any selected repair conflicts or lacks a safe
   recovery boundary.

In an automatic plan, equivalent proposals may combine only when they produce
the same exact result and retain every originating finding. Order never chooses
a winner.

## Results

Reuse the accepted semantic results. A `complete` repair is complete only for the
selected repair scope; unrelated doctor findings may remain. `attention` reports
remaining manual or non-blocking findings. `incomplete` means requested repair
coverage could not finish safely. Invalid relink or selection input is `invalid`; stale state,
conflicts, missing authority, dirty affected paths, or missing recovery are
`blocked`; attempted write, verification, or recovery failures are `failed`;
caller cancellation is `interrupted` when no residual failure remains.

A verified unchanged target is a complete no-op. Changed but unsatisfied
preconditions are blocked, not no-ops.

## Non-Goals

- No implicit mutation from bare `doctor`.
- No second repair/fix synonym, saved plan, generic apply, diagnostic-string
  dispatch, or plugin fixer registry.
- No fuzzy or semantic target choice.
- No silent partial batch, conflict ordering, or best-effort writes.
- No repair of authored meaning, ownership, destructive intent, or external
  side effects that lack complete verification and recovery.

## Questions Closed By Acceptance

The maintainer-approved contract closed the questions preserved in this review:

1. The canonical name is `repair`, not `fix`.
2. Explicit selection uses the three-value `--relink` tuple with a fresh source
   occurrence, expected destination, and contained target path. Automatic selection
   selects the complete current safe-exact set; there is no opaque selector.
3. The first automatic catalogue is same-target path, case, encoding, and
   unique-fragment correction. Missing-target relinks remain guided user intent.
4. Generated navigation, route intent, recovery cleanup, Framework lifecycle,
   and Extension lifecycle remain with their targeted or future contracts.

The exact JSON schema, numeric exits, parser and filesystem choices, backup
naming, concurrency, and lifecycle syntax remain deliberately deferred.

## Evidence Plan

- Exercise representative domains through one typed proposal boundary and the
  complete six-domain gate. This validates the accepted journey without making
  this contextual record a second contract.
- Exercise several domains together for conflict reporting across automatic and
  explicit relink selection.
- Test stale occurrences and explicit relinks, conflicts, equivalent proposals,
  dirty paths, dry-run, confirmation, no-op, concurrent changes, verification
  failure, recovery, interruption, and fresh rediagnosis.
- Reject the command if freeform finding text, a previous report, display order,
  severity, or suggestion text can select an effect. Explicit relinks and
  automatic selection must be resolved again against current typed proposals and
  current source bytes.

## Related Evidence

- [Council Synthesis](check-fix-council.md)
- [Current Doctor Contract Set](../../../crystallized/documents/cli/contracts/doctor/_doctor.md)
- [Current Repair Contract Set](../../../crystallized/documents/cli/contracts/repair/_repair.md)
- [Doctor Interface Contract](../../../crystallized/documents/cli/contracts/doctor/interface.md)
- [Repair Interface Contract](../../../crystallized/documents/cli/contracts/repair/interface.md)
- [Shared CLI Operation Contract](../../../crystallized/documents/cli/shared-operation-contract.md)
- [Decision Agenda](../../../working/cli-release/decision-agenda.md)
- [Historical Diagnosis And Repair Contract](../../cli-v2/documents/contracts/diagnosis-and-repair.md)

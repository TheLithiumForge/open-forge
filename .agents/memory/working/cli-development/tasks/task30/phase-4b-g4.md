---
open-forge:
  description: Task 30 G4 presentation subtask with its evidence, the reviewed design proposals, and the decisions the maintainer accepted for the execution packet
  tags: [Memory, Working, CLI, Task, Subtask, Contextual]
---

# Task 30 — Phase 4b / G4 Presentation Model

## Status

Design gate closed on 2026-09-14, and implementation plus documentation
propagation completed on 2026-09-16. The maintainer's accepted decisions are
recorded under [Accepted Decisions](#accepted-decisions). All foundation and
command subtasks in the [G4 packet](../task30-g4/_task30-g4.md), verification,
and documentation propagation are merged and green. Task 31 M3 is complete as
part of the rendering-system work; no renderer work remains pending.

## Evidence source

- [Command output design](../../../../emerging/analysis/cli-experience-audit/command-output-design.md)
- [Presentation field audit](../../../../emerging/analysis/cli-experience-audit/presentation-field-audit.md)
- [Finding model](../../../../emerging/analysis/cli-experience-audit/finding-model.md)
- [Model-level snapshot testing](../../../../emerging/analysis/cli-experience-audit/model-level-snapshot-testing.md)
- [View layer and test architecture](../../../../emerging/analysis/cli-experience-audit/view-layer-and-test-architecture.md)

The Emerging evidence measured repeated renderers, missing detail, unclear
severity, and the need for a selection stage. It also requires snapshots before
behavior-changing renderer work.

## Actionable packet

- Write the presentation contract before delegating volume. Define the accepted
  report projections, shared status/stream rules, four detail levels, and
  diagnostics before changing output.
- Establish the permanent snapshot boundary before renderer rewrites. Capture
  the existing output, review intentional changes, and keep behavior-preserving
  refactors diffable.
- Keep selection separate from rendering: selection decides what a reader sees;
  a renderer only writes the selected report projection.
- Include Task 31 M3 in the same renderer-revamp packet: accept the single
  escaping decision in the output contract, capture the pre-change snapshots,
  and implement the escaper and renderer rewrites under the same reviewed
  output diffs. Do not schedule M3 as a post-revamp cleanup or change JSON
  opportunistically.
- Keep host line-ending normalization in the shared human-presentation text
  utility, with coverage for LF, CRLF, CR, and mixed input. Keep this separate
  from JSON value encoding and from authored or persisted byte contracts.
- Turn the per-command proposals into a review checklist covering status,
  doctor, install/update/index/repair/cleanup, context/find/references, Route,
  extension, and library. For each command record its user question, default
  tier, selected fields, healthy/zero/not-applicable suppression, ordering,
  subject, `Next`, context echo, size budget, JSON projection, and exit/stream
  meaning before changing a renderer.
- Treat the finding-model and index/doctor split as explicit decision gates.
  The historical direction to retain diagnostic kinds, JSON, and exit behavior
  must be rechecked before any grouping or deletion; presentation grouping is
  not permission to remove a public finding category.
- Use the AOT-safe snapshot boundary in [Phase 7 scenarios](phase-7-scenarios.md):
  normalize only named environment-dependent paths, versions/fingerprints, and
  line endings, never counts, ordering, encoding, or diagnostic content.

## Decision gates

The G4 implementation and documentation propagation are complete as of
2026-09-16.

These gates are closed. The maintainer accepted on 2026-09-14: no TSV promise
(C10), no self-referential wording in output (shared rules), the presentation
folder restructure scheduled now as a physical layout and dependency rule
without separate projects (C18), and the finding-model aggregation for
coverage kinds (C5). The earlier sentence below described the pre-merge gate.
The implementation and documentation propagation are now complete, so the
packet's decisions describe current behavior.

## Preparation Reconciliation

The maintainer approved correcting current contracts and retiring the unused
legacy-root receipt notice. [SG-R1/SG-R2](review-second-gate-pre-g4.md#accepted-disposition-and-follow-ups)
record the completed correction, source evidence and every deviation. This is
preparation for the design gate; no new output format or renderer is accepted.

Keep the Emerging proposals as historical evidence. Do not polish their old
transcripts before designing replacements. Carry only these corrections into
new examples:

- `command-output-design.md` proposes `repair --rebind` and an example based on a
  recorded baseline. The current lock has no workspace binding or baseline
  fingerprints. Neither premise belongs in new transcripts.
- Its proposed JSON deletion list predates G1. `baselineFingerprint` and
  `fingerprintKind` are already removed; region ownership remains valid. Do not
  treat that historical list as authority to delete current ownership fields.
- The repeated unsupported `SKILL.md` key example was resolved before B1. Choose
  a real current metadata failure for new examples rather than recreating it.
- `finding-model.md` lists `extension.lifecycle-document-missing`. The current
  missing ownership lock is informational and does not gate commands. Define
  the new presentation from the present result model and command contracts.
- Generated Entries use heading boundaries. B1 already removes retired guards
  during the next successful Index rewrite of an affected section. The separate
  AGENTS/CLAUDE managed-host boundary remains a structural follow-up.

Capture representative fresh output when drafting the new command transcripts.
Capture and commit the reviewed before snapshot before any later behavior change.
No extra characterization run or rewrite of the old proposals is required just
to reconfirm these already-qualified removals.

## Design Proposals Under Review

Three independent output proposals and one consolidation were written for this
gate on 2026-09-13 and 2026-09-14. They are sealed design evidence. The
maintainer's review of the consolidation produced the decisions below; the
proposals themselves are not rewritten and are not execution authority.

- [astra](phase-4b-g4-output-proposal-astra.md): contract-and-source
  specification with a status coverage matrix and a 28-row selection matrix.
- [opus](phase-4b-g4-output-proposal-opus.md): three content classes and the
  finding-model question as the first decision.
- [fable](phase-4b-g4-output-proposal-fable.md): transcripts captured from a
  build of the current tree, with two implementation discoveries recorded in
  the [Task 30 open findings](../task30-cli-experience-remediation.md#open-findings).
- [consolidated](phase-4b-g4-output-proposal-consolidated.md): the merged
  rules, per-command matrix, corrected transcripts, sixteen decisions and the
  implementation handoff. It follows astra most closely and says why.

## Accepted Decisions

Recorded on 2026-09-14 from the maintainer's review of the consolidated
proposal. The full table with each decision's wording is in the
[G4 packet](../task30-g4/_task30-g4.md#accepted-decisions); the packet is the
current source for them.

- **Current implementation (C1–C11, C13–C18):** `--format text|json`,
  `--detail minimal|standard|full|debug`, and repeatable
  `--detail-filter error|warning|info|all`; `minimal` is the default and
  `debug` adds diagnostics. The single report uses one schema-3 envelope for
  JSON, with statuses `completed`, `completed-with-warnings`, `incomplete`,
  `invalid-input`, `blocked`, `failed`, and `cancelled`. Error, Warning, and
  Info are the severity words; finding codes appear in text only at `full` and
  `debug`; there are no listing caps; and the accepted interaction, index,
  escaping, reference-counting, install, no-TSV, record-only implementer, and
  three-layer physical-layout decisions are implemented.

<!-- The following gate wording is retained as historical evidence. -->
- **Accepted (C1–C11, C13–C18):** `--detail minimal|standard|full|debug`
  with `minimal` as default and `debug` as the top level; a repeatable
  `--detail-filter error|warning|info|all`; `--format text|json`; `debug` adds
  diagnostics; `Error`, `Warning`, `Info` as the
  severity words; coverage-kind Doctor findings aggregated in the result model;
  no listing caps; one report model and one schema-3 envelope with the detail
  level applied to JSON too; terminal-first escaping; the accepted status names;
  no TSV promise; a
  human-first `install`; finding codes in text only at `full` and `debug`; the
  complete interaction design; `references` counting authored links only; the
  `index` Entries-region fix; the record-only implementer rule with a later
  documentation pass; the three-layer physical split without separate projects.
- **Assumptions left to the packet author (C12, C6 filtering, level wording):**
  workspace echo at `standard` and above, the `--detail-filter` semantics, and
  the level names are recorded as assumptions in the packet's
  [conventions](../task30-g4/00-conventions.md#shared-presentation-rules). Correct
  them there if they are wrong.
- **Unresolved:** the packet is merged, but maintainer questions remain open:
  whether `Next:` must always be the final line when Extension Install emits a
  required advisory after it; whether the status/update report should echo
  Workspace explicitly in one-line examples; and whether nullable counts,
  diagnostic-vocabulary scope, and command-local wording questions need another
  shared-contract pass. They are recorded without a decision in
  [41](../task30-g4/41-documentation-propagation.md).
- **Not frozen:** no transcript in the packet is frozen. Each subtask's
  snapshots are reviewed against its catalogue when the subtask is merged.

## G4 Closeout

The six executable test suites run with `--parallel collections`; the former
serial default is gone. The supported Windows Native AOT gate requires
`vswhere` on `PATH` and cannot run in a sandboxed worker, so the overseer must
run it on an unsandboxed host. `reference.cycle` is dead; its remaining literal
belongs only to unrelated Workspace Settings unknown-key tests in [11](../task30-g4/11-doctor.md).

Every confirmation now renders the established minimal plan review before the
prompt. Explicit `--allow-path` grants and interactive Allow-always decisions
are staged and published only during confirmed application. The Library
confirmation-required codes are `library-attach.confirmation-required`,
`library-sync.confirmation-required`, and `library-detach.confirmation-required`.

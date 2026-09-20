---
open-forge:
  description: Planned Task 30 in-process scenario and model-snapshot boundary after the view contract is accepted
  tags: [Memory, Working, CLI, Task, Subtask, Contextual, Testing, Scenarios, Snapshot]
---

# Task 30 — Phase 7 Scenarios And Model Snapshots

## Status

**Specified, not started.** This phase supplies behavior evidence after G4; it
does not authorize output changes, test deletion, project splitting, or a
runner-default change.

## Current state, measured

Measured on 2026-09-17. **Most of the machinery this phase asked for already
exists — what is missing is the evidence about it.** Re-measure before starting
any slice.

- **The in-process boundary exists and is the norm.**
  `Shell/Composition/CliCoreApplication.RunAsync` is the argv-in, exit-out
  entry point, and **108 integration test files** already use it.
- **Process smoke is substantial**: 55 end-to-end test classes.
- **Model snapshots exist and are AOT-safe ordinary text**, now in the snapshot
  library's default `__snapshots__` location beside each owning test class,
  derived from the caller file path with an explicit update switch — which is
  exactly the shape this phase specified.
- **No coverage matrix exists.** The phrase appears only in the G4 proposals and
  in this file. Nothing maps a journey to its cheapest proving boundary.
- **Scenario kinds are unevenly covered.** Integration files mentioning each
  seeded shape: `malformed` 66, `external` 64, `nested` 20, and **`dirty` 0,
  `relocated` 0**. Two of the seven named shapes have no coverage at all.

So the work is the matrix and the two missing shapes, not building a harness.

## Execution slices

| Order | Slice | Depends on | Changes output |
| ----- | ----- | ---------- | -------------- |
| 1 | **70 — Coverage matrix.** Map every journey and command status to its cheapest proving boundary, and name each assertion that is intentionally process-only. Build it from the tests that exist, not from the analyses. | none | no |
| 2 | **71 — The two uncovered shapes.** Add seeded `dirty` and `relocated` workspace journeys at the in-process boundary, asserting the contracts this phase names: encoding, finding subject and `Next`, exit status, write-freedom, and recovery consequence. | 70 | no |
| 2 | **72 — Normalization audit.** Confirm snapshots normalize only environment-dependent representation. Counts, ordering, encoding defects and promised diagnostic content must not be normalized away. The capture redaction that rewrites machine-specific roots is the thing to audit: it is necessary, and it is exactly the mechanism that could hide a real difference. | none | no |

70 gates 71 because the matrix decides which boundary each new journey belongs
at. 72 is independent. None of the three changes command output; a slice that
finds itself editing a rendered string has left its scope.

## Evidence source

- [Model-level snapshot testing](../../../../emerging/analysis/cli-experience-audit/model-level-snapshot-testing.md)
- [Test layer consolidation](../../../../emerging/analysis/cli-experience-audit/test-layer-consolidation.md)
- [Test strategy and scenarios](../../../../emerging/analysis/cli-experience-audit/test-strategy-and-scenarios.md)
- [View layer and test architecture](../../../../emerging/analysis/cli-experience-audit/view-layer-and-test-architecture.md)
- [Presentation field audit](../../../../emerging/analysis/cli-experience-audit/presentation-field-audit.md)

The test-architecture subtask owns usefulness review, project boundaries, and
per-test temp isolation. This phase owns the missing behavior/scenario packet:
what a user journey proves and where complete rendered models are snapshotted.

## Actionable boundary

- Use the in-process `RunAsync` boundary for argv, exit, streams, cancellation,
  environment, and multi-command journeys that do not need a child process.
- Keep full rendered-command snapshots at the in-process integration boundary,
  after G4 accepts the presentation contract. Use AOT-safe ordinary text files
  with a caller-file-path-derived location and an explicit update switch.
- Normalize only environment-dependent representation in snapshots: physical
  paths, version/fingerprint values where the contract marks them unstable,
  line endings, and other named platform fields. Do not normalize counts,
  ordering, encoding defects, or diagnostic content that the contract promises.
- Add small transcript/scenario journeys for seeded clean, dirty, nested,
  relocated, missing, malformed, and externally sourced workspaces. Assert
  encoding, output-size budget, finding subject/Next behavior, exit status,
  write-freedom, and recovery consequences where each is a distinct contract.
- Keep process smoke for AOT, executable discovery, real process streams,
  signal/cancellation, relocation, and other boundaries unavailable in-process.
  Use the [test architecture](phase-7-8-test-architecture.md) packet for
  fixture ownership, usefulness dispositions, and parallel qualification.

## Acceptance

- A coverage matrix maps each journey and command status to its cheapest
  proving boundary and names any intentionally process-only assertion.
- Snapshot files are deterministic, reviewable, AOT-safe, and cannot hide
  counts, encoding, ordering, or diagnosis regressions through over-normalizing.
- Every surviving scenario owns an isolated mutable fixture or a documented
  narrow synchronization contract; no scenario depends on the repository
  checkout or the user's profile.
- The process-smoke suite remains nonzero and meaningful after any scenario
  migration, and no runner default changes without the separate architecture
  qualification receipts.

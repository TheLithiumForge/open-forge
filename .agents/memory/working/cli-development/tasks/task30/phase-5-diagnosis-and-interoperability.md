---
open-forge:
  description: Planned Task 30 diagnosis and interoperability phase for truthful findings, extension inputs, and shared path grammar
  tags: [Memory, Working, CLI, Task, Subtask, Contextual, Diagnosis, Interoperability]
---

# Task 30 — Phase 5 Diagnosis And Interoperability

## Status

**Specified, not started.** This phase follows the accepted state-model work and
the G4 contract decision. It is a decision-and-behavior boundary, not permission
to rewrite all findings or add a registry.

## Current state, measured

Measured against the merged tree on 2026-09-16, after G4 closed. G4 delivered
part of this phase incidentally, so the slices below are scoped to what is
actually left. **Re-measure before starting any slice**; these are a starting
point, not a specification.

- **A shared path normalizer already exists.**
  `Framework/Filesystem/Shared/Paths/PortableWorkspacePath` offers
  `TryNormalize` and `CreatePortableKey`, and is used by Doctor, Extension,
  Install, Library, Route and Update.
- **Repair is the outlier.** It normalizes through its own command-local
  `Shared/Request/RepairRelinkNormalizer` and does not use the shared type. That
  is the concrete remainder of the "one normalizer" boundary, not a from-scratch
  build.
- **Subjects are already partly enforced.**
  `CliReportInvariantsTests.AssertSubject` asserts that every listed subject
  carries a path or an identifier, across the whole capture corpus. The
  untested half is the rest of the quality bar: consequence, copy-pasteable next
  action, and repair truthfulness.
- **There is no Git cleanliness gate to reconfirm.** No gate exists anywhere in
  `src/cli/core` or `src/cli/root`. The `.git` references that do exist are
  destination policies protecting the directory from writes, which is unrelated.
  This boundary item is therefore a record, not work.
- **One code, one situation** is now an enforced invariant and a recorded rule in
  the G4 conventions. Any finding split proposed here inherits it.

## Execution slices

| Order | Slice | Depends on | Changes output |
| ----- | ----- | ---------- | -------------- |
| 1 | [50 — Command ownership of diagnosis](50-diagnosis-ownership.md) | none | no |
| 2 | [51 — Truthful blocking findings](51-truthful-findings.md) | 50 | yes |
| 2 | [52 — Interoperability inputs](52-interoperability-inputs.md) | none | yes |
| 2 | [53 — One logical path normalizer](53-one-path-normalizer.md) | none | possibly |
| 3 | [54 — Snapshot consolidation and pruning](54-snapshot-consolidation.md) | 51, 52, 53 | no |

**Parallelism.** 50, 52 and 53 all start together — they share no source folder,
and only 51 depends on the ownership table. 51 follows 50, because that table
decides which findings must meet its bar in which command. 54 runs last and
alone: it moves the very capture files the others regenerate, so it must not
overlap them.

Each slice owns its own file above. Those files carry the measured current
state, the actionable boundary and the acceptance; this table only orders them.

## Acceptance

- A command-ownership table covers every currently blocking diagnosis and the
  index/doctor/repair handoff is tested from the same seeded workspaces.
- Standard SKILL and extension inputs produce bounded, actionable findings;
  malformed manifests do not escape as raw exceptions and missing content is
  not a silent success.
- Path forms normalize identically across install, update, remove, inspect,
  doctor, and repair, with platform-specific physical paths kept out of JSON
  logical values.
- Any finding taxonomy, severity, output, or exit change has a written G4
  contract decision and reviewed snapshot/evidence diff.

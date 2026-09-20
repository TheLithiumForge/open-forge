---
open-forge:
  description: Task 30 phase 5-D slice 53 bringing Repair onto the shared portable path normalizer and proving identical logical path handling across every command
  tags: [Memory, Working, CLI, Task, Subtask, Paths, Contextual, Active]
---

# 53 — One logical path normalizer

## Outcome

One shared normalizer decides what `./`, slash direction, a trailing slash and a
hybrid reference mean, and every command agrees. Logical paths stay relative and
slash-shaped; physical paths are derived with the platform API and stay out of
JSON logical values.

## Depends on

Nothing. Runs in parallel with [50](50-diagnosis-ownership.md) and
[52](52-interoperability-inputs.md).

## Current state, measured

Measured on 2026-09-16. **This is mostly done already — re-measure before
assuming the gap is still where it was.**

- The shared normalizer exists:
  `Framework/Filesystem/Shared/Paths/PortableWorkspacePath`, offering
  `TryNormalize` and `CreatePortableKey`.
- It is already used by **Doctor, Extension, Install, Library, Route and
  Update**.
- **Repair is the outlier.** It normalizes through its own command-local
  `Commands/Repair/Shared/Request/RepairRelinkNormalizer` and does not use the
  shared type.

So the work is adoption and proof, not construction.

## Actionable boundary

- Bring Repair onto `PortableWorkspacePath`. Retire `RepairRelinkNormalizer`, or
  narrow it to whatever is genuinely relink-specific — deduplication and
  contradiction detection across several `--relink` values may legitimately
  remain command-local. Say which parts you kept and why.
- **This should be behaviour-preserving.** If adopting the shared normalizer
  changes a rendered path, a finding, or an exit code, **stop and report it**
  with the exact before and after. That is a divergence to rule on, not a
  capture to regenerate.
- Prove the agreement rather than asserting it: one test that feeds the same set
  of awkward inputs — `./a/b`, `a\b`, `a/b/`, `.agents/x` versus `x`, and a
  hybrid — through every command's path handling and asserts one answer.
- Keep platform-specific physical paths out of JSON logical values. If a
  physical path is needed as evidence, it belongs in a labelled evidence field
  at `full` or `debug`, matching how raw causes are handled.

## Acceptance

- Repair uses the shared normalizer; anything left in `RepairRelinkNormalizer`
  is justified in the ledger.
- One test proves identical normalization across install, update, remove,
  inspect, doctor and repair.
- No JSON logical value carries a platform-specific physical path.
- All four gates green. Ideally no capture changes at all; any that does is
  explained per situation.

## Changes ledger

- Repair canonical source and target paths: direct Repair-only path validation -> existing role and delimiter validation composed with the shared `PortableWorkspacePath` decision; accepted logical values retain their exact slash-shaped text.
- Repair relink collection: local `RepairRelinkNormalizer` remains exact tuple deduplication plus per-source contradiction detection; no portable-key deduplication or case folding was introduced because those are relink-set semantics and selected-target case remains meaningful.
- Cross-command path evidence: no agreement test -> one unit test feeds `./a/b`, `a\b`, `a/b/`, `.agents/x`, `x`, and `a/b\c` through Install, Update, Remove, Inspect, Doctor, and Repair boundaries and compares each result with `PortableWorkspacePath`.
- JSON logical paths: unchanged; the change adds no physical path projection or new finding/result field.

## Divergences observed

- The recorded current-state wording describes `RepairRelinkNormalizer` as a path normalizer, but measurement found it already narrowed to exact relink tuple deduplication and contradiction detection. It was retained unchanged and justified above rather than renamed or given portable-key semantics.
- The required whitespace gate still reports the five pre-existing diagnostics outside this slice; no changed file was reported. The first build attempt also hit the worktree NuGet app-data access issue and succeeded with the documented offline-audit and shared-compilation switches.

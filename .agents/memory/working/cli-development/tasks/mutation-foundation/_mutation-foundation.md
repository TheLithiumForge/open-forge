---
open-forge:
  description: Build and accept locking, lifecycle, planning, application, and recovery foundations before mutations
  tags: [Memory, Working, CLI, Task, Mutation, Lifecycle, Recovery, Contextual]
---

# Mutation Foundation

## Task State

- State: Complete at exact production candidate `e7d937f` under current authority
  `01dd552`. The exact historical child commits remain recorded in their Tasks.
  Final managed, portable `linux-x64` Native AOT, static-absence, format, diff,
  and independent-review gates pass. The later canonical lifecycle correction is
  integrated at `1d404c5cef3f5fd464ca771fc132a657f792f533`.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisites: Accepted read-only facts, Extension Inspect, and the pure
  Generated Navigation foundation. Public `index` is a consumer, not a
  prerequisite.
- Last updated: 2026-08-29.

## Expected Outcome

Commands can form local immutable plans, acquire the real workspace lock,
revalidate expected state, prepare and verify one external recovery bundle before
the first existing-target effect, apply bounded atomic changes, verify resulting
identity and bytes, and write schema-versioned lifecycle state without a universal
mutation engine. The replacement CLI does not inspect, report, or mutate Git.

## Invariants

- Planning performs no mutation.
- Lock acquisition precedes final revalidation and every workspace effect.
- `extension create` remains the only accepted no-workspace mutation exception.
- Shared primitives expose preconditions, changes, receipts, locking, atomic
  replacement, lifecycle storage, and recovery-bundle facts. Commands retain
  policy and ordering.
- One immutable, verified external bundle covers every planned existing-target
  effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`) and is ready before
  the first target effect. Create-only and semantic/byte no-op operations create
  none. Handled failure or
  cancellation reports the actual residual draft or final path; no restore,
  rollback, compensation, or recovery-derived current-target classification
  occurs. A closed final may remain after abrupt termination without a crash or
  power-loss guarantee.
- Lock ownership is a persistent reusable file held with `FileShare.None`; the
  CLI writes no lock metadata and never deletes or truncates the file.

## Child Tasks

- [x] [Freeze workspace-lock, lifecycle-envelope, mutation-precondition, receipt, and recovery contracts](01-contracts.md) — Complete; historical contract implementation `7cb93e9`, current corrected production candidate `e7d937f` — Implementer: Overseer, sequential
- [x] [Implement real lock ownership, lifecycle reads and writes, and expected-state revalidation](02-lock-lifecycle.md) — Complete; historical implementation `5b926e7`, persistent-byte/no-metadata correction in `e7d937f` — Implementer: Overseer-managed Task Mastermind, sequential
- [x] [Implement one atomic per-file application primitive with exact verified receipts](03-apply.md) — Complete; historical implementation `cdfde34`, matching-preparation integration in `e7d937f` — Implementer: Overseer-managed Task Mastermind, sequential
- [x] [Implement recovery bundles and interruption boundaries](04-recovery.md) — Complete at exact production candidate `e7d937f` under authority `01dd552` — Implementer: Overseer-managed Task Mastermind, sequential
- [x] [Integrate and accept the mutation foundation before any mutating command](05-acceptance.md) — Complete; final gates and independent review pass — Implementer: Overseer

Children 01 through 03 preserve exact historical commits and evidence while the
final production candidate supplies the accepted authority corrections. M1 owns
neutral catalogue facts and a held-lease mechanical deletion guard only. Public
Status and Doctor remain O1. Cleanup selection, default and empty-no-lease
policy, whole-command deletion orchestration, guidance, result aggregation, and
E2E remain O2.

The integrated lifecycle correction makes every newly created document emit all
five ordered root keys. Framework-created documents use a complete empty
Extensions section. Existing missing, null, malformed, or incomplete Framework
or Extensions state remains untrusted and blocks update planning without a repair
write. Its focused and full managed, Native AOT dogfood, diff, and independent
review gates pass.

## Final Evidence

Locked restore and the warning-free Release build pass. Focused M1 Unit is
`43/43` and Integration is `47/47`; full managed Unit, Integration, and EndToEnd
are `1096/1096`, `458/458`, and `120/120`. Focused published `linux-x64`
Recovery/source-generation Native AOT is `10/10`, and the already-published full
Native AOT Integration runner is `458/458`. Every test run has zero failures and
zero skips. Static replacement-product Git results are zero. Whitespace-format,
diff, and final independent review pass; review is `ROBUST`, safe to commit, with
confidence `0.98`.

The complete solution-format command exits successfully but reports existing
workspace reference-load warnings; the narrower whitespace oracle is clean. No
process-crash harness, permission manipulation, fake filesystem, fake failure
seam, or six-RID claim is part of M1. A closed final bundle may structurally
remain after abrupt termination, but there is no crash or power-loss guarantee.

## Entries

<!-- open-forge:generated-index:start -->
- [Freeze workspace-lock, lifecycle-envelope, mutation-precondition, receipt, and recovery contracts](01-contracts.md) - #Memory #Working #CLI #Task #Mutation #Contract #Lifecycle #Recovery #Contextual
- [Implement persistent lock ownership, lifecycle reads and writes, and expected-state revalidation](02-lock-lifecycle.md) - #Memory #Working #CLI #Task #Mutation #Lock #Lifecycle #Contextual
- [Implement one atomic per-file application primitive with exact verified receipts](03-apply.md) - #Memory #Working #CLI #Task #Mutation #Filesystem #Atomic #Contextual
- [Implement external recovery bundles and interruption boundaries for mutations](04-recovery.md) - #Memory #Working #CLI #Task #Mutation #Recovery #Contextual
- [Integrate and accept the mutation foundation before any mutating command](05-acceptance.md) - #Memory #Working #CLI #Task #Mutation #Acceptance #Integration #Contextual
<!-- open-forge:generated-index:end -->

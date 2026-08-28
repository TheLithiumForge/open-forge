---
open-forge:
  description: Integrate and accept the mutation foundation before any mutating command
  tags: [Memory, Working, CLI, Task, Mutation, Acceptance, Integration, Contextual]
---

# Accept The Mutation Foundation

## Task State

- State: Complete at exact production candidate `e7d937f`, under current parent
  authority `01dd552`.
- Responsible role: Mastermind.
- Parent: [Mutation Foundation](_mutation-foundation.md).
- Last updated: 2026-08-29.

## Acceptance Review

Inspect contracts, source, tests, and direct consumers. Confirm shared primitives
contain no route, Extension, install, update, repair, or cleanup policy. Confirm no
universal mutation plan, DI container, fake filesystem, process execution,
hidden restoration/rollback/compensation, legacy lifecycle recognition, or
unlocked workspace effect.

Confirm the current authority, rather than historical child closeouts, is
implemented: the persistent lock preserves its existing bytes and is owned only
through a `FileShare.None` handle; `FileChangeApplier` requires a matching
verified `RecoveryBundlePreparation` for every existing-target effect
(`Replace`, `ReplaceGeneratedRegion`, or `Delete`) and none for Create;
standalone Extension Create remains create-only and bundle-free. Future command
orchestration owns complete-operation preparation and before-first-effect
timing; M1 does not coordinate that sequence or success-time removal.

Before M1 acceptance, delete the complete replacement-CLI Git capability and
its evidence. This includes `Framework/Git/**` models, repository inspector, and
process runner; every matching Unit or Integration test; every static reference;
and any Git inspection, recovery-check, bypass flag, option, or binding. A static
absence audit must prove that no production or test reference to those replacement
capabilities remains. Frozen legacy and historical authorities remain untouched.

## Executable Evidence

Final acceptance ran the focused mutation-foundation Unit and Integration
evidence defined by Child 04: small contract facts; real-filesystem exact-byte
bundle preparation and application; Create-only store `NotNeeded` and positive
storage absence; rejected preparation; representative residuals; guarded
eligible-candidate deletion and positive absence; neutral catalogue facts; and
persistent lock-byte preservation plus contention. Residual evidence proves
that pre-cancellation creates no artifact and returns a null residual, while a
deterministic collision retains and reports the existing final; code audit
proves later write/move failures track the owned draft or final without a
manufactured seam. It also ran the published `linux-x64` ZIP/source-generation
recovery smoke and the already-published full Integration runner. Static
inspection proves no current replacement command discovers, reports, executes,
or mutates repository state.

`RecoveryBundleCatalogue` proves only neutral candidate path, kind, and
integrity facts: absent storage is available-and-empty, unavailable storage is
distinct, a final receives one semantic read, and a draft is path-only
`Incomplete`. It does not implement Status or Doctor result policy.
`RecoveryBundleDeletionGuard` proves only the mechanical held-lease boundary:
with an existing same-workspace lease and one caller-selected eligible candidate
(`Verified` final or ordinary exact-name `Incomplete` draft), re-enumerate once
and re-establish exact path/name/kind. Immediately before ordinary `File.Delete`,
semantically reread a final or repeat exact path/name/kind revalidation for a
draft, then verify positive absence and leave unowned items untouched. It does
not implement Cleanup selection, defaults, lease arbitration, multi-candidate
orchestration, guidance, result aggregation, EndToEnd behavior, operation
success, or deletion timing.

## Pass Condition

- Every primitive has direct evidence and typed failure states.
- Lock ownership and post-lock revalidation precede every workspace effect.
- Lifecycle and recovery-manifest source generation passes with reflection
  disabled.
- Every existing-target effect (`Replace`, `ReplaceGeneratedRegion`, or `Delete`)
  requires its matching opaque verified-final `RecoveryBundlePreparation`
  before that individual effect. Future commands own preparing one bundle for
  the whole plan and orchestrating its completion before the first effect;
  create-only and no-op operations create none.
- Given an existing held same-workspace lease and one caller-selected eligible
  candidate—a `Verified` final or ordinary exact-name `Incomplete` draft—the
  deletion guard applies the kind-specific immediate revalidation, uses ordinary
  deletion, and verifies positive absence. Its typed `Attention` path is
  structurally audited without a fake failure seam. Pre-cancellation creates no
  artifact and returns a null residual; deterministic collision retains and
  reports the existing final; later write/move residual tracking is code-audited.
  M1 claims no candidate selection, whole-operation verification ordering,
  success removal, or deletion timing.
- The neutral catalogue validates final payload lengths and hashes only by
  bounded streaming and never extracts, discloses, retains, or materializes
  payload bytes; drafts remain path-only `Incomplete` facts.
- The deletion guard accepts a held same-workspace `FileShare.None` lease,
  re-enumerates once, and re-establishes the selected eligible candidate's exact
  path/name/kind. Immediately before ordinary deletion, it semantically rereads
  a final or repeats exact path/name/kind revalidation for a draft, deletes only
  that candidate, verifies positive absence, and leaves unknown or lookalike
  items untouched.
- The complete replacement-CLI Git capability, related tests, references, and
  flags or options are absent under a recorded static zero-result audit.
- Platform-specific differences are explicit and within accepted contracts.
- Future Status/Doctor and Cleanup work can consume neutral bundle facts without
  binding or restoring moved-root bundles.

Parent, Plan, Checkpoint, the task indexes, and Children 01–05 are aligned in
this docs-only closeout. Public Index is `Ready` and next, but is not implemented
or `Active`.

## Deferred Command Boundaries

- O1 owns actual Status and Doctor implementation and maps neutral catalogue
  facts to their public contracts.
- O2 owns actual Cleanup candidate selection, defaults, verified-empty/no-lease
  policy, lease acquisition and contention policy, whole-command success
  deletion orchestration, cleanup guidance, result aggregation, and EndToEnd
  evidence. Relevant-domain Repair remains in the same later operations lane.
- MFR-003 distinguishes the implemented create-only store `NotNeeded` result
  from any future command-planning no-op.
- MFR-006 records caller-selected eligible-candidate guarded deletion and
  positive absence for a `Verified` final or ordinary exact-name `Incomplete`
  draft, plus the structurally audited typed `Attention` code path; it does not
  claim candidate selection, whole-operation ordering, a manufactured deletion
  failure, or whole-command policy.
- MFR-007 records neutral catalogue, held-lease deletion-guard, and persistent
  lock facts; it does not claim an executable Cleanup command.

No executable process-crash or power-loss guarantee, permission manipulation,
or fake operating-system failure seam is required. The structural boundary that
a closed final may remain after abrupt process termination is preserved.

## Final Acceptance Evidence

- Locked restore passed for all six projects. Release build passed with zero
  warnings and zero errors.
- Focused mutation-foundation Unit `43/43` and Integration `47/47` passed with
  zero failures or skips.
- Full managed Unit `1096/1096`, Integration `458/458`, and EndToEnd `120/120`
  passed with zero failures or skips.
- Published `linux-x64` focused recovery/source-generation Native AOT `10/10`
  and the cheap full published Integration runner `458/458` passed with zero
  failures or skips.
- Static replacement-CLI Git capability audit returned zero tracked files and
  zero production/test references. Format verification and `git diff --check`
  passed; format reported only the repository's existing workspace-load warning.
- Final independent review returned `ROBUST`, safe to commit,
  confidence `0.98`.

## Failure Routing

Return contract defects to Contracts, lock/schema defects to Lock/Lifecycle,
effect or preparation-integration defects to Apply/Recovery, bundle and
interruption defects to Recovery, and unprovable platform safety to Architecture.

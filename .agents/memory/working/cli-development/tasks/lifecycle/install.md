---
open-forge:
  description: Implement root Framework installation into a selected workspace
  tags: [Memory, Working, CLI, Task, Install, Framework, Lifecycle, Contextual]
---

# Implement Install

## Task State

- State: Active after integrated D0 and SF1-SF4. Its command-local/module slice
  is developing in parallel with C1 on branch `codex/root-install`; C3 is
  already integrated.
  The exact public Install JSON result schema is accepted and frozen in the
  Interface. Command-local result and presentation work may proceed against it,
  including the typed residual values `none`, `retained`, and `unknown`.
  Protected root composition, shared serialization, help, process evidence,
  and final integration are integration-owned sequential seams; this does not
  create a behavior dependency between C1, C2, and C3, and Install remains
  independent of Extension Create.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/install/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/install/behavior.md).

## Expected Outcome

Root `install` places the accepted Framework payload into one selected workspace,
preserves user content, records exact Framework lifecycle identity, refreshes
generated navigation, and provides one verified external recovery bundle covering
every existing target it replaces.

## Architecture

- Observe workspace, existing Framework and legacy-ordinary content, collisions,
  generated navigation, and lifecycle state before planning. The replacement CLI
  does not inspect or report repository state.
- `InstallPlan` contains exact managed files/regions, bridge behavior, generated
  projections, lifecycle write, external recovery-bundle readiness, and effect
  order.
- Consume the neutral `Framework/Distribution` reader over ordinary .NET
  embedded resources. Runtime never reads repository source paths.
- Publish required nullable per-target `sourceAssetPath`; compare only the closed
  base Install subset and preserve other trusted scoped Framework targets and
  generated regions.
- Prompt once only after successful complete preflight for a prompt-capable
  human application that would write. Keep refusal, end-of-input, and
  cancellation no-write `interrupted`; require explicit `--automatic` for a
  non-prompt-capable human application that would write.
- Acquire the persistent external zero-byte workspace lock, then reuse the shared
  parent-first ordinary-BCL directory capability. When `.agents` is missing,
  expose it as the first ordinary planned/reported lease-bound directory-create
  effect. Keep directory effects separate from file effects and retain verified
  created directories as reported residuals after later failure or interruption.
- Keep install policy and managed-file ownership local; share payload identity and
  lifecycle facts with Update.
- Form the exact fully present ordered Interface result graph. Keep embedded
  source and managed-footprint objects atomically nullable, findings/effects
  non-null, residual state typed, and shared envelope coordinates outside the
  command-local result.

## Evidence

Cover empty/existing workspace, source and embedded-payload parity, moved-binary
Native AOT resource access, user text
preservation, canonical and bridge files, collisions, old-format files left
ordinary, dry run, lock race, bundle preparation and retention on partial
failure/cancellation, generated navigation, lifecycle provenance, trusted
scoped-target preservation, the complete prompt/no-prompt matrix, directory
creation races, missing-`.agents` lease-bound creation, pre-effect lock
cancellation/contention, retained residuals after later failure, persistent
external zero-byte lock identity and reuse, second-run
behavior, process, packed-layout, and AOT.

## Stop Conditions

Stop before overwriting user content without an accepted managed region, reading
payload from the development checkout, migrating legacy state, or claiming
installation complete before lifecycle and verification succeed. Stop before a
command-local directory mechanism, directory rollback or recovery, broader
interaction framework, new prompt grammar, or any behavior that weakens the
accepted no-write and authority boundaries. Do not move or duplicate the shared
external workspace-lock identity or merge its versioned subtree with recovery.

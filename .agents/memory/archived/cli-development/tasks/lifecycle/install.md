---
open-forge:
  description: Implement root Framework installation into a selected workspace
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Install, Framework, Lifecycle]
---

# Implement Install

## Task State

- State: Complete in the Assured profile. Final reviewed candidate
  `11994e4d21ddc807b7480afc39ae3612e5a69a56` is squash-integrated into local
  `develop` at `c60fcb98a57e9ec80769b9cb1d399ce13a227863`; both have exact tree
  `464a4a6b6ef6447209edffbf53df7348c70691ed`. Command-local behavior and the
  protected root composition, source-generated serialization, help, process,
  and Native AOT seams are closed.
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

## Completion Evidence

- The final Release solution build has `0` warnings and `0` errors. Managed Unit
  `1390/1390`, Integration `598/598`, and EndToEnd `136/136` pass with zero
  failures and zero skips. Post-rebase focused Unit `41/41`, Integration `26/26`,
  and published process `16/16` also pass.
- The supported `linux-x64` Native AOT root publishes as an x86-64 ELF and
  reports version `0.0.0-dev`. Native AOT Integration `598/598` and EndToEnd
  `136/136` pass with zero failures and zero skips.
- Two independent final Sol/xhigh reviews returned `ROBUST — PASS_RECHECK` and
  `PASS_RECHECK`. The external-lock findings `WLOCK-001` through `WLOCK-004` are
  closed, and no material Install finding remains.
- Native dry-run dogfood from the exact final candidate safely blocked with exit
  `5` on the repository's existing `install.generated-region-unsafe` state. It
  reported no effects, no lifecycle request, no workspace changes, and no new
  external lock. The managed apphost could not start because the local .NET 10
  runtime is unavailable, so no managed dogfood result is claimed.
- No push, remote action, publication, deployment, or release occurred.

## Stop Conditions

Stop before overwriting user content without an accepted managed region, reading
payload from the development checkout, migrating legacy state, or claiming
installation complete before lifecycle and verification succeed. Stop before a
command-local directory mechanism, directory rollback or recovery, broader
interaction framework, new prompt grammar, or any behavior that weakens the
accepted no-write and authority boundaries. Do not move or duplicate the shared
external workspace-lock identity or merge its versioned subtree with recovery.

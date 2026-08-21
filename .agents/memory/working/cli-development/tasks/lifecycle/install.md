---
open-forge:
  description: Implement root Framework installation into a selected workspace
  tags: [Memory, Working, CLI, Task, Install, Framework, Lifecycle, Contextual]
---

# Implement Install

## Task State

- State: Planned after Extension Create and Mutation Foundation.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/install/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/install/behavior.md).

## Expected Outcome

Root `install` places the accepted Framework payload into one selected workspace,
preserves user content, records exact Framework lifecycle identity, refreshes
generated navigation, and provides verified recovery.

## Architecture

- Observe workspace, existing Framework and legacy-ordinary content, collisions,
  Git, generated navigation, and lifecycle state before planning.
- `InstallPlan` contains exact managed files/regions, bridge behavior, generated
  projections, lifecycle write, Git/recovery prerequisites, and effect order.
- Use embedded canonical payload assets generated deterministically at build time.
  Runtime never reads repository source paths.
- Keep install policy and managed-file ownership local; share payload identity and
  lifecycle facts with Update.

## Evidence

Cover empty/existing workspace, source and dogfood payload parity, user text
preservation, canonical and bridge files, collisions, old-format files left
ordinary, dry run, lock race, Git states, partial failure/recovery, generated
navigation, lifecycle exactness, second-run behavior, process, packed-layout, and
AOT.

## Stop Conditions

Stop before overwriting user content without an accepted managed region, reading
payload from the development checkout, migrating legacy state, or claiming
installation complete before lifecycle and verification succeed.

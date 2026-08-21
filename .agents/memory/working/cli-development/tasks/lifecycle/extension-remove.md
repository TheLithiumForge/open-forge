---
open-forge:
  description: Implement Extension removal with preserved user content and recovery integrity
  tags: [Memory, Working, CLI, Task, Extension, Remove, Lifecycle, Contextual]
---

# Implement Extension Remove

## Task State

- State: Planned after Extension Update.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/remove/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/remove/behavior.md).

## Expected Outcome

`extension remove` removes only lifecycle-managed content for selected packages,
preserves user and other package content, updates generated navigation and
lifecycle state, and exposes exact recovery.

## Architecture

- Observe package dependencies, managed identities, user divergence, shared
  files/regions, generated projections, and lifecycle before planning.
- `ExtensionRemovePlan` names exact package order and file/region effects.
- Reuse lifecycle, ownership, atomic, Git, and recovery primitives. Keep selection,
  dependency refusal, removal policy, findings, and result local.

## Evidence

Cover one/many packages, dependency blockers, shared targets, user-modified
content, missing managed content, lifecycle errors, dry run, lock race, Git,
partial failure/recovery, generated navigation, package isolation, second run,
process, and AOT.

## Stop Conditions

Stop before deleting unowned content, cascading dependency removal without
contract authorization, changing Framework lifecycle, or removing recovery
evidence before verified completion.

---
open-forge:
  description: Implement Extension update with source review, ownership, and recovery integrity
  tags: [Memory, Working, CLI, Task, Extension, Update, Lifecycle, Contextual]
---

# Implement Extension Update

## Task State

- State: Planned after Extension Install.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/update/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/update/behavior.md).

## Expected Outcome

`extension update` reconciles selected installed packages from recorded lifecycle
identity to reviewed source packages while preserving user content, package
isolation, Framework state, and one verified external recovery bundle covering
every existing target it replaces or deletes.

## Architecture

- Share package identity, manifest, payload, lifecycle, ownership, and source
  review facts with Install.
- Keep update selection, current-versus-recorded classification, conflict policy,
  multi-package order, command-local plan, findings, and result local.
- Revalidate both source package identity and workspace expected state under lock.

## Evidence

Cover no-op, one/many updates, dependency order, missing/changed source, installed
drift, user-owned text, package conflicts, lifecycle missing/malformed/unknown,
dry run, lock/source race, bundle preparation and retention after partial
failure/cancellation, generated navigation, second run, process, packed source,
and AOT.

## Stop Conditions

Stop before updating unrecorded packages, overwriting unrecognized drift,
resolving ambiguous package identity, or mixing Framework and Extension lifecycle
sections.

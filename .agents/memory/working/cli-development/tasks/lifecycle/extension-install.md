---
open-forge:
  description: Implement Extension installation from exact reviewed package identity
  tags: [Memory, Working, CLI, Task, Extension, Install, Lifecycle, Contextual]
---

# Task 14: Extension Install

## Task State

- State: Queued after Task 12 “CLI Architecture Authority Remediation”.
- Permanent mapping: Task 14 “Extension Install” in the
  [project control ledger](../../project-control.md).
- Prerequisites: Extension Inspect, root Install, and Mutation Foundation are
  complete. Route Move and Task 12 integration provide the accepted activation
  base; Route Remove and root Update are not prerequisites.
- Planned progress horizon: phase 0 of 5, milestone 0 of 8. The streamlined
  phases are Preflight, explicit Gray/Red, one coherent implementation and
  focused-verification pass, one fresh whole-task review with at most one
  grouped improvement pass, and acceptance.
- Parent: [Lifecycle Commands](_lifecycle.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/install/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/extension/install/behavior.md).

## Expected Outcome

`extension install` applies one or more explicitly selected, reviewed Extension
packages to a workspace, records isolated Extension lifecycle identity, preserves
Framework lifecycle, and produces one verified external recovery bundle covering
every existing target it replaces or deletes.

## Architecture

- Reuse shared Extension catalogue/manifest/payload facts and root lifecycle
  primitives.
- Keep selection modes, dependency order, source-review policy, package conflicts,
  `ExtensionInstallPlan`, findings, and result local.
- Plan every package and collision before the lock. Revalidate source and workspace
  expectations under the lock before effects.
- Lifecycle `extensions` entries remain isolated by exact package identity.

## Evidence

Cover exact and automatic selection, none/one/many packages, dependencies,
collisions across packages and Framework, source review, malformed payload,
unsupported compatibility, dry run, lock/source race, bundle preparation and
retention after partial failure/cancellation at each package, lifecycle
isolation, generated navigation, idempotence, process, packed packages, and AOT.

## Stop Conditions

Stop before package download outside accepted sources, runtime code execution,
silent conflict resolution, Framework lifecycle mutation, or applying an
unreviewed/changed package.

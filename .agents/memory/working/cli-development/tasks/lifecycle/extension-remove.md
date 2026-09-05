---
open-forge:
  description: Implement Extension removal with preserved user content and recovery integrity
  tags: [Memory, Working, CLI, Task, Extension, Remove, Lifecycle, Contextual]
---

# Task 18: Extension Remove

## Task State

- State: Queued after Task 17 “Extension Update”.
- Permanent mapping: Task 18 “Extension Remove” in the
  [project control ledger](../../project-control.md).
- Status/Doctor obligation: extend the explicit typed contributor inventory and
  affected evidence before Task acceptance.
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
- Reuse lifecycle, ownership, atomic, and external recovery-bundle primitives.
  Keep selection, dependency refusal, removal policy, findings, and result local.

## Doctor Observation Ownership

Before Task 18 acceptance, Extension Remove owns the exact installed-manifest
observation universe: the accepted workspace root and depth, exact
`extension.json` filename, physical containment, ordinary-read boundary, the
existing source-generated parser, canonical identity, and exact comparison with
trusted lifecycle ownership. It must extend the accepted typed contributor views
and Doctor before acceptance. Until those facts are available, Task 16 emits no
`extension.unmanaged-like-content` finding and retains its bounded observation
limitation.

The universe excludes package-source manifests and legacy
`open-forge.extensions.json`. No broad `.agents` recursion, payload/path/byte
resemblance, Framework bridge, static CLI composition, dependency injection, or
runtime registry may substitute for the producer facts. Static composition is
wiring only.

## Evidence

Cover one/many packages, dependency blockers, shared targets, user-modified
content, missing managed content, lifecycle errors, dry run, lock race, bundle
preparation and retention after partial failure/cancellation, generated
navigation, package isolation, second run, process, and AOT.

## Stop Conditions

Stop before deleting unowned content, cascading dependency removal without
contract authorization, changing Framework lifecycle, or removing recovery
evidence before verified completion.

---
open-forge:
  description: Prepare functional Extension package vocabulary and consumer-owned destinations after the remaining commands
  tags: [Memory, Working, CLI, Task, Extension, Evolution, Content, Destination, Contextual, Queued]
---

# Task 24: Extensions Evolution

## Task State

- State: Queued after Task 23 “Workspace Libraries”. Functional draft
  preparation is active alongside command work; implementation waits for the
  remaining commands and user review.
- Permanent mapping: Task 24 “Extensions Evolution” in the
  [project control ledger](../project-control.md).
- Source: accepted contextual [Extensions Evolution idea](../../../emerging/ideas/extensions-overhaul.md).
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Phase and milestone: not assigned. Implementation activation requires a reviewed
  contract, explicit maintainer acceptance, and a fresh post-command baseline.
- Prior analysis: preparation tip
  `8a153f23dabb05019eae2c15fae51331b9e88335`, tree
  `bafd3d1e3173ad9342bd25a6086330dcfbe5ee16`, explored a narrower six-command
  internal refactor. It is non-authoritative input for Task 26 “Extension
  Internal Consolidation”; it neither defines nor activates this task.

## Accepted Boundary

The accepted Extensions Evolution idea is contextual input for a future local
and offline Extension boundary. Task 24 preserves two explicit decision fronts:

- choose atomically among the current on-disk `payload/` package directory, the
  recorded `content/` candidate, and the newly suggested `contents/` candidate;
- define consumer-owned permission for exact workspace-relative Extension copy
  destinations beyond `.agents/` without allowing package metadata or source
  bytes to grant themselves access.

No vocabulary or destination-policy shape is accepted yet. The current
Extensions MVP Architecture, package layout, `.agents/**` destination boundary,
contracts, and lifecycle commands remain authoritative until a later contract
freeze. No compatibility layer follows from recording the alternatives.

Extension permission, ownership, update, removal, collision, recovery, and
security remain one Extension-local lifecycle. Task 25 separately owns any
future Library projection outside `.agents/`; matching destination mechanics do
not merge copied-file and live-link ownership. A neutral path-admission
primitive may be shared only after both consumers prove identical meaning.

The source-reader fail-closed and Update parent-catalogue candidates discovered
by the narrower preparation return to this future behavior-contract review.
Task 26 cannot use them to change behavior during internal consolidation.

## Implementation Conditions

After the remaining command sequence is complete, activation requires a final
draft covering the package-directory choice, exact permission carrier and
scope, protected destinations, physical containment, ownership, update,
removal, recovery, and malformed-input, collision, and interruption cases under the accepted
local-tool threat boundary. The maintainer reviews and accepts
that draft before any contract or implementation change.

Draft preparation is authorized now and assigns no phase or milestone horizon.
It does not authorize implementation, lifecycle-schema changes, compatibility
machinery, remote action, a runtime registry, or publication. Task 25 may
prepare its functional draft in parallel, but
shared or public implementation remains serialized. Task 26 follows both tasks
so the final accepted behavior is consolidated once.

## Functional Draft

The [joint functional proposal](extensions-destination-proposal.md) presents
recommended package vocabulary, consumer permission, concrete copy and link
examples, lifecycle behavior, and the remaining user decision. It is contextual
and must reach the user before implementation.

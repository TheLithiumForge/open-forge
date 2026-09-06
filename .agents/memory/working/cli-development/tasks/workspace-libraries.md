---
open-forge:
  description: Define and later implement accepted contextual Workspace Libraries after the retained command sequence
  tags: [Memory, Working, CLI, Task, Workspace, Library, Contextual, Queued]
---

# Task 23: Workspace Libraries

## Task State

- State: Queued post-command last-stage implementation. It follows Task 20
  “Cleanup” and precedes Task 24 “Extensions Evolution”.
- Permanent mapping: Task 23 “Workspace Libraries” in the
  [project control ledger](../project-control.md).
- Source: accepted contextual [Workspace Libraries design](../../../emerging/ideas/workspace-libraries.md).
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Phase and milestone: not assigned. Read-only Preflight and draft preparation
  may run while the retained command sequence is active. Shared semantic
  implementation and integration wait for Task 20 and a separate contract
  freeze with explicit maintainer acceptance.

## Accepted Boundary

Workspace Libraries are a candidate middle layer between project-owned files
and static Extension installation. The accepted design is contextual input for
future contract work. It does not change the current CLI contracts, Framework
routes, Extension boundary, lifecycle schema, Doctor vocabulary, or active
command order.

No implementation contract is authored here. The design remains the source of
the candidate list/inspect, attach, sync, and detach concepts, their contained
source-root boundary, relative-link requirement, consumer-owned controls, and
recovery questions.

The maintainer accepted these design decisions: one contained source root with
the complete eligible `.agents/**` inventory at identical consumer-relative
paths; a separate `.agents/open-forge.libraries.json` record with Sync
retirement only after complete source inventory and exact expected-link proof,
and all-or-nothing Detach with record publication or removal ordered last; and
real relative file symlinks with no copy fallback or initial Git diagnostics or
actions. The final drafted Library contracts and CLI package still require a
distinct maintainer review and contract freeze before implementation.

## Activation Conditions

Before implementation, the final drafted Library contracts and CLI package
must pass the distinct maintainer review and contract freeze. The link-aware
guard for Route Update, Index, Route Move, and Route Remove, together with a
real-filesystem regression, must also be accepted before any Attach dogfood.

The contract freeze must cover the record and lifecycle contracts, ownership,
recovery, collision, source availability, and supported-platform behavior.
Status, Doctor, recovery, and Extension evidence may only be extended by later
accepted contracts. No phase, milestone, or evidence gate is assigned until
the queued task is activated.

## Non-Goals

This queued task does not authorize symlink or submodule behavior in the
current CLI, external destinations, a new Framework root, imported-content
runtime behavior, Git fetch/pull/checkout/switch/stage/commit operations,
compatibility machinery, or publication before its contract freeze. It does
not move, delete, or rewrite source files and does not alter any current
command.

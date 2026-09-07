---
open-forge:
  description: Execute the accepted Workspace Libraries contracts through bounded preparation, implementation, evidence, and integration
  tags: [Memory, Working, CLI, Task, Workspace, Library, Contextual, Active, Queued]
---

# Task 23: Workspace Libraries

## Task State

- State: Task-locally Active at phase 2 of 5, milestone 1 of 8. The accepted
  contract freeze is complete, and Gray correction is active. Red authors
  remain read-only until immutable Gray acceptance. In the
  project queue, Task 23 remains Queued after Task 20 “Cleanup” for semantic
  Green and integration. It completes the remaining command sequence before
  queued Tasks 24 “Extensions Evolution”, 25 “Workspace Library Destination
  Projections”, and 26 “Extension Internal Consolidation”.
- Permanent mapping: Task 23 “Workspace Libraries” in the
  [project control ledger](../project-control.md).
- Accepted contract source at the recorded tip:
  `.agents/memory/crystallized/documents/cli/contracts/library/_library.md`.
- Accepted design source at the recorded tip:
  `.agents/memory/crystallized/documents/cli/technical-designs/workspace-libraries.md`.
- Contextual source: [Workspace Libraries idea](../../../emerging/ideas/workspace-libraries.md).
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Accepted contract tip: `c3f01acb76c572ee486fdc24c6a2379b27459391`,
  tree `5ccffb7156f73aac3c15a49588b7eafcfa661a0a`. The production baseline is
  deferred until the exact post-Task 20 integrated baseline is refrozen.

## Accepted Boundary

Workspace Libraries are the accepted middle layer between project-owned files
and static Extension installation. The accepted contracts and technical design
define List, Inspect, Attach, Sync, and Detach plus their contained source-root,
relative-link, consumer-control, ownership, and recovery boundaries. They do
not change Framework roots, Extension identity, or the active command order.

The maintainer accepted these design decisions: one contained source root with
the complete eligible `.agents/**` inventory at identical consumer-relative
paths; a separate `.agents/open-forge.libraries.json` record with Sync
retirement only after complete source inventory and exact expected-link proof,
and all-or-nothing Detach with record publication or removal ordered last; and
real relative file symlinks with no copy fallback or initial Git diagnostics or
actions. The accepted contracts and technical design now govern bounded
preparation; Green still requires the post-Task 20 baseline refreeze.

The first release remains strictly `.agents/**`-only. Task 25 separately
preserves a possible later consumer-approved destination expansion; it does not
widen the active Task 23 contract or implementation.

## Activation Conditions

Gray correction may proceed against the accepted contracts. Red authors remain
read-only until immutable Gray acceptance. Semantic
Green and integration wait for Task 20 integration and an exact baseline
refreeze. The link-aware guard for Route Update, Index, Route Move, and Route
Remove, together with a real-filesystem regression, must be accepted before any
Attach dogfood.

Status, Doctor, recovery, and Extension evidence may only be extended by the
accepted contracts and directly required integration facts. No Green owner or
production baseline is assigned until the post-Task 20 refreeze.

## Non-Goals

This queued task does not authorize symlink or submodule behavior in the
current CLI, external destinations, a new Framework root, imported-content
runtime behavior, Git fetch/pull/checkout/switch/stage/commit operations,
compatibility machinery, or publication outside its accepted contracts. It does
not move, delete, or rewrite source files and does not alter any current
command.

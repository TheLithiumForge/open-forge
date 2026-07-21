# Benchmark Building Blocks

Building blocks separate stable tasks from the worker-visible context used to test them. A scenario is framework-agnostic. A primitive contributes one routed behavior or source of truth. Meta-scenarios choose exact combinations.

## Scenarios

- [Local tool discovery](scenarios/discovery/local-tool/scenario.json) - Shape a small private terminal tool through concise discovery.
- [Standup journal delivery](scenarios/delivery/standup-journal/scenario.json) - Implement and verify a compact journal CLI.
- [Handoff note implementation](scenarios/delivery/handoff-note/scenario.json) - Settle a bounded technical gap and deliver a first CLI slice.
- [Ledger remove planning](scenarios/planning/ledger-remove/scenario.json) - Prepare one removal behavior for implementation without writing code.
- [Archive export recall](scenarios/recall/archive-export/scenario.json) - Recover constraints, non-goals, and checks without mutating the workspace.

## Primitives

- [Standup journal contract](primitives/memory/delivery/standup-journal-contract/primitive.json) - Accepted product, persistence, and calendar truth.
- [Standup project location](primitives/workspace/delivery/standup-project-location/primitive.json) - Coarse destination for the implementation.
- [Handoff note product direction](primitives/memory/delivery/handoff-note-product-direction/primitive.json) - Accepted product truth with a deliberate technical gap.
- [Immutable ledger removal](primitives/memory/planning/immutable-ledger-removal/primitive.json) - Accepted append-only reversal semantics.
- [Hard-delete ledger removal](primitives/memory/planning/hard-delete-ledger-removal/primitive.json) - Independently coherent hard-delete semantics that conflict only when combined with immutable removal.
- [Archive export contract](primitives/memory/recall/archive-export-contract/primitive.json) - Accepted compatibility and verification truth.
- [Read-only recall](primitives/directive/recall/read-only-recall/primitive.json) - Binding non-mutation boundary for recall tasks.
- [Source-backed summary](primitives/pattern/recall/source-backed-summary/primitive.json) - Inspectable answer shape tying claims to their owners.

## Composition Contract

`scenario.json` contains only a string `id`. `primitive.json` contains a string `id` and one supported `kind`. Folder names organize the catalogue; manifests own identity.

Scenario payloads contain ordinary task files only. Primitive payloads contain their routed Open Forge files. Reviews and personas stay outside the worker workspace. File collisions are errors, and composition order is explicit in each meta-scenario.

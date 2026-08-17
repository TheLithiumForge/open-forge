---
open-forge:
  description: Routing-only entrypoint for the accepted `route create` command contracts
  responsibility: Route `route create` help to its Interface and Behavior Contracts
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, Create, Template, Mutation, CurrentTruth]
---

# route create Command Contract Set

## Status And Authority

This is the accepted current Crystallized entrypoint for `route create`. The
command does not ship yet; implementation and executable proof remain pending
Gate 5. Its sibling Interface and Behavior Contracts define command meaning.
This entrypoint does not duplicate their detail. No Route Technical Design is
needed.

## Contract Roles

- [`interface.md`](interface.md) is the caller-visible Interface Contract.
- [`behavior.md`](behavior.md) is the technology-neutral Behavior Contract
  behind that interface.

This entrypoint and its generated `Entries` provide navigation and help routing
only. They do not define command detail.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted current technology-neutral contract for deterministic route create resolution, effects, safety, recovery, and conformance](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Behavior #Route #Create #Template #Mutation #CurrentTruth
- [Accepted current public contract for creating one ordinary routed Markdown file with explicit metadata and optional Template body content](interface.md) - #Memory #Crystallized #CLI #Release #Command #Interface #Route #Create #Template #Mutation #CurrentTruth
<!-- open-forge:generated-index:end -->

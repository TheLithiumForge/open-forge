---
open-forge:
  description: Routing-only entrypoint for the accepted current `route list` command contracts
  responsibility: Route the route-list Interface and Behavior Contracts without defining command meaning
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Route, List, CurrentTruth]
---

# route list Command

## Status And Authority

This is the accepted current Crystallized entrypoint for the read-only
`route list` command. The [Interface Contract](interface.md) defines the caller-visible
grammar and result. The [Behavior Contract](behavior.md) defines deterministic,
technology-neutral resolution and conformance. This entrypoint does not create a
second command contract and no command-local Technical Design file is needed for
this operation. Implementation and executable evidence are tracked in
[CLI Development](../../../../../../working/cli-development/_cli-development.md).

The command does not ship yet. It is stateless, read-only, and non-shipping: a
request may inspect the selected workspace, but it never changes authored files,
generated `Entries`, or other persistent state.

## Routing And Help

This entrypoint routes the two local contracts used by `route list`. Help for the
operation exposes the accepted source operand, structural depth flag, shared
global flags, and examples. Generated `Entries` provide navigation only and are
not part of route-list enumeration authority.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Technology-neutral behavior and conformance for deterministic routed-topology enumeration](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #List #Behavior #Topology #CurrentTruth
- [Current public interface and observable result for read-only routed-topology enumeration](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Route #List #Interface #Topology #CurrentTruth
<!-- open-forge:generated-index:end -->

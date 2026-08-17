---
open-forge:
  description: Routing-only entrypoint for the accepted current `references` command contracts
  responsibility: Route the references Interface and Behavior Contracts without defining command meaning
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, References, CurrentTruth]
---

# references Command

## Status And Authority

This is the accepted current Crystallized entrypoint for the read-only
`references` command. The [Interface Contract](interface.md) defines its public
grammar and observable result. The [Behavior Contract](behavior.md) defines
deterministic, technology-neutral reference resolution, scan coverage, safety,
and conformance. This entrypoint does not create another command contract and
no command-local Technical Design file exists for this operation.

The public command identity and final physical contract route are `references`
and `contracts/references/`. Recognizing the compatibility entrypoint
`_references.md` must not create a second logical source: the same physical
entrypoint is canonicalized by identity and processed once. Gate 5 executable
proof must preserve this physical-identity regression.

The command does not ship yet. It is stateless, read-only, and non-shipping: it
reports direct reference facts without modifying the workspace, loading target
bodies into context, fetching external URLs, or maintaining a persistent index.

## Routing And Help

This entrypoint routes the two local contracts used by `references`. Help for the
operation exposes one required source reference, the exact direction selector,
the incoming source-universe filters, shared global flags, and examples.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Technology-neutral behavior and conformance for direct incoming and outgoing reference inspection](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #References #Behavior #Links #CurrentTruth
- [Current public interface and observable result for direct incoming and outgoing reference facts](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #References #Interface #Links #CurrentTruth
<!-- open-forge:generated-index:end -->

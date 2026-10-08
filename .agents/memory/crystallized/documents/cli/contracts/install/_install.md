---
open-forge:
  description: Route the current install contracts for setup selection, restoration, management establishment and verification
  responsibility: Route the root install Interface and Behavior files without adding lifecycle meaning or implementation detail
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Install, Framework, Lifecycle, CurrentTruth]
---

# Install Command Contract Set

## Routing Boundary

This is a routing-only contract-set entrypoint for the direct root `install`
command. It exposes the two local contract roles for Framework management
establishment and exact managed no-op. It is not a `framework` group and it does
not create a root `init` operation.

## Status And Authority

This is the accepted current Crystallized contract set for the root Framework
`install` operation. The CLI is available as a public beta. [CLI Distribution](../../distribution.md)
records qualified platforms and published versions. [CLI Development](../../../../../working/cli-development/_cli-development.md)
records current implementation and release work. `install` may establish a safely absent or
eligible initial state and verify an exact managed state. Managed divergence is
not ordinary install work and directs to root `update`. Explicit configuration
selects built-in routes, restores eligible missing defaults, manages their
Git-ignore choices, and changes the frontmatter form through the [bounded
conversion rule](interface.md#configure-conversion). Its sibling Interface and
Behavior files are the detailed authorities for the public surface and the
technology-neutral operation behind it.

Install owns the closed base Framework subset and consumes the neutral embedded
Framework distribution. It preserves trusted scoped Framework targets created by
later Route Init operations. Ordinary Install neither treats them as base
divergence nor reconciles them. Configure's bounded form conversion also
includes eligible owned delivered scoped copies.

The accepted CLI Architecture defines the shared implementation boundary. Gate 5
must prove source-generated YamlDotNet and STJ serialization, fixed Markdig where
used, real `System.IO`, Native AOT, OS locking, isolated tests, and package
journeys. This contract set does not claim that implementation or proof.

## Contract Roles

- [`interface.md`](interface.md) defines the exact syntax including
  `--automatic`, the recognized footprint, management-establishment and initial
  force boundary, observable effects, results, errors, examples, and public
  conformance.
- [`behavior.md`](behavior.md) defines deterministic resolution, lifecycle
  trust and fingerprint facts, one complete establishment plan, application or
  preview, verification, safety, recovery, result formation, and
  technology-neutral conformance.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Current install rules for setup selection, additive restoration, bounded frontmatter conversion, management establishment and recovery](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Install #Framework #Behavior #Determinism #Lifecycle #Safety #Recovery #CurrentTruth
- [Accepted current Interface for installing, configuring and verifying built-in Framework routes](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Install #Framework #Interface #Lifecycle #Safety #Recovery #CurrentTruth

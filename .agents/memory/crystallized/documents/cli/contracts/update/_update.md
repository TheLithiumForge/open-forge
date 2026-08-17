---
open-forge:
  description: Route the accepted non-shipping root Framework update contracts for trusted managed reconciliation
  responsibility: Route the root update Interface and Behavior files without adding lifecycle meaning or implementation detail
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Update, Framework, Lifecycle, CurrentTruth]
---

# Update Command Contract Set

## Status And Authority

This is the routing-only contract-set entrypoint for the direct root Framework
`update` command. Its sibling Interface and Behavior files define trusted
managed Framework reconciliation. The new CLI does not ship yet.

`install` establishes management and verifies an exact managed no-op. Managed
divergence belongs to this `update` operation. There is no Framework group and no
Framework uninstall or remove leaf.

## Contract Roles

- [`interface.md`](interface.md) defines the exact public syntax, lifecycle
  boundaries, modes, effects, statuses, output, errors, examples, and public
  conformance.
- [`behavior.md`](behavior.md) defines technology-neutral current-fact
  resolution, semantic comparison, complete planning, preflight, application,
  verification, recovery, result formation, and conformance.
- No Technical Design file exists. The accepted CLI Architecture defines the
  shared implementation boundary. Gate 5 must prove source-generated YamlDotNet
  and STJ serialization, fixed Markdig where used, real `System.IO`, Native AOT,
  OS locking, isolated tests, and package journeys. This contract does not claim
  that implementation or proof.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Accepted technology-neutral behavior for trusted Framework update planning, force replacement, prune deletion, and recovery](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Update #Framework #Behavior #Determinism #Lifecycle #Safety #Recovery #CurrentTruth
- [Accepted non-shipping Interface for trusted managed Framework reconciliation with force and prune boundaries](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Update #Framework #Interface #Lifecycle #Safety #Recovery #CurrentTruth
<!-- open-forge:generated-index:end -->

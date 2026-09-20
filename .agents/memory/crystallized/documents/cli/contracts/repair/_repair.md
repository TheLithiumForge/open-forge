---
open-forge:
  description: Route the accepted current Repair Interface and Behavior contracts for exact and guided local repair
  responsibility: Route the Repair contracts without adding mutation meaning or implementation detail
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Repair, CurrentTruth]
---

# Repair Command Contract Set

## Status And Authority

This routed set is the accepted current Crystallized authority for the
non-shipping `repair` command. Its sibling Interface and Behavior files are the
detailed authorities for the public surface and technology-neutral operation.
This entrypoint provides navigation only. Implementation and executable evidence are tracked in
[CLI Development](../../../../../working/cli-development/_cli-development.md).

Repair is a constrained mutation operation. It does not promise to resolve every
Doctor finding and does not author content, choose ownership, or replace a
targeted lifecycle operation.

## Contract Roles

- [`interface.md`](interface.md) defines the complete public grammar, selection
  paths, relink values, repair catalogue, output, semantic results, errors,
  examples, and public verification.
- [`behavior.md`](behavior.md) defines fresh diagnosis, proposal and intent
  resolution, planning, conflict handling, preflight, dry-run, application,
  verification, recovery, post-diagnosis, and conformance.
- No command-local Technical Design file exists for this command. The [CLI
  Architecture](../../architecture.md) defines shared implementation
  boundaries; this contract set remains technology-neutral.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Current technology-neutral exact and guided Repair lifecycle, safety, recovery, and conformance](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Repair #Behavior #Mutation #Safety #Recovery #CurrentTruth
- [Current accepted interface for automatic, guided, explicit-relink, and dry-run Repair](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Repair #Interface #Mutation #Relink #Safety #CurrentTruth

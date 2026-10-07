---
open-forge:
  description: Start the routed command scope that exposes one local Interface, Behavior, and optional Technical Design set
  tags: [Template, CLI, Command, Contract, Entrypoint, Routing]
---

# `{command-path}` Command Contract Set

{
Use to route one command's Interface, Behavior, and optional Technical Design: which files define it, and what is their current authority state?

Copy this file to `contracts/{command-path}/_{command-name}.md`. Replace metadata,
including removing the `Template` tag, so it describes the independent routed
scope. This is a copy-ready starter, not a current command instance. Replace
the title, links, link depth, and prompts. Keep routing and authority context here, with command detail in the contracts. State the destination's actual lifecycle and authority. Remove the Technical Design item when that file does
not exist. Run the applicable index command to regenerate `Entries`, then remove
this guidance and unused optional prompts while preserving the entrypoint sections.
}

## Routing Boundary

This is a routing-only contract-set entrypoint for one leaf command. A group
entrypoint routes child operations and group help only. It does not define a
group Interface, Behavior, or Technical Design contract.

## Status And Authority

{State the set's lifecycle, whether the command ships, and which source is
authoritative for each contract question.}

## Contract Roles

- [`interface.md`](interface.md) defines the complete public surface and
  observable result.
- [`behavior.md`](behavior.md) defines deterministic technology-neutral
  semantics, effects, safety, recovery, and conformance.
- If present, [`technical-design.md`](technical-design.md) records implementation
  choices subordinate to accepted Architecture and distinguishes accepted
  choices from pending executable proof.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- none - No entries - #Empty

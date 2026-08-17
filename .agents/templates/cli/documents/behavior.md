---
open-forge:
  description: Start a technology-neutral command behavior definition for deterministic resolution, effects, safety, recovery, and conformance
  tags: [Template, CLI, Command, Contract, Behavior]
---

# `{command-path}` Behavior Contract

{
Template selection:

- Need: One command-local document that completely defines deterministic semantics behind an accepted CLI interface.
- Primary question: How must every conforming implementation resolve, execute, verify, and report this operation without depending on one technology?

Copy this file to `contracts/{command-path}/behavior.md`. Replace metadata,
including removing the `Template` tag, so it describes the independent file's
scope, state, and authority. This is a copy-ready starter, not a current command
instance. Copying it creates an independent file; later Template changes do not
update that file. Replace the title, links, link depth, and placeholders. Remove
sections that cannot apply, but preserve the responsibilities stated by this
starter. Repeat rows and subsections for every applicable invariant, behavior,
effect, result condition, and independently testable fact. Do not add permanent
requirement IDs or temporary-state placeholders. Remove this braced source
guidance.
}

## Status And Authority

{State lifecycle, authority, implementation availability, and the Interface
Contract this behavior satisfies.}

## Operation Invariants

- {State one complete deterministic invariant.}

## Request Resolution

{Define normalization, default resolution, identity, containment, duplicates,
ordering, and invalid or blocked boundaries.}

## Current Facts And Coverage

{Define the source universe, meaning the complete set of sources the operation
may inspect, and the mutable state when the operation can change state. Define
required completeness, ambiguity, and how incomplete facts affect continuation
and results.}

## Selection And Result Formation

{Define matching, selection, projection, ordering, deduplication, and the
conditions that form each public semantic result.}

## Effects

{For a read operation, state that no persistent mutation occurs. For a mutation,
define the complete planning, preflight, dry-run, apply, revalidation,
verification, recovery, and interruption flow.}

## Safety And Recovery

{Keep this section for mutation or external-effect boundaries. Define authority,
concurrency, no-op behavior, rollback, residual evidence, and forbidden weaker
fallbacks.}

## Presentation Relationship

{Define the one typed result consumed by human and structured renderers. Do not
duplicate exact public output already defined by the Interface Contract.}

## Conformance Evidence

{List applicable `Unit`, `Integration`, `EndToEnd`, and `PackageEndToEnd` evidence.
State the exact Behavior and related Interface facts or sections that each tier
proves. Use real boundaries for integration and delivered-process evidence.}

## Related Sources

- {Interface Contract in the destination}
- {Optional Technical Design in the destination}
- {Shared contracts and accepted Framework meaning in the destination}

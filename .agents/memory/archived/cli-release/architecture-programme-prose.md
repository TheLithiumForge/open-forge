---
open-forge:
  description: Programme-era prose removed from the CLI Architecture when it was split into layer records, covering the greenfield reset framing, the build sequence, and Task delegation rules
  tags: [Memory, Archived, CLI, Architecture, Reset, History]
---

# Archived CLI Architecture Programme Prose

## Why this is here

The [CLI Architecture](../../crystallized/documents/cli/architecture.md) was
written during the 2026-08-21 greenfield reset, while the replacement CLI was
being built. It therefore carried two kinds of content: the architecture of the
system, and the running state of the programme that was building it.

When the document was split into [layer records](../../crystallized/documents/cli/layers/_layers.md)
on 2026-09-11, the second kind was removed. The commands are built; a build
order and a "becomes shipping only after" clause describe a programme that has
largely run, and keeping them in a `#CurrentTruth` record invites a reader to
treat them as current architecture.

What was still current was extracted first, as the archive rules require:

- The **authority links** — Command Contract Set, Shared Operation Contract,
  command contracts, Result Coordinates, Technical Designs — stayed in the land
  Architecture's Status And Authority section.
- The **dependency-order principle** — _"a consumer never invents a missing
  shared contract, identity, safety primitive, composition boundary, or
  evidence foundation"_ — is a durable rule and stayed, restated under
  Dependency Direction.
- The **delegation boundary** — _"a bounded implementation Task cannot invent or
  reinterpret cross-cutting architecture"_ — is orchestration rather than
  architecture. It is already carried by the
  [CLI Development](../../working/cli-development/_cli-development.md) route's
  own Axioms, which is where a Task reads it.
- The **package and platform boundary** stayed, because
  [CLI Distribution](../../crystallized/documents/cli/distribution.md) is the
  declared authority for it and the land Architecture links there.

Nothing below asserts current authority. Its Open Forge metadata is archival.

## Durable Implementation Sequence

> Stable dependency order runs from cross-cutting shell and filesystem
> foundations to read-only fact formation, then generated navigation and
> mutation foundations, then mutation producers, aggregate Status and Doctor
> views, repair and cleanup, and complete distribution. A consumer never invents
> a missing shared contract, identity, safety primitive, composition boundary,
> or evidence foundation.
>
> The exact queue, readiness, completion state, and integration receipts belong
> to the active CLI Development route, its Plan, Tasks, and project control.
> They are not durable Architecture.

The sequence described the order in which the greenfield programme built the
command set. That order has run. The surviving rule is the last sentence of the
first paragraph.

## Planning, Tasks, And Delegation

> A bounded implementation Task cannot invent or reinterpret cross-cutting
> architecture, public behavior, shared schema, safety, package, platform, or
> release meaning. It returns an unresolved boundary to the current project
> authority before code continues.
>
> Current Directives, Workflows, role sources, and Task records define work
> orchestration, evidence procedure, review, and acceptance. Local passing tests
> do not accept a change that violates this Architecture or its linked
> contracts.

This is orchestration, not architecture. The CLI Development route states the
same boundary where a Task reads it.

## Reset framing removed from Status And Authority

> This document defines the accepted implementation architecture for the
> non-shipping replacement CLI after the 2026-08-21 greenfield reset.
>
> The removed implementation remains historical evidence in the [reset
> record](implementation-reset-2026-08-21.md). Historical source may inform a
> Task, but it does not constrain class shape, source placement, or
> implementation.
>
> The replacement remains non-shipping.

The reset record is still linked from the archive route and remains readable.
The "non-shipping" claim was programme state; release state belongs to the
CLI Development route and to CLI Distribution.

## Goals clauses removed

> - let bounded implementers execute closed Tasks without inventing
>   architecture.

A statement about how the programme was staffed rather than about the system.

> The implementation must not add ... a compatibility path to `open-forge-old`.

`open-forge-old` is gone. The surviving half of that sentence — no runtime
plug-in discovery, no dependency injection for shell composition, no service
locator, no fake filesystem, no universal command result, no universal mutation
engine, no native interop — stayed, because each remains a live constraint.

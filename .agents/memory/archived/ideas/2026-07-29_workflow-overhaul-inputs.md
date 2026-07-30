---
open-forge:
  description: Historical inputs used to replace the eight-section Workflow schema with the minimal goal, steps, completion, and optional dependency contract
  tags: [Memory, Archived, Idea, Contextual, Historical, Workflow, Architecture, Product, Refactor]
---

# Workflow Overhaul Inputs

## Status

Archived on 2026-07-29 after the accepted Workflow contract, first-party recipes, validation, and documentation were migrated together.

The current result lives in the [Workflow contract](../../crystallized/documents/framework/primitives/workflows.md) and [Workflow shape decision](../../crystallized/decisions/workflow-shape.md). This file preserves the redesign inputs and does not define active runtime requirements.

## Why Revisit Workflows

The existing model proved useful distinctions between goals, dependencies, phases, iteration, and routed composition, but it was designed during an earlier Framework stage. The current Framework provides a cleaner basis for deciding what Workflows must own and what belongs in Skills, Templates, routing, or deterministic tooling.

## Preserved Inputs

- Test whether Workflows should remain a standard shipped `route` and what value earns that baseline surface.
- Prefer a materially smaller authoring and runtime contract than the legacy schema unless each retained section proves necessary.
- Reassess whether top-down selection and the current phase vocabulary still improve wayfinding enough to justify their cost.
- Consolidate generic development recipes and remove Skill dependencies that merely restate native agent capability.
- Manual installations need an on-demand, human-readable way to author validator-compliant Workflows. A shipped Template or authoring reference should make the Markdown contract complete without repository-only architecture or CLI help.
- Earlier Workflow handoffs named the active Workflow route and active step so delegated or resumed work retained precise execution position. The redesign should decide whether this remains required, recommended, or replaced by a more general handoff contract.
- Earlier designs allowed explicitly typed Workflow-local Core, Workspace, and Memory `routes`. The current Framework keeps `root routes` separate and composes them through links. A redesign may reconsider tightly coupled packaging or locality only if its value justifies special loading, validation, update, and runtime discovery semantics.
- Containment and unconditional dependencies should remain visibly distinct unless a better model replaces them.
- Optional prior work must not become hidden ceremony or block an explicitly selected Workflow.
- Workflow composition should remain explicit enough that agents and people can inspect which goal is primary, which capabilities are invoked, and how handoffs occur.

## Redesign Surface

Review the complete system together:

- role and boundary with Skills
- authoring shape and Templates
- linear and iterative behavior
- goal and completion evidence
- phase wayfinding
- Required Routes and dependency semantics
- delegation and handoffs
- Workflow-local packaging, goal-scoped context, and the current cross-root link boundary
- installed runtime wording
- CLI discovery, creation, help, and validation

## Historical Context

- [Archived Workflow redesign](../../archived/ideas/workflow-redesign.md)
- [Current Workflow contract](../../crystallized/documents/framework/primitives/workflows.md)
- [Workflow shape decision](../../crystallized/decisions/workflow-shape.md)

---
open-forge:
  description: Preserve requirements and unresolved boundaries that should inform the planned Workflow architecture redesign
  tags: [Memory, Idea, Contextual, Candidate, Workflow, Architecture, Product, Refactor]
---

# Workflow Overhaul Inputs

## Status

The current Workflow contract remains #CurrentTruth until a replacement is accepted. This file preserves redesign inputs, not active runtime requirements.

## Why Revisit Workflows

The existing model proved useful distinctions between goals, dependencies, phases, iteration, and routed composition, but it was designed during an earlier Framework stage. The completed migration and Core optimization should provide a cleaner basis for deciding what Workflows must own and what belongs in Skills, Templates, routing, or deterministic tooling.

## Preserved Inputs

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

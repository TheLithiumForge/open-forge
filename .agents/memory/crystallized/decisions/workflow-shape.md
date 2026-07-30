---
open-forge:
  description: Workflows use a minimal goal, steps, and completion recipe while optional routed dependencies remain distinct from containment
  tags: [Memory, Decision, CurrentTruth, Workflow, Routing, Orchestration]
---

# Workflow Shape

## Context

Earlier Workflow experiments proved useful distinctions between goals, dependencies, iteration, containment, and completion. They also accumulated eight mandatory sections, fixed modes, phase tags, selection ceremony, and reporting requirements that capable agents largely ignored or repeated mechanically.

## Decision

Open Forge uses a minimum complete recipe of `Goal`, `Steps`, and `Completion` in that order.

The accepted design separates several concerns:

- `Goal` identifies the intended result
- `Steps` contain the procedure and any boundaries, branching, iteration, delegation, capabilities, verification, or stopping behavior it needs
- `Completion` identifies the result or evidence that ends the recipe
- `Required Routes` is optional and appears between `Goal` and `Steps` only for unconditional routed dependencies
- Generated `Entries` express containment while `Required Routes` express dependency
- Recipe-specific headings may improve one Workflow without expanding the universal schema
- Organizational `entrypoints` can route Workflows without pretending to be recipes
- Other Core `root routes` remain separate and enter through explicit links, Steps, or handoffs

## Rationale

Three required sections preserve the distinctions needed to confirm fit after loading, execute, and finish a recipe. Visible `description` values, tags, `route` meaning, and current user direction remain the pre-load selection surface. The sections are small enough to author manually, understand without repository-only documentation, and follow without turning useful work into form completion.

Optional `Required Routes` preserve deterministic dependency loading and visible failure without requiring `none` in every independent recipe. Keeping containment separate from dependency prevents generated tree structure from becoming an implicit execution graph.

Descriptions and topical tags already provide pre-load selection. Fixed modes and phases did not add enough selection value to justify another vocabulary. Iteration, outputs, constraints, and prior-work suggestions remain expressible where the individual recipe needs them.

## Alternatives And Tradeoffs

- Free-form recipes remove all schema cost but make selection, execution, completion, and validation inconsistent
- The previous eight-section schema made every distinction explicit but duplicated information and encouraged mechanical compliance
- Fixed phases improved catalogue grouping but overlapped descriptions and topical tags while implying a lifecycle the Framework does not impose
- A mandatory Loop exposed repetition but forced linear recipes to describe its absence
- Mandatory Constraints and Outputs made those concepts visible but often repeated Steps, Goal, Skills, Directives, or Completion
- Workflow-local primitive trees improve package locality but turn recipes into mixed Core containers and require special loading, validation, update, and discovery rules

## Consequences

- Existing first-party Workflows move useful mode, constraint, loop, and output meaning into the three retained sections
- Phase tags and phase-aware validation are removed from current source and tooling
- CLI validation checks the minimum shape and optional dependency section without rejecting useful recipe-specific headings
- Direct execution remains valid, and no Workflow must be named in commentary or closeout merely for audit ceremony
- Handoffs identify a Workflow or active step when that information materially helps resumption rather than as a Workflow-specific requirement
- A Workflow earns a `route` through distinctive reusable execution value rather than by restating ordinary capable-agent behavior

## Authoritative Sources

- [Current Workflow contract](../documents/framework/primitives/workflows.md)
- [Installed Workflows entrypoint](../../../workflows/_workflows.md)
- [Workflows maintenance contract](../documents/maintenance/payload/agents/workflows.md)

## Historical Context

Historical redesign detail and the former TDD example remain in [Workflow redesign](../../archived/ideas/workflow-redesign.md). The requirements that triggered this simplification remain in the archived [Workflow overhaul inputs](../../archived/ideas/2026-07-29_workflow-overhaul-inputs.md).

## Decision Relationships

- [Distinct Core primitive roles](core-primitives.md)
- [Routing surfaces](routing-surfaces.md)

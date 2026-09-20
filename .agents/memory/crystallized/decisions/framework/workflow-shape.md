---
open-forge:
  description: Workflows use a minimal goal, steps, and completion recipe while relationships remain explicit in ordinary links and steps
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
- Recipe-specific headings may improve one Workflow without expanding the universal schema
- Organizational `entrypoints` can route Workflows without pretending to be recipes
- Other Core `root routes` remain separate and enter through ordinary links, Steps, or handoffs

## Rationale

Three required sections preserve the distinctions needed to confirm fit after loading, execute, and finish a recipe. Visible `description` values, tags, `route` meaning, and current user direction remain the pre-load selection surface. The sections are small enough to author manually, understand without repository-only documentation, and follow without turning useful work into form completion.

No current first-party Workflow uses a separate dependency section. Ordinary Markdown links and explicit reading or invocation instructions in Steps preserve visible relationships without another Workflow-only schema, parser, loading rule, or validation surface.

Descriptions and topical tags already provide pre-load selection. Fixed modes and phases did not add enough selection value to justify another vocabulary. Iteration, outputs, constraints, and prior-work suggestions remain expressible where the individual recipe needs them.

## Alternatives And Tradeoffs

- Free-form recipes remove all schema cost but make selection, execution, completion, and validation inconsistent
- The previous eight-section schema made every distinction explicit but duplicated information and encouraged mechanical compliance
- Fixed phases improved catalogue grouping but overlapped descriptions and topical tags while implying a lifecycle the Framework does not impose
- A mandatory Loop exposed repetition but forced linear recipes to describe its absence
- Mandatory Constraints and Outputs made those concepts visible but often repeated Steps, Goal, Skills, Directives, or Completion
- A dedicated dependency section would make unconditional preload relationships mechanically discoverable, but no current Workflow demonstrates enough value to justify the additional universal contract
- Workflow-local primitive trees improve package locality but turn recipes into mixed Core containers and require special loading, validation, update, and discovery rules

## Consequences

- Existing first-party Workflows move useful mode, constraint, loop, and output meaning into the three retained sections
- Phase tags and phase-aware validation are removed from current source and tooling
- Future deterministic validation should check the minimum shape without rejecting useful recipe-specific headings
- The frozen MVP may continue to parse and validate its former dependency section without making that syntax part of the current Framework or new CLI
- Direct execution remains valid, and no Workflow must be named in commentary or closeout merely for audit ceremony
- Handoffs identify a Workflow or active step when that information materially helps resumption rather than as a Workflow-specific requirement
- A Workflow earns a `route` through distinctive reusable execution value rather than by restating ordinary capable-agent behavior

## Authoritative Sources

- [Current Workflow contract](../../documents/framework/primitives/workflows.md)
- [Installed Workflows entrypoint](../../../../skills/use-workflow/references/open-forge/_open-forge.md)
- [Workflows maintenance contract](../../documents/maintenance/payload/agents/workflows.md)

## Historical Context

Historical redesign detail, including the former dependency section and TDD example, remains in [Workflow redesign](../../../archived/ideas/workflow-redesign.md). The requirements that triggered this simplification remain in the archived [Workflow overhaul inputs](../../../archived/ideas/2026-07-29_workflow-overhaul-inputs.md).

## Decision Relationships

- [Distinct Core primitive roles](core-primitives.md)
- [Routing surfaces](routing-surfaces.md)

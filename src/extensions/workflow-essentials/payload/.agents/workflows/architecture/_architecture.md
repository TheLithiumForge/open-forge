---
open-forge:
  description: Workflow for understanding, designing, and recording architecture changes
  tags: [OpenForge, Extension, Core, Workflow, Architecture, Design, Index]
---

# Architecture

Architecture is the workflow for understanding the current system, evaluating structural options, and defining a direction that future work can follow.

## Axioms

- Use this workflow when the user wants to design, evaluate, change, document, or review system structure.
- Do not propose architecture before mapping the current structure and accepted constraints.
- Prefer incremental designs that can be implemented, tested, and reviewed in clear slices.
- Keep rationale, behavior, reusable structures, and migration work in their owning routes.
- Generated `entries` are navigation and reserved load policy only.

## Required Skills

- `.agents/skills/workflow-primitives/loaded-context-check.md` - load relevant #Memory, #Workspace, #Directive, #Guidance, and #Pattern routes.
- `.agents/skills/architecture/map-current-system.md` - map structure, ownership, boundaries, data flow, and constraints.
- `.agents/skills/architecture/compare-architecture-options.md` - compare viable options by fit, cost, risk, reversibility, and migration path.
- `.agents/skills/architecture/frame-architecture-decision.md` - frame accepted rationale without hiding behavior in decision notes.
- `.agents/skills/architecture/slice-migration.md` - break architecture changes into implementation and verification slices.
- `.agents/skills/workflow-primitives/memory-routing.md` - route decisions, documents, patterns, guidance, and follow-up material.
- `.agents/skills/workflow-primitives/completion-handoff.md` - finish with status, verification needs, and resume context when useful.

## Steps

1. Load the required skills and relevant routed context.
2. Map the current system, including boundaries, ownership, data flow, constraints, and existing decisions.
3. Define the architectural goal, constraints, and success criteria.
4. Compare viable options and state rejected options when they matter.
5. Select or propose a direction with explicit tradeoffs and consequences.
6. Slice the direction into implementation, verification, migration, cleanup, and rollback work.
7. Route accepted rationale, reusable structures, and follow-up context to the right #Memory or #Core routes.

## Loop

This workflow is iterative until direction is accepted or deferred.

Repeat steps 2 through 6 when new constraints, source findings, risks, rejected options, or user decisions change the architecture. Stop when the user accepts a direction, asks for only analysis, or defers the decision.

## Outputs

- current-system map summary
- architectural goal and constraints
- compared options and tradeoffs
- selected or proposed direction
- rejected options when useful
- migration and verification slices
- proposed #Memory and #Core updates

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

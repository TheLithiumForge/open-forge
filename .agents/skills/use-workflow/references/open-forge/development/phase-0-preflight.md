---
open-forge:
  description: Produce a read-only assurance blueprint with architecture readiness, behavior classes, evidence, path boundaries, and review budget before mutation
  tags: [Workflow, Development, Phase, Preflight, Planning, Evidence, Review, Toolchain]
---

# Phase 0 - Preflight

## Goal

Give the primary owner a bounded read-only blueprint for one proposed high-consequence behavior change before task state, Git state, contracts, production, or executable evidence changes.

## Steps

1. Read the request, current authority, architecture, relevant implementation and tests, baseline, repository constraints, direct consumers, and public surfaces.
2. Confirm whether the work truly requires the assured profile. Identify any existing archetype or golden slice that would allow standard or derivative execution instead.
3. Map capability boundaries, dependency direction, placement, accepted contracts, behavior classes, safety invariants, compatibility obligations, and every unresolved decision that could change meaning or mutation scope.
4. Select the active toolchain, focused evidence, integration evidence, public scenario, and full gate from repository evidence. Do not assume a language, runner, command, or filter syntax.
5. Define expected paths, protected paths, direct integration neighborhood, allowed external effects, baseline strategy, useful freeze boundaries, implementation ownership, parallel lanes, correction boundary, and review budget.
6. Return the compact blueprint to the primary owner. Do not create or edit task state, branch state, contracts, code, tests, fixtures, snapshots, generated output, or external systems.

## Completion

- The execution profile and assurance cost are justified.
- Architecture readiness, accepted behavior classes, evidence tiers, toolchain, path boundaries, ownership, gates, and review budget are explicit.
- Existing archetypes and reusable foundations were considered before creating new structure.
- Settled direction and unresolved material decisions are distinct.
- No mutation occurred.

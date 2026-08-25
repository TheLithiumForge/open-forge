---
open-forge:
  description: Accelerate a related development program by extracting archetypes, golden slices, shared foundations, delta packets, parallel lanes, and batch gates
  tags: [Workflow, Development, Program, Orchestration, Planning, Implementation, Efficiency, Review, Batch]
---

# Program Development

## Goal

Deliver a sequence of related commands, features, migrations, or components without repeatedly paying foundation-level architecture, context, review, and verification cost for every item.

## Steps

1. **Map the program.** Record the accepted program outcome, dependency graph, shared foundations, safety boundaries, external constraints, task sequence, and final evidence. Identify which items are genuinely independent and which establish prerequisites.
2. **Classify archetypes.** Group items by contract shape, integration pattern, evidence strategy, source placement, and risk. Mark the first item of each archetype as a foundation slice and later items as derivative until evidence proves otherwise.
3. **Build the golden slice.** Apply the appropriate standard or assured workflow to the first item of each archetype. Capture the accepted architecture, placement map, contract pattern, implementation shape, test harness, public scenario, and known failure modes.
4. **Freeze shared foundations.** Promote only capabilities required by accepted consumers with identical meaning, at their nearest shared scope. Keep consumer semantics local. Resolve shared safety or compatibility primitives before dependent derivative slices.
5. **Generate delta packets.** For every remaining item, state only its differences from the golden slice: outcome, changed behavior, extra invariants, expected paths, protected paths, direct integration neighborhood, focused evidence, and escalation triggers.
6. **Run parallel lanes.** Execute non-overlapping derivative slices concurrently when their contracts and shared inputs are stable. Give each slice one owner and one integration point. Do not parallelize owners over the same mutable foundation.
7. **Review and gate by batch.** Run focused evidence per slice. Review a representative or triggered slice rather than every repetition, then run integration, public, and full gates at a coherent batch boundary. Escalate any slice that introduces a new public contract, shared foundation, safety boundary, or materially different behavior back to foundation treatment.
8. **Learn from yield.** Track accepted material review findings, rework, context reloads, repeated tool failures, human corrections, and critical-path time. Remove a recurring review with near-zero yield, encode repeated findings as tests or rules, and lower rigor only after comparable slices remain correct.
9. **Close the program.** Reconcile shared and local sources, run final program evidence, record residual risks and deferred archetypes, and preserve the golden examples and escalation rules for later work.

## Completion

- The program was decomposed into explicit foundations, archetypes, and derivative slices.
- Each archetype has a golden implementation and evidence pattern.
- Later slices used compact deltas rather than repeating broad discovery and architecture.
- Parallel work had non-overlapping ownership and explicit integration points.
- Review and full-gate cost was paid at meaningful risk and batch boundaries.
- Any new architecture, contract, safety, or compatibility need was escalated instead of forced through a derivative packet.

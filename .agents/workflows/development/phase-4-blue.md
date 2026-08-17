---
open-forge:
  description: Make one material production-structure improvement after Green without changing accepted behavior or evidence
  tags: [Workflow, Development, Phase, Blue, Refactoring, Readability]
---

# Phase 4 - Blue

## Goal

Inspect the green production structure and make one material improvement to clarity, locality, boundaries, duplication, or measured performance while preserving the frozen contract and green evidence.

## Steps

1. Read the Task, frozen contract, Green handoff, accepted evidence, source-structure rules, accepted architecture, allowed production paths, and forbidden surfaces supplied by the Mastermind.
2. Confirm the selected focused evidence is green before structural change.
3. Review names, module focus, navigability, responsibility locality, duplication, error handling, dependency direction, resource ownership, and measured hot paths where they matter to the Task.
4. Make only material structural improvements within accepted architecture. Keep files focused and prefer direct readable composition over abstraction without demonstrated consumers. If the inspection exposes a material architecture or behavior problem, return it to the Mastermind instead of changing behavior here.
5. Run focused evidence throughout, then the selected toolchain checks and scoped tests after the final change. The public scenario and full gate remain Mastermind-owned cycle steps.
6. Return the structural changes, before-and-after evidence, and consciously deferred improvements. When inspection finds no material improvement, return that conclusion with its evidence and make no change.

Do not change public behavior, callable contracts, tests, fixtures, snapshots, accepted expectations, Task state, or unrelated surfaces. Do not discover behavior, add speculative abstractions, or extend the pass for cosmetic preference.

## Completion

- The implementation is materially clearer, simpler, or safer than the Green version, or bounded inspection found no material structural defect and produced no change.
- The frozen contract, accepted behavior, and green evidence are unchanged.
- Selected focused checks remain green after the final structural result.
- No speculative abstraction, unrelated cleanup, or preference-only change entered this one Blue pass.

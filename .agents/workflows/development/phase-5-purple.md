---
open-forge:
  description: Make one material test-improvement pass after Green and Blue without changing behavior or expectation meaning
  tags: [Workflow, Development, Phase, Purple, Testing, Refactoring, Evidence]
---

# Phase 5 - Purple

## Goal

Make one material improvement to the clarity, locality, reuse, or maintainability of already-green tests and test support while preserving the accepted contract, production, observable behavior, and expectation meaning.

## Steps

1. Read the Task, frozen contract and expectations, Blue handoff, accepted evidence, allowed test surfaces, accepted architecture, and grounded improvement scope supplied by the Mastermind.
2. Confirm the selected focused evidence is green before changing test structure.
3. Edit only tests, fixtures, snapshots, and test-only helpers. Improve names, focus, navigability, responsibility locality, duplication, and support boundaries without adding, removing, weakening, or reinterpreting an expectation.
4. Keep independent contract inventories independent from production values. Share fixtures and test support only at their nearest demonstrated common scope.
5. Run focused evidence throughout, then the selected toolchain checks and scoped tests after the final change. The public scenario and full gate remain Mastermind-owned cycle steps.
6. Return the material improvements, before-and-after evidence, changed paths, and consciously preserved local exceptions to the Mastermind. Return a missing, incorrect, or incomplete expectation to Red instead of correcting it in Purple.

Do not edit contracts, production, accepted behavior, expectation meaning, Task state, or unrelated surfaces. Do not discover alternate behavior, add speculative test infrastructure, or extend the pass for cosmetic preference.

## Completion

- The already-green test surface is materially clearer, more local, or easier to maintain, or the bounded pass found no justified improvement and produced no change.
- The frozen contract, production, observable behavior, and expectation meaning are unchanged.
- Selected focused checks remain green after the final test-structure result.
- No speculative abstraction, new expectation, unrelated cleanup, or preference-only change entered this one Purple pass.

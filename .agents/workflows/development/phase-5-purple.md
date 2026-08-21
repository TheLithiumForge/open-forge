---
open-forge:
  description: Make one material test-improvement pass after Green and Blue without changing behavior or expectation meaning
  tags: [Workflow, Development, Phase, Purple, Testing, Refactoring, Evidence]
---

# Phase 5 - Purple

## Goal

Inspect the already-green tests, test projects, fixtures, and test support in one bounded improvement pass and make a material improvement to clarity, locality, reuse, or maintainability when justified, while preserving the accepted contract, production behavior, observable behavior, and expectation meaning.

## Steps

1. Read the Task, frozen contract and expectations, Blue handoff, accepted evidence, allowed test surfaces, accepted architecture, and grounded improvement scope supplied by the Mastermind.
2. Confirm the selected focused evidence is green before changing test structure.
3. Edit only tests, test projects and their directly affected solution or path references, fixtures, snapshots, test-only helpers or support, and the narrow production test-access declarations required by an accepted test-project rename or split. When the accepted Task explicitly identifies a production source as a test-only probe with no product consumer, Purple may remove that source from production and relocate its useful evidence to the test tier that owns the boundary. Improve names, focus, navigability, responsibility locality, duplication, and support boundaries without adding, removing, weakening, or reinterpreting an expectation or changing production behavior.
4. Keep independent contract inventories independent from production values. Share fixtures and test support only when at least two real test projects or fixtures need identical semantics, and place that support at their nearest demonstrated common scope.
5. Run focused evidence throughout, then the selected toolchain checks and scoped tests after the final change. The public scenario and full gate remain Mastermind-owned cycle steps.
6. Return the material improvements, before-and-after evidence, changed paths, and consciously preserved local exceptions to the Mastermind. Return a missing, incorrect, or incomplete expectation to Red instead of correcting it in Purple.
7. After integrated inspection, when Purple changes files, the Mastermind updates
   the authoritative Task progress in the same coherent commit and commits the
   accepted Purple phase before the public scenario or full gate continues. That
   Purple commit is separate from Blue when Blue also changed files. When Purple
   makes no change, its evidence is recorded in the next coherent phase or
   acceptance commit and no empty commit is created.
   When Purple changes files, the Mastermind compares the next starting tree
   with the Purple commit. When Purple reports no material change, continuation
   starts from the most recent mutating-phase commit plus the recorded Purple
   result. The Mastermind verifies that the contract and production behavior are
   protected.

Do not edit contracts, production behavior, accepted behavior, expectation meaning, Task state, or unrelated surfaces, and do not stage or commit phase work. The only production-file exceptions are the narrow test-access declaration required by an accepted test-project rename or split and an explicitly accepted test-only probe relocation with no product consumer. Do not discover alternate behavior, add speculative test infrastructure, or extend the pass for cosmetic preference. The Mastermind alone performs the Task update and phase commit at the boundary below.

## Completion

- The already-green test surface is materially clearer, more local, or easier to maintain, or the bounded pass found no justified improvement and produced no change.
- The frozen contract, production behavior, observable behavior, and expectation meaning are unchanged; any production edit is limited to the accepted test-access declaration for test-project identity or relocation of an explicitly accepted test-only probe with no product consumer.
- Selected focused checks remain green after the final test-structure result.
- The Mastermind inspected the actual Purple paths, diff, and evidence, recorded
  Task progress, and committed a separate accepted Purple phase before
  continuation, unless Purple made no change.
- No speculative abstraction, new expectation, unrelated cleanup, or preference-only change entered this one Purple pass.

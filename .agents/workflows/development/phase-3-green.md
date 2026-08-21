---
open-forge:
  description: Implement the smallest correct custom behavior that satisfies the frozen Red evidence
  tags: [Workflow, Development, Phase, Green, Implementation, Testing]
---

# Phase 3 - Green

## Goal

Make the complete affected Red evidence pass with the smallest correct implementation of the accepted callable contract and behavior.

## Steps

1. Read the Task, frozen callable contract, complete Red case matrix, allowed production paths, forbidden surfaces, accepted architecture, and baseline supplied by the Mastermind.
2. Implement only the production behavior required by the accepted case matrix and contract. Preserve existing system boundaries and use the simplest readable design that is correct. Do not make an unaccepted architecture or library choice.
3. Run the narrowest failing evidence after each meaningful change, then the selected focused toolchain checks and scoped tests. Classify failures before changing anything. The full gate remains a later cycle step.
4. Classify any remaining failure as implementation, expectation, contract, setup, environment, or unrelated state. Return non-implementation failures to the Mastermind rather than editing protected evidence or inventing behavior.
5. Return changed production paths, passing evidence, assumptions, and residual limitations to the Mastermind for integrated inspection.
6. After integrated inspection, the Mastermind updates the authoritative Task
   progress in the same coherent commit and commits the accepted Green phase
   before Blue mutates files. The Green commit makes the frozen Red evidence
   pass. The Mastermind compares Blue's starting tree with the Green commit and
   verifies that the Red expectations, fixtures, and snapshots remain
   unchanged.

Do not edit the callable contract, tests, fixtures, snapshots, accepted expectations, Task state, or unrelated surfaces, and do not stage or commit phase work. Do not use Green to compensate for incomplete Red evidence; return that gap to the Mastermind for the earliest applicable phase. The Mastermind alone performs the Task update and phase commit at the boundary below.

## Completion

- Every frozen Red expectation passes at the selected focused boundary.
- The implementation satisfies the accepted contract and case matrix with no unrelated scope.
- Protected contracts and evidence remain unchanged.
- The Mastermind inspected the actual Green paths, diff, and passing evidence,
  recorded Task progress, and committed the accepted Green phase before Blue
  continued.
- Remaining failures, if any, have an explicit classification and are outside an unjustified Green change.

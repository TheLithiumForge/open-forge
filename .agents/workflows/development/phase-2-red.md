---
open-forge:
  description: Create complete affected executable behavior evidence against the frozen Gray callable contract
  tags: [Workflow, Development, Phase, Red, Testing, Evidence]
---

# Phase 2 - Red

## Goal

Express all accepted affected behavior as executable evidence that fails because the frozen callable contract does not yet have its production implementation. The Gray, Red, and Green subcycle is the behavior-discovery boundary and must expose the affected behavior defects before Blue or Purple.

## Steps

1. Read the Task, frozen callable contract, accepted behavior and architecture, baseline, allowed test surfaces, and evidence requirements supplied by the Mastermind.
2. Build the complete affected case matrix at the cheapest sufficient tiers. Include success, boundary, invalid-input, safety, failure, and externally visible behavior justified by the Task.
3. Edit only tests, test-local fixtures, and snapshots. Prefer real functions, filesystems, processes, and observable state over interaction mocks when the accepted boundary can be exercised safely.
4. Run the selected focused toolchain checks and evidence needed by the touched surfaces. Prove that each new failure represents missing accepted behavior rather than a broken module, dependency, configuration, syntax error, environment, or unrelated baseline defect.
5. Return the case matrix, failing evidence, changed paths, and any contract gap to the Mastermind. Do not defer missing behavior coverage to Blue or Purple.

Do not edit production code, the callable contract, accepted expectations, Task state, or unrelated surfaces. Do not change evidence to remove a valid Red failure, and do not replace complete affected Red evidence with a later full gate.

## Completion

- The accepted affected behavior has proportionate executable coverage, including every justified case class.
- New evidence executes and fails for intended missing behavior, with setup, environment, and unrelated failures distinguished.
- Production code, the frozen contract, and accepted expectation meaning remain unchanged.
- The Mastermind can send a complete behavior matrix to Green without requiring Blue or Purple to discover behavior.

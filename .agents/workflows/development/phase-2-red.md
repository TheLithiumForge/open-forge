---
open-forge:
  description: Freeze accepted behavior classes and representative failing evidence before production mutation
  tags: [Workflow, Development, Phase, Red, Testing, Evidence]
---

# Phase 2 - Red

## Goal

Express accepted behavior as executable evidence that fails for the intended missing implementation, while freezing expectation meaning before production mutation.

## Steps

1. Read the accepted architecture, callable contract, behavior matrix, baseline, expected test paths, protected production paths, evidence tiers, and focused commands.
2. Cover success, boundary, invalid-input, safety, failure, regression, and externally visible behavior at the cheapest sufficient tiers.
3. Before Green, require complete affected evidence for every accepted behavior row and material behavior class. New or changed behavior must fail for the intended missing implementation. Preserved behavior may already pass; record that result as preservation evidence. Safety, destructive, compatibility, regression, boundary, failure, and externally visible behavior must be explicit before production mutation. Representative-only Red evidence belongs to a Standard profile and must not be used to shorten this Assured phase.
4. Edit only tests, test-local fixtures, snapshots, and directly required test support. Keep independent contract inventories independent from production constants.
5. Prove that each failure comes from missing accepted behavior rather than syntax, setup, dependency, environment, or unrelated baseline defects.
6. Record the frozen behavior matrix, changed paths, intended failures, and exact baseline or snapshot. Return any missing evidence or contract gap before Green.

## Completion

- Every accepted affected behavior row and material behavior class has frozen expectation meaning and an observed result. Missing behavior fails for the intended reason; preserved behavior has passing evidence.
- Safety, destructive, compatibility, and regression boundaries are explicit before production changes.
- Production and callable contracts remain unchanged.
- Green receives a complete accepted behavior matrix and exact protected test boundary.

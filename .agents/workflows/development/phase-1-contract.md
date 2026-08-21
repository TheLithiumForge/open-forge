---
open-forge:
  description: Define and freeze the Gray callable contract for one authorized custom-behavior Task
  tags: [Workflow, Development, Phase, Contract, Interface]
---

# Phase 1 - Gray Contract

## Goal

Define and expose the smallest complete callable production surface required by the accepted Task without implementing its domain behavior or expectations.

## Steps

1. Read the Task, accepted behavior and architecture, exact scope, baseline, allowed paths, forbidden surfaces, and required evidence supplied by the Mastermind.
2. Define focused production interfaces, named values, discriminated results, callable signatures, and dependency boundaries within the accepted direction. Reuse accepted shared contracts instead of creating command-local alternatives. Return any unresolved material contract or architecture decision to the Mastermind.
3. Add only the compilable production skeleton needed to expose that surface. Missing behavior must fail explicitly as not implemented rather than returning plausible placeholder data.
4. Run the selected focused toolchain checks needed to prove that the callable surface is coherent. Do not substitute a hard-coded command or full gate for the selected evidence.
5. Return changed paths, exact signatures, evidence, assumptions, and unresolved contract questions to the Mastermind, who freezes the accepted callable contract.
6. After integrated inspection, the Mastermind updates the authoritative Task
   progress in the same coherent commit and commits the accepted Gray phase
   before Red mutates files. That Gray commit freezes the callable surface. The
   Mastermind compares Red's starting tree with the Gray commit and verifies
   that Gray production and the frozen contract are protected.

Do not implement domain behavior, author tests or snapshots, weaken strictness, choose an unaccepted architecture or library, edit Task state, stage or commit phase work, or push. Do not turn a contract gap into a Green implementation. The Mastermind alone performs the Task update and phase commit at the boundary below.

## Completion

- The callable surface is focused, strict, compilable, and independently inspectable within accepted architecture.
- Missing behavior is explicit and no domain behavior, tests, snapshots, or unrelated surfaces changed.
- Selected focused evidence shows that the callable surface is coherent.
- The Mastermind inspected the actual Gray paths, diff, and evidence, recorded
  Task progress, and committed the accepted Gray phase before Red continued.
- The Mastermind has enough evidence to accept and freeze the contract or return an unresolved material decision before Red begins.

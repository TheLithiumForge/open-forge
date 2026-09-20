---
open-forge:
  description: Candidate CLI follow-up for contract editorial gates, output budgets, Route rebuild decisions, and interaction durability
  tags: [Memory, Working, CLI, Task, Potential, Contextual, Contracts, Presentation]
---

# Candidate — CLI Contract Editorial And Route Follow-up

## Status

Candidate; not an active Task. The existing Task 30 G4, Task 31, and G1 state
boundaries must be checked first so this record does not duplicate execution.

## Why this exists

The retrospective showed that several costly defects were faithfully built from
accepted contracts. The missing control was an editorial gate before a
contract became implementation work: what does a user see, is the output worth
its size, which values are stable, and which question is the command actually
answering? The audit also left the Route family rebuild and the interaction-
versus-durability imbalance outside a current acceptance boundary.

## Candidate scope

- Define one presentation/data-model contract for all commands: default detail
  tier, selection before rendering, healthy/zero/not-applicable suppression,
  severity and subject ordering, `Next` usability, context echo rules, output
  budget, JSON projection, and stable fields.
- Add a pre-implementation editorial checklist that records the user question,
  expected answer, output-size budget, stream/exit meaning, and the evidence
  boundary. Keep current defaults unchanged until the checklist is accepted.
- Decide whether the Route command family should be rebuilt behind its existing
  public contracts after G1/G4, or whether the documented local differences
  justify retaining separate implementations. Route Move/Remove finding codes
  and JSON must remain distinct unless a new contract explicitly changes them.
- Resolve the wizard/confirmation/help/error journey as a product contract,
  including showing a plan before confirmation and making identity/next-action
  errors directly actionable.
- Measure and correct the interaction-versus-durability balance without
  discarding durable recovery, ownership, or safety evidence merely to reduce
  visible output.

## Constraints and promotion

The historical presentation decision to retain diagnostic kinds, JSON, and exit
behavior is a current decision boundary to recheck, not an invitation to delete
finding categories. Promotion requires maintainer acceptance of the contract,
reviewed before/after snapshots, and an explicit dependency on Task 30 G1 and
G4 plus the Task 31 M1/M3 stop boundaries. This candidate does not authorize
implementation or public output changes.

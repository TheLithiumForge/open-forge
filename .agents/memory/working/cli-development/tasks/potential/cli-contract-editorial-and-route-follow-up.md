---
open-forge:
  description: Candidate CLI follow-up for contract editorial gates, output budgets, Route rebuild decisions, and interaction durability
  tags: [Memory, Working, CLI, Task, Potential, Contextual, Contracts, Presentation]
---

# Candidate — CLI Contract Editorial And Route Follow-up

## Status

Candidate, not an active Task. Tasks 30 and 31 are closed. Recheck each proposal
against current contracts and unfinished Tasks before selecting it.

## Why this exists

The retrospective showed that several costly defects were faithfully built from
accepted contracts. The missing control was an editorial gate before a
contract became implementation work: what does a user see, is the output worth
its size, which values are stable, and which question is the command actually
answering? The audit also left the Route family rebuild and the interaction-
versus-durability imbalance outside a current acceptance boundary.

## Candidate scope

- Assess remaining gaps against the existing shared reporting and presentation
  contracts. Four detail levels, format selection, selection before rendering,
  and the shared result envelope already exist. Output budgets and any changed
  suppression, ordering, or next-action rule need their own accepted scope.
- Add a pre-implementation editorial checklist that records the user question,
  expected answer, output-size budget, stream/exit meaning, and the evidence
  boundary. Keep current defaults unchanged until the checklist is accepted.
- Decide whether the Route command family should be rebuilt behind its existing
  public contracts, or whether the documented local differences
  justify retaining separate implementations. Route Move/Remove finding codes
  and JSON must remain distinct unless a new contract explicitly changes them.
- Reproduce remaining help, identity, and error journey problems. Plans already
  precede confirmation. Task 72's bounded wizard viewport is locally accepted.
  The wider Task 39 error audit remains paused rather than complete.
- Measure and correct the interaction-versus-durability balance without
  discarding durable recovery, ownership, or safety evidence merely to reduce
  visible output.

## Constraints and promotion

The historical presentation decision to retain diagnostic kinds, JSON, and exit
behavior is a current decision boundary to recheck, not an invitation to delete
finding categories. Promotion requires maintainer acceptance of the contract,
reviewed before/after snapshots, and current scope and ownership boundaries.
The completed G1/G4 and Task 31 packets are historical evidence. This candidate does not authorize
implementation or public output changes.

## Other questions retained from the historical analyses

The [readiness analysis](../../../../archived/cli-development/analysis/one-zero-release-readiness.md)
proposes a stable 1.0 compatibility promise for command names and flags,
statuses and exits, schemas, frontmatter, workspace state, customization, and
migrations. It also recommends qualification of the exact future stable
candidate across matching hosts, packages, archives, and checksums, plus public
stable-channel smoke checks and an upgrade from the published beta. Prior beta
evidence does not qualify a different stable candidate. Verified release notes,
limitations, and a fresh dependency review are further recommendations. These
remain unaccepted future proposals, not new release gates or publication
authorization.

The [dogfood audit](../../../../archived/cli-development/analysis/cli-experience-audit/repository-dogfood-and-configuration.md)
also records a dirty-tree Git advisory as historical direction. Its current
implementation and acceptance disposition were not verified in the currency
audit. Revalidate that question before treating it as implemented, rejected,
or newly approved. No new Git behavior is selected here.

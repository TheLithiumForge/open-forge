---
open-forge:
  description: Provide an optional independent read-only review of a delivery cycle and route material findings to the earliest invalidated phase
  tags: [Workflow, Development, Phase, Review, Quality, Evidence]
---

# Phase 6 - Whole-Task Review

## Goal

Give the Mastermind an independent, read-only assessment when a fresh perspective is useful, identifying any correctness, evidence, or structural finding that requires the one permitted correction cycle before final acceptance.

## Steps

1. Read the accepted outcome and constraints from the Task, accepted contracts and architecture, baseline, complete branch diff and commits, phase evidence, Purple result, public-surface scenario, full-gate evidence, and applicable source rules without inheriting the implementer's conclusion. The Mastermind supplies a clean first-pass packet that identifies the accepted Task sections but withholds prior reviewer or advisor rationale and Observation comparisons; do not read those recorded conclusions until the independent return is complete.
2. Review intent coverage, callable contracts, expectation completeness, production correctness, safety, error behavior, source structure, test structure, duplication, maintainability, and evidence credibility.
3. Inspect or reproduce selected read-only evidence where it materially tests a concern. Do not fix findings, edit Task state, or run the final gate.
4. Classify each finding as blocking, residual risk, optional improvement, or preference. A blocking finding cites exact evidence, states its material consequence, and identifies the earliest invalidated applicable phase.
5. Report explicitly when no blocking finding remains and provide residual risk or an unproved boundary. Include the conclusion, evidence and reasoning, strongest viable option or counterargument, material tradeoffs, and what would change the conclusion. The Mastermind groups the report with its integrated inspection, persists a sanitized rationale in the Task and any matching Emerging Observation, compares it with prior conclusions only after the independent pass, and decides whether the exceptional correction cycle is worth its cost.

Applicable repository material defines established cosmetic choices. Do not reopen a phase for an equivalent design, speculative abstraction, or personal preference that violates no accepted source and has no material consequence. Do not fix findings or reinterpret an unsettled product decision in this optional review.

## Completion

- When used, the review is independent, evidence-backed, and read-only.
- Every finding names its impact, evidence, classification, and earliest invalidated applicable phase.
- Blocking findings are distinguished from residual risk and optional improvement, and preference-only findings do not trigger a cycle.
- The Mastermind can decide whether to use the single correction cycle or finalize. Omitting this optional review does not remove the required Mastermind final review.
- The review rationale is complete enough for the Mastermind to record longitudinal convergence or divergence without exposing hidden orchestration or environment identifiers.

---
open-forge:
  description: Preserve reviewer conclusions, reasoning, alternatives, tradeoffs, and longitudinal comparison as sanitized evidence
  tags: [LoadNow, Core, Directive, Review, Evidence, Reasoning, Tradeoff, Observation, Dogfooding]
---

# Review Evidence

## Instructions

- Every reviewer, writing reviewer, improvement reviewer, challenger, advisor, or council return must include a concise conclusion, the evidence and reasoning that produced it, the strongest viable option or counterargument, material tradeoffs, and what would change the conclusion. Preserve a clear no-finding conclusion with the same rationale shape when no correction is recommended.
- Keep first-pass independent reviews independent. Do not prime a fresh reviewer with earlier conclusions merely to measure agreement. After the return, the Mastermind compares it with relevant prior evidence and records meaningful convergence, divergence, repeated recommendations, changed assumptions, and newly decisive facts.
- The Mastermind persists a sanitized review rationale in the active Task or review record. When the conclusion, failure mode, correction, or tradeoff may be reusable, surprising, costly to rediscover, or useful for longitudinal comparison, create or extend the matching Emerging Observation before handoff or closeout.
- Record the role and bounded review scope. Do not record provider, model, AI, runtime-profile, session, task, review, handoff, or hidden orchestration identifiers; personal, user, or machine identifiers; secrets or tokens; local absolute paths; or incidental environment fingerprints. Keep only the normalized repository-relative evidence needed to understand and reproduce the conclusion.
- Repeated agreement is promotion evidence, not automatic authority. Propose the smallest matching Pattern, Directive, Workflow, agent instruction, or implementation change only after recurrence and impact justify it; preserve disagreement and counterexamples rather than manufacturing consensus.
- Do not create a second review workflow or one observation per invocation. Extend an existing Observation when scope and meaning align, and keep operational Task evidence distinct from reusable Emerging evidence.

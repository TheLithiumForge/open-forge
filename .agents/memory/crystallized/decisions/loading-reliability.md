---
open-forge:
  description: Reliability-critical context loads unconditionally and early; conditional context must be cheap to skip and cheap to recover from
  tags: [Memory, Decision, CurrentTruth, Loading, Routing, Reliability]
---

# Loading Reliability

Accepted 2026-07-08 from the dogfood evidence synthesis across rounds v2 through v4.

- Anything reliability-critical must be loaded unconditionally and early; anything conditional must be cheap to skip and cheap to recover from. This principle answers most "when should X load" questions.
- No load-policy tag is mechanically enforced. Compliance is always agent self-enforcement, so every loading design must make compliance cheaper than skipping; never assume tooling forces a load.
- Unconditional category-level rules ("read everything beside this entrypoint") outperform per-entry judgment calls. The workspace-wide directives rule was the only mechanism with full compliance in every dogfood round.
- Concrete, colocated, backtick-quoted path lists outperform prose lists of cross-referenced paths.
- Loading follow-up material early, at selection time, measurably improves late compliance such as closeout writes. Deferring a load to "when it becomes relevant" is the observed failure pattern.
- Wording stays imperative to the agent without implying tool behavior: prefer "read X before Y" over "X must be loaded". The imperative remains mandatory for compliant agents.
- Reuse the same structure, rules, and shapes everywhere so agents can trust the structure instead of carrying decision overload.

Evidence: the evaluation syntheses routed by `.agents/memory/crystallized/documents/evaluations/_evaluations.md`, with detailed pre-harness reports under `benchmarks/results/pre-harness/`.

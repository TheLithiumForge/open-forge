---
open-forge:
  description: Reliability-critical context loads unconditionally and early; conditional context must be cheap to skip and cheap to recover from
  tags: [Memory, Decision, CurrentTruth, Loading, Routing, Reliability]
---

# Loading Reliability

Accepted 2026-07-08 from the dogfood evidence synthesis across rounds v2 through v4.

- Anything reliability-critical must be loaded unconditionally and early; anything conditional must be cheap to skip and cheap to recover from. This principle answers most "when should X load" questions.
- No load-policy tag is mechanically enforced. Compliance is always agent self-enforcement, so every loading design must make compliance cheaper than skipping; never assume tooling forces a load.
- The active route must settle scope before content is loaded. The baseline root directive route makes every direct root file workspace-wide; a selected child directive route makes every direct file in that child binding within the selected scope. A second file-internal applicability gate regressed the simple top-to-bottom model without new evidence and is not used.
- The accepted dogfood result remains literal: unconditional root directive loading was the only tested mechanism with full compliance across every measured session. Conditional routes must still be cheap to select from visible path, description, tags, and ancestor meaning before their contents are opened.
- Concrete, colocated, backtick-quoted path lists outperform prose lists of cross-referenced paths.
- Loading follow-up material early, at selection time, measurably improves late compliance such as closeout writes. Deferring a load to "when it becomes relevant" is the observed failure pattern.
- Wording stays imperative to the agent without implying tool behavior: prefer "read X before Y" over "X must be loaded". The imperative remains mandatory for compliant agents.
- Reuse the same structure, rules, and shapes everywhere so agents can trust the structure instead of carrying decision overload.
- Treat the complete routed #KeepInMind catalogue as baseline-loaded binding context. Recheck it at loader entry, context restoration, meaningful phase transitions or handoffs, and closeout; a broken route chain is a structural defect to repair, not a reason to weaken the contract.

Evidence: the evaluation syntheses routed by `.agents/memory/crystallized/documents/evaluations/_evaluations.md`, with detailed pre-harness reports under `benchmarks/results/pre-harness/`.

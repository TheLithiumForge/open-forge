---
open-forge:
  description: Reliability-critical context loads unconditionally and early; conditional context must be cheap to skip and cheap to recover from
  tags: [Memory, Decision, CurrentTruth, Loading, Routing, Reliability]
---

# Loading Reliability

Accepted 2026-07-08 from the dogfood evidence synthesis across rounds v2 through v4 and revised 2026-07-20 for the relative-link route format.

- Anything reliability-critical must be loaded unconditionally and early; anything conditional must be cheap to skip and cheap to recover from. This principle answers most "when should X load" questions.
- No load-policy tag is mechanically enforced. Compliance is always agent self-enforcement, so every loading design must make compliance cheaper than skipping; never assume tooling forces a load.
- The active route must settle scope before content is loaded. Every direct directive file carries #LoadNow. The baseline root directive route therefore reads workspace-wide direct files through ordinary parent traversal; loading a selected child directive route first establishes scope, then reads its direct files through the same rule. A second file-internal applicability gate regressed the simple top-to-bottom model without new evidence and is not used.
- The accepted dogfood result remains literal: unconditional root directive loading was the only tested mechanism with full compliance across every measured session. Conditional routes must still be cheap to select from visible path, description, tags, and ancestor meaning before their contents are opened.
- Concrete, colocated path lists outperform prose lists of cross-referenced paths. Relative Markdown links preserve that directness while making destinations clickable and graphable through standard tools.
- Loading follow-up material early, at selection time, measurably improves late compliance such as closeout writes. Deferring a load to "when it becomes relevant" is the observed failure pattern.
- Wording stays imperative to the agent without implying tool behavior: prefer "read X before Y" over "X must be loaded". The imperative remains mandatory for compliant agents.
- Reuse the same structure, rules, and shapes everywhere so agents can trust the structure instead of carrying decision overload.
- Treat every routed #KeepInMind result and its visible #LoadNow closure as continuity context. This does not include unrelated descendants. Recheck the set at loader entry or resume, actual context restoration, handoff, and closeout, plus transitions where its follow-ups may have changed; a broken route chain is a structural defect to repair, not a reason to weaken the contract.
- `open-forge load --bodies` may batch the loader, transitive visible #LoadNow closure, every routed #KeepInMind result with its own visible #LoadNow closure, and adjacent overwrites when the CLI is available. Use it at every required #KeepInMind boundary. It is an optimization over the same complete plain-file traversal, not a separate source of loading truth. (revised 2026-07-27)

Evidence: the evaluation syntheses routed by `.agents/memory/crystallized/documents/evaluations/_evaluations.md`, with detailed pre-harness reports under `benchmarks/results/pre-harness/`.

The [current routing loading contract](../documents/framework/routing/loading.md) expresses the accepted result.

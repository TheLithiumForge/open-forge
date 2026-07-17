---
open-forge:
  description: Cross-run synthesis of the v7 benchmark generation - first validation of the new workflow shape, narrower-scope precedence proven at scale, scope control failing at the tool boundary
  tags: [Memory, Document, Record, CurrentTruth, Evaluation, Evidence, Synthesis, Benchmark]
---

# v7 Synthesis

Generation: v7, 2026-07-10. Four runs, Codex GPT-5, no variable overlays, framework at commit e523e90 plus the then-staged workflow-shape changes. Raw per-run reports: `benchmarks/results/2026-07-10-seed-*-codex-gpt5-no-variables.md`. This is the first generation run through the reproducible harness and the first against the accepted workflow shape (`.agents/memory/crystallized/decisions/workflow-shape.md`).

## Headline

The strongest evidence round so far. Routing carried non-obvious seeded opinions into implementations with high fidelity, the new workflow contract was read and honored, and the single most important planted test - surfacing a spec contradiction instead of silently resolving it - passed. The one systematic failure is operational, not informational: scope control breaks at the editing-tool boundary.

## What The Framework Got Right

- New workflow contract validated. Seed-1's worker explicitly acknowledged that `worker-implementation.md` declares no Required Routes - the "none" semantics read and applied on first exposure. The rubric line scoring "Required Routes read before Step 1, or none acknowledged" worked as a measurement.
- Narrower-scope precedence proven where it matters. Seed-3's worker detected the deliberately planted contradiction (product vision requires `remove`, the immutable-ledger decision forbids hard deletes, both #CurrentTruth), named the exact conflicting routes in debrief, and resolved it correctly - CLI `remove` preserved, implemented as a reversal endpoint. That is the loader's precedence axiom plus conflict-reporting working unprompted in a cold workspace.
- Seeded opinions beat agent instinct. Seed-2's counter-instinct decisions (append-only NDJSON, immutable amend chains, no delete command, hand-rolled sortable ids, no date libraries, Monday-covers-Friday-plus-weekend) all reached the implementation, verified by independent code inspection - route-carried context overrode defaults an agent would otherwise reach for.
- Memory growth held at scale. Seeds 2 and 3 scored full marks: handoff, session, and observation written with indexes updated. Seed-3 also rechecked #KeepInMind routes before finalizing.
- Product fidelity was 2 of 2 on every build seed, with verification run independently by the orchestrator, not taken from worker claims.

## What Failed

- Scope control at the tool boundary, four of four occurrences (three v7 build seeds plus the earlier v6 worker): the first `apply_patch` call resolved paths against the parent repos folder and created an accidental sibling directory before self-correction. Routing was not the cause - the same workers self-reported the violation unprompted. Prose scope instructions lose to operational tool defaults. Tracked as `.agents/memory/emerging/observations/2026-07-10_patch-scope-violations.md`; fix is backlog item 1.
- Vision mode was under-equipped, not under-performed. Seed-0's weak process scores (no workflow used, no promotion confirmation, candidate ideas embedded in crystallized truth, no handoff) trace to the harness shipping only a build workflow - there was no vision workflow to select. The constraints the worker missed are exactly the ones the reworked workflow-essentials vision workflow now carries. Fix is backlog item 2.
- #KeepInMind closeout compliance is real but not uniform: seed-3 performed the literal recheck, seed-2 admitted skipping it while still writing closeout memory. Better than the pre-rework rounds; not yet a guarantee.
- Recurring smell: workers hand-edit generated index regions because benchmark workspaces carry no CLI. Either document the manual fallback as acceptable or ship the index tool into test workspaces.

## Scoring Texture

Across the three build seeds, Directive Compliance landed at 1 (the scope violation), while Memory Growth, Communication, and Product Fidelity trended 2. Seed rubric totals were near-ceiling on seeds 2 and 3, including every temptation check. Seed-0 scored low on process dimensions for the harness-gap reason above; its product output (the brief) was still rated cold-session usable.

## Consequences Already Routed

- Observation: `emerging/observations/2026-07-10_patch-scope-violations.md` (recurrence-driven, four occurrences).
- Backlog: operational scope-control fix (item 1), harness vision workflow (item 2), dev-workflow seeded validation (item 3).
- Results placement confirmed and routed: per-run reports stay in `benchmarks/results/`; this route holds the synthesis.

## What v8 Should Test

Run seed-0 with a vision workflow installed, keep everything else fixed, and see whether the promotion-confirmation and candidate-routing misses disappear - that isolates the harness-gap hypothesis. Add the scope-control directive as a variable overlay on one build seed before making it default, so its effect is measured rather than assumed. Include the dev-workflow extension in one run.

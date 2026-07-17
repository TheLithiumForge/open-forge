---
open-forge:
  description: Cross-run synthesis of the v10 A/B generation - the closeout command moved recheck compliance from ~50% to 3 of 3, and the printout proved to be the compliance surface
  tags: [Memory, Document, Record, CurrentTruth, Evaluation, Evidence, Synthesis, Benchmark]
---

# v10 Synthesis

Generation: v10, 2026-07-10. Four runs, Codex GPT-5. Build seeds carried `variable-closeout-command`; seed-0 carried `workflow-essentials`. Pre-registered against the gen8/gen9 no-variables baseline (roughly 50% closeout compliance). Raw reports: `benchmarks/results/2026-07-10-seed-*-gen10-*.md`.

## The A/B Verdict: The Overlay Works, And Says Why

- First-attempt #KeepInMind recheck: three of three build seeds ran the exact command (`open-forge find --tag KeepInMind --bodies`) before finishing, against the ~50% baseline. Seed-2 delivered the cleanest closeout ever recorded: command, session, handoff, grounded observation, reindex, and a second command run to confirm the new observation appeared in the printout.
- Full follow-up loop: two of three. Seed-1 ran the command, wrote and indexed an observation, but skipped the session record.
- The causal story is sharp: everything the printout surfaced was done, three of three; the single missed item was the one thing the printout does not surface. Sessions routes are not #KeepInMind-tagged, so session records never appear in the closeout output. The printout is the compliance surface.
- Pre-registered criterion met. The tool-assisted loader wording was promoted to core on 2026-07-11: the recheck axiom now names the command, explicitly optional ("when the CLI is available"), with a new loader-descriptor check that tool references stay tool-assisted and never load-bearing.

## The Discovery Behind The Residual Gap

The recurring "missing session record" across gen8-v10 turns out to be partly a contract gap, not an agent failure: the sessions entrypoint never contained a write obligation. Handoffs and observations have explicit write triggers; sessions only had a fallback rule ("write it as session context when it belongs nowhere else"). The rubric has been scoring an unwritten expectation - workers complied with the contract as written. `variable-sessions-keepinmind` now carries both candidate fixes (the #KeepInMind tag so sessions appear in the closeout printout, plus an explicit write axiom) for the v11 A/B against gen10 as the control.

## Seed-0: The Framework Lever Is Exhausted, By Design

- With workflow-essentials installed, the worker faced two vision workflows and selected the harness one as the narrower scope - the first live exercise of narrower-workflow preference, handled correctly and stated in debrief.
- The pack still paid off through relevance routing: the vision skill references (clarify-intent, shape-mvp, fit-and-risk) were opened before questioning, and breadth improved measurably - non-goals elicited (0 to 2 versus gen9), tradeoffs sharper, curveball handling improved to partial (challenged timers, still offered duration capture without probing why).
- The recall-window question was missed for the fourth consecutive generation, now surviving the last untested framework lever. Per the pre-registration, discovery depth of this kind is conclusively model territory; seed-0 is the sharpest instrument for the model comparison.

## Watch Item

Seed-1 logged a read-only discovery command against the drive root outside the workspace - self-flagged, no writes, correctly capped by the first-attempt scope rule. One occurrence after two clean generations reads as noise; watch, do not react.

## What v11 Should Test

1. Sessions A/B: seeds 1-3 with `variable-closeout-command` plus `variable-sessions-keepinmind` against gen10 as control (single-variable delta). Success: session records appear in the closeout printout and get written, closing the full loop three of three. On success, promote the tag and the write axiom to the core payload.
2. Model comparison when a second model is available: same seeds, no variables, Fable or Sonnet against the codex-gpt5 baseline; seed-0's curveball and recall-window misses are the discriminators.

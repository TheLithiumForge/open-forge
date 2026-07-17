---
open-forge:
  description: Cross-run synthesis of the v8 benchmark generation - scope-control and vision-workflow fixes validated, closeout recheck now the isolated persistent gap
  tags: [Memory, Document, Record, CurrentTruth, Evaluation, Evidence, Synthesis, Benchmark]
---

# v8 Synthesis

Generation: v8, 2026-07-10. Four runs, Codex GPT-5, no variable overlays, same seeds as v7, with the 2026-07-10 harness fixes applied (worker-root pinning, verify-cwd scope axiom, first-attempt rubric scoring, index-regeneration directive, worker-vision workflow). Raw reports: `benchmarks/results/2026-07-10-seed-*-gen8-no-variables.md`. This generation was designed as a measurement of the v7 fixes; both hypotheses confirmed.

## Fixes Validated

- Scope control: zero violations in three build seeds, against three of three in v7. Directive Compliance scored 2 across all builds. The diagnosis (tool working-directory default, not routing) and the operational fix (verify-cwd before first write, pinned absolute roots) are confirmed. This also completes the framework's first full observation lifecycle: noticed as recurrence, recorded, acted on, validated one generation later.
- Vision workflow: seed-0 process scores jumped where the workflow carries them - vision process 1 to 2, promotion confirmation 1 to 2, candidate-ideas routing 0 to 2, decisions with rationale 0 to 2, and #KeepInMind was rechecked. The v7 "harness gap, not agent failure" hypothesis was correct.
- Index regeneration: seed-1 used `open-forge index` instead of hand-editing generated regions; the v7 smell did not recur.
- Report privacy: workspace-relative paths held; no machine paths in any artifact.

## The Isolated Remaining Gap: Closeout Discipline

With scope and vision fixed, one weakness now stands alone across seeds:

- #KeepInMind closeout recheck: seed-0 did it, seed-2 admitted skipping it, seed-3 loaded observations late. Two of four clean.
- Closeout memory depth: seeds 1 and 2 wrote only a handoff (no session or observation for meaningful implementation work); seed-3 wrote all three.
- Depth decay in the #LoadNow chain: seed-3 skipped the emerging analysis and ideas child entrypoints even though the chain reaches them - compliance thins with routing depth at scale.

Every structural and wording lever has now been applied to this gap; per the loading-reliability decision, the remaining lever is friction reduction - one command that makes the closeout recheck cheaper than skipping it.

## Judgment Signals (Not Framework Gaps)

- Seed-0's time-tracking curveball was swallowed again (0 in both generations) and the Monday/weekend question was never asked. These discriminate model judgment, not routing; keep seed-0 as a judgment test and use the curveball for model comparisons.
- Seed-3's worker overstated fidelity in debrief (import is temp-file rename, not strict append; the CLI parses money client-side); independent orchestrator inspection caught both - the verification design doing its job.

## Consequences Routed

- The patch-scope observation updated to validated; its generalizable lesson (operational rules beat prose scope at tool boundaries) is a promotion candidate for the future defaults extension.
- Results filename convention now includes the generation component.
- Backlog re-prioritized: `open-forge context` with a closeout tier is the top framework lever.

## What v9 Should Test

Model comparison on the stable harness (same seeds, Fable and Sonnet against the GPT-5 baseline - the curveball and closeout discipline are the discriminators), and, once `open-forge context` exists, an A/B of closeout compliance with and without it - the same experiment shape that validated the dump tool in v4.

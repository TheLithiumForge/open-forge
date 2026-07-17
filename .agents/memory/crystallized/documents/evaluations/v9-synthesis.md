---
open-forge:
  description: Cross-run synthesis of the v9 benchmark generation - a same-configuration replication of v8 that confirms fidelity and scope stability and quantifies the closeout compliance ceiling
  tags: [Memory, Document, Record, CurrentTruth, Evaluation, Evidence, Synthesis, Benchmark]
---

# v9 Synthesis

Generation: v9, 2026-07-10. Four runs, Codex GPT-5, no variable overlays, same framework baseline as v8. This generation ran before the closeout command overlay existed, which makes it something more useful than the planned model comparison: a same-configuration replication. Raw reports: `benchmarks/results/2026-07-10-seed-*-gen9-no-variables.md`.

## What Replication Confirms

- Product fidelity is now boringly reliable: every build seed scored 2 for the third consecutive generation, verified independently each time. Seed-2 preserved every counter-instinct opinion again (append-only NDJSON, immutable amendments, no delete, hand-rolled ids, local-window-over-UTC math with injected clock and zone); seed-3 surfaced and resolved the planted remove-versus-immutability conflict again, cleanly, with the tension recorded in the handoff. Whether routes carry semantics is a settled question.
- Scope control held for the second straight generation: zero violations anywhere. The v7 fix is durable, not lucky.
- Selective loading improved at scale: seed-3 read the scoped package patterns and skipped irrelevant routes with justification, and did not repeat v8's missed emerging roots. Seed-2's debrief gave a deliberate per-family loading rationale.
- Vision process mechanics held: seed-0 used `worker-vision.md`, asked acceptance before promotion, routed candidate ideas to emerging and decisions with rationale, and this time also wrote observation, session, and handoff.

## What Replication Quantifies

- Closeout discipline is stochastic, not trending: across gen8 plus gen9 (eight runs), roughly half executed the full closeout (explicit #KeepInMind recheck plus session record). The misses wander between seeds and between generations - seed-2 missed the recheck in gen8 and was perfect in gen9; seed-1 and seed-3 wrote full memory in one generation and skipped the session in the other. That is exactly the self-enforcement ceiling the loading-reliability decision predicts, and it is now measured: the no-variables baseline for the closeout A/B is approximately 50%.
- Seed-0's judgment gaps are model-stable across three generations: the time-tracking curveball was absorbed three of three times, and the Monday/weekend recall-window question was never asked, three of three. These discriminate the model, not the framework, and make seed-0 the sharpest instrument for the model-comparison generation.

## Consequences Routed

- `benchmarks/variable-closeout-command/` created: a loader-overwrite overlay announcing `open-forge find --tag KeepInMind --bodies` as the one-command recheck. The obligation is unchanged; the command makes compliance cheaper than skipping - the v4 dump-tool experiment shape, aimed at the last standing gap, with a two-generation baseline to measure against.
- Registered in the benchmarks README with the one-loader-overwrite-per-run caution.

## What v10 Should Test

1. Closeout A/B: seeds 1 through 3 with `variable-closeout-command` against the gen8/gen9 baseline. Success is the recheck-plus-session rate moving decisively above the ~50% baseline; if it does, promote the tool-assisted loader wording to core.
2. Seed-0 with the workflow-essentials pack as a variable: the richer vision skill (clarify-intent, fit-and-risk references) is the only untested framework lever against the discovery-depth gap; if judgment gaps persist even with it, they are conclusively model territory for the Fable/Sonnet comparison.
3. Commit before running: gen9 reports again note uncommitted edits in the provenance line, and the tree has been staged and green for two days.

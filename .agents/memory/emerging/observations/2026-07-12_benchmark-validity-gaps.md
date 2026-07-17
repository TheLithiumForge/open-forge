---
open-forge:
  description: Raw-report reconciliation found provenance, isolation, collision, replication, and synthesis-consistency gaps in the benchmark corpus
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, Benchmark, Evaluation, Evidence, Reproducibility]
---

# Observation: Benchmark Engineering Value Exceeds Its Current Causal Validity

Date: 2026-07-12. Scope: the 20 structured v7-v11 run reports, accepted v7-v10 syntheses, harness/run plans, retained workspaces inspected during the assessment, and current collaboration-runtime behavior.

## Strong Signals

- All 20 reports contain the required sections and five core rubric scores, and reported baseline commits match retained workspaces.
- Generation 8 and 9 baseline trees are byte-identical across all four seeds.
- Generation 9 to 10 build baselines differ only by the closeout loader overwrite, making generation 10 the cleanest treatment comparison.
- Independent verification repeatedly found real product defects and worker overstatements.
- Product Fidelity scored 2/2 across all 12 v7-v10 build runs, supporting strong combined-system fidelity for precise routed content, prompts, and the tested model/runtime.

## Validity Gaps

- All structured generations use one Codex/GPT-5 family label; exact model revisions are unavailable.
- Reports reference dirty/staged source states and do not preserve portable framework/harness/seed tree hashes, transcripts, tool-call traces, or route-read events.
- Worker-context isolation is not recorded. Filesystem separation does not prove the worker did not inherit orchestrator rubric/persona context.
- One run per seed/cell is not replication; seed tasks differ; order is fixed; grading is unblinded and single-rater; no flat equal-content or no-framework control exists.
- Generation 11 had concurrent report-path ownership collisions, eight retained workspaces for four reports, bundled treatment changes, and an out-of-plan same-model seed-0 run. Its reports explicitly reject a clean causal verdict.
- Accepted syntheses overstate some raw evidence: the observable generation-8/9 session baseline is at most 3/8 overall or 2/6 build runs, not roughly 50% full-loop compliance; the claimed four consecutive recall-window misses conflict with v7; and v10 misattributes relevance-loaded vision references to a workflow whose Required Routes are `none`.

## Candidate Action

- Treat current reports as interpreted engineering evidence, not causal ground truth.
- Use collision-proof run UUIDs and machine-readable manifests with clean tree/patch hashes, exact runtime/model/tool versions, prompt/rubric hashes, context-isolation mode, worker-visible file manifest, and artifact hashes.
- Spawn workers with no inherited orchestrator context and include a rubric-only canary.
- Generate Markdown reports from validated JSON; preserve worker/debrief/tool traces; use executable seed validators as primary outcomes.
- Add equal-content controls, independent factors, randomized order, multiple replicates, multiple model/runtime families, and blinded double-rating for subjective items.
- Add automated corpus checks for composition, seed immutability, treatment deltas, report uniqueness, and synthesis consistency.

## Statistical Gate Correction: 2026-07-13

- The earlier candidate roadmap's "at least 10 replicates per cell" is suitable for a pilot, not an S++ reliability gate.
- Ten successes in ten independent trials have a lower two-sided 95% bound of only about 69-72%, depending on exact versus Wilson interval.
- A conservative exact-binomial gate needs 36 successes in 36 independent trials just to place the lower 95% bound at or above 90%; any failure requires a larger sample.
- Assertions inside one run are correlated and do not increase the independent sample size. Confirmatory counts must be powered from pilot rates, task clustering, non-inferiority margins, and preregistered stopping rules.
- This does not mean every experiment needs 36 runs per cell. It means strong reliability claims must name their population and interval, while smaller matrices remain explicitly engineering smoke or pilot evidence.

Suggested promotion destination after a clean validating generation: benchmark governance and CI #Core routes; until then keep this finding contextual.

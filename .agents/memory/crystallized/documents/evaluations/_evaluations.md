---
open-forge:
  description: Accepted evaluation syntheses; load when a loading, routing, or design decision needs its source evidence
  tags: [Memory, Document, Record, CurrentTruth, Evaluation, Evidence]
---

# Evaluations

Evaluations are accepted syntheses of evidence from testing Open Forge against real agent sessions.

This route holds coagulated summaries only. Per-run reports from the reproducible benchmark harness live in `benchmarks/results/`, and the detailed pre-harness reports (v2 through v6) live in `benchmarks/results/pre-harness/`.

## Axioms

- Read `Entries` when current work needs evaluation evidence, recommendation history, or the reasoning behind loading and routing decisions.
- Treat synthesis bodies as records, not active behavior or automatically accepted recommendations.
- Prefer crystallized decisions for accepted conclusions and policy; load syntheses when evidence or source reasoning matters.
- Write one synthesis per benchmark generation here after extracting accepted conclusions to their owning routes; raw run reports stay in `benchmarks/results/`.

## Entries

<!-- open-forge:generated-index:start -->
- `recommendations-synthesis.md` - Accepted record of dogfood evidence and recommendation synthesis across critique and v2 through v4 experiments - #Memory #Document #Record #CurrentTruth #Dogfood #Recommendation #Synthesis #Evidence
- `v10-synthesis.md` - Cross-run synthesis of the v10 A/B generation - the closeout command moved recheck compliance from ~50% to 3 of 3, and the printout proved to be the compliance surface - #Memory #Document #Record #CurrentTruth #Evaluation #Evidence #Synthesis #Benchmark
- `v7-synthesis.md` - Cross-run synthesis of the v7 benchmark generation - first validation of the new workflow shape, narrower-scope precedence proven at scale, scope control failing at the tool boundary - #Memory #Document #Record #CurrentTruth #Evaluation #Evidence #Synthesis #Benchmark
- `v8-synthesis.md` - Cross-run synthesis of the v8 benchmark generation - scope-control and vision-workflow fixes validated, closeout recheck now the isolated persistent gap - #Memory #Document #Record #CurrentTruth #Evaluation #Evidence #Synthesis #Benchmark
- `v9-synthesis.md` - Cross-run synthesis of the v9 benchmark generation - a same-configuration replication of v8 that confirms fidelity and scope stability and quantifies the closeout compliance ceiling - #Memory #Document #Record #CurrentTruth #Evaluation #Evidence #Synthesis #Benchmark
<!-- open-forge:generated-index:end -->

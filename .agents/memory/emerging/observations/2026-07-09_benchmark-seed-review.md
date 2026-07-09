---
open-forge:
  description: Benchmark seeds are strong for route-following evaluation, with stale overlay wording and scoring calibration still to tighten
  tags: [Memory, Observation, Contextual, Candidate, Benchmark, Dogfood, Evaluation]
---

# Observation: Benchmark Seed Review

Date: 2026-07-09. Source: direct review of `benchmarks/`.

- The benchmark design is useful because it separates worker-visible payload from orchestrator-only rubrics, keeps seeds stable, and tests route-following through real product constraints rather than abstract compliance prompts.
- Seed strength increases by level: seed 0 tests memory creation from conversation, seed 1 tests compact seeded truth, seed 2 tests non-obvious implementation semantics, and seed 3 tests scale, scoped routes, and conflict reporting.
- The dump-tool overlay still contains stale examples for `#OpenForge` and `.agents/protocols/...`; it should be updated before using it as a serious comparison variable.
- The shared rubric still says `Required Skill Packages`; if workflow wording moves to required routes, benchmark scoring should follow that terminology.
- Results will be most trustworthy when every run records route load order, independent verification, and whether closeout memory happened before the final response.

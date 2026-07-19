---
open-forge:
  description: Historical review of early benchmark seed strengths, stale overlay wording, and scoring calibration gaps
  tags: [Memory, Archived, Observation, Contextual, Historical, Benchmark, Dogfood, Evaluation]
---

# Observation: Benchmark Seed Review

Status: archived 2026-07-18.  
Original route: `.agents/memory/emerging/observations/2026-07-09_benchmark-seed-review.md`.  
Archived because: the harness and scenario contracts were substantially rebuilt and this early review no longer describes the current instrument.  
Current owner or replacement: `benchmarks/harness/`, raw reports, accepted evaluation syntheses, and `.agents/memory/emerging/observations/2026-07-12_benchmark-validity-gaps.md`.

Date: 2026-07-09. Source: direct review of `benchmarks/`.

- The benchmark design is useful because it separates worker-visible payload from orchestrator-only rubrics, keeps seeds stable, and tests route-following through real product constraints rather than abstract compliance prompts.
- Seed strength increases by level: seed 0 tests memory creation from conversation, seed 1 tests compact seeded truth, seed 2 tests non-obvious implementation semantics, and seed 3 tests scale, scoped routes, and conflict reporting.
- The dump-tool overlay still contains stale examples for `#OpenForge` and `.agents/protocols/...`; it should be updated before using it as a serious comparison variable.
- The shared rubric still says `Required Skill Packages`; if workflow wording moves to required routes, benchmark scoring should follow that terminology.
- Results will be most trustworthy when every run records route load order, independent verification, and whether closeout memory happened before the final response.

---
open-forge:
  description: Explore parallel Luna implementers for already-frozen Red evidence while one Brilliant Implementer and Task Mastermind retain semantic ownership
  tags: [Memory, Idea, Contextual, Candidate, Workflow, Testing, Red, Delegation, Parallelism, Luna, Efficiency]
---

# Parallel Red Evidence Implementation

## Opportunity

Reduce the critical-path cost of a cross-project Red phase without distributing
test meaning. A Brilliant Implementer can first freeze the exact behavior,
contracts, evidence tier, fixture boundary, and expected Red failure. The Task
Mastermind can then assign the bounded implementation details to separate
Luna/max implementers, normally at most one per independently runnable test
project.

This is an opt-in experiment, not a replacement for the current development
Workflow. The Task Mastermind still supervises every lane, and the Brilliant
Implementer remains accountable for production meaning and the relationship
between Red evidence and the later implementation.

## Candidate Shape

1. The Brilliant Implementer produces one explicit Red packet: accepted
   behavior, exact test cases, tier placement, owned fixtures, expected failure,
   protected production surfaces, and the smallest allowed file set.
2. The Task Mastermind checks that the work divides into non-overlapping test
   projects. It may assign one Luna/max Implementer per project, with exclusive
   files and no shared writer.
3. Every C# lane independently reads the complete current `_csharp.md`,
   `design.md`, and `style.md` Directives and reports fresh SHA-256 fingerprints.
4. Each Luna lane implements only the frozen evidence details. It does not
   invent behavior, change a public contract, choose architecture, add a
   test-only production seam, or edit production unless a separate accepted
   packet explicitly authorizes that surface.
5. Each lane runs only the targeted project build and Red selection, reports
   nonzero selected and executed counts, and distinguishes expected product Red
   from compilation, filter, infrastructure, or third-party failure.
6. The Task Mastermind reviews the joined evidence for cross-project semantic
   parity, evidence-tier correctness, Open Forge ownership, duplicate coverage,
   fixture quality, and false-green risk. The original Brilliant Implementer
   resolves any contract gap before Green begins.
7. The Task Mastermind converges the disjoint lanes into one coherent Red
   boundary and owns its Task-state update and commit when commits are
   authorized.

## Guardrails

- Test only Open Forge-owned behavior. Do not test a third-party library,
  framework wording, package-manager internals, operating-system exception
  details, or another tool's implementation.
- Keep Unit evidence pure, Integration evidence on real owned boundaries, and
  End-to-End evidence at the published process or package journey.
- Do not fan out merely because several test files exist. Decline the split
  when work shares one active fixture, one project, one mutable production
  boundary, or one unresolved semantic decision.
- Do not let Luna lanes weaken assertions to make Red or Green convenient.
  Missing meaning returns to the Brilliant Implementer; it is not filled by
  inference.
- Do not run competing builds into one artifact root. Serialize the build, then
  parallelize isolated `--no-build` selections only when the repository's test
  rules permit it.
- Quiet work remains unobserved progress rather than failure, but the Task
  Mastermind requests concise checkpoints and applies the normal latency
  circuit breaker when a lane cannot be supervised.

## Current Evidence

Task 14 "Extension Install" supplied the first negative selection check. Its
remaining evidence work was already confined to one Integration-project
boundary and files actively owned by the Brilliant Implementer, while the Unit
mapping case was complete. The Task Mastermind correctly declined parallel
Luna writers because they would have collided without reducing the critical
path. This supports making the split conditional on real project-level
independence rather than treating agent count as progress.

The existing [Supervised Luna Preparation Trial Results](../observations/2026-09-03_supervised-luna-preparation-trial-results.md)
support Luna/max for bounded implementation, shell work, evidence execution,
and output summarization, but do not yet prove this parallel Red shape. A later
task with genuinely disjoint Unit, Integration, and End-to-End files is the
useful positive trial.

## Experiment Measures

For each selected task, record the number of project lanes, critical-path time,
agent/context handoffs, overlapping-file conflicts, stale or zero-test runs,
accepted review corrections, duplicated or missing cases, false-green defects,
and whether the joined Red packet required the Brilliant Implementer to repair
meaning after delegation. Compare those facts with a similar single-owner Red
phase; raw agent count and raw test count are not success measures.

## Open Questions

- Should the Brilliant Implementer author executable skeletons, or is an exact
  immutable evidence packet sufficient for reliable Luna implementation?
- Is one lane per test project the right upper bound, or should one project
  remain single-owner even when its files are otherwise independent?
- Can serial build plus parallel isolated test execution reduce latency without
  making artifact provenance harder to prove?
- Does the Task Mastermind's convergence review catch semantic drift cheaply
  enough to outperform one Brilliant Implementer writing all Red evidence?

## Promotion Signals

Consider a small optional Workflow addition only after several genuinely
parallel tasks show lower critical-path time without more contract corrections,
test-tier mistakes, overlapping edits, stale evidence, or post-acceptance
defects. Keep the idea experimental or narrow it if the Mastermind frequently
declines the split, spends more time converging than the lanes save, or must
repair meaning that should never have left the Brilliant Implementer.

## Related Records And Sources

- [Review Orchestration Trial Controls](review-orchestration-trial-controls.md)
- [Continuous Targeted Review Orchestration](continuous-targeted-review-orchestration.md)
- [Supervised Luna Preparation Trial](../../../skills/use-workflow/references/open-forge/supervised-luna-preparation-trial.md)
- [Supervised Luna Preparation Trial Results](../observations/2026-09-03_supervised-luna-preparation-trial-results.md)
- [Evidence Tiers](../../../patterns/testing/evidence-tiers.md)
- [Testing Directive](../../../directives/open-forge/testing/_testing.md)

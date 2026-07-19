---
open-forge:
  description: Improve internal structure while preserving observable behavior; use when duplication, boundaries, naming, complexity, or maintainability need correction without a feature change
  tags: [Extension, Workflow, Quality, Refactoring]
---

# Refactoring

Refactoring improves internal design under explicit behavior-preservation evidence.

## Mode

iterative

## Goal

- outcome: simpler or safer internal structure with intended observable behavior preserved
- acceptance: the structural objective is met and baseline plus post-change verification show no intended behavior change
- stop: expected behavior is unknown, baseline verification is unreliable, a product behavior change becomes necessary, or scope exceeds authority

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `.agents/skills/quality/SKILL.md` - behavior baselines, refactoring slices, evidence standards, and regression control

## Constraints

- Define the behavior-preservation boundary before editing.
- Establish a trustworthy baseline or state why one cannot be obtained.
- Do not combine unrelated feature work with the refactor.
- Do not weaken existing tests or assertions to accommodate the new structure.

## Steps

1. State the structural problem, intended improvement, scope, and observable behaviors that must remain stable.
2. Inspect ownership, callers, data flow, contracts, tests, and project patterns around the target.
3. Run or inspect baseline verification and record any pre-existing failures.
4. Choose the smallest reversible structural slice with a clear invariant.
5. Apply that slice without changing public behavior or unrelated code.
6. Run narrow verification, then proportionate broader checks for affected callers and integration seams.
7. Review the result for reduced complexity, clearer ownership, naming, duplication, and alignment with local patterns.
8. Repeat only for the next justified slice; separate any discovered behavior change into a distinct task or decision.

## Loop

Repeat steps 4 through 7 one small slice at a time while verification stays trustworthy. Stop on a regression, an unclear behavior boundary, a required product change, or completion of the structural objective.

## Outputs

- structural objective and preservation boundary
- baseline and post-change verification
- refactoring slices and files changed
- pre-existing failures, discovered behavior changes, and residual risk

## Completion

- [ ] observable behavior-preservation boundary is explicit
- [ ] baseline and post-change evidence are compared
- [ ] tests and assertions were not weakened
- [ ] feature changes and unrelated cleanup were excluded or separated
- [ ] final response or handoff names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

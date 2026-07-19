---
open-forge:
  description: Implement this workspace's seeded project from its Open Forge routes; use when asked to build, continue, or finish the project the routes describe
  tags: [Extension, Workflow, PhaseDelivery, Implementation, Benchmark]
---

# Worker Implementation Workflow

Worker implementation turns this workspace's routed project truth into a working implementation.

## Mode

iterative

## Goal

- outcome: a working implementation that satisfies the routed vision and decisions
- acceptance: the configured verification passes (typecheck, build, full test command) and closeout memory is written
- stop: blocker, or the routed scope is complete

## Required Routes

none - the loader chain and the routes selected in Step 1 carry everything this workflow needs.

## Constraints

- The worker gives a concise final response; it does not write a benchmark or dogfood report artifact unless the prompt explicitly asks, since durable evaluation reports belong to whoever spawned the worker, after independent verification.

## Steps

1. Load the workspace, memory, pattern, directive, and workflow routes relevant to the project.
2. Restate the implementation plan only if useful; otherwise build.
3. Create the smallest project structure that satisfies the routed vision.
4. Write core behavior first, then adapters and entry points.
5. Add tests for core contracts and real end-to-end behavior.
6. Run the configured verification: typecheck, build, full test command.
7. Update Open Forge memory for grounded reusable findings, and write the routed closeout memory before the final response.

## Loop

Steps 4 through 6 repeat until verification passes or a blocker requires input. Otherwise linear.

## Outputs

- changed files
- verification results
- assumptions made and known gaps
- closeout memory

## Completion

- [ ] verification status is known
- [ ] closeout memory written or its blocker reported
- [ ] final response states changed files, verification results, assumptions, known gaps, and names this workflow

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

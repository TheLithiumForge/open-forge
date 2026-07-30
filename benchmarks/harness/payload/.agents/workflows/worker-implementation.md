---
open-forge:
  description: Implement this workspace's seeded project from its Open Forge routes
  tags: [Extension, Workflow, Implementation, Benchmark]
---

# Worker Implementation Workflow

## Goal

Turn this workspace's routed project truth into a working implementation.

## Starting Context

Use when the user asks to build, continue, or finish the project this workspace's routes describe.

## Steps

1. Load `AGENTS.md` and follow the loader.
2. Load the workspace, memory, pattern, directive, and workflow routes relevant to the project.
3. Restate the implementation plan only if useful; otherwise build.
4. Create the smallest project structure that satisfies the routed vision.
5. Write core behavior first, then adapters and entry points.
6. Add tests for core contracts and real end-to-end behavior.
7. Run the configured verification (typecheck, build, full test command).
8. Update Open Forge memory for grounded reusable findings, and write the routed closeout memory before the final response.
9. Final response: changed files, verification results, assumptions made, known gaps.

## Report Ownership

The worker gives a concise final response. The worker does not write a benchmark or dogfood report artifact unless the prompt explicitly asks for one; durable evaluation reports belong to whoever spawned the worker, after independent verification.

## Completion

Complete when verification status is known and closeout memory is written or its blocker is reported.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

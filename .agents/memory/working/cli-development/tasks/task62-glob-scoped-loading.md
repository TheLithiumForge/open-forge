---
open-forge:
  description: Retain the pending maintainer review of delivered applyTo filtering and glob UX follow-ups
  tags: [Memory, Working, Task, Framework, Loading, Frontmatter, CLI, Contextual, Active]
---

# Task 62: Glob-scoped loading

## Outcome

Finish the maintainer review of the delivered applyTo filtering, glob UX and review-polish follow-ups. The original implementation completed phase 3/3, milestone 6/6 and shipped. Its [original outcome and receipts](../../../archived/cli-development/tasks/task62-original-implementation.md) are historical.

## Current review horizon

Task 62 “Glob-scoped loading” (phase 2/2): milestone 2/3. M3 maintainer review is pending in [Execution](task62/execution.md#review-and-polish-follow-up).

The delivered behavior treats applyTo as a filter over sources selected by routing and loading rules. It never activates a source by itself. A leading ! is literal. Context receives explicit working files through --for. The [dialect plan](task62/glob-dialect-plan.md) and current contracts retain the detailed matching rules. Historical plans do not override later filter-only or UX decisions.

## Plan

1. Review the later filter-only, glob UX and polish evidence in Execution against the current Loader, command contracts and public documentation.
2. Record the maintainer disposition and any bounded correction needed for acceptance. Keep excluded roots and APM integration outside this Task.
3. Close the later review horizon only after its M3 disposition is recorded. Preserve the original implementation receipts.

## Current state

The implementation and grouped review corrections are delivered. M3 maintainer review remains open. This cleanup does not advance it or reopen implementation. The [live packet](task62/_task62.md) links the remaining review material and archived initial plans.

## Completion criteria

- [ ] The maintainer review has a recorded disposition.
- [ ] Any accepted correction is verified against the affected current contracts and behavior.
- [ ] M3 closes and the remaining Working packet retires without changing the original receipts.

## Related work

- [Task 53](task53-loading-and-scoping-audit.md) owns the wider loading audit.
- [Task 55](../../../archived/cli-development/tasks/task55-alternative-root-decision.md) closed the root-placement question separately.

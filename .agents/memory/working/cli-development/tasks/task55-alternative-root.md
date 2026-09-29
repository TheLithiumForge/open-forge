---
open-forge:
  description: Investigate alternative Framework roots such as .apm and APM interoperability separately from applyTo loading
  tags: [Memory, Working, Task, Framework, Root, APM, Investigation, Contextual]
---

# Task 55: Alternative workspace root such as `.apm`

## Outcome

Decide whether and how Open Forge can live under an alternative root, including
directly inside `.apm/`, and how that relates to APM packaging and compilation.
Keep this separate from file-glob context selection.

## Restoration and authority

Restored under its permanent ID on 2026-09-29 at the maintainer's request.
The [historical task](../../../archived/cli-development/tasks/task55-alternative-root.md)
preserves its original scope and the 2026-09-28 folding into Task 62. That folding
no longer governs current work. The prior recommendation to keep `.agents/`
does not settle this reopened investigation.

Restoration authorizes this planning record. It does not authorize changing the
Framework root, installing APM, running compilation, or implementing an adapter.
Existing `.agents/` behavior remains the current product contract.

## Scope

- Inventory assumptions about `.agents/` in discovery, Loader references,
  source IDs, containment, settings and locks, packages, tests, and docs.
- Compare a configurable single root, direct placement inside `.apm/`, optional
  export/adapters, and retaining the current root. Evaluate multiple concurrent
  roots separately rather than treating them as implied by an alternative root.
- Verify APM's current formats, package behavior, and compilation from its own
  documentation when this investigation resumes.
- Determine whether routing, progressive loading, and Core/Memory meanings
  survive each option, and identify unsupported mappings.
- Keep APM-specific exports, pointer files, package experiments, and general
  frontmatter interoperability here. Root and scoped `applyTo` parsing belongs
  solely to [Task 62](task62-glob-scoped-loading.md).

## Proposed plan

1. Confirm current APM behavior and inventory root assumptions.
2. Compare options, compatibility, migration cost, and loading behavior.
3. Propose a bounded scratch experiment and identify any installation or external
   effects before execution. The historical prototype remains a candidate.
4. Present a recommendation for a separate maintainer decision.

## Current state

Open and unstarted under the restored scope. No new APM investigation or
prototype was run during Task 62 planning. Schedule this independently.

## Done when

- [ ] Current root assumptions and external evidence are recorded.
- [ ] Alternatives and compatibility consequences are compared.
- [ ] Any accepted prototype has inspected results and a stated disposition.
- [ ] The maintainer accepts, rejects, or defers the recommendation.

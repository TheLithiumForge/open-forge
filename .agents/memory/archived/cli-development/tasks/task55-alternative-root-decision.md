---
open-forge:
  description: "Historical record: Record the decision to retain .agents as the sole Framework root and define the APM coexistence boundary"
  tags: [Memory, Task, Framework, Root, APM, Decision, Contextual, Complete, Archived, Historical]
---

# Task 55: Alternative workspace root such as `.apm`

## Archive Status

Archived on 2026-10-04 from `.agents/memory/working/cli-development/tasks/task55-alternative-root.md` after the maintainer selected Memory cleanup. Maintainer decision retains .agents and closes the investigation without requiring APM certification.

This record preserves historical evidence. The [current CLI development route](../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Outcome

Decide whether and how Open Forge can live under an alternative root, including
directly inside `.apm/`, and how that relates to APM packaging and compilation.
Keep this separate from file-glob context selection.

## Restoration and authority

Restored under its permanent ID on 2026-09-29 at the maintainer's request.
The [historical task](task55-alternative-root.md)
preserves its original scope and the 2026-09-28 folding into Task 62. That folding
no longer governs current work. The prior recommendation to keep `.agents/`
does not settle this reopened investigation.

Restoration authorizes this planning record. It does not authorize changing the
Framework root, installing APM, running compilation, or implementing an adapter.
Existing `.agents/` behavior remains the current product contract.

## Scope

This is the investigation scope recorded when the Task was restored. The
decision below closes the product question without requiring every listed
research step.

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
  solely to [Task 62](../../../working/cli-development/tasks/task62-glob-scoped-loading.md).

## Proposed plan

This plan preserves the scope that prompted the investigation. The maintainer's
decision below closes the product question without requiring these research
steps or a proof of concept.

1. Confirm current APM behavior and inventory root assumptions.
2. Compare options, compatibility, migration cost, and loading behavior.
3. Propose a bounded scratch experiment and identify any installation or external
   effects before execution. The historical prototype remains a candidate.
4. Present a recommendation for a separate maintainer decision.

## Decision (2026-10-01)

The maintainer chose to retain `.agents/` as the current and sole Framework
root. Multiple concurrent roots and dedicated APM adapters are not selected.
Ordinary coexistence with APM remains product direction, like coexistence with
other harnesses. This is not an APM compatibility certification.

No proof of concept, APM installation, or code change is required by this
decision. The earlier scope, comparison questions, and preliminary local
triage remain as historical context. That triage recorded that `apm.yml`
targets `opencode` and `codex`. It did not verify external APM behavior.

## Current state

**Status:** Task 55 "Alternative workspace root such as .apm" (phase 1/1):
milestone 1/1. The maintainer's decision is recorded and the Task is closed.

## Decision completion

These checkboxes track decision capture and record reconciliation. They do not
claim that an APM installation, compatibility test, proof of concept, or
external certification was performed.

- [x] The maintainer's root and APM direction is recorded with its evidence
      boundary.
- [x] The project-control ledger, plan, Task index, and backlog reflect the
      closure without moving this Task or creating another category.

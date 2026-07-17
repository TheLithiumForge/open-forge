---
open-forge:
  description: Session record of applying the accepted workflow shape and shipping the dev-workflow extension
  tags: [Memory, Session, Contextual, Workflow, Extension]
---

# Session: Workflow Shape Applied

Date: 2026-07-10. The workflow redesign was accepted and applied; the maintainer's TDD dev workflow shipped as an independent extension.

## What Happened

1. `_workflows.md` (payload and installed copies) migrated to the new contract: Goal, Required Routes, Steps, Loop, Outputs, Completion axioms; containment-versus-dependency distinction; orchestration and delegation rules.
2. The workflows descriptor, agent-primitives concept, and formatting concept updated in lockstep; alignment checks now name the new sections.
3. workflow-essentials' three workflows (implementation, vision, architecture) rewritten in the full new shape with Goal, Constraints, and Completion checklists.
4. New independent extension `dev-workflow` created under `src/extensions/dev-workflow/`: one `dev` workflow implementing contract-first TDD (plan, red, implement, green, blue refactor, closeout). It requires workflow-essentials' shared skills per the sharing contract; the README says to install both.
5. Benchmark harness migrated: the worker-implementation workflow moved to the new shape and the rubric now scores "Required Routes read before Step 1, or none acknowledged".
6. The accepted design crystallized as `.agents/memory/crystallized/decisions/workflow-shape.md`; the idea file marked applied and kept for rationale detail.
7. To-explore material saved per maintainer request: `emerging/ideas/workspace-usage.md`, the docs-reconciliation and description-rot notes appended to the open-question-recommendations analysis, and the CLI design already in `emerging/ideas/cli-design.md`.

## Unresolved

- Run one seeded benchmark generation against the new shape, including dev-workflow (backlog item 1).
- The workspace, docs-placement, and CLI design recommendations await maintainer exploration before promotion to decisions.

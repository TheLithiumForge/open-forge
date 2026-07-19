---
open-forge:
  description: Historical session record of applying the accepted workflow shape and shipping the dev-workflow extension
  tags: [Memory, Archived, Session, Contextual, Historical, Workflow, Extension]
---

# Session: Workflow Shape Applied

Status: archived 2026-07-18.  
Original route: `.agents/memory/working/sessions/2026-07-10_workflow-shape-applied.md`.  
Archived because: the workflow shape shipped and later phase-aware revisions became current truth.  
Current owner or replacement: `.agents/memory/crystallized/decisions/workflow-shape.md`, workflow Core routes, and current extension payloads.

Date: 2026-07-10. The workflow redesign was accepted and applied; the maintainer's TDD dev workflow shipped as an independent extension.

## What Happened

1. `_workflows.md` (payload and installed copies) migrated to the new contract: Goal, Required Routes, Steps, Loop, Outputs, Completion axioms; containment-versus-dependency distinction; orchestration and delegation rules.
2. The workflows descriptor, agent-primitives concept, and formatting concept updated in lockstep; alignment checks now name the new sections.
3. workflow-essentials' three workflows (implementation, vision, architecture) rewritten in the full new shape with Goal, Constraints, and Completion checklists.
4. New independent extension `dev-workflow` created under `src/extensions/dev-workflow/`: one `dev` workflow implementing contract-first TDD (plan, red, implement, green, blue refactor, closeout). It requires workflow-essentials' shared skills per the sharing contract; the README says to install both.
5. Benchmark harness migrated: the worker-implementation workflow moved to the new shape and the rubric now scores "Required Routes read before Step 1, or none acknowledged".
6. The accepted design crystallized as `.agents/memory/crystallized/decisions/workflow-shape.md`; the idea file marked applied and kept for rationale detail.
7. To-explore material saved per maintainer request: `.agents/memory/archived/ideas/workspace-usage.md`, the docs-reconciliation and description-rot notes in `.agents/memory/archived/analysis/2026-07-09_open-question-recommendations.md`, and `.agents/memory/archived/ideas/cli-design.md`; current follow-ups were extracted before archival.

## Unresolved

- Run one seeded benchmark generation against the new shape, including dev-workflow (backlog item 1).
- The workspace, docs-placement, and CLI design recommendations await maintainer exploration before promotion to decisions.

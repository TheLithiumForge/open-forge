---
open-forge:
  description: Explore an optional reusable record set for consequential multi-gate programs without adding another Core primitive or mandatory methodology
  tags: [Memory, Idea, Contextual, Candidate, Program, Plan, Audit, Decision, Checkpoint, Template, Pattern, Workflow, Extension]
---

# Durable Program Records

## Opportunity

Consequential release, migration, redesign, and cleanup efforts may need more
durable structure than one plan or Checkpoint can responsibly own. The new CLI
release program currently separates:

- A small routed program entrypoint.
- One live gate and task plan.
- An evidence audit.
- Stable findings.
- Per-artifact disposition registers.
- An unaccepted decision agenda.
- A concise active Checkpoint.

That separation makes exhaustive work resumable without turning a Checkpoint
into a large specification or treating candidate findings as accepted truth.

## Candidate Reusable Shape

```text
.agents/memory/working/<program>/
├── _<program>.md
├── program-plan.md
├── audit.md
├── findings.md
├── artifact-register.md
└── decision-agenda.md

.agents/memory/working/checkpoints/<program>.md
```

The exact files should remain proportional. A small workstream may need only a
plan or Checkpoint. Split records only when evidence volume, artifact count,
decision depth, or gate duration gives each file an independent responsibility.

## Candidate Packaging

- A **Pattern** could define the responsibility split, one-authority boundary,
  lifecycle, and review checks.
- A **Template pack** could provide copy-ready entrypoint, plan, finding,
  artifact-register, decision-agenda, and Checkpoint skeletons.
- An optional **Workflow** could explain initialization, maintenance, decision
  integration, independent review, and closeout.
- The development-toolkit Extension is a better initial package candidate than
  the shared Framework.

A dedicated Skill or agent does not currently appear justified. Existing
Planner, Analyst, Explorer, Architect, Implementation, and Acceptance Reviewer
roles already perform the underlying work. The useful reusable value is record
shape and lifecycle, not another reasoning capability.

## Core Boundary

Do not add a new Core primitive or mandatory program methodology. Existing
Working Memory, Checkpoints, Decisions, Documents, Templates, Patterns, and
Workflows already express the required roles. Core should continue defining
their general semantics without requiring every workstream to adopt this
ceremony.

## Evidence Before Promotion

1. Complete the CLI release program using this structure.
2. Trial the shape in at least one other consequential release, migration, or
   redesign.
3. Identify which records materially improved resumption, decision quality,
   artifact coverage, or review.
4. Remove fields and files that became narration or duplicate authority.
5. Confirm the shape composes with an external tracker or another declared task
   authority without copying mutable truth.
6. Promote only the smallest independently useful Pattern, Templates, and
   optional Workflow.

## Related Evidence

- [Current CLI release trial](../../working/cli-release/_cli-release.md)
- [Optional task work modes](task-work-modes.md)
- [Checkpoint contract](../../working/checkpoints/_checkpoints.md)
- [Decision lifecycle metadata](decision-lifecycle-metadata.md)

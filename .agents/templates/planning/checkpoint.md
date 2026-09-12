---
open-forge:
  description: Starting structure for an active workstream's current state, evidence, open questions, and next steps
  tags: [Extension, Template, Memory, Planning, Working, Checkpoint, Contextual]
---

# {Workstream} Checkpoint

{
Template selection:

- Need: Additional current context for one active workstream when durable sources alone are not enough to resume it.
- Primary question: Where does this workstream stand, and what should happen next?

Keep current state in the Task when that is sufficient. If a dedicated Checkpoint is useful, use one current Checkpoint for the workstream and link to it from the records that rely on its state. The [Work Records Pattern](../../patterns/work-records.md) defines the default relationships.

Copy into the appropriate Working Checkpoints route and follow its [active-state and closeout rules](../../memory/working/checkpoints/_checkpoints.md). This is a changing resumption record. Create a Handoff only when a transfer boundary needs a stable snapshot while the Checkpoint may continue to change.

Replace the Template metadata with accurate destination metadata, adapt the useful sections, and remove all braced guidance. Apply the destination's active continuity tags only while the Checkpoint is active. Recorded state and evidence do not accept their own conclusions.
}

## Goal And Sources

{Identify the active workstream and link to the source that defines its goal. Link to the Task, Plan, project rules, or external systems needed for continuation.}

## Current State

{State the current step and what is in progress. Identify the actual working location and baseline when continuation depends on them, including unfinished or unsaved work that must be preserved. Link to task status or completed history maintained elsewhere.}

## Accepted Direction And Evidence

{Link to accepted decisions and the decisive evidence needed to resume. State their scope and limits where those affect the next action. Keep candidate conclusions distinguishable from accepted direction.}

## Open Questions

{Name blockers, missing inputs, and unresolved decisions. Identify what can proceed and what depends on resolving each question.}

## Next Steps

{Give the exact next useful action and where it should begin. Link to the Plan for the remaining sequence instead of copying it.}

---
open-forge:
  description: Starting structure for a sealed transfer snapshot with the state, evidence, blockers, and next action needed to resume
  tags: [Extension, Template, Memory, Handoff, Working, Contextual]
---

# {Work} Handoff

{
Template selection:

- Need: One stable boundary snapshot for an actual transfer or explicitly planned resumption while the active Checkpoint may continue to change.
- Primary question: What boundary state must the recipient or resumed context preserve to continue without private context?

Do not create a Handoff for a routine pause, ordinary closeout, or possible future interruption.
Copy this Template into the appropriate Working Handoffs route and seal it when the named boundary occurs.
Record boundary status, next action, blockers, and verification state in this snapshot. A live Checkpoint may supplement but not replace it.
Never edit a sealed Handoff. Record later state in the active Checkpoint or a new Handoff. When this snapshot no longer supports its transfer or resumption, keep any durable result and archive the whole Handoff.
Link to detailed current sources instead of copying complete history.
Replace the frontmatter, title, and placeholders, then remove this braced guidance.
}

## Goal And Scope

{State the intended recipient or named resumption, outcome, current boundary, and any explicit stop or approval gate.}

## Current State

{State where the work stands now in enough detail to resume accurately.}

## Accepted Direction

{Link to accepted Decisions and current sources that guide continuation. Keep candidates clearly separate.}

## Completed And Verified

{State completed outcomes and the evidence that supports them.}

## Remaining Work And Blockers

{State unresolved work, blockers, risks, approval needs, and unavailable dependencies.}

## Next Action

{State the exact next useful action and where it should begin.}

## Required Context

{Link to the minimum routes, files, commands, or external systems needed to continue.}

## Exit

{State when the transfer ends and the Handoff should be archived.}

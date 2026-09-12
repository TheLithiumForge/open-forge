---
open-forge:
  description: Sealed transfer snapshots that preserve one boundary for resumption
  tags: [LoadNow, Memory, Handoff, AgentCommunication, Contextual]
---

# Handoffs

## What state must survive this transfer or planned resumption?

A Handoff is a sealed snapshot for another reader. It preserves the boundary-specific state needed to resume after an actual transfer or explicitly planned resumption after a context boundary.

## Axioms

### Creation And Scope

- Read `Entries` when work is resumed, transferred, delegated, interrupted, or reviewed after a context break.
- Treat Handoffs as contextual transfer notes, not complete history or accepted truth.
- Create and seal a Handoff when an actual transfer or explicitly planned resumption needs a stable boundary snapshot while the active Checkpoint may continue to change.
- Do not create one for a routine pause, ordinary closeout, or possible future interruption.
- Make the intended reader or resumed work clear from the route, description, or content.

### Content And Lifecycle

- Keep Handoffs short. Link to current state, durable sources, code, or other details instead of copying them.
- Record the boundary status, next action, blockers, and verification state in the Handoff itself. A live Checkpoint may supplement but not replace this snapshot.
- Keep the snapshot unchanged while it serves as a sealed Handoff. Record later state in the active Checkpoint or a new Handoff.
- When a Handoff no longer supports an active transfer, keep any useful result and archive it.

## Entries

<!-- open-forge:generated-index:start -->

- none - No entries - #Empty

<!-- open-forge:generated-index:end -->

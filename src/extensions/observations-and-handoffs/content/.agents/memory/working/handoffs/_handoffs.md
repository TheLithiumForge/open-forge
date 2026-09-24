---
open-forge:
  description: Sealed transfer snapshots that preserve one boundary for resumption
  tags: [Extension, Memory, Handoff, AgentCommunication, Contextual]
---

# Handoffs

## What state must survive this transfer or planned resumption?

A Handoff is a sealed snapshot for another reader. It preserves the state needed at an actual transfer or an explicitly planned resumption across a context boundary.

## Axioms

### Creation And Scope

- Check `Entries` when work is resumed, transferred, delegated, interrupted, or reviewed after a context break.
- Treat Handoffs as contextual transfer notes, not complete history or authority by themselves.
- Create and seal a Handoff when an actual transfer or explicitly planned resumption needs a stable snapshot while the active Checkpoint may continue to change.
- Do not create one for a routine pause, ordinary closeout, or possible future interruption.
- Make the intended reader or resumed work clear from the route, description, or content.

### Content And Lifecycle

- Keep Handoffs short. Link to current state, durable sources, code, or other details instead of copying them.
- Record the boundary status, next action, blockers, and verification state in the Handoff itself. A live Checkpoint may supplement the snapshot, not replace it.
- Keep the snapshot unchanged while it serves as a sealed Handoff. Record later state in the active Checkpoint or a new Handoff.
- When a Handoff no longer supports an active transfer, preserve useful results and archive it.

## Entries

- none - No entries - #Empty

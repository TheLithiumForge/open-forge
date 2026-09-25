---
open-forge:
  description: Temporary memory that helps agents continue or resume active work
  tags: [LoadNow, Memory, Working, Contextual]
---

# Working Memory

## What temporary context is needed to continue or resume this work?

Working Memory holds temporary context for continuing or resuming active work.

## Axioms

### Use And Status

- Check `Entries` before deciding that no Working Memory applies.
- Save the temporary context needed to resume unfinished work.
- An explicitly accepted temporary choice may remain here when its source, scope, and expected expiration are clear.

### Maintenance

- Keep Working Memory small, current, and easy to replace.
- When the active need ends, preserve useful results, then move, archive, consolidate, or prune the record.

## Entries

- [Minimal current backlog after the public beta and the 1.0 Memory trim](backlog.md) - #Memory #Working #Backlog #Contextual
- [Current state, current step, and next steps for one active workstream](checkpoints/_checkpoints.md) - #Extension #Memory #Working #Checkpoint #Contextual
- [Current CLI migration selection, retained task state and task-owned evidence](cli-development/_cli-development.md) - #Memory #Working #Contextual #Active #KeepInMind #CLI #Task
- [Sealed transfer snapshots that preserve one boundary for resumption](handoffs/_handoffs.md) - #Extension #Memory #Handoff #AgentCommunication #Contextual
- [Review local planning around the Planning Extension, templates, task lifecycle, completion tracking, organization, and archival before any source change](local-planning.md) - #Memory #Working #Task #Contextual #Active #Framework #Planning #Templates #Lifecycle #Archival

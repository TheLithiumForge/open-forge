---
open-forge:
  description: Current maintenance contract for the Core Working Memory entrypoint and its optional Extension-provided Checkpoints and Handoffs routes
  responsibility: Preserve active resume state, expected expiration, durable-result extraction, standard nested roles, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Working]
---

# Working Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/working/_working.md`](../../../../../../../../../src/open-forge/.agents/memory/working/_working.md) is the canonical installed Working Memory `entrypoint`. The repository [Working `entrypoint`](../../../../../../../working/_working.md) is its counterpart with local Entries.

The [Working state contract](../../../../../framework/memory/working.md) defines Working Memory through expected expiration. The [transition contract](../../../../../framework/memory/transitions.md) owns extraction and movement when active need ends.

## Contract

- The title is followed by `## What temporary context is needed to continue or resume this work?` and a short paragraph describing unfinished-work records for a later session.
- Frontmatter preserves #LoadNow, #Memory, #Working and #Contextual.
- At task start or resume, check Entries and read any record for that work, then compare it with current files before relying on it.
- Working's own Axioms require updating or creating a record before pausing or handing off unfinished work, with the goal, current state, next step and open questions.
- Those Axioms limit saving to temporary state another session needs to resume or transfer the work. Finished work needs no record unless its outcome still matters.
- Records stay small, current and replaceable, with the goal, present state, next step and open questions.
- Working does not establish acceptance. An accepted temporary choice may remain when its source, scope and expected expiration are clear.
- Useful results reach their defining sources before an expired record is moved, consolidated or pruned.
- Checkpoints and Handoffs are optional Extension roles, not Core defaults or requirements for every pause. The state catalogue ships empty and contains no project-specific active state.

## Verification

- Use the [payload integrity checks](../../../_payload.md#verification) for Core and optional Extension bytes and metadata rendering.
- Compare shipped and repository rules outside Entries, including the saving Axioms. Report any missing counterpart update within the task's edit boundary.
- Manually trace start, resume, pause, handoff and closeout with an ordinary Working record. Verify its resume fields and temporary-state limit, without requiring a named record category or sealed transfer snapshot.

## Entries

- [Current maintenance contract for the installable Checkpoints Memory entrypoint](checkpoints.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Checkpoint #Resumability
- [Current maintenance contract for the installable Handoffs Memory entrypoint](handoffs.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Handoff #AgentCommunication

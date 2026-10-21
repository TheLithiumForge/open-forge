---
open-forge:
  description: Current maintenance contract for the Core Working Memory entrypoint and its optional Extension-provided Checkpoints and Handoffs routes
  responsibility: Preserve active resume state, expected expiration, durable-result extraction, standard nested roles, recursive scope, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Working]
---

# Working Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/working/_working.md`](../../../../../../../../../src/open-forge/.agents/memory/working/_working.md) is the canonical installed Working Memory `entrypoint`. The repository [Working `entrypoint`](../../../../../../../working/_working.md) keeps the shared state rules and local Entries. It omits the saving-policy sentence because this repository has no saving-policy copy.

The [Working state contract](../../../../../framework/memory/working.md) defines Working Memory through expected expiration. The [transition contract](../../../../../framework/memory/transitions.md) owns extraction and movement when active need ends.

## Contract

- Frontmatter preserves #LoadNow, #Memory, #Working and #Contextual. Working is optional, needs Memory and holds unfinished-work records for a later session.
- At task start or resume, check Entries and read any record for that work, then compare it with current files before relying on it.
- Records stay small, current and replaceable, with the goal, present state, next step and open questions.
- Working does not establish acceptance. An accepted temporary choice may remain when its source, scope and expected expiration are clear.
- Useful results reach their defining sources before an expired record is moved, archived, consolidated or pruned.
- The introduction points to [memory/saving.md](../saving.md) for automatic progress capture. The policy is outside Working so private records do not hide shared instructions. Its removal can leave Working records in place.
- Checkpoints and Handoffs are optional Extension roles, not Core defaults or requirements for every pause. The state catalogue ships empty and contains no project-specific active state.

## Verification

- Use the [payload integrity checks](../../../_payload.md#verification) for Core and optional Extension bytes and metadata rendering.
- Compare shared shipped and repository rules outside Entries, allowing the declared saving-policy sentence difference. Inspect the shipped saving-policy pointer and verify its shared location and optional removal behavior.
- Manually trace start, resume and closeout with an ordinary Working record. Check that named record categories and sealed transfer snapshots remain optional.

## Entries

- [Current maintenance contract for the installable Checkpoints Memory entrypoint](checkpoints.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Checkpoint #Resumability
- [Current maintenance contract for the installable Handoffs Memory entrypoint](handoffs.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Handoff #AgentCommunication

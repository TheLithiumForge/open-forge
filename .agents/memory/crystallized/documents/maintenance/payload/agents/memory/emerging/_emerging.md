---
open-forge:
  description: Current maintenance contract for the Core Emerging Memory entrypoint and its optional Extension-provided Analysis, Ideas, and Observations routes
  responsibility: Preserve candidate authority, capture threshold, uncertainty, consolidation, standard nested roles, continuity loading, and source alignment
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Emerging, Candidate]
---

# Emerging Memory Maintenance Contract

## Source

[`src/open-forge/.agents/memory/emerging/_emerging.md`](../../../../../../../../../src/open-forge/.agents/memory/emerging/_emerging.md) is the canonical installed Emerging Memory `entrypoint`. The repository [Emerging `entrypoint`](../../../../../../../emerging/_emerging.md) dogfoods the same authored contract and adds repository candidates through generated `Entries`.

The [Emerging state contract](../../../../../framework/memory/emerging.md) defines candidate meaning and capture thresholds. The [transition contract](../../../../../framework/memory/transitions.md) owns consolidation, promotion, archival, and pruning.

## Contract

- Frontmatter preserves #KeepInMind, #Memory, #Emerging, #Contextual and #Candidate. The state is optional and needs Memory.
- At each applicable KeepInMind refresh, check Entries and capture useful candidate material before it is lost. An empty result is valid and raw activity alone does not justify Memory.
- Keep source, uncertainty and scope visible when they affect later use. Refine material while its validity, use or destination is unsettled.
- Candidate status does not establish acceptance. Accepted durable results move to their defining source. Useful outcomes and reasoning survive archival or pruning of rejected or replaced material.
- Repeated or stale material is reviewed for consolidation, promotion, archival or pruning. Review may leave it unchanged.
- [The progress-saving policy](../saving.md) governs unfinished-work state separately. Removing it does not disable Emerging capture.
- Analysis, Ideas and Observations retain their distinct optional Extension roles. Core ships no candidate records and starts with empty Entries.
- The shared text stays compact enough for repeated reading. Project candidates belong in records rather than the shipped catalogue.

## Verification

- Use the [payload integrity checks](../../../_payload.md#verification) for the state, optional Extension bytes and metadata rendering.
- [SourceLoadingClosureResolverTests](../../../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Loading/SourceLoadingClosureResolverTests.cs) cover KeepInMind propagation and unselected scopes.
- Compare shared source and repository rules outside Entries. Manually review capture, uncertainty and acceptance transitions. Loading tests do not prove that candidate capture happens.
- Check Emerging remains active when its row is enabled even if the saving policy is removed.

## Entries

- [Current maintenance contract for the installable Analysis Memory entrypoint](analysis.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Analysis #Reasoning #Candidate
- [Current maintenance contract for the installable Ideas Memory entrypoint](ideas.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Idea #Exploration #Candidate
- [Current maintenance contract for the installable Observations Memory entrypoint](observations.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Observation #AgentLearning #Candidate

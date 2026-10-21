---
open-forge:
  description: Current maintenance contracts for the Core Memory root and its standard state routes
  responsibility: Preserve self-growing Memory, its authority boundary, standard state `routes`, source alignment, and complete operational contract
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, MemoryModel]
---

# Memory Runtime Maintenance

## Source

[`src/open-forge/.agents/memory/_memory.md`](../../../../../../../../src/open-forge/.agents/memory/_memory.md) is the canonical installed Memory `entrypoint`. The repository [Memory `entrypoint`](../../../../../../_memory.md) dogfoods the same authored contract and adds repository-local generated `Entries`.

The [Memory architecture](../../../../framework/memory/_memory.md), [Memory model](../../../../framework/memory/model.md), four routed state contracts, [transition contract](../../../../framework/memory/transitions.md), and [accepted-state contract](../../../../framework/truth.md) define coherent Framework meaning beyond runtime instructions.

## Contract

- The title is followed by `## What is worth remembering for current or future work?` and a short paragraph describing the folder.
- Frontmatter keeps #LoadNow and #Memory. The root defines self-growing Markdown state for continuity, accepted knowledge, candidates and history, with useful records and scopes added without a structural ceiling.
- A record need not pass through every state.
- The two Axioms groups, `What to keep` and `Where it goes`, retain capture thresholds, source visibility, integration timing, placement and state transitions.
- Communication and coordination are saved only when they must survive the current context. A durable outcome needs future value in its accepted result, reasoning, required behavior or reusable shape. No value means no durable record. Closeout also preserves unsettled findings worth revisiting.
- Records distinguish stated, observed and inferred material when trust requires it. Accepted durable knowledge reaches its defining source before dependent work and by closeout. Useful reasoning remains available.
- Saving or moving material does not accept it or grant another category's role. Accepted instructions for future behavior go to the source for that role.
- Reuse a suitable record before creating another. Place material in the narrowest useful existing state or scope. Moving it leaves no stale competing copy and follows the destination's rules.
- A record needs no named category. Use an existing state and the narrowest useful scope, without recreating removed folders merely for capture.
- Ordinary records and scoped routes may be added within granted authority. Placement that changes accepted meaning or crosses an unresolved authority boundary requires a question. New top-level states require explicit user direction.

### Loading and content

These are maintainer facts for CLI selection and loading, not requirements on catalogue prose. Each state requires the Memory catalogue, while states remain independently selectable. Unselected branches stay unloaded.

| Source | Shipped loading |
| --- | --- |
| Memory catalogue | #LoadNow when enabled in the loader |
| Working and Crystallized | #LoadNow through Memory |
| Emerging | #KeepInMind through Memory |
| Archived | On demand through Memory |
| Decisions, Documents, Analysis, Ideas, Observations, Checkpoints and Handoffs | Optional Extension routes, each retaining its own source and tags |

- The shipped Memory Entries list four state catalogues. Preset selection omits unselected states. Progress saving belongs to Working's own Axioms, with no separate policy file or Install selection special case.
- State catalogues ship with empty Entries and no project records. The repository may add local records and maintain different Entries without claiming identical navigation.

## Verification

- Use the [payload integrity checks](../../_payload.md#verification) for shipped Memory bytes and metadata rendering.
- [SourceLoadingClosureResolverTests](../../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Loading/SourceLoadingClosureResolverTests.cs) cover required loading, continuity and unselected branches.
- Compare shared authored source and repository rules outside Entries. Review state independence, Working's saving Axioms and Extension separation manually.
- Check preset selection and navigation in isolated fixtures. Validate links directly for contract-only changes. Do not run maintenance commands against the source tree to manufacture installation evidence.

## Entries

- [Current maintenance contract for the installable Archived Memory entrypoint](archived.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Archived #Historical
- [Current maintenance contract for the Core Crystallized Memory entrypoint and its optional Extension-provided Decisions and Documents routes](crystallized/_crystallized.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Crystallized
- [Current maintenance contract for the Core Emerging Memory entrypoint and its optional Extension-provided Analysis, Ideas, and Observations routes](emerging/_emerging.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Emerging #Candidate
- [Current maintenance contract for the Core Working Memory entrypoint and its optional Extension-provided Checkpoints and Handoffs routes](working/_working.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Working

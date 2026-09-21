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

- Frontmatter keeps #LoadNow, #Memory, and #OrganicGrowth so the root Memory contract enters baseline context with its Framework classification and growth role visible
- Memory can grow through useful records and routed scopes without a fixed structural ceiling, while unrelated branches stay outside active context. People and agents add records deliberately when their value justifies the cost of finding and reviewing them
- The root `entrypoint` preserves the scoped knowledge authority of accepted records while keeping user direction, platform constraints, runtime safety, and declared external facts applicable. Recording another category's content does not give Memory that category's role
- Communication, coordination, direction, uncertainty, and learning are saved when they must survive the current context. Ordinary conversation does not become Memory automatically
- Clear user direction is accepted within its scope. Tentative, exploratory, inferred, or meaningfully unclear conclusions remain in a #Contextual Memory route until accepted
- Before creating a durable record, the agent checks what must survive, what changes current meaning, whether reasoning will matter later, whether behavior is required, and whether a reusable shape should guide future work. When none apply, no durable record is needed
- Each useful durable outcome is integrated before other work depends on it and before closeout. Each affected source is updated for the distinct part it defines. Useful reasoning remains in Memory and related sources are linked. Useful durable meaning does not remain only in chat
- Closeout accounts for accepted durable outcomes, unsettled reusable findings, and continuation state. Accepted durable meaning does not remain only in a #Contextual #Memory `route`. An accepted temporary choice may remain contextual when its source, scope, and expected expiration are clear.
- The Memory `entrypoint` inherits the loader's universal authority, reserved tag meanings, scoping, and management rules instead of restating them
- Working, Emerging, Crystallized, and Archived remain the four standard state `routes`, each with its own `entrypoint` and positive state semantics
- Decisions, Documents, Analysis, Ideas, Observations, Checkpoints, and Handoffs are optional Extension-provided roles under the standard Memory states. Their maintenance contracts remain linked from the corresponding state routes.
- Movement follows changes in meaning and links to the right #Core route or external system when Memory should not define the accepted result
- A new top-level Memory state requires explicit user direction because it changes the shared state model. Ordinary routed scopes remain recursively customizable.
- The authored source stays compact and complete for runtime use even as the stored Memory tree grows

## Standard Loading And Content Shape

| Memory `route`                                                                                            | Loading contract                                                                 |
| --------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| Memory                                                                                                    | Core #LoadNow baseline `entry`                                                   |
| Working and Crystallized                                                                                  | Core #LoadNow navigation for active and accepted records                         |
| Emerging                                                                                                  | Core target-sensitive #KeepInMind continuity review                              |
| Archived                                                                                                  | Core on-demand historical navigation without #LoadNow or #KeepInMind             |
| Optional Extension routes: Decisions, Documents, Analysis, Ideas, Observations, Checkpoints, and Handoffs | Not part of Core; each selected Extension supplies its own route source and tags |

Canonical leaf `entrypoints` ship with empty generated `Entries`. The repository dogfood tree may add local records through regenerated indexes without changing authored source.

## Verification

- Core installation closure tests verify the Core Memory state `routes`, loading policy, root authority wording, `managed route` reconciliation through scopes, generated navigation, and legacy user-owned Sessions preservation during reinstall
- Compare every canonical Memory source with its dogfood authored content outside generated `Entries`
- Run `open-forge index` and `open-forge doctor` against both the repository and `src/open-forge/`

## Entries

- [Current maintenance contract for the installable Archived Memory entrypoint](archived.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Archived #Historical
- [Current maintenance contract for the Core Crystallized Memory entrypoint and its optional Extension-provided Decisions and Documents routes](crystallized/_crystallized.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Crystallized
- [Current maintenance contract for the Core Emerging Memory entrypoint and its optional Extension-provided Analysis, Ideas, and Observations routes](emerging/_emerging.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Emerging #Candidate
- [Current maintenance contract for the Core Working Memory entrypoint and its optional Extension-provided Checkpoints and Handoffs routes](working/_working.md) - #Memory #Document #CurrentTruth #Evergreen #Maintenance #Governance #Payload #Working

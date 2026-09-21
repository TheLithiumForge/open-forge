---
open-forge:
  description: Current framework-wide acceptance, contextual status, current truth, Evergreen synchronization, and relationship between present state and rationale
  responsibility: Define how direction becomes accepted state and how affected representations stay aligned without turning tags or records into authority
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Authority, Acceptance, Synchronization]
---

# Accepted State And Synchronization

## Scope

This document defines how Open Forge distinguishes unsettled context from accepted current state and keeps affected representations synchronized.

It is not another runtime primitive or Memory state. It connects existing authority, truth-status, synchronization, and recorded-state contracts.

The [loader](../../../../loader.md#defined-tags) remains authoritative for the exact installed meanings of #Contextual, #CurrentTruth, and #Evergreen. This current view explains how those independent properties work together across Core, Memory, workspace routes, documents, and declared external systems.

## Independent Properties

Acceptance and synchronization answer different questions:

| Property      | Question                                                                  | Meaning                                                   |
| ------------- | ------------------------------------------------------------------------- | --------------------------------------------------------- |
| #Contextual   | May this inform work without being treated as accepted state?             | Supporting, uncertain, exploratory, or historical context |
| #CurrentTruth | Is this accepted current state within its stated scope?                   | Present state that may govern dependent understanding     |
| #Evergreen    | Must this material stay aligned when accepted state affecting it changes? | Synchronization obligation only                           |

#Evergreen does not create authority, acceptance, loading, or precedence. Material may be #CurrentTruth without needing synchronization, and a derived explanation may be #Evergreen without becoming authoritative for the underlying fact it represents.

Loading and tags make material visible or classifiable. Authority still comes from the source type, scope, accepted direction, and declared external authority.

## Acceptance

Validation establishes whether evidence supports a claim. Acceptance establishes which knowledge or decisions may be treated as current within their scope. Restoring or moving a record does not establish acceptance by itself.

Clear direction is accepted within its stated scope without requiring a ritual phrase or redundant confirmation. A request to perform an action delegates the routine, reversible, in-scope choices required to carry it out when the request determines them clearly. It does not silently accept an unsettled consequential scope, risk, cost, external effect, irreversibility, or accepted direction.

Tentative, comparative, exploratory, or ambiguous language remains contextual until dependent work requires clarification or the direction becomes clear.

| Direction                                    | Normal treatment                                                               |
| -------------------------------------------- | ------------------------------------------------------------------------------ |
| “Consider architecture B”                    | Emerging candidate or active analysis                                          |
| “Both are possible. Investigate B.”          | Emerging candidate or active analysis                                          |
| “I definitely prefer architecture B”         | Accepted within the stated decision scope                                      |
| “Use architecture B. That is our direction.” | Update the affected authoritative current source and preserve useful rationale |
| “Use B only for this experiment”             | Scoped Working or experimental state, not universal current truth              |

Acceptance never extends beyond the expressed scope. A clear local preference does not silently become a workspace-wide rule, and a current experiment does not become durable canon.

Agents may suggest any durable change. They may apply it directly when clear direction or necessary routine, reversible, in-scope delegated authority provides sufficient authority, then report what changed. When ambiguity would materially affect the result, they preserve contextual state or ask before dependent work treats one interpretation as accepted.

## Applying Accepted Change

Acceptance does not determine durability, reusable shape, or binding behavior. Before creating durable knowledge from accepted direction, determine what must survive the task, what changes accepted current meaning, whether rationale will matter later, whether behavior is mandatory, and whether an inspectable shape should guide future related results. If none apply, create no new durable knowledge artifact.

When accepted direction changes current state:

1. Update every affected authoritative document, `route`, implementation, external system, or other source for the distinct part of current meaning it defines
2. Update only affected editable #Evergreen material before work depends on it and no later than closeout
3. Preserve useful rationale in a Decision when the reason may matter later
4. Put mandatory future behavior in the matching Directive or another applicable Axiom
5. Create or update a Pattern only when an accepted inspectable shape should guide future related results or repeated changes
6. Keep an explicitly accepted temporary choice in Working when its source, scope, and expected expiration are clear
7. Preserve useful context from the previous state in the appropriate archive
8. Update relationships so old locations no longer imply current authority, and report affected material that could not be updated

The current authoritative source states the accepted concept well enough to use on its own. A linked decision explains why when useful rationale exists. This is limited overlap at distinct entry boundaries, not competing authority.

The [Memory transition contract](memory/transitions.md#integration-and-closeout) defines reconciliation across durable outcomes, temporary continuation state, and unsettled reusable findings. The [Core primitive model](primitives/model.md) defines the distinct questions answered by Directives, Guidance, Patterns, Skills, Templates, and Map `routes`. Workflow recipes remain useful procedures selected and followed through the native Skill mechanism; they are not a separate Core primitive.

Selection remains distinct from execution. A native Skill selects a Workflow recipe when its goal fits; following its Steps is execution under the Skill's native contract, active Directives, and current user direction.

## Current Views, Decisions, And History

Current documents integrate what is accepted now. Decisions preserve why important choices were accepted, including useful alternatives, constraints, and consequences. Archives preserve non-current context after useful present meaning has been extracted.

An Evergreen current document explains the present concept without forcing readers to reconstruct it from decisions. A decision links forward to the authoritative source that expresses its current result, and that source may link back when the rationale helps.

The [Memory model](memory/model.md#authority-boundary) defines the knowledge authority of accepted records and how relevant outcomes reach their matching categories without giving Memory those categories' roles.

## Boundaries

Open Forge does not automatically turn every conversation, activity, observation, or tool result into durable state.

The [Memory model](memory/model.md#recorded-state-threshold) defines the capture threshold for Working, Emerging, accepted, and historical records. Acceptance and Evergreen synchronization remain deliberate even when capture is cheap and Git makes changes recoverable.

## Related Current Sources

- [Framework Architecture](architecture.md)
- [Memory Architecture](memory/_memory.md)
- [Memory transitions](memory/transitions.md)
- [Core primitive model](primitives/model.md)
- [Patterns](primitives/patterns.md)
- [Loading and continuity](routing/loading.md)

## Decisions And Rationale

- [Memory model](../../decisions/framework/memory-model.md)
- [Tag semantics](../../decisions/framework/tags.md)
- [Typed authority and role terminology](../../decisions/framework/authoritative-source-terminology.md)

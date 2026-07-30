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

| Property | Question | Meaning |
|---|---|---|
| #Contextual | May this inform work without being treated as accepted state? | Supporting, uncertain, exploratory, or historical context |
| #CurrentTruth | Is this accepted current state within its stated scope? | Present state that may govern dependent understanding |
| #Evergreen | Must this material stay aligned when accepted state affecting it changes? | Synchronization obligation only |

#Evergreen does not create authority, acceptance, loading, or precedence. Material may be #CurrentTruth without needing synchronization, and a derived explanation may be #Evergreen without becoming authoritative for the underlying fact it represents.

Loading and tags make material visible or classifiable. Authority still comes from the source type, scope, accepted direction, and declared external authority.

## Acceptance

Clear direction is accepted within its stated scope without requiring a ritual phrase or redundant confirmation. A request to perform an action also accepts decisions required to carry out that action when the request determines them clearly.

Tentative, comparative, exploratory, or ambiguous language remains contextual until dependent work requires clarification or the direction becomes clear.

| Direction | Normal treatment |
|---|---|
| “Consider architecture B” | Emerging candidate or active analysis |
| “Both are possible. Investigate B.” | Emerging candidate or active analysis |
| “I definitely prefer architecture B” | Accepted within the stated decision scope |
| “Use architecture B. That is our direction.” | Update the affected authoritative current source and preserve useful rationale |
| “Use B only for this experiment” | Scoped Working or experimental state, not universal current truth |

Acceptance never extends beyond the expressed scope. A clear local preference does not silently become a workspace-wide rule, and a current experiment does not become durable canon.

Agents may suggest any durable change. They may apply it directly when clear direction or the requested action provides sufficient authority, then report what changed. When ambiguity would materially affect the result, they preserve contextual state or ask before dependent work treats one interpretation as accepted.

## Applying Accepted Change

When accepted direction changes current state:

1. Update the authoritative document, route, code, external system, or other source that expresses the result
2. Update only affected editable #Evergreen material before work depends on it and no later than closeout
3. Preserve useful rationale in a decision when the reason may matter later
4. Preserve useful context from the previous state in the appropriate archive
5. Update relationships so old locations no longer imply current authority
6. Report affected material that could not be updated

The current authoritative source states the accepted concept well enough to use on its own. A linked decision explains why when useful rationale exists. This is limited overlap at distinct entry boundaries, not competing authority.

## Current Views, Decisions, And History

Current documents integrate what is accepted now. Decisions preserve why important choices were accepted, including useful alternatives, constraints, and consequences. Archives preserve non-current context after useful present meaning has been extracted.

An Evergreen current document explains the present concept without forcing readers to reconstruct it from decisions. A decision links forward to the authoritative source that expresses its current result, and that source may link back when the rationale helps.

The [Memory model](memory/model.md#authority-boundary) defines why recorded behavior does not become active and why accepted behavior that should guide work moves to the matching #Core route.

## Boundaries

Open Forge does not automatically turn every conversation, activity, observation, or tool result into durable state.

The [Memory model](memory/model.md#recorded-state-threshold) defines the capture threshold for Working, Emerging, accepted, and historical records. Acceptance and Evergreen synchronization remain deliberate even when capture is cheap and Git makes changes recoverable.

## Related Current Sources

- [Framework Architecture](architecture.md)
- [Memory Architecture](memory/_memory.md)
- [Core primitive model](primitives/model.md)
- [Loading and continuity](routing/loading.md)

## Decisions And Rationale

- [Memory model](../../decisions/memory-model.md)
- [Tag semantics](../../decisions/tags.md)
- [Typed authority and role terminology](../../decisions/authoritative-source-terminology.md)

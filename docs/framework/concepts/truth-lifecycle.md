# Truth Lifecycle

## Description

This maintainer concept explains how Open Forge moves from uncertain context to accepted state and synchronized current views. It is not an additional runtime primitive or Memory state.

## Reading Order

The installed contract progresses from general meaning to local state:

```text
loader authority and truth tags
  -> Memory state model
  -> state-specific entrypoints
  -> current documents and decision history
  -> project-owned routed destinations
```

Each rule lives where the agent first has enough context to understand it.

## Loader Semantics

#Contextual marks supporting material that is not accepted current truth.

#CurrentTruth marks accepted current state within its stated scope. A clear user instruction, correction, or confirmation is accepted within its stated scope without another confirmation. A request to act also accepts decisions required to perform that action. Ambiguous direction remains contextual until work depends on it.

#Evergreen marks material that must stay aligned when accepted state affecting it changes. Its update boundary applies regardless of route type or Memory placement. #Evergreen creates no authority or load policy.

## Memory Lifecycle

Memory is authoritative for the capture, movement, consolidation, and archival of recorded state:

```text
working       active resumability context
emerging      useful but unsettled material
crystallized  accepted durable current memory
archived      non-current historical context
```

The states are not a mandatory sequence. Their entrypoints define the invariants that make each state valid, including candidate preservation, current-truth consolidation, extraction before archival, and validation before restoration.

Memory may record or explore behavior without activating it. Accepted behavior that should guide future work belongs in the matching #Core route.

## Current Views And History

Current documents are authoritative for coherent present state. Any routed material may carry #Evergreen when it must stay aligned with accepted state.

Decisions preserve useful rationale and historical context without becoming a competing source for current behavior. Archived Memory preserves non-current material after useful current state has been extracted.

## Why

This distribution avoids a second lifecycle abstraction. The loader explains universal authority and tag behavior; Memory explains its own states; specific routes add only the rules needed to keep their contents valid.

## Alignment Checks

Truth handling is aligned when:

- #CurrentTruth authority and #Evergreen synchronization remain independent
- clear user direction does not trigger redundant confirmation
- an action request accepts decisions required to perform it
- ambiguous or exploratory material stays contextual
- only affected editable #Evergreen material changes
- Memory may describe behavior without activating it
- current documents contain present state while decisions and archives preserve useful history
- every rule is understandable when first encountered in the installed top-down reading order

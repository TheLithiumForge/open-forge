---
open-forge:
  description: Open Forge separates tags for loading, Framework composition, truth status, synchronization, and ordinary classification
  tags: [Memory, Decision, CurrentTruth, Tags, Routing]
---

# Tags

## Context

Earlier tags mixed identity, loading, truth, synchronization, and ordinary classification. Some names duplicated the same behavior, while tool-like wording implied enforcement that tags and Markdown cannot provide.

## Decision

Open Forge uses a small defined tag vocabulary for loading, Framework composition, truth status, and synchronization. Other tags remain ordinary routing and search signals unless a loaded contract defines them.

Defined loading tags use direct agent-imperative semantics. No tag creates authority, scope, precedence, or mechanical enforcement by itself.

#Contextual and #CurrentTruth distinguish supporting from accepted state. #Evergreen independently marks synchronization responsibility. #Core, #Memory, and #Extension describe Framework composition rather than authority levels.

Tags remain bare in Markdown so people, agents, and deterministic tools can recognize them without private syntax.

## Rationale

Separating visibility, authority, truth, and synchronization prevents one tag from silently carrying several contracts.

Imperative wording accurately describes agent responsibility. Bare tags provide cheap anchors and future graph signals while readable prose and routes retain the full meaning.

## Alternatives And Tradeoffs

- #OpenForge fused Framework identity with loading and became redundant with composition tags and route placement
- #LoadWithParentEntrypoint duplicated #LoadNow traversal
- #LoadForPostWorkReview deferred loading to the point where evaluations showed agents forget it
- Treating #Evergreen as authority would conflate synchronization with accepted truth
- Hash-free metadata could be easier to style but would lose the current cheap visible anchor

Defined tags need stable meanings because changing them affects routing, authoring, tooling, and current records together.

## Consequences

- The loader is authoritative for exact installed definitions
- Load-policy tags appear only when their baseline or continuity cost is justified
- Route type tags support selection and search while their entrypoints define role semantics
- Normal words define the local concept; tags classify or point to established concepts
- Retired tags remain historical rationale rather than installed warnings

## Authoritative Sources

- [Loader defined tags](../../../loader.md#defined-tags)
- [Routing loading contract](../documents/framework/routing/loading.md)
- [Framework composition model](../documents/architecture.md#framework-composition)
- [Canonical Markdown syntax](../documents/framework/markdown/syntax.md)

## Decision Relationships

- [Loading reliability](loading-reliability.md)
- [Memory model](memory-model.md)
- [Routing model](routing-model.md)
- [Canonical Markdown authoring](canonical-markdown.md)

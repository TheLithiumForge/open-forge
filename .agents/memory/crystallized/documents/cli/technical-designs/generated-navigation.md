---
open-forge:
  description: Exact shared formation and projection design for Generated Navigation
  responsibility: Define how observed catalogue evidence and intended logical membership form deterministic generated-navigation input
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, GeneratedNavigation, Routing]
---

# Generated Navigation Technical Design

## Boundary

`Framework/GeneratedNavigation/` owns pure deterministic formation, projection,
generated-region recognition, and bounded change facts. It reads no document
bodies while forming the source/topology input and performs no filesystem
effect. Commands retain selection, finding, status, write policy, application,
and result meaning.

## Callable Surface

The intended-membership formation overload is exactly:

```csharp
Build(SourceCatalogue observedCatalogue, IReadOnlyList<SourceLogicalSource> intendedSources)
```

It extends existing formation without creating a prospective catalogue or
source framework. Existing `Build(SourceCatalogue)` preserves exact current-state
behavior and member identity by delegating with the observed catalogue's current
sources. Formation continues to use `SourceRouteTopologyBuilder`.

The projection request consumes the resulting cohesive immutable formation fact
instead of independently assembling sources and topology.

## Formation

The formation retains the observed `SourceCatalogue` as catalogue evidence.
Intended sources separately define logical membership, lookup, topology, Loader
presence, Loader roots, and intended target collisions.

Root-entrypoint and route-parent ambiguities derive from intended sources.
Physical-alias ambiguities derive only from retained observed candidates. Target
collisions compare intended base and overwrite layers with one another and with
remaining observed occupants while excluding removed-layer evidence. An
unobserved intended source never fabricates a candidate, catalogue issue, or
physical alias.

Formation reads no document bodies, discovers no generated lines, and owns no
command selection, finding, status, write policy, or application meaning. It
uses real current catalogue evidence and creates no virtual filesystem,
temporary checkout, prospective source catalogue, or hidden Index.

## Loader And Topology Rules

When a Loader is present, intended roots are the structurally valid, physically
unique recognized entrypoints that directly represent each `.agents/<slug>`
folder. Generated `Entries` remain comparison input only.

A missing Loader produces zero roots. Operand-free Index selection blocks, while
an explicit complete detached selection may still proceed. A missing
intermediate entrypoint keeps the lower tree detached; no parent is invented.
Multiple recognized entrypoints representing one root folder are ambiguous.

Only physical aliases proven on the current host may collapse, and only when
their route and document-form identities are compatible. Proven incompatible
aliases block. Broader portable case, Unicode, and device-name equivalence is
deferred rather than guessed. Formation does not change current-visible Route or
Context facts.

## Projection And Region Facts

Projection consumes accepted source topology and authored metadata facts. It
recognizes one already selected valid generated region, forms canonical direct-
child `Entries` bytes including the empty projection, deduplicates by physical
identity, preserves deterministic ordinal order, and returns immutable expected-
region and bounded-change facts.

The region has exactly one accepted final marker pair with non-nested,
non-overlapping ownership. Reads are strict UTF-8. A malformed boundary produces
typed failure facts. Change facts preserve bytes outside the generated interior
and distinguish exact unchanged bodies from bounded updates. The capability
does not apply those changes.

## Related Current Sources

- [CLI Architecture](../architecture.md)
- [Index Interface Contract](../contracts/index-candidate/interface.md)
- [Index Behavior Contract](../contracts/index-candidate/behavior.md)
- [Shared CLI Operation Contract](../shared-operation-contract.md)

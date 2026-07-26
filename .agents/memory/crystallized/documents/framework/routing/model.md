---
open-forge:
  description: Current Open Forge model for entrypoints, entries, top-down selection, direct-child navigation, routed destinations, and relevance-scaled context
  responsibility: Define how Open Forge routes expose and select relevant destinations without flattening or replacing their meaning
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Routing, Entrypoint, Selection, Context]
---

# Routing Model

## Scope

This document is authoritative for how Open Forge exposes and selects routed destinations.

The [scope and inheritance contract](scope.md) defines how route placement narrows meaning. The [loading contract](loading.md) defines when visible routes are read or refreshed. The [path contract](paths.md) defines how authored links and CLI route identities resolve. The [routed Markdown contract](../markdown/routes.md) defines their canonical representation.

Each installed component source remains authoritative for the meaning and behavior of its own routed contents. Routing connects those sources without becoming a central copy of their contracts.

## Route Model

A `route` identifies a destination and explains why it may matter.

An `entrypoint` is the Markdown file that makes one folder routable. Every folder in a visible route chain contains exactly one recognized entrypoint. Open Forge authors one canonical filename, while the [compatibility boundary](../markdown/compatibility.md) records input aliases accepted during migration or interoperability.

An `entry` is one generated line under an entrypoint's final `Entries` section. It exposes one direct routed file or direct child entrypoint through:

- A natural-language description sufficient to select or skip the route
- A containing-file-relative Markdown destination
- Compact tags for loading, type, scope, topic, and search signals

An entrypoint exposes direct children only. Nested content becomes visible through the next selected entrypoint rather than being flattened into every ancestor.

This preserves intermediate scope, keeps indexes local, and prevents higher route surfaces from growing with an entire subtree.

## Selection Surface

The entry description is the pre-load selection surface. It communicates enough trigger, purpose, or outcome to choose the route without opening its body.

The destination identifies where the complete routed content lives. Tags make useful signals cheap to scan, but they do not replace readable descriptions or route placement.

Mandatory, workspace-wide, or otherwise consequential scope remains visible through the route and authored content rather than depending on a tag alone.

After selection, the routed source defines its complete concept, instruction, capability, recipe, record, or relationship. An optional frontmatter `responsibility` may bound what that opened file is responsible for defining. It does not participate in pre-load selection or create authority.

Generated entries remain navigation metadata. They do not privately define instructions, behavior, authority, current truth, or component semantics.

## Top-Down Navigation

Routing proceeds from known general context into selected detail:

1. Enter through the canonical workspace entry and loader
2. Read the baseline and continuity context required by the [loading contract](loading.md)
3. Use the goal and visible entries to select a relevant root route or scope
4. Read the selected entrypoint before considering its direct entries
5. Repeat through only the branches relevant to the work
6. Follow explicit links to the files or external systems authoritative for detailed truth

Selection uses the request, path, description, tags, ancestor meaning, and already loaded current truth. It does not require a hidden relevance registry.

A body may confirm its goal or purpose for a reader who arrives directly, but it does not repeat a large pre-load selection contract.

## Relationships And Detailed Truth

Routes may connect Framework content, workspace knowledge, source code, repositories, datasets, issue systems, or other declared external sources.

The routed destination remains authoritative for the detailed question delegated to it. A workspace route that points to an external system explains why and when that system matters without copying its contents.

Relative links, heading anchors, descriptions, and established tags provide explicit graph-like relationships. Derived semantic, vector, or graph tools may improve discovery, but their indexes remain advisory and rebuildable.

## Recursive Navigation

The same entrypoint and direct-child contract works at every depth.

A category can extend its own kind recursively, organize material through neutral scope routes, or link across branches. Component sources define any special meaning their direct files or child categories add.

A deep file below an unrepresented folder is not reachable through generated routing. Every intermediate folder that participates in a route needs its own entrypoint.

## Scaling Property

Open Forge has no fixed structural expansion ceiling.

Ordinary active-context cost grows primarily with selected route depth, selected branches, and followed relationships rather than with the total number of stored scopes. Unselected sibling projects or disciplines remain nearly absent from active context until integration work selects them deliberately.

This is a structural property, not a promise of constant lookup or reasoning performance.

## Related Current Sources

- [Scope and inheritance](scope.md)
- [Loading and continuity](loading.md)
- [Path identity and containment](paths.md)
- [Routed Markdown representation](../markdown/routes.md)
- [Framework Architecture](../architecture.md)
- [Canonical loader](../../../../../loader.md)

## Decisions And Rationale

- [Routing model](../../../decisions/routing-model.md)
- [Routing surfaces](../../../decisions/routing-surfaces.md)
- [Loading reliability](../../../decisions/loading-reliability.md)

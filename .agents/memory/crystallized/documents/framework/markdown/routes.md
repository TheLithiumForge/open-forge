---
open-forge:
  description: Current canonical Markdown and filename representation for Open Forge entrypoints, route entries, and generated index regions
  responsibility: Define the canonical file and Markdown representation shared by routed entrypoints and generated navigation
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Markdown, Authoring, Syntax, Routing]
---

# Routed Markdown Representation

## Scope

This document is authoritative for the Markdown and filename forms shared by routed Open Forge files.

It defines how routing constructs are represented. The [routing model](../routing/model.md), [scope contract](../routing/scope.md), [loading contract](../routing/loading.md), and installed component sources define what routes, scopes, loading, authority, and component-specific sections mean. Component maintenance documents preserve the corresponding source and verification obligations.

## Category Entrypoints

The canonical Open Forge filename is:

```text
_{folder-name}.md
```

For example, `.agents/patterns/` uses `_patterns.md`.

An Open Forge-authored category entrypoint is represented by:

1. Scoped frontmatter
2. One level-1 title
3. A compact definition of the category
4. Any category-level `Axioms` or boundaries
5. A final level-2 `Entries` section
6. One bounded generated region

Category meaning and `Axioms` appear before `Entries`. The [routing model](../routing/model.md) defines when a folder is routable and what an entrypoint exposes. The [scope and inheritance contract](../routing/scope.md) defines the meaning of local, missing, and sentinel Axioms.

When an entrypoint explicitly declares that it adds no local Axioms, the canonical form is:

```md
## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
```

An entrypoint may instead omit `Axioms` or leave the section empty. `none` is not a valid Axioms sentinel because loaded ancestor Axioms always remain active.

## Route Entries

Generated `Entries` use one canonical line shape:

```md
- [Description](relative/path.md) - #Type #Scope
```

Each line contains:

- One hyphen list marker
- One inline Markdown link
- A non-empty descriptive label
- A containing-file-relative destination
- One ` - ` separator
- One or more useful bare tags

The label provides enough trigger, purpose, or outcome for pre-load selection. The destination identifies the route. The tags provide compact loading, type, scope, and search signals. The [path contract](../routing/paths.md) defines destination resolution, normalization, encoding, and containment.

Each route entry stays on one physical line. Normal links elsewhere may target headings or external URLs.

The generated empty state is:

```md
- none - No entries - #Empty
```

It is the only `Entries` line without a link.

## Generated Regions

The loader and every category entrypoint end with:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

The marker-bounded body is derived navigation metadata. Deterministic generation may replace only the content between the markers and must preserve authored content outside them.

The `Entries` heading and generated region:

- Appear exactly once
- Form the final section
- Keep the markers complete and ordered
- Contain no authored prose

The [routing model](../routing/model.md) defines which direct destinations the region represents. Generated entries are derived navigation, not an independent definition of behavior, authority, or current truth.

## Related Current Sources

- [Canonical Markdown syntax](syntax.md)
- [Markdown compatibility boundary](compatibility.md)
- [Open Forge Routing scope](../routing/_routing.md)
- [Framework Architecture](../architecture.md)
- [Canonical loader](../../../../../loader.md)

## Decisions And Rationale

- [Routing surfaces](../../../decisions/routing-surfaces.md)
- [Scope and slugs](../../../decisions/scope-and-slugs.md)
- [Canonical Markdown authoring](../../../decisions/canonical-markdown.md)

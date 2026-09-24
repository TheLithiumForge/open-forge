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
3. A primary question and compact definition of the category
4. Any category-level `Axioms` or boundaries
5. A final level-2 `Entries` section
6. One bounded generated region

The category's primary question appears as an ordinary level-2 heading immediately after the title, followed by the definition and any necessary supporting explanation. The question describes the role. This presentation adds no metadata field or special parsing or loading rule, and it does not make the entrypoint define every answer beneath it.

Category meaning and `Axioms` appear before `Entries`. The [routing model](../routing/model.md) defines when a folder is routable and what an entrypoint exposes. The [scope and inheritance contract](../routing/scope.md) defines the meaning of local, missing, and sentinel Axioms.

For a newly authored entrypoint that adds no local Axioms, use this canonical form:

```md
## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.
```

For compatibility, readers also accept an entrypoint that omits `Axioms` or leaves the section empty. Both forms add no local Axioms and leave loaded ancestor Axioms active; they are accepted input, not the preferred authoring form. `none` is not a valid Axioms sentinel because loaded ancestor Axioms always remain active.

The frozen MVP does not yet enforce this metadata-integrity target. It may synthesize a fallback description and tags when generating an entry. The [MVP Architecture](../../cli/mvp-architecture.md#metadata-and-overwrite-integrity) records that temporary limitation without changing this canonical form.

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
- One `-` separator
- One or more useful bare tags

The label provides enough trigger, purpose, or outcome for pre-load selection. The destination identifies the route. The tags provide compact loading, type, scope, and search signals. The [path contract](../routing/paths.md) defines destination resolution, normalization, encoding, and containment.

Each route entry stays on one physical line. Normal links elsewhere may target headings or external URLs.

The generated empty state is:

```md
- none - No entries - #Empty
```

It is the only `Entries` line without a link.

## Generated Regions

The loader and every category entrypoint contain one generated section:

```md
## Entries

- none - No entries - #Empty
```

One top-level canonical ATX `## Entries` heading owns the body from the end of
its heading span to the next top-level heading of level 1 or 2, or EOF. Fenced,
indented-code, quoted, nested-list, Setext, differently cased and differently
leveled lookalikes do not establish this semantic section. Trailing horizontal
heading whitespace and an initial BOM are accepted. Duplicate `## Entries`
headings are diagnosed; no arbitrary first section is selected.

The heading-owned body contains derived navigation only. Generation replaces that
body and preserves every byte outside it, including following authored sections.
Canonical authoring places Entries last, but a following section does not make
its bounded body invalid. Every generated entry reflects a reachable direct
routed source's authored description and tags. Generation and validation fail
closed when the section, destination, or metadata meaning is unavailable.

Old generated guard comments are migration input only. Readers ignore exact
retired guard lines inside this section; Index removes them when rewriting its
body. They never define ownership. New documents never emit them.

The [routing model](../routing/model.md) defines which direct destinations the region represents. Generated entries are derived navigation, not an independent definition of behavior, authority, or current truth.

## Related Current Sources

- [Canonical Markdown syntax](syntax.md)
- [Markdown compatibility boundary](compatibility.md)
- [Open Forge Routing scope](../routing/_routing.md)
- [Framework Architecture](../architecture.md)
- [Canonical loader](../../../../../loader.md)

## Decisions And Rationale

- [Routing surfaces](../../../decisions/framework/routing-surfaces.md)
- [Scope and slugs](../../../decisions/framework/scope-and-slugs.md)
- [Canonical Markdown authoring](../../../decisions/framework/canonical-markdown.md)

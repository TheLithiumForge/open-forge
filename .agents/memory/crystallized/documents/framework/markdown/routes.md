---
open-forge:
  description: Current canonical Markdown representation shared by Open Forge entrypoints, route entries, routed paths, and generated indexes
  responsibility: Define the shared authored representation of routed Open Forge Markdown
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Markdown, Authoring, Syntax, Routing]
---

# Routed Markdown Representation

## Scope

This document is authoritative for the Markdown and filename forms shared by routed Open Forge files.

It defines how routing constructs are represented. The [routing model](../routing/model.md), [scope contract](../routing/scope.md), [loading contract](../routing/loading.md), and installed component sources define what routes, scopes, loading, authority, and component-specific sections mean. Component maintenance documents preserve the corresponding source and verification obligations.

## Category Entrypoints

A routed folder has exactly one recognized category `entrypoint`.

The canonical Open Forge filename is:

```text
_{folder-name}.md
```

For example, `.agents/patterns/` uses `_patterns.md`.

An Open Forge-authored category entrypoint contains:

1. Scoped frontmatter
2. One level-1 title
3. A compact definition of the category
4. Stable category-level `Axioms` or boundaries when needed
5. A final level-2 `Entries` section
6. One bounded generated region

Category meaning and `Axioms` appear before `Entries`. Detailed routed content belongs in direct files or child entrypoints.

A missing `Axioms` section adds no local Axioms. When an entrypoint declares the absence explicitly, use one `inherited` or `none` sentinel without substantive local Axioms. Neither form cancels loaded ancestor Axioms.

## Route Entries

Generated `Entries` use one canonical line shape:

```md
- [Decision-grade description](relative/path.md) - #Type #Scope
```

Each line contains:

- One hyphen list marker
- One inline Markdown link
- A non-empty descriptive label
- A containing-file-relative destination
- One ` - ` separator
- One or more useful bare tags

The label explains why the route matters. The destination identifies it. The tags make loading, type, scope, and search signals cheap to inspect.

Route-entry destinations identify files. They do not contain query strings or fragment anchors. Percent-encode unsafe path characters so the destination remains one whitespace-free path. Canonical output does not use angle-bracket destinations.

Each route entry stays on one physical line. Normal links elsewhere may target headings or external URLs.

The generated empty state is:

```md
- none - No entries - #Empty
```

It is the only `Entries` line without a link.

## Routed Paths

Canonical Open Forge-authored slugs use stable lowercase kebab-case unless an external stable name requires another spelling.

A concrete slug and its entrypoint appear together:

```text
mobile-app/
  _mobile-app.md
```

Installed paths and generated entries contain concrete slugs. Placeholders such as `[scope]`, `[route]`, or `[state]` are documentation, Template, and tooling notation only.

User-authored routed files may use any clear portable filename that does not collide with a reserved entrypoint or companion form.

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
- List direct routed files and direct child entrypoints only

The loader lists direct active root entrypoints. A category entrypoint lists direct routed files and child entrypoints. Each component source defines any narrower rule for what its generated entries represent.

Generated entries never privately define behavior, authority, or current truth. Those meanings come from authored routed sources.

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

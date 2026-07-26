---
open-forge:
  description: Current canonical Markdown syntax for Open Forge metadata, headings, lists, links, code literals, tags, and semantically named sections
  responsibility: Define the canonical Markdown syntax used to represent Open Forge contracts
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Markdown, Authoring, Syntax]
---

# Canonical Markdown Syntax

## Scope

This document is authoritative for the canonical Markdown notation used when Open Forge assigns meaning to authored structure.

It defines notation, not the behavior of routes, Framework primitives, Memory states, or deterministic tools. The [routed Markdown contract](routes.md) defines the shared representation of routed files, and each component's authoritative source defines its own semantic sections and behavior. Ordinary prose remains ordinary Markdown unless a Framework contract assigns additional meaning to part of it.

## Design Goals

Open Forge Markdown is designed to be:

- Human-readable in raw text
- Understandable without the CLI or a private database
- Cheap for agents to scan and navigate
- Stable and reviewable in Git
- Explicit enough for deterministic validation
- Connected through normal relative links
- Canonical where syntax carries Framework meaning

Open Forge chooses one preferred authoring form for every machine-meaningful construct. [Compatibility input](compatibility.md) may remain readable during migration or interoperability, but it does not become another recommended authoring form.

## Canonical Forms

Open Forge-authored files use:

- ATX headings using `# Heading`, `## Heading`, and deeper levels as needed
- Blank-line separation using `\n\n` between paragraphs, headings, lists, and fenced blocks
- Hyphens using `- item` for unordered lists
- Sequential decimal markers using `1. item`, `2. item`, and `3. item` for ordered lists
- Triple-backtick fences using ```` ``` ```` before and after code blocks
- Inline Markdown links using `[label](destination)` for routes and clickable references
- YAML frontmatter using `---` before and after the metadata block when indexed metadata is required
- Bare tags using `#Tag` where tags appear in prose or route metadata

Use short headings, compact paragraphs, line-based lists, and examples only when they clarify the contract. Use a table when comparison or exact mapping is clearer than prose or a list. Machine-readable sections use only their declared line shape.

## Frontmatter

Open Forge-authored entrypoints and indexed Markdown files use scoped metadata:

```yaml
---
open-forge:
  description: Current architecture of an example system
  responsibility: Define the accepted structure, relationships, and boundaries of the example system
  tags: [Memory, Document, CurrentTruth, Architecture]
---
```

The canonical block:

- Appears at the start of the file
- Uses the `open-forge:` scope
- Provides one natural-language `description`
- May provide one natural-language `responsibility`
- Provides a YAML list of tags without `#` prefixes

The `description` is the pre-load selection surface. It explains enough purpose, trigger, or outcome for a reader to select or skip the route without opening its body. It is natural and suggestive rather than a repeated formula.

The optional `responsibility` is the stable boundary of what the file is responsible for defining. It guides edits after the file is opened, does not create authority or loading behavior, and changes only through a deliberate redefinition, split, or merge of that boundary. Content that develops an independent responsibility moves to another authoritative source and is linked instead of widening the field casually.

Use `responsibility` only when it adds a useful boundary beyond the route and description. Category entrypoints normally do not need it because their route, definition, and generated entries already express their responsibility.

Direct-load files that are never indexed do not need Open Forge metadata unless another tool or contract requires it. Standard files such as `SKILL.md` keep the metadata required by their active runtime.

## Tags

Tags add compact loading, type, state, scope, topic, or search signals. They do not replace readable scope in paths and descriptions.

Open Forge-authored tags:

- Stay bare in Markdown, such as #Core or #CurrentTruth
- Omit the `#` prefix inside frontmatter lists
- Begin with a letter
- Use singular PascalCase names by default
- Contain only letters, numbers, and internal hyphens
- Appear only when they improve selection, classification, loading, or retrieval

The [loader](../../../../../loader.md#defined-tags) is authoritative for reserved tag meanings. Ordinary tags remain routing and search signals unless a loaded source explicitly defines more.

## Headings And Semantic Sections

ATX headings are the canonical section syntax. A contract that names a heading also establishes its exact heading level, spelling, and order.

For example:

```md
## Axioms

- Keep the routed contract explicit.
```

Setext headings and alternate heading names may render as Markdown, but Open Forge does not promise to interpret them as equivalent semantic sections.

The component source that requires a named section defines its meaning. Markdown syntax alone does not make `Axioms` binding, `Required Routes` unconditional, or another heading semantically active.

## Links And Code Literals

Use relative Markdown links for routed destinations and related repository documents. Resolve each local destination from the file containing the link.

Use backticks for:

- Commands and flags
- Code literals
- Defined Open Forge terms when precision matters
- Concrete paths discussed as text rather than used as destinations

Use normal words when their ordinary English meaning is intended. Keep tags bare so Markdown-aware agents, search, graph, and future retrieval tools can recognize them.

Prefer anchored relative links to repeated explanations when another authoritative source already contains the meaning.

## Related Current Sources

- [Routed Markdown representation](routes.md)
- [Markdown compatibility boundary](compatibility.md)
- [Framework Architecture](../architecture.md)
- [Canonical loader](../../../../../loader.md)

## Decisions And Rationale

- [Canonical Markdown authoring](../../../decisions/canonical-markdown.md)
- [User-facing writing](../../../decisions/user-facing-writing.md)
- [Tag semantics](../../../decisions/tags.md)

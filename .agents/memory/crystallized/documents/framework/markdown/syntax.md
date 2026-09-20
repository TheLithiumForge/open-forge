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

- Readable in raw text
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
- Triple-backtick fences using ` ``` ` before and after code blocks
- Inline Markdown links using `[label](destination)` for routes and clickable references
- YAML frontmatter using `---` before and after the metadata block when indexed metadata is required
- Bare tags using `#Tag` where tags appear in prose or route metadata

Machine-readable sections use only their declared line shape. For this repository's maintained prose, the [Open Forge Writing Standard](../../maintenance/writing.md) defines detailed prose guidance; it does not add an installed Markdown contract.

## Visible Source And Control Markers

Open Forge keeps agent-facing meaning visible in raw and rendered Markdown.
HTML comments do not carry instructions, selection guidance, behavioral
requirements, or other authored meaning.

The only canonical HTML comments are these exact machine-owned boundary
tokens:

```text
<!-- open-forge:start -->
<!-- open-forge:end -->
```

They must be paired and position-valid under the owning workspace-block contract. The markers contain no instruction body; Markdown
between them remains visible.

Generated Entries use heading boundaries, with no comment tokens.

Templates keep removable source guidance in visible multiline `{...}`
placeholders. Ordinary prose uses visible Markdown rather than another comment
or renderer-specific concealment mechanism.

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

Deterministic tools interpret this exact Open Forge metadata shape rather than
promising arbitrary YAML support. Canonical output uses the `open-forge:`
scope, single-line or indented multiline text values, and an inline tag list.
The [compatibility boundary](compatibility.md) separates current canonical
syntax, frozen-MVP compatibility, and historical CLI-v2 proposals.

Canonical output and compatible input are distinct. The new CLI must accept or
reject noncanonical value forms through an explicit compatibility decision. A
YAML implementation does not make arbitrary YAML part of the Framework.

Authored frontmatter must be semantically complete: a non-empty natural-language `description` must describe the routed file accurately enough to select or skip it, and tags must accurately classify its loading, role, state, scope, or useful topic. Tools and review fail closed when required authored metadata is missing, malformed, ambiguous, or semantically inconsistent with the routed source rather than inventing meaning from filenames or bodies.

The frozen MVP has not implemented this enforcement yet. It may fabricate a fallback description and tags; the [MVP Architecture](../../cli/mvp-architecture.md#metadata-and-overwrite-integrity) records that temporary liability without changing the canonical contract.

The `description` helps a reader decide whether to open the file. It provides enough purpose, trigger, or outcome to select or skip the route before loading its body. It is natural and suggestive rather than a repeated formula.

The optional `responsibility` helps an editor decide what belongs in the file. It states the stable boundary of what the file defines and guides edits after the file is opened. It creates no authority or loading behavior. Change it only through a deliberate redefinition, split, or merge. Move content with an independent responsibility to another authoritative source and link to it.

Use `responsibility` only when it adds a useful boundary beyond the route and description. Category entrypoints normally do not need it because their route, definition, primary question, and generated entries already express their responsibility.

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

CommonMark Setext headings may be discovered and selected structurally by the
new CLI. They do not become canonical Open Forge authoring or equivalent
semantic sections merely because the parser represents them as headings. Parser
extensions do not add other public heading forms without a later compatibility
decision.

The component source that requires a named section defines its meaning. Markdown syntax alone does not make `Axioms` binding or another heading semantically active.

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
- [Historical CLI-v2 evidence](../../../../archived/cli-v2/_cli-v2.md)
- [Framework Architecture](../architecture.md)
- [Open Forge Writing Standard](../../maintenance/writing.md)
- [Canonical loader](../../../../../loader.md)

## Decisions And Rationale

- [Canonical Markdown authoring](../../../decisions/framework/canonical-markdown.md)
- [User-facing writing](../../../decisions/framework/user-facing-writing.md)
- [Tag semantics](../../../decisions/framework/tags.md)

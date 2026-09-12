---
open-forge:
  description: Open Forge uses one canonical authoring form wherever Markdown carries Framework meaning while treating compatibility syntax as input-only
  tags: [Memory, Decision, CurrentTruth, Framework, Markdown, Authoring, Syntax, Compatibility]
---

# Canonical Markdown Authoring

## Context

Markdown offers several visually similar ways to express headings, lists, metadata, links, and other structures. Treating every equivalent rendering as equivalent Framework syntax would make authoring, parsing, generation, review, and agent behavior needlessly ambiguous.

Open Forge also needed a stable way to state what an opened file defines without turning that metadata into another authority or loading mechanism.

## Decision

Open Forge uses one canonical authoring form whenever Markdown structure carries Framework meaning. Ordinary prose remains ordinary Markdown until a Framework contract assigns semantic meaning to a structure.

Generated output, examples, Templates, scaffolding, validation help, and public authoring guidance use the canonical form. Tools may accept selected legacy or interoperability forms as input without presenting them as equivalent authoring choices.

Frontmatter uses `description` as the natural-language pre-load selection surface. An optional `responsibility` field states the stable boundary of what the opened file defines. Responsibility guides edits but creates no authority, scope, or loading behavior.

## Rationale

One authored form reduces agent decision cost, parser surface, inconsistent examples, and maintenance ambiguity while leaving ordinary Markdown expressive.

Separating canonical output from compatible input allows migration and interoperability without teaching several competing contracts. Keeping the semantic contract in readable files also prevents CLI behavior from privately defining the Framework.

Responsibility metadata deters opportunistic scope growth. A changed responsibility therefore calls for deliberate redefinition, splitting, or merging rather than wording that merely absorbs unrelated content.

## Alternatives And Tradeoffs

- Supporting every Markdown equivalent as canonical would reduce immediate authoring constraints but multiply interpretation and validation paths
- Rejecting every legacy form would simplify tools but make migration and interoperability unnecessarily brittle
- Making `responsibility` mandatory would add metadata to entrypoints and small files whose route and content already make their boundary clear
- Letting a tool define syntax privately would make file-native behavior incomplete without that tool

Canonical forms require explicit documentation and careful migrations when an accepted form changes.

## Consequences

- Unsupported equivalents may render correctly for people without carrying Open Forge semantics
- Parsers distinguish accepted input from canonical generated output
- Canonical examples show each Framework symbol directly
- Responsibility remains optional and stable when it adds a useful editing boundary
- Authoring assistance may be generated from the same current Markdown contracts

## Authoritative Sources

- [Open Forge Markdown scope](../../documents/framework/markdown/_markdown.md)
- [Canonical Markdown syntax](../../documents/framework/markdown/syntax.md)
- [Routed Markdown representation](../../documents/framework/markdown/routes.md)
- [Markdown compatibility boundary](../../documents/framework/markdown/compatibility.md)

## Decision Relationships

- [Routing model](routing-model.md)
- [Routing surfaces](routing-surfaces.md)
- [Tag semantics](tags.md)
- [User-facing writing](user-facing-writing.md)
- [CLI frontmatter YAML boundary (historical)](../../../archived/cli-v2/decisions/cli-frontmatter-yaml-boundary.md)

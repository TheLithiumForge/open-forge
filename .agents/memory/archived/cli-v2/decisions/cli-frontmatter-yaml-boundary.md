---
open-forge:
  description: Historical CLI-v2 source: Use the yaml package for frontmatter syntax while Open Forge retains a narrow semantic schema and parser-independent document facts
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Frontmatter YAML Boundary

## Context

Open Forge needs precise metadata values, source locations, duplicate detection,
formatter-compatible sequences, quoted text, comments, and multiline scalars.
The frozen CLI's line-oriented parser rejected valid frontmatter after Prettier
wrapped an inline tag sequence. Extending that parser would require Open Forge
to reproduce increasingly subtle YAML lexical rules while still claiming to
support only three metadata fields.

The CLI must remain small, portable, safe for untrusted workspace input, and
independent from arbitrary YAML semantics.

## Decision

Use the `yaml` package to parse frontmatter syntax behind the focused
`MarkdownDocumentFacts` boundary. This is a boundary-specific production
dependency, not a general schema-library policy and not support for arbitrary
YAML documents.

Parse only one size-bounded frontmatter region with strict YAML 1.2 behavior,
the failsafe schema, string keys, unique keys, line tracking, and alias
expansion disabled. Inspect the parsed document nodes and reject aliases, anchors,
explicit tags, directives, multiple documents, nested unsupported values, and
every other construct outside the accepted Open Forge metadata contract.

Open Forge validation then accepts only `description`, optional
`responsibility`, and `tags` from exactly one recognized `open-forge`, `rune`,
or root source. Library errors, warnings, nodes, and messages never enter the
public CLI contract; the focused boundary maps them to stable positional
findings and ordinary TypeScript facts.

Canonical output remains workspace-owned. A focused serializer constructs the
exact ordered Open Forge metadata shape and uses the YAML implementation only
for correct scalar escaping and representation. It does not stringify an
arbitrary object or let package defaults define canonical authoring.

Keep headings, generated regions, fenced-code ranges, local references, and
concealed-source facts in bounded Markdown readers. Do not add a Markdown AST
dependency without separate representative evidence.

## Rationale

YAML quoting, comments, indentation, block scalars, flow and block sequences,
duplicate keys, and source locations interact in ways that are costly to
reimplement reliably. The selected package provides a document-node boundary,
positions, strict errors, duplicate detection, TypeScript declarations, and—at
the time of acceptance—no transitive runtime dependency while remaining
bundleable into the one portable artifact. The implementation Task must verify
those properties against the exact locked package version.

Keeping semantic validation in Open Forge preserves the small accepted format.
The package answers how valid YAML syntax is represented; Open Forge contracts
continue to answer which syntax and values have Framework meaning.

## Alternatives And Tradeoffs

- Extending the line-oriented parser would avoid one dependency but gradually
  recreate a fragile YAML subset and retain formatter incompatibilities.
- `js-yaml` is mature, but its ordinary value-conversion interface provides a
  weaker fit for rejecting syntax nodes while preserving exact source ranges.
- A general Markdown AST would cover more syntax than current consumers need
  and enlarge the security, performance, and dependency boundary.
- Parsing directly into JavaScript values would erase syntax evidence needed
  to reject aliases, explicit tags, directives, and ambiguous source forms.

The dependency adds bundled code and requires lockfile, supply-chain,
cross-runtime, startup, malformed-input, and resource-bound evidence when the
parser Task implements it.

## Consequences

- Formatter-produced multiline flow and block tag sequences can satisfy the
  same semantic contract as canonical inline tags.
- Canonical Open Forge output remains stable, compact, scoped, and independent
  from arbitrary input style.
- Frontmatter consumers depend on `MarkdownDocumentFacts`, not YAML nodes.
- Parser limits, accepted node kinds, and stable findings require direct tests.
- The exact package version enters `package.json` and the lockfile only when the
  authorized route-inventory implementation Task begins.
- A package change cannot broaden accepted metadata without an explicit
  contract decision.

## Authoritative Sources

- [Canonical Markdown syntax](../../documents/framework/markdown/syntax.md)
- [Markdown compatibility boundary](../../documents/framework/markdown/compatibility.md)
- [CLI Route Inventory Contract](../../documents/cli/contracts/route-inventory.md)
- [CLI Architecture](../../documents/cli/architecture.md)
- [Markdown Document Facts Pattern](../../../../patterns/open-forge/cli/markdown/markdown-document-facts.md)

## Decision Relationships

- [CLI Authored Route Inventory](cli-authored-route-inventory.md)
- [CLI Runtime Boundary Validation](cli-runtime-boundary-validation.md)
- [Canonical Markdown Authoring](../framework/canonical-markdown.md)

---
open-forge:
  description: "Historical CLI-v2 source: Inspect one Markdown source into stable domain facts without exposing its scanner or a future syntax tree"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Markdown Document Facts

Use this Pattern when several capabilities need structured facts from one
Markdown source and should remain independent of its parsing implementation.

## Boundary

The accepted [Canonical Markdown Syntax](../../../../memory/crystallized/documents/framework/markdown/syntax.md), [Local References](../../../../memory/crystallized/documents/cli/contracts/local-references.md), [Route Inventory](../../../../memory/crystallized/documents/cli/contracts/route-inventory.md), and [Source Review](../../../../memory/crystallized/documents/cli/contracts/source-review.md) contracts are authoritative for recognized syntax and consumer semantics. The [Frontmatter YAML Decision](../../../../memory/crystallized/decisions/cli/cli-frontmatter-yaml-boundary.md) owns the private syntax-parser choice. This Pattern owns the reusable single-read document-facts boundary.

## Shape

```text
source text
  -> line and offset map
  -> frontmatter and ignored block ranges
  -> focused heading, reference, and generated-region readers
  -> typed document facts and positional findings
  -> domain consumers
```

The consumer boundary contains only facts it can use:

```text
MarkdownDocumentFacts
  frontmatter
  headings
  references
  generated regions
  concealed source
  findings
```

Scanner state, tokens, and a future syntax tree remain private implementation
details.

The accepted `yaml` dependency is likewise private to the frontmatter reader.
It parses one bounded syntax region and supplies nodes and positions; focused
Open Forge validation produces the public frontmatter fact. Consumers never
receive YAML nodes, resolved arbitrary values, warnings, or library errors.

The frontmatter fact records its recognized source form, source range,
description, optional responsibility, tags, and findings. Block facts record
only ranges and kinds that another focused reader needs, such as fenced code or
HTML comments; they do not recreate a general Markdown tree.

Concealed-source facts preserve exact locations for constructs whose source
meaning may not appear in a rendered preview, including HTML comments,
reference definitions, collapsed raw HTML, indirect destinations, and
dangerous format controls. A security consumer classifies those facts through
the [reviewed source boundary](../commands/reviewed-source-boundary.md); the document reader
does not grant trust or emit unescaped terminal output.

## Local Shape

```text
markdown/
  document-facts.ts
  inspect-document.ts
  scan-blocks.ts
  read-frontmatter.ts
  read-headings.ts
  read-inline-links.ts
  read-generated-regions.ts
```

Begin each focused reader beside the consumer that first needs it. Promote the
document boundary and a reader only to the nearest common scope of demonstrated
consumers.

## Shape Constraints

- Read one source into one position map and preserve line, column, offset, and range evidence in consumer-visible facts.
- Keep ignored and claimed ranges explicit so focused readers cannot reinterpret the same bytes inconsistently.
- Return typed facts and positional findings through a parser-independent boundary.
- Keep YAML syntax recognition, Open Forge semantic validation, and canonical
  metadata serialization as distinct responsibilities.
- Let focused scanners or a future syntax tree populate the same facts without exposing parser-specific tokens to commands or domains.
- Keep trust classification and terminal rendering in the source-review consumer rather than the document reader.

## Review

- Can a parser replacement preserve every consumer-visible fact?
- Does any consumer re-read or privately parse the same source?
- Can link-shaped text inside code, comments, or frontmatter escape its ignored
  range?
- Can concealed source be displayed safely with a precise location and escaped
  evidence?
- Does malformed supported syntax carry a useful position and reason?
- Can changing the YAML implementation preserve every accepted fact, rejection,
  position, limit, and canonical output?
- Are unsupported semantics visibly unverified rather than falsely valid?
- Do exact syntax and consumer semantics come from their CurrentTruth contracts rather than this parser boundary?

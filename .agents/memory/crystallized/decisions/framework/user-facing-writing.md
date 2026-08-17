---
open-forge:
  description: Open Forge writing is clear on first read, technically precise, consistent, predictable, and concise
  tags: [Memory, Decision, CurrentTruth, Formatting, Documentation, Routing]
---

# User-Facing Writing

## Context

Earlier Open Forge files mixed compressed fragments, repeated prohibitions, internal jargon, historical prototypes, inconsistent punctuation, and selection detail in the wrong surface. The result could be technically accurate while remaining difficult to read or maintain.

## Decision

Open Forge maintainers write for technically literate readers who may be new to the Framework. Writing should be easy to understand on the first read, correct, consistent, predictable, and concise, in that order. Correctness remains a hard boundary.

The voice is calm, neutral, boring, and direct. Open Forge uses the same term for the same concept and does not change words only for variety. It assumes basic software knowledge but does not require engineering or prior Open Forge experience.

The repository-only [Writing Directive](../../../../directives/writing.md) makes this behavior mandatory. The [Writing Standard](../../documents/maintenance/writing.md) defines the complete practice, and this Decision records why it was chosen.

Current installed and public files describe the accepted contract. Historical alternatives and rejected prototypes remain in Decisions or archives. Route descriptions stay compact but contain enough trigger, purpose, or outcome for selection, while detailed meaning stays in the routed destination.

Writing uses typed authority terminology, canonical Markdown when structure carries Framework meaning, relative links for navigable relationships, and code formatting for commands, literals, defined terms, or paths discussed as text.

## Rationale

Readable language reduces interpretation cost for people and agents. Positive statements define the valid model more clearly than lists of rejected behavior. Linked detail keeps independent entry boundaries understandable without copying complete sources.

Consistent punctuation and formatting remove low-value variation from authoring and make generated and handwritten files easier to compare.

## Alternatives And Tradeoffs

- Dense or headline-style fragments save words but often require more inference than complete sentences
- Legal, academic, and promotional wording can sound precise while making simple ideas harder to understand
- Repeating every linked contract would improve local completeness at the cost of drift and context pollution
- Pure references without a local summary would make independent entry points difficult to understand
- Publishing rejected prototypes as warnings would increase baseline context and keep obsolete models visible
- A mechanical vocabulary replacement would ignore legitimate context and lifecycle meanings

Concise prose still requires judgment. The shortest wording is not always the clearest.

## Consequences

- The repository Writing Directive requires maintainers to apply the Writing Standard when changing public, runtime, current-document, Template, and Maintenance text
- Essence summaries keep identity, mechanism, important distinctions, and boundaries while leaving detail in its source
- Links carry navigable relationships and enough local meaning to justify following them
- Descriptions remain natural rather than beginning with one repeated formula

## Authoritative Sources

- [Repository Writing Directive](../../../../directives/writing.md)
- [Open Forge Writing Standard](../../documents/maintenance/writing.md)
- [Open Forge Dictionary](../../documents/maintenance/helpers/dictionary.md)
- [Open Forge Markdown scope](../../documents/framework/markdown/_markdown.md)
- [Current routing model](../../documents/framework/routing/model.md)

## Decision Relationships

- [Typed authority and role terminology](authoritative-source-terminology.md)
- [Canonical Markdown authoring](canonical-markdown.md)
- [Routing surfaces](routing-surfaces.md)

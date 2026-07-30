---
open-forge:
  description: User-facing Open Forge files use positive natural language and compact selection surfaces
  tags: [Memory, Decision, CurrentTruth, Formatting, Documentation, Routing]
---

# User-Facing Writing

## Context

Earlier Open Forge files mixed compressed fragments, repeated prohibitions, internal jargon, historical prototypes, inconsistent punctuation, and selection detail in the wrong surface. The result could be technically accurate while remaining difficult to read or maintain.

## Decision

Open Forge uses direct, positive, natural language that makes the intended relationship and action explicit.

Current installed and public files describe the accepted contract. Historical alternatives and rejected prototypes remain in Decisions or archives. Route descriptions stay compact but contain enough trigger, purpose, or outcome for selection, while detailed meaning stays in the routed destination.

Writing uses typed authority terminology, canonical Markdown when structure carries Framework meaning, relative links for navigable relationships, and code formatting for commands, literals, defined terms, or paths discussed as text.

## Rationale

Readable language reduces interpretation cost for both people and agents. Positive statements define the valid model more precisely than catalogues of rejected behavior, while linked detail keeps independent entry boundaries understandable without copying complete contracts.

Consistent punctuation and formatting remove low-value variation from authoring and make generated and handwritten files easier to compare.

## Alternatives And Tradeoffs

- Dense fragments save words but often require more inference than complete sentences
- Repeating every linked contract would improve local completeness at the cost of drift and context pollution
- Pure references without a local summary would make independent entry points difficult to understand
- Publishing rejected prototypes as warnings would increase baseline context and keep obsolete models visible
- A mechanical vocabulary replacement would ignore legitimate context and lifecycle meanings

Concise prose still requires judgment. The shortest wording is not always the clearest.

## Consequences

- The shared writing standard applies punctuation, sentence style, and terminology consistently across public, runtime, current-document, Template, and Maintenance surfaces
- Links carry navigable relationships and enough local meaning to justify following them
- Descriptions remain natural rather than beginning with one repeated formula

## Authoritative Sources

- [Open Forge Writing Standard](../documents/maintenance/writing.md)
- [Open Forge Markdown scope](../documents/framework/markdown/_markdown.md)
- [Current routing model](../documents/framework/routing/model.md)

## Decision Relationships

- [Typed authority and role terminology](authoritative-source-terminology.md)
- [Canonical Markdown authoring](canonical-markdown.md)
- [Routing surfaces](routing-surfaces.md)

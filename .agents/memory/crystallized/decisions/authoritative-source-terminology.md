---
open-forge:
  description: Open Forge names semantic authority by source type and reserves ownership language for possession or managed lifecycle
  tags: [Memory, Decision, CurrentTruth, Terminology, Authority, Documentation]
---

# Typed Authoritative Source Terminology

## Context

`Owner` and `current owner` had accumulated several meanings across Open Forge. The same words could refer to an authoritative document, a routed destination, an external system, a person responsible for work, an Extension package managing file bytes, or a Template destination.

Readers could not reliably infer which meaning applied without prior Framework knowledge. A mechanical replacement would preserve the ambiguity under another generic noun.

## Decision

Open Forge uses typed terminology for semantic authority:

- `authoritative source` when the type is unknown or irrelevant
- `authoritative document` when a current document expresses an accepted concept
- `authoritative route` when routed content expresses applicable Framework or workspace meaning
- `authoritative system` when an external system contains live source code, issue state, product data, or another subject
- `responsible person` or `responsible role` when human accountability is meant

Wording uses the most specific natural term supported by the sentence. `Authoritative source` is the generic fallback, not a mandatory repeated phrase.

`Ownership` remains valid when possession or managed lifecycle is the actual subject. Examples include user ownership of installed files, Extension ownership of managed bytes, shared file-owner sets in receipts, and ownership transfer during Template instantiation.

Historical records retain their original terminology unless a link or short clarification must be updated.

## Rationale

Typed vocabulary makes the authority model understandable before a reader learns Open Forge's internal categories. It distinguishes semantic authority from human responsibility and from mechanical file ownership.

Keeping legitimate ownership language preserves useful established meanings in Git, packaging, installation, and Template lifecycle contracts.

## Alternatives And Tradeoffs

- `Canonical source` is familiar but can imply immutability or centralization
- `Source of record` works well for records but less naturally for code, directives, and live systems
- `Authoritative home` communicates placement but is informal and awkward for external systems
- `Current authority` can sound like a person or institution
- `Semantic authority` is precise but unnecessarily academic for ordinary public language
- Replacing every occurrence of `owner` mechanically would erase important possession and lifecycle distinctions

The accepted vocabulary requires contextual editing rather than global search and replace.

## Consequences

- Current product, Framework, Template, and public documentation use typed authoritative-source language
- Emerging records are called records or destinations rather than authorities they do not yet possess
- Task accountability names a responsible person or role
- Extension receipts and other managed-file mechanics retain precise ownership terminology
- Loader wording is reviewed during its dedicated migration rather than changed incidentally during this terminology scrub

## Authoritative Sources

- [Open Forge Principles](../documents/principles.md#one-authoritative-source-and-visible-relationships)
- [Top Open Forge Architecture](../documents/architecture.md#authority-and-current-knowledge)
- [Open Forge Framework Architecture](../documents/framework/architecture.md#current-knowledge-roles)
- [Temporary Terminology Helper](../../working/terminology-helper.md)

## Decision Relationships

- [User-facing writing](user-facing-writing.md)
- [Distinct Core primitive roles](core-primitives.md)

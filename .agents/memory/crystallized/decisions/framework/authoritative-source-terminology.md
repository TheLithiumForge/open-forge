---
open-forge:
  description: Open Forge states role and authority relationships directly, names semantic authority by source type, and reserves ownership for possession or managed lifecycle
  tags: [Memory, Decision, CurrentTruth, Terminology, Authority, Documentation]
---

# Typed Authority And Role Terminology

## Context

`Owner` and `current owner` had accumulated several meanings across Open Forge. The same words could refer to an authoritative document, a routed destination, an external system, a person responsible for work, an Extension package managing file bytes, or a Template destination.

Readers could not reliably infer which meaning applied without prior Framework knowledge. A mechanical replacement would preserve the ambiguity under another generic noun.

`Operator` created a similar problem for people and agents. It could mean a user directing work, an orchestrating agent coordinating subagents, or merely a technical-sounding replacement for `user`.

## Decision

Open Forge uses typed terminology for semantic authority:

- `authoritative source` when the type is unknown or irrelevant
- `authoritative document` when a current document expresses an accepted concept
- `authoritative route` when routed content expresses applicable Framework or workspace meaning
- `authoritative system` when an external system contains live source code, issue state, product data, or another subject
- `responsible person` or `responsible role` when human accountability is meant

Wording uses the most specific natural term supported by the sentence. `Authoritative source` is the generic fallback, not a phrase every sentence must repeat. Ordinary prose prefers a direct verb when it says more: a document `defines`, a Decision `records`, and an external system `contains` current information.

`Ownership` remains valid when possession or managed lifecycle is the actual subject. Examples include user ownership of installed files, Extension ownership of managed bytes, shared file-owner sets in receipts, and ownership transfer during Template instantiation.

Historical records retain their original terminology unless a link or short clarification must be updated.

Open Forge prefers stating role and authority relationships directly. Use `user` when naming the person or group using, adopting, owning, customizing, directing, or consuming an Open Forge environment improves clarity. Otherwise, state the relationship through phrases such as `user direction`, `under user control`, `requires explicit user direction`, `delegated authority`, or `responsible person or role`.

Orchestrators and subagents are agents. An orchestrator may relay user direction or exercise delegated authority within a stated scope, but it does not need a separate `operator` role. Use `decision-maker` only when responsibility for a consequential choice is specifically relevant.

The repository-only [Writing Directive](../../../../directives/writing.md) enforces maintainer use of the [Open Forge Dictionary](../../documents/maintenance/helpers/dictionary.md) when applying this terminology in this repository; neither source is installed in adopting workspaces.

## Rationale

Typed vocabulary makes the authority model understandable before a reader learns Open Forge's internal categories. It distinguishes semantic authority from human responsibility and from mechanical file ownership.

Keeping legitimate ownership language preserves useful established meanings in Git, packaging, installation, and Template lifecycle contracts.

Direct relationship language remains readable when one user works directly with an agent, when an orchestrator coordinates several agents, and when a team delegates different decisions without requiring readers to learn another role taxonomy.

## Alternatives And Tradeoffs

- `Canonical source` is familiar but can imply immutability or centralization
- `Source of record` works well for records but less naturally for code, directives, and live systems
- `Authoritative home` communicates placement but is informal and awkward for external systems
- `Current authority` can sound like a person or institution
- `Semantic authority` is precise but unnecessarily academic for ordinary public language
- `Operator` distinguishes an active role but sounds mechanical and becomes ambiguous when an orchestrator mediates between the user and subagents
- `Steward`, `lead`, and `principal` each imply a governance or team structure Open Forge does not require
- Replacing every occurrence of `owner` mechanically would erase important possession and lifecycle distinctions

The accepted vocabulary requires contextual editing rather than global search and replace.

## Consequences

- Current product, Framework, Template, and public documentation use typed authoritative-source language
- Emerging records are called records or destinations rather than authorities they do not yet possess
- Task accountability names a responsible person or role
- Public and runtime wording uses `user` only when useful and otherwise states direction, control, delegation, approval, or discussion directly
- Orchestrators and subagents remain agents, with consequential delegated authority named explicitly
- Extension receipts and other managed-file mechanics retain precise ownership terminology
- Loader, runtime, current-document, Template, and public wording apply the same relationship-first vocabulary

## Authoritative Sources

- [Open Forge Principles](../../documents/principles.md#one-authoritative-source-and-visible-relationships)
- [Top Open Forge Architecture](../../documents/architecture.md#authority-and-current-knowledge)
- [Open Forge Framework Architecture](../../documents/framework/architecture.md#current-knowledge-roles)
- [Open Forge Dictionary](../../documents/maintenance/helpers/dictionary.md)

## Decision Relationships

- [User-facing writing](user-facing-writing.md)
- [Repository Writing Directive](../../../../directives/writing.md)
- [Distinct Core primitive roles](core-primitives.md)

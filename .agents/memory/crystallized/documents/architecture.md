---
open-forge:
  description: Current Open Forge system model, component boundaries, context flow, authority, evolution, scaling, and tool boundary
  tags: [Memory, Document, CurrentTruth, Evergreen, Architecture, Framework, ACE]
---

# Open Forge Architecture

## Scope

This document is authoritative for the system-level architecture that realizes the [Open Forge vision](vision.md). It defines component responsibilities, dependency direction, context flow, authority, state, and tool boundaries. Scoped architecture documents are authoritative for component internals.

## Architecture Drivers

The [Open Forge Principles](principles.md) define the product identity that this architecture must preserve. The system is structured around five accepted architectural needs:

- Workspace meaning remains inspectable and independently owned
- Relevant context is selected without loading the whole environment
- Accepted direction remains under user control while normal execution remains autonomous
- A small useful base can evolve recursively without becoming one universal methodology
- Deterministic tools make correct behavior cheaper without becoming privately authoritative

## System Model

An Open Forge environment combines four areas:

| Area | Responsibility | Depends on |
|---|---|---|
| Framework | Shared routing, Core primitives, reusable Templates, and Memory mechanics | No other Open Forge area |
| Workspace context | Local goals, knowledge, constraints, decisions, methods, history, and scopes | The framework routes it uses |
| Extensions | Optional reusable capabilities | The framework routes they extend |
| Deterministic tools | Mechanical loading, navigation, validation, installation, packaging, and safety | The human-readable files they inspect and change |

User direction establishes goals and accepted direction. An agent runtime consumes the environment, performs work with its native capabilities, and proposes changes. Agent providers and execution runtimes remain external to Open Forge. Minimal provider bridges may expose the canonical workspace entry without defining independent policy.

## Framework Composition

Core and Memory are the two cooperating areas of the standard Framework. Core provides the entry, routing, authority, loading, relationship, and primitive semantics on which Memory and installed Extensions depend. Memory uses those semantics to preserve continuity, candidate learning, accepted records, and useful history.

The standard base installation ships Core and Memory together because durable continuity and deliberate evolution are part of the product. A workspace may still reshape or remove standard routes through ordinary Framework customization.

Extensions are optional packages outside the base Framework. They add whole files through existing Core or Memory routes instead of creating another root or interpretation model. Once installed, each file receives its runtime meaning and authority from its route, role, scope, content, and accepted direction rather than from package order or metadata.

`#Core`, `#Memory`, and `#Extension` are routing and classification signals, not authority levels or numbered runtime stages. `#Extension` also identifies optional package provenance. The [loader](../../../loader.md#defined-tags) defines their exact installed meanings.

## Authority And Dependency Direction

Human-readable Markdown is authoritative for Open Forge rules, recorded state, relationships, and workspace-specific context. A declared external system may be authoritative for source code, issues, product data, or another live subject when an Open Forge route points to it explicitly.

Generated route entries, indexes, receipts, caches, and retrieval databases are derived from those authoritative sources. They may make discovery, validation, or change cheaper, but deleting and rebuilding them cannot change what the environment means.

Dependencies point toward human-readable authoritative sources:

1. The framework is complete without workspace-specific content, extensions, deterministic tools, or a particular agent provider
2. Workspace context and extensions build on framework routes without redefining their universal meaning
3. Deterministic tools and agent runtimes consume the same inspectable contract
4. Generated entries, indexes, receipts, caches, and retrieval databases remain replaceable

## Context Flow

Open Forge assembles three kinds of context:

| Context | Contains | Use |
|---|---|---|
| Baseline | The small set of framework rules and route maps that apply to nearly all work | Establishes how to enter, navigate, interpret authority, and find more context |
| Continuity | Live commitments, open approval gates, handoffs, and other state needed to resume current work | Preserves ongoing work without loading its entire history |
| Selected | Routed files and relationships relevant to the current goal | Supplies the detailed knowledge, constraints, and methods needed now |

For example, resuming a CLI redesign may load the baseline rules and route map, one active handoff that names the current gate, and the selected CLI architecture and decisions. Unrelated extension history remains unloaded unless the redesign depends on it.

The operating flow is:

1. A goal or request establishes the work
2. The canonical workspace entry, such as `AGENTS.md`, points to the framework loader
3. The agent reads baseline and applicable continuity context
4. Top-down routes expose the scopes and relationships relevant to the goal
5. The agent reasons and acts with native capabilities, selected context, optional extensions, and deterministic tools
6. Results, evidence, corrections, and accepted direction update their authoritative files or external systems

This flow moves context into and out of work. It does not require a project to follow a predefined lifecycle.

## Structural Model

A `route` is a visible path through small Markdown `entrypoints`. Each entrypoint exposes direct children with relative links, descriptions sufficient to select or skip them, and descriptive tags. A `scope` is a routed subtree that narrows authority or meaning.

Agents move top-down from known context into relevant detail. Loaded ancestor rules remain active below them, while a child adds only what is specific to its scope. Relative Markdown links and established tags connect material across branches without creating competing authoritative sources.

For example, the loader exposes Memory, Memory exposes Crystallized, Crystallized exposes Documents, and Documents exposes this architecture through its path, description, and tags. An agent can select this view without opening unrelated documents or their history.

Any authoritative route may introduce narrower scopes and initialize only the framework areas it needs. Scopes inherit broader meaning and add local context through files, routes, links, and tags.

Selection cost should grow primarily with route depth and the number of selected branches, not with the total number of stored scopes. Unselected sibling scopes should add almost no active-context cost. The [Open Forge Routing scope](framework/routing/_routing.md) is authoritative for the complete route contract.

## Authority And Current Knowledge

Loading changes visibility, not authority. Authority comes from the authoritative source, its scope, accepted user direction, applicable framework rules, and any declared external source of truth.

Each subject has one authoritative source for each distinct question:

| Source role | Answers | Contains |
|---|---|---|
| Current document | What is true now, and how does it work? | A complete usable explanation of the accepted concept |
| Decision | What was chosen, and why? | The accepted choice, relevant alternatives, tradeoffs, consequences, and rationale |
| Directive | What behavior applies during work? | Binding instructions within its declared scope |
| Archived memory | What happened before? | Useful historical material that no longer governs current work |

A current document is authoritative for what is true now and links backward to useful rationale. A decision is authoritative for why a choice was accepted and links forward to where its result now lives.

An Evergreen current document must stay synchronized with the accepted state it explains. It states the current concept well enough to use without reading its supporting decisions.

For example, an architecture document should state that routing is top-down and explain how that design works. It can link to the [routing decision](../decisions/routing-model.md) for the reasoning and historical choice. The decision links back to the architecture that is authoritative for the current design.

This creates limited intentional overlap: both files identify the accepted choice, the current document is authoritative for the complete current concept, and the decision is authoritative for the reason behind it.

Bidirectional links do not create circular authority. A decision may cite the prior current state that framed the choice, then link to the updated authoritative source that expresses its result. Chronology explains how the files evolved; each source role determines what the source governs now.

## State And Evolution

Memory is part of the framework substrate and organizes recorded state by its current role:

| State | Purpose |
|---|---|
| Working | Temporary context needed to continue or resume active work |
| Emerging | Useful candidate material that is not accepted current truth |
| Crystallized | Accepted durable state within its declared scope |
| Archived | Historical context that no longer governs current work |

The states are not a rigid pipeline. Material moves when meaning, scope, and authority justify the transition. Clear direction may update a Crystallized authoritative source directly, while tentative ideas remain Working or Emerging. Material that is no longer current is extracted, archived, consolidated, or pruned.

The environment also evolves through Core primitives. Templates provide copy-ready starting content whose ownership transfers to the destination. Patterns continue to guide reusable shapes, Directives and Axioms bind behavior, and Extensions add optional routed capabilities. Detailed state transitions and primitive relationships belong to the [Framework Architecture](framework/architecture.md).

## Tool Boundary

The CLI and compatible future tools perform deterministic operations over human-readable Open Forge files and their declared external relationships. They may batch context, traverse routes, validate structure, maintain derived indexes, and apply reviewable file changes.

Tools must expose their effects through files or output. They may reduce reasoning and interaction cost, but they cannot silently infer accepted truth, become privately authoritative, or become required for ordinary inspection.

## Cross-Cutting Invariants

The following constraints apply across every Open Forge area:

1. Every important subject has one authoritative source for each distinct question
2. Workspace meaning remains reconstructable from human-readable authoritative sources and declared external systems
3. Loading changes visibility and timing, not authority
4. Unselected scopes do not routinely enter active context
5. Generated and machine-optimized state remains replaceable
6. Core, local content, Extensions, and tools use the same visible routing and relationship model
7. Structure and validation improve reliability without claiming mechanical control over agent reasoning

## Current Tradeoffs And Limits

- Explicit files, routes, and relationships require deliberate maintenance in exchange for inspectability, portability, and correctability
- Routing quality depends on clear descriptions, scopes, and authority. Deterministic validation can prove structural integrity but not perfect semantic relevance
- Open Forge has no fixed structural expansion ceiling, but this is not a promise of constant performance. Active context and navigation cost still grow with selected routes and relationships
- Agent behavior remains nondeterministic. The environment can make correct behavior much easier without guaranteeing compliance
- The current CLI and Extensions implementations are MVPs. Their scoped architectures document current behavior, stable boundaries, and liabilities, while Emerging records keep candidate overhaul designs from masquerading as current truth

## Architecture Views

The architecture is intentionally split by authoritative scope:

- This top architecture is authoritative for the system map and cross-cutting invariants
- The [Framework Architecture](framework/architecture.md) is authoritative for Core and Memory internals
- The [Open Forge Routing scope](framework/routing/_routing.md) is authoritative for navigation, recursive scope, inheritance, loading, continuity, and path identity
- The [Extensions MVP Architecture](extensions/architecture.md) is authoritative for current optional capability composition, lifecycle, safety boundaries, and liabilities
- The [CLI MVP Architecture](cli/architecture.md) is authoritative for current commands, deterministic state, safety, verification, implementation, and liabilities

The extensions and CLI views document the current MVPs without treating current implementation choices as permanent. Their linked Emerging records preserve prospective overhaul design until it is accepted. This document links to the current architectures and does not duplicate their internal contracts.

## Related Current Views

- [Open Forge vision](vision.md)
- [Open Forge principles](principles.md)

## Decisions And Rationale

These decisions preserve useful rationale behind the current architecture. Their architectural results remain expressed by this document and its scoped views:

- [Product direction](../decisions/product-direction.md)
- [Distinct Core primitive roles](../decisions/core-primitives.md)
- [Templates as a Core primitive](../decisions/template-primitive.md)
- [Routing model](../decisions/routing-model.md)
- [Memory model](../decisions/memory-model.md)
- [Extensions and CLI](../decisions/extensions-and-cli.md)
- [Scope and slugs](../decisions/scope-and-slugs.md)
- [Tag semantics](../decisions/tags.md)
- [Typed authority and role terminology](../decisions/authoritative-source-terminology.md)

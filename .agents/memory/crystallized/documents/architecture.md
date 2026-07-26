---
open-forge:
  description: Current Open Forge system model, component boundaries, context flow, authority, evolution, scaling, and tool boundary
  tags: [Memory, Document, CurrentTruth, Evergreen, Architecture, Framework, ACE]
---

# Open Forge Architecture

## Scope

This document owns the system-level architecture that realizes the [Open Forge vision](vision.md). It defines component responsibilities, dependency direction, context flow, authority, state, and tool boundaries. Scoped architecture documents own component internals.

## System Model

An Open Forge environment combines four areas:

| Area | Responsibility | Depends on |
|---|---|---|
| Framework | Shared routing, primitives, and Memory mechanics | No other Open Forge area |
| Workspace context | Local goals, knowledge, constraints, decisions, methods, history, and scopes | The framework routes it uses |
| Extensions | Optional reusable capabilities | The framework routes they extend |
| Deterministic tools | Mechanical loading, navigation, validation, installation, packaging, and safety | The human-readable files they inspect and change |

The operator establishes goals and accepted direction. An agent runtime consumes the environment, performs work with its native capabilities, and proposes changes. Agent providers and execution runtimes remain external to Open Forge. Minimal provider bridges may expose the canonical workspace entry without owning independent policy.

## Where Meaning Lives

Human-readable Markdown owns Open Forge rules, recorded state, relationships, and workspace-specific context. A declared external system may own information such as source code, issues, or product data when an Open Forge route points to it explicitly.

Generated route entries, indexes, receipts, caches, and retrieval databases are derived from those owners. They may make discovery, validation, or change cheaper, but deleting and rebuilding them cannot change what the environment means.

## Dependency Direction

Dependencies point toward human-readable owners:

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
6. Results, evidence, corrections, and accepted direction update their owning files or external systems

This flow moves context into and out of work. It does not require a project to follow a predefined lifecycle.

## Structural Model

A `route` is a visible path through small Markdown `entrypoints`. Each `entrypoint` exposes its direct children with a relative path, a short description sufficient to decide whether to open them, and descriptive tags. A `scope` is a routed subtree that narrows ownership or meaning.

Agents move top-down from known context into relevant detail. Loaded ancestor rules remain active below them, while a child adds only what is specific to its scope. Relative Markdown links and established tags connect material across branches without creating competing owners.

This document is one example of a route: the loader exposes Memory, Memory exposes Crystallized, Crystallized exposes Documents, and Documents exposes this architecture through its path, description, and tags. An agent can decide whether the architecture is relevant before reading its body.

Any routed owner may introduce narrower scopes and initialize only the framework areas it needs. Scopes inherit broader meaning and add local context through files, routes, links, and tags.

Selection cost should grow primarily with route depth and the number of selected branches, not with the total number of stored scopes. Unselected sibling scopes should add almost no active-context cost. The [Framework Architecture](framework/architecture.md) owns the current route contract, while the [routing decision](../decisions/routing-model.md) preserves earlier rationale that remains useful during migration.

## Authority And Current Knowledge

Loading changes visibility, not authority. Authority comes from the owning source, its scope, accepted operator direction, applicable framework rules, and any declared external source of truth.

Each subject has one current owner. Different owners answer different questions:

| Owner | Answers | Contains |
|---|---|---|
| Current document | What is true now, and how does it work? | A complete usable explanation of the accepted concept |
| Decision | What was chosen, and why? | The accepted choice, relevant alternatives, tradeoffs, consequences, and rationale |
| Directive | What behavior applies during work? | Binding instructions within its declared scope |
| Archived memory | What happened before? | Useful historical material that no longer governs current work |

An Evergreen current document must stay synchronized with the accepted state it explains. It states the current concept well enough to use without reading its supporting decisions. When a decision supplies useful rationale, the document links to it. The decision states the accepted choice and why it was made, then links forward to the current owner.

For example, an architecture document should state that routing is top-down and explain how that design works. It can link to the [routing decision](../decisions/routing-model.md) for the reasoning and historical choice. The decision links back to the architecture that owns the current design.

This creates limited intentional overlap: both files identify the accepted choice, the current document owns the complete current concept, and the decision owns the reason behind it.

## Memory States

Memory is part of the framework substrate and organizes recorded state by its current role:

| State | Purpose |
|---|---|
| Working | Temporary context needed to continue or resume active work |
| Emerging | Useful candidate material that is not accepted current truth |
| Crystallized | Accepted durable state within its declared scope |
| Archived | Historical context that no longer governs current work |

The states are not a rigid pipeline. Material moves when meaning, scope, and authority justify the transition. Clear direction may update a Crystallized owner directly, while tentative ideas remain Working or Emerging. Superseded material is extracted, archived, consolidated, or pruned.

Detailed authority resolution, state transitions, starter routes, and primitive behavior belong to the [Framework Architecture](framework/architecture.md).

## Tool Boundary

The CLI and compatible future tools perform deterministic operations over human-readable Open Forge files and their declared external relationships. They may batch context, traverse routes, validate structure, maintain derived indexes, and apply reviewable file changes.

Tools must expose their effects through files or output. They may reduce reasoning and interaction cost, but they cannot silently infer accepted truth, privately own meaning, or become required for ordinary inspection.

## Architecture Views

The architecture is intentionally split by ownership:

- This top architecture owns the system map and cross-cutting invariants
- The [Framework Architecture](framework/architecture.md) owns Core and Memory internals
- The [Extensions MVP Architecture](extensions/architecture.md) owns current optional capability composition and redesign boundaries
- The [CLI MVP Architecture](cli/architecture.md) owns current commands, lifecycle, safety, implementation, and redesign boundaries

The extensions and CLI views document the current MVPs before their planned overhauls. They preserve useful invariants without treating current implementation choices as permanent. This document links to those architectures and does not duplicate their internal contracts.

## Related Current Views

- [Open Forge vision](vision.md)
- [Framework Architecture](framework/architecture.md)
- [Extensions MVP Architecture](extensions/architecture.md)
- [CLI MVP Architecture](cli/architecture.md)

## Migration Inputs

These earlier decisions may preserve useful rationale, but they remain subject to reconciliation with the current architecture:

- [Product-direction rationale](../decisions/product-direction.md)
- [Routing rationale](../decisions/routing-model.md)
- [Memory-model rationale](../decisions/memory-model.md)
- [Extensions and CLI rationale](../decisions/extensions-and-cli.md)
- [Scope and slug rationale](../decisions/scope-and-slugs.md)

---
open-forge:
  description: Current architecture of the shipped Open Forge Framework, including Core, Memory, routing, authority, loading, relationships, customization, and distribution
  tags: [Memory, Document, CurrentTruth, Evergreen, Architecture, Framework, Core, ACE]
---

# Open Forge Framework Architecture

## Scope

This document is authoritative for the current internal architecture of the Open Forge Framework. The Framework implements the common [Adaptive Context Engineering vision](../vision.md) through two areas:

- Core provides the routing substrate and reusable agent-facing primitives
- Memory provides continuity, candidate learning, accepted records, and useful history

The [top Open Forge architecture](../architecture.md) is authoritative for the complete system map and the [composition relationship](../architecture.md#framework-composition) among Core, Memory, and Extensions. This document explains how Core and Memory work internally. It names extension and tool boundaries only where they constrain the Framework.

The installable source, dogfood environment, Maintenance, and tooling must remain aligned with this architecture. Historical files may explain earlier choices, but they do not constrain current design merely because they exist.

The [Open Forge Markdown scope](markdown/_markdown.md) contains the current canonical syntax, routed representation, and compatibility specifications used by this architecture.

The [Open Forge Routing scope](routing/_routing.md) defines the detailed current contracts for navigation, selection, recursive scope, inheritance, loading, continuity, and path identity. This document retains the routing summary needed to understand the complete Framework.

## Framework Promise

The Framework turns a workspace into a navigable, persistent, and adaptable context environment without replacing native agent reasoning or installing a universal methodology.

It must make these questions cheap to answer:

- Where do I enter the workspace?
- Which context applies to this goal?
- What is binding, accepted, tentative, or historical?
- Which file or external system is authoritative for the meaning?
- What broader context remains active in this scope?
- Which missing decisions could materially change this work, and which can proceed under visible assumptions?
- Where should a new rule, result, idea, observation, or decision go?
- How can the workspace evolve without accumulating competing truth?

Readable Markdown contains the complete semantic answer. Deterministic tools may make every operation faster and safer, but the files remain sufficient to inspect, navigate, and maintain the Framework.

Once direction is sufficient, work continues through coherent in-scope steps until completion or until a material decision, conflict, uncertainty, or authority boundary requires user input. Routine continuation does not require another `continue` message.

## Architectural Invariants

The following structural constraints realize the [Open Forge Principles](../principles.md) throughout Core and Memory. They are architectural consequences rather than a second authoritative source for product identity:

1. Every important concept has one authoritative source for each distinct question
2. A `route` exposes enough information to select relevant context before loading its body
3. Loaded ancestor `Axioms` remain active below them without being copied into children
4. Loading controls visibility and timing, not authority
5. Scope is explicit in the `route` path, `entrypoint` meaning, `description`, and relationships
6. Workspace meaning remains readable and reconstructable without the CLI, caches, receipts, or retrieval databases
7. Standard `routes` are useful defaults while remaining removable, replaceable, and recursively customizable
8. Removed defaults stay removed unless restoration is explicitly requested
9. Memory may describe any subject without activating behavior it records
10. Accepted behavior that should govern work belongs in Core
11. Generated metadata is derived and rebuildable
12. Structure and tools improve the probability of correct agent behavior without claiming mechanical control over reasoning
13. Instantiating a template transfers ownership to the created result; the template does not manage it
14. Framework contracts target the broadest stable authoritative `route` that preserves their required meaning
15. Agents surface consequential unsettled choices before dependent work, recommend coherent defaults, and avoid questions that do not materially change the result
16. Authored frontmatter, generated Entries, and overwrite pairs remain semantically inspectable and fail closed when their required meaning cannot be established

## Shipped Framework

Open Forge ships a standard Framework rather than an empty routing library. Its `routes` embody the useful starting environment developed through dogfooding while remaining ordinary files that users may reshape.

The standard structure is conceptually:

```text
AGENTS.md
provider bridge files when supported
.agents/
  loader.md
  directives/
  guidance/
  patterns/
  skills/
  templates/
  workflows/
  maps/
  memory/
    working/
      checkpoints/
      handoffs/
    emerging/
      analysis/
      ideas/
      observations/
    crystallized/
      decisions/
      documents/
    archived/
```

The canonical workspace `entry` and loader form the entry boundary. Core `route` categories provide the shared primitive vocabulary. Memory supplies the standard state model and starter `routes` used to preserve continuity and evolution.

This tree is the distributed product shape, not an untouchable taxonomy. A user may add scopes, add or remove `routes`, replace framework files, use only a subset of the primitives, or reorganize local material through valid `route` chains. Removing a standard `route` removes that capability from the local profile; it does not make the remaining Framework invalid. Validation checks the structure that exists rather than demanding that deleted defaults reappear.

Core is the dependency floor because every other Open Forge area relies on its `entry`, routing, authority, and relationship semantics. The shipped Framework includes both Core and Memory because persistence and deliberate evolution are central to the product rather than optional afterthoughts.

## Canonical Entry

[`AGENTS.md`](../../../../../AGENTS.md) is the canonical workspace entry. It stays extremely small:

1. Identify Open Forge as the workspace operating contract
2. Direct the agent to [the loader](../../../../loader.md) before work begins
3. Require applicable Open Forge instructions throughout the task

Provider-specific harness files are minimal bridges to canonical Framework entries. They may use provider-native imports to expose `AGENTS.md` and preload the loader, but they do not restate Open Forge policy or become separate authoritative sources.

Managed entry blocks preserve workspace-owned content outside their markers. A provider bridge can therefore be installed or updated without claiming the entire file.

## Core

Core provides the smallest common language needed to route, interpret, and apply workspace context. It does not try to encode ordinary reasoning, a development lifecycle, or a complete methodology.

Framework wording uses #Core collectively when any suitable Core `route` may satisfy a requirement. It names a specific primitive when that primitive's distinct semantics matter, such as Directives for binding behavior, and enumerates concrete standard `routes` when the default set itself is the subject. This keeps customizable Frameworks valid without weakening precise contracts.

Core mechanics are:

- The loader and `entrypoint` contract
- Authority, inheritance, loading, tag, and overwrite semantics

The seven Core primitives are:

- Directives
- Guidance
- Patterns
- Skills
- Templates
- Workflows
- Map `routes`

These primitives are distinct because they answer different questions. Their default `entrypoints` and current local contents are exposed by the [directives](../../../../directives/_directives.md), [guidance](../../../../guidance/_guidance.md), [patterns](../../../../patterns/_patterns.md), [skills](../../../../skills/_skills.md), [templates](../../../../templates/_templates.md), [workflows](../../../../workflows/_workflows.md), and [maps](../../../../maps/_maps.md) `routes`.

### Routing

Routing moves from the canonical loader through small `entrypoints` that expose direct children. Each `description` supports pre-load selection, relative links identify destinations, and tags add compact loading, type, scope, and search signals. Selected descendants inherit loaded ancestor `Axioms`, while unselected siblings remain outside active context.

Work may select several scopes without merging them. Each keeps its own `route` chain and authority, explicit relationships connect them, and conflicts about a shared result are resolved by clear direction or the authoritative source for that result rather than by path depth or load order.

Loading determines when routed context becomes visible, not what authority it has. Baseline and continuity context both follow loaded parent routes. Continuity content is refreshed while its scope remains active. Other context remains selected on demand.

The detailed current contracts are separated by responsibility:

- The [routing model](routing/model.md) defines `entrypoints`, `entries`, direct-child navigation, selection, and relevance-scaled growth
- The [scope and inheritance contract](routing/scope.md) defines the root boundary, concrete `slugs`, universal scoping, `managed route` relationships, and loaded inheritance
- The [loading and continuity contract](routing/loading.md) defines baseline, continuity, selected context, refresh boundaries, and deterministic assistance
- The [path contract](routing/paths.md) defines containing-file-relative Markdown links, workspace-relative tool `routes`, normalization, and containment
- The [routed Markdown representation](markdown/routes.md) defines their canonical authored syntax and filename forms

The [loader](../../../../loader.md) remains authoritative for the exact installed terms and reserved-tag wording. Component `entrypoints` define the distinct meaning of the categories they expose.

## Authority

Open Forge does not reduce authority to one global ranking because different authoritative sources answer different questions. It resolves meaning through type, scope, current direction, and declared authority.

The operating rules are:

1. Platform constraints and runtime safety bound every action
2. Clear current user direction governs goals, priorities, consequential tradeoffs, and accepted changes within its scope
3. A declared external source of truth is authoritative for the facts delegated to it
4. Loaded `Axioms`, including those supplied by Directives, govern Framework interpretation and applicable behavior
5. A selected authoritative source governs the accepted state of its subject
6. Narrower selected material of the same non-directive kind is preferred when it safely specializes broader material
7. Loaded directives add constraints to ancestor directives rather than silently replacing them
8. A user-owned overwrite is interpreted within its base file's role and scope and has final precedence only for corresponding content

Loading a file makes it visible. A tag can classify it or affect loading. Neither operation creates authority by itself.

The [accepted-state contract](truth.md) defines how clear and tentative direction are treated, how affected current and #Evergreen sources change, and how rationale or useful context from the previous state is preserved. Unresolved conflicts are reported with their authoritative sources and scopes rather than silently resolved through file order.

## Core Primitives

Core primitives give reusable content distinct application semantics instead of treating every useful file as generic knowledge. The primitive kind states how selected material should be used; routing states when it becomes visible.

The [Core primitive model](primitives/model.md) defines the complete role vocabulary, selection questions, authority boundaries, relationships, recursive scope, admission threshold, and why Core has no separate Rules primitive.

Every shipped primitive has a focused conceptual contract under [Core Primitives](primitives/_primitives.md): Directives, Guidance, Patterns, Skills, Templates, Workflows, and Map. These documents deepen each role's meaning, boundaries, lifecycle, and relationships without becoming parallel runtime instructions.

Installed category `entrypoints` own the complete compact operational and file requirements that users and agents receive. Maintenance contracts own canonical sources, alignment obligations, distribution details, and verification.

## Memory

[Memory](../../../_memory.md) is the Framework's self-growing Markdown state for live work, agent communication and coordination, continuity, accepted records, historical context, and candidate learning. It can grow through useful records and routed scopes without a fixed structural ceiling, while unrelated branches stay outside active context. It preserves useful information across work without turning every conversation or recorded statement into current truth or active behavior.

Memory state and scope are independent. State describes how recorded material should currently be treated. Scope describes the person, project, component, discipline, repository, collection, or other subject to which it applies. The `route` path expresses both dimensions without a centralized registry.

Memory loading follows state purpose. Working and Crystallized navigation enter baseline context, Emerging learning is revisited at applicable continuity boundaries, and Archived history remains on demand. Individual records still load by relevance unless their own tags give them a baseline or continuity role.

The detailed current contracts are separated by responsibility:

- The [Memory model](memory/model.md) defines purpose, authority boundaries, state and scope composition, recursive growth, capture thresholds, and shipped defaults
- The [Working](memory/working.md), [Emerging](memory/emerging.md), [Crystallized](memory/crystallized.md), and [Archived](memory/archived.md) contracts define what makes each standard state valid
- The [transition contract](memory/transitions.md) defines capture, direct movement, consolidation, promotion, supersession, archival, restoration, and relationship updates
- The [accepted-state contract](truth.md) defines framework-wide acceptance, #Contextual and #CurrentTruth treatment, and #Evergreen synchronization

The states are not maturity scores or a mandatory pipeline. Installed Memory `entrypoints` retain the complete compact operational contract users receive, while these current documents explain the coherent design without becoming hidden runtime dependencies.

## Current Knowledge Roles

Open Forge distinguishes standard roles for current meaning, accepted rationale, binding behavior, and useful history:

| Role             | Primary question                        | Contract                                                      |
| ---------------- | --------------------------------------- | ------------------------------------------------------------- |
| Current document | What is true now, and how does it work? | Explains a coherent accepted concept completely enough to use |
| Decision         | What was chosen, and why?               | Preserves a discrete accepted choice and useful rationale     |
| Directive        | What behavior is mandatory here?        | Binds work in its loaded route-selected scope                 |
| Archive          | What happened before?                   | Preserves useful history without governing current work       |

The [accepted-state contract](truth.md#current-views-decisions-and-history) defines how current documents, decisions, and archives relate without becoming competing authority.

Vision, Principles, and Architecture are specialized current documents with distinct questions. `Principles` is the formal role for stable filters used to judge unfamiliar choices. `Foundation` describes how load-bearing a principle is, and `identity` describes what the complete foundational set preserves. `Essence` may summarize a concept in its description or opening, but it is not a separate knowledge role.

A scope earns its own Principles document only when several recurring unfamiliar choices depend on stable filters that are specific to that scope and not already answered by broader principles. Otherwise, the scope follows the broader principles and keeps its distinct structural meaning in Architecture. Scoped principles supplement broader principles; they do not silently override them.

The [same contract](truth.md#independent-properties) defines #CurrentTruth and #Evergreen as independent acceptance and synchronization properties.

## Relationships

Open Forge uses ordinary Markdown relationships before adding specialized retrieval machinery.

The primary relationship surface is:

- Frontmatter `descriptions` that become `entry` labels
- Containing-file-relative links that identify authoritative sources and related context
- Heading anchors that target the relevant concept
- Established descriptive tags that support classification, association, and search
- Short prose that explains why a relationship matters

Tags are signals rather than a second authority or inference system. They stay readable in Markdown and cheap for agents and tools to search.

When another authoritative source already contains the detailed meaning, a file links to it instead of restating it. Controlled mirrors are allowed only where an independently complete entry boundary needs a small synchronized contract.

This relationship model is intentionally sufficient for future graph, semantic, or vector retrieval. A derived system may traverse or rank these relationships, but its index remains rebuildable and advisory.

## Recursive Customization

The [scope and inheritance contract](routing/scope.md) defines the complete recursive customization model. In summary, each `root route` exists only where the loader exposes it, while ordinary routed `slugs` may narrow any meaning that follows below that root. Scoping does not reorder `route` segments recognized by a declared manager or recreate a `root route`.

The `root routes` compose through explicit links by default. A Workflow may use a Skill or Pattern without physically containing a second Core tree.

Users may add roots or deeper `routes`, replace standard `routes`, and remove defaults that provide no local value. Generic routing remains available to every valid `entrypoint` tree. A `route` receives its behavior from explicit `entrypoints`, loaded ancestors, and accepted local contracts.

The preferred customization choices are:

1. Add a local routed file when the new meaning stands independently
2. Add a scope when authority or meaning needs a narrower `route`
3. Use a user-owned `{name}.overwrite.md` companion for a small local adjustment to a mostly suitable base
4. Edit or replace the base when the desired model is fundamentally different
5. Remove `routes` that provide no local value

The [overwrite contract](routing/overwrites.md) defines companion identity, inherited routing and loading, file-local precedence, generated boundaries, ownership, and lifecycle.

These are clarity preferences rather than limits on ownership. Users own the installed files and may choose the representation that remains easiest for their workspace to understand.

An installer or updater preserves existing user content by default. Missing standard routes are not assumed to be accidental. Completion, upgrade, replacement, and explicit restoration are distinct intents even if the future CLI exposes their final mechanics differently.

## Generated And Deterministic State

Generated `Entries` regions are derived navigation metadata. Their authored sources are `route` files, `entrypoint` frontmatter, and the filesystem structure. Rebuilding a generated region cannot change the intended meaning of the routed content.

Receipts, caches, indexes, vector databases, and other machine state may support installation, validation, retrieval, or safe removal. They remain:

- Inspectable or explainable through their public effect
- Replaceable or rebuildable
- Outside the semantic authority path
- Unnecessary for ordinary Markdown inspection

Deterministic tools may:

- Assemble ordered context
- Follow explicit `route` chains
- Validate links, `entrypoints`, metadata, and inheritance
- Rebuild generated `entries`
- Preview and apply bounded file changes
- Preserve user content
- Detect collisions and unsafe paths
- Report provenance and affected authoritative sources

They may not silently promote a candidate, infer accepted direction, or make a private database the only place where a rule or relationship exists.

## Extensions Boundary

Extensions add optional reusable content and supporting files. Routed Extension content uses the same `routes` and primitive meanings as other Framework content. Packaging does not create a second loader, root authority model, or runtime interpretation system.

After installation, routed content retains its destination's role, scope, loading behavior, and authority. Native formats and supporting files retain the meaning defined by their consumers. Package metadata is not required to interpret the installed content.

The [Extensions Architecture](../extensions/architecture.md) defines package meaning, composition, and the boundary with installed Framework behavior. The [Extension command contracts](../cli/contracts/extension/_extension.md) define the accepted package representation and managed operations.

The [CLI MVP Architecture](../cli/mvp-architecture.md) records the frozen implementation. Further distribution, compatibility, migration, multi-root, dependency, and governance questions remain contextual in the [Extensions evolution candidate](../../../emerging/ideas/extensions-overhaul.md).

## Distribution And Dogfood

Users receive the installable Framework from [`src/open-forge/`](../../../../../src/open-forge/). The installed Framework is its complete operational contract. Every definition, authority boundary, loading rule, and routing instruction required to navigate or use it must appear in an installed file. When required meaning is intentionally separated, the installed file that depends on it directs the reader to the installed route containing that meaning.

The replacement CLI embeds this complete canonical source tree through ordinary
.NET project resources under one fixed logical-name prefix. Root `install` owns
the closed base Framework installation. Framework-aware `route init` is a later
scoping operation: after a trusted current root installation exists, it reuses
the same embedded payload and canonical topology to copy selected managed
entrypoints into one explicit concrete scoped route. There is no separate
`install --route`, runtime source-checkout dependency, blueprint catalogue, or
general Template renderer.

Inserted scope entrypoints remain user-owned. Copied canonical Framework assets
and bounded generated regions may be lifecycle-managed only through their exact
concrete target identity and recorded canonical source-asset provenance.

Repository Maintenance, source history, public documentation, and unpublished design context cannot be hidden runtime dependencies. Public documentation may explain the Framework and provide examples, but it cannot replace instructions needed during normal agent work. Installed wording uses ordinary language and terms defined by the installed Framework rather than relying on repository-only vocabulary.

The [system dependency direction](../architecture.md#authority-and-dependency-direction) applies within the Framework: Core contracts stand alone, Memory may depend on Core, and Extensions or tools may depend on Core or Memory. A Core or Memory contract cannot require optional Extensions, CLI behavior, packaging, or future modules to explain its meaning.

When a lower-level contract accepts input from several sources, it states the requirement for any writer instead of naming a particular workflow, Extension, package, module, or tool as the actor. Each higher-level authoritative source explains its own participation.

This repository's root `.agents/` tree dogfoods the Framework and adds local routes for Open Forge development. Shared behavior should match the installable source. Deliberate repository-only differences remain visibly local. The current repository-local template set is one such evaluation boundary: the shared source ships its route contract, while concrete templates remain local until reviewed.

Current documents and maintenance contracts govern the design and review of source files. Source files are authoritative for their exact installed wording. Build output and generated indexes are derived from those authoritative sources.

## Reliability

The Framework improves reliability through early context, explicit authority, consistent routing, visible scope, inherited rules, continuity checkpoints, and deterministic validation.

It cannot guarantee that a nondeterministic agent reads, understands, or follows every instruction. Therefore:

- Reliability-critical context must appear early and be cheap to load
- Conditional routes must be cheap to select or skip
- Missing continuity must be recoverable
- Deterministic defects must be detected mechanically when possible
- Claims about agent behavior require evidence rather than inference from document quality
- Conflicts, unavailable authoritative sources, and unverifiable assumptions remain visible

Correct behavior should be the cheapest path, but review remains part of any consequential workflow.

## Related Current Sources

- [Open Forge vision](../vision.md)
- [Open Forge principles](../principles.md)
- [Top Open Forge architecture](../architecture.md)
- [Open Forge Markdown scope](markdown/_markdown.md)
- [Open Forge Routing scope](routing/_routing.md)
- [Core Primitives scope](primitives/_primitives.md)
- [Memory Architecture](memory/_memory.md)
- [Accepted State and Synchronization](truth.md)
- [Payload Maintenance](../maintenance/payload/_payload.md)
- [Extensions Architecture](../extensions/architecture.md)
- [CLI MVP Architecture](../cli/mvp-architecture.md)
- [Current replacement CLI Architecture](../cli/architecture.md)
- [Historical CLI-v2 evidence](../../../archived/cli-v2/_cli-v2.md)
- [Canonical Framework loader](../../../../loader.md)
- [Current Memory contract](../../../_memory.md)
- [Current map to important repository authoritative sources](../../../../maps/sources-of-truth.md)

## Decisions And Rationale

These decisions preserve useful rationale behind the current Framework architecture. Their accepted results remain expressed by this document and its scoped current views:

- [Product direction rationale](../../decisions/product/product-direction.md)
- [Distinct Core primitive role rationale](../../decisions/framework/core-primitives.md)
- [Templates as a Core primitive rationale](../../decisions/framework/template-primitive.md)
- [Routing model rationale](../../decisions/framework/routing-model.md)
- [Routing surface rationale](../../decisions/framework/routing-surfaces.md)
- [Scope and slug rationale](../../decisions/framework/scope-and-slugs.md)
- [Tag rationale](../../decisions/framework/tags.md)
- [Loading reliability rationale](../../decisions/framework/loading-reliability.md)
- [Memory model rationale](../../decisions/framework/memory-model.md)
- [Workflow shape rationale](../../decisions/framework/workflow-shape.md)
- [Source and packaging rationale](../../decisions/framework/source-and-packaging.md)
- [Typed authority and role terminology](../../decisions/framework/authoritative-source-terminology.md)
- [Canonical Markdown authoring rationale](../../decisions/framework/canonical-markdown.md)
- [Adaptive decision elicitation](../../decisions/framework/adaptive-decision-elicitation.md)

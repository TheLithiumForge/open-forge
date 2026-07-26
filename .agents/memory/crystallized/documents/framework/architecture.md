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

The [top Open Forge architecture](../architecture.md) is authoritative for the complete system map and the relationships among the Framework, workspace context, extensions, deterministic tools, operators, and agent runtimes. This document explains how Core and Memory work internally. It names extension and tool boundaries only where they constrain the Framework.

The Framework described here is the intended architecture to which the installable source, dogfood environment, governance, and tooling must migrate. Older files remain useful migration inputs, but they do not constrain this design merely because they exist.

The [Open Forge Markdown scope](markdown/_markdown.md) contains the current canonical syntax, routed representation, and compatibility specifications used by this architecture.

The [Open Forge Routing scope](routing/_routing.md) owns the detailed current contracts for navigation, selection, recursive scope, inheritance, loading, continuity, and path identity. This document retains the routing summary needed to understand the complete Framework.

## Framework Promise

The Framework turns a workspace into a navigable, persistent, and adaptable context environment without replacing native agent reasoning or installing a universal methodology.

It must make these questions cheap to answer:

- Where do I enter the workspace?
- Which context applies to this goal?
- What is binding, accepted, tentative, or historical?
- Which file or external system is authoritative for the meaning?
- What broader context remains active in this scope?
- Where should a new rule, result, idea, observation, or decision go?
- How can the workspace evolve without accumulating competing truth?

Human-readable Markdown contains the complete semantic answer. Deterministic tools may make every operation faster and safer, but the files remain sufficient to inspect, navigate, and maintain the Framework.

## Architectural Invariants

The following structural constraints realize the [Open Forge Principles](../principles.md) throughout Core and Memory. They are architectural consequences rather than a second authoritative source for product identity:

1. Every important concept has one authoritative source for each distinct question
2. Routes expose enough information to select relevant context before loading its body
3. Loaded ancestor Axioms remain active below them without being copied into children
4. Loading controls visibility and timing, not authority
5. Scope is explicit in the route path, entrypoint meaning, description, and relationships
6. Workspace meaning remains human-readable and reconstructable without the CLI, caches, receipts, or retrieval databases
7. Standard routes ship as useful defaults while remaining removable, replaceable, and recursively customizable
8. Removed defaults stay removed unless restoration is explicitly requested
9. Memory may describe any subject without activating behavior it records
10. Accepted behavior that should govern work belongs in Core
11. Generated metadata is derived and rebuildable
12. Structure and tools improve the probability of correct agent behavior without claiming mechanical control over reasoning
13. Instantiating a template transfers ownership to the destination; the template does not manage the result
14. Framework contracts target the broadest stable authoritative route that preserves their required meaning

## Shipped Framework

Open Forge ships a standard Framework rather than an empty routing library. Its routes embody the useful starting environment developed through dogfooding while remaining ordinary files that users may reshape.

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
  workspace/
  memory/
    working/
      handoffs/
      sessions/
    emerging/
      analysis/
      ideas/
      observations/
    crystallized/
      decisions/
      documents/
    archived/
```

The canonical workspace entry and loader form the entry boundary. Core route categories provide the shared primitive vocabulary. Memory supplies the standard state model and starter routes used to preserve continuity and evolution.

This tree is the distributed product shape, not an untouchable taxonomy. A user may add scopes, add or remove routes, replace framework files, use only a subset of the primitives, or reorganize local material through valid route chains. Removing a standard route removes that capability from the local profile; it does not make the remaining Framework invalid. Validation checks the structure that exists rather than demanding that deleted defaults reappear.

Core is the dependency floor because every other Open Forge area relies on its entry, routing, authority, and relationship semantics. The shipped Framework includes both Core and Memory because persistence and deliberate evolution are central to the product rather than optional afterthoughts.

## Canonical Entry

[`AGENTS.md`](../../../../../AGENTS.md) is the canonical workspace entry. It stays extremely small:

1. Identify Open Forge as the workspace operating contract
2. Direct the agent to [the loader](../../../../loader.md) before work begins
3. Require applicable Open Forge instructions throughout the task

Provider-specific harness files are minimal bridges to that canonical entry. They may use the import syntax required by a provider, but they do not restate Open Forge policy or become separate authoritative sources.

Managed entry blocks preserve workspace-owned content outside their markers. A provider bridge can therefore be installed or updated without claiming the entire file.

## Core

Core provides the smallest common language needed to route, interpret, and apply workspace context. It does not try to encode ordinary reasoning, a development lifecycle, or a complete methodology.

Framework wording uses #Core collectively when any suitable Core route may satisfy a requirement. It names a specific primitive when that primitive's distinct semantics matter, such as Directives for binding behavior, and enumerates concrete standard routes when the shipped default set itself is the subject. This keeps customizable Frameworks valid without weakening precise contracts.

Core contains:

- The loader and entrypoint contract
- Authority, inheritance, loading, tag, and overwrite semantics
- Directives
- Guidance
- Patterns
- Skills
- Templates
- Workflows
- Workspace routes

These categories are distinct because they answer different questions. Their default entrypoints and current local contents are exposed by the [directives](../../../../directives/_directives.md), [guidance](../../../../guidance/_guidance.md), [patterns](../../../../patterns/_patterns.md), [skills](../../../../skills/_skills.md), [templates](../../../../templates/_templates.md), [workflows](../../../../workflows/_workflows.md), and [workspace](../../../../workspace/_workspace.md) routes.

### Loader And Entrypoints

The [routing model](routing/model.md) is authoritative for the complete entrypoint, entry, direct-child navigation, and selection contract.

The loader is the canonical Framework entry after the workspace harness. It defines the universal terms and rules required to navigate the installed environment. It exposes direct root routes rather than flattening their descendants into a central catalogue.

An `entrypoint` is a Markdown file that makes its folder routable. Open Forge-authored entrypoints use `_{folder-name}.md`. Compatibility names may be recognized during migration or interoperability, but each routable folder has only one active entrypoint.

An `entry` is one generated line under an entrypoint's `Entries` heading. It identifies one direct routed file or one direct child entrypoint through:

- A description that explains enough meaning to select or skip it
- A containing-file-relative Markdown path to the destination
- Tags that provide compact loading, type, scope, and search signals

Each folder in a visible nested route has its own entrypoint. Parent entries expose only direct children. They do not flatten deeper files because flattening would duplicate route knowledge, erase intermediate scope, and make every higher index grow with the whole subtree.

Descriptions are natural, descriptive selection surfaces rather than formulaic declarations. Optional `responsibility` may bound what an opened file is responsible for defining when that adds distinct value. A primary question remains an authoring and migration test for authority and placement rather than another required frontmatter field.

### Route Types And Scope

The [route scope and inheritance contract](routing/scope.md) is authoritative for the complete meanings and recursive composition of these route types.

A `root route` is exposed directly by the loader.

A `framework route` is a standard Core or Memory route shipped by Open Forge.

A `scope route` is a local routed subtree whose concrete folder name narrows authority or meaning.

A `scoped framework route` initializes a standard Framework route inside a scope that needs it.

A `slug` is the concrete folder name used for a scope in an installed path. Template placeholders may explain possible layouts in documentation or tools, but installed workspaces contain concrete slugs.

Scope placement changes meaning. For example:

```text
memory/crystallized/mobile-app/documents/
```

This path contains documents for `mobile-app` inside the broader Crystallized state.

```text
memory/mobile-app/crystallized/documents/
```

This path gives `mobile-app` its own full Memory lifecycle.

Both are valid when their entrypoints make the meaning clear.

### Top-Down Selection

The [routing model](routing/model.md) and [loading contract](routing/loading.md) own the complete selection and context-entry sequence.

Routing proceeds from known general context to selected detail:

1. Load the canonical entry and loader
2. Read the baseline routes exposed for immediate loading
3. Recover continuity routes that must survive a changed session or selected branch
4. Use the current goal and visible entries to select relevant scopes
5. Load each selected entrypoint before following its entries
6. Follow explicit relationships to the files or external systems authoritative for detailed truth

An entry description is the selection surface. The routed body is authoritative for the complete concept, instruction, capability, recipe, state, or external relationship. Selection wording belongs in the entry description; body content should not require the agent to rediscover why the route was selected.

This model keeps unselected siblings out of active context. Workspace size may increase indefinitely through additional branches, while the ordinary context cost grows mainly with the routes and relationships selected for the goal.

### Inheritance

The [route scope and inheritance contract](routing/scope.md) owns the complete loaded-inheritance model.

An Axiom is a binding instruction under an `Axioms` heading in a loaded Framework file.

Loaded ancestor entrypoint Axioms apply throughout their selected descendants. A child adds only rules that are specific to its narrower route. It does not copy its ancestors or use an empty declaration to cancel them.

Inheritance follows the loaded route chain. Merely finding or inspecting a file as inactive source, history, or an example does not activate the route that file would govern.

### Loading

The [routing loading and continuity contract](routing/loading.md) is authoritative for the complete visibility, timing, refresh, and deterministic-assistance model.

The loader defines the exact reserved loading tags. Architecturally, they serve two different needs:

- `#LoadNow` identifies visible child context that an already-loaded parent requires immediately
- `#KeepInMind` identifies continuity context that must be recovered across task entry, resume, context restoration, handoff, closeout, or another transition that may have changed its follow-ups

`#LoadNow` follows visible parent-child routing. A hidden descendant does not load merely because it carries the tag.

`#KeepInMind` deliberately crosses the current selected branch so standing commitments and candidate follow-ups are not lost when the active route changes. Each result remains subject to the authority and scope established by its route and content.

Files without a reserved loading tag remain on demand. Ordinary descriptive tags help selection and search without changing loading or authority.

At every required `#KeepInMind` boundary, the optional `open-forge load --bodies` command may batch the loader, visible immediate-loading closure, continuity set, and adjacent overwrites into one ordered stream. The same context remains obtainable through the plain-file contract.

## Authority

Open Forge does not reduce authority to one global ranking because different authoritative sources answer different questions. It resolves meaning through type, scope, current direction, and declared authority.

The operating rules are:

1. Platform constraints and runtime safety bound every action
2. Clear current operator direction governs goals, priorities, consequential tradeoffs, and accepted changes within its scope
3. A declared external source of truth is authoritative for the facts delegated to it
4. Loaded Core Axioms and directives govern Framework interpretation and applicable behavior
5. A selected authoritative source governs the accepted state of its subject
6. Narrower selected material of the same non-directive kind is preferred when it safely specializes broader material
7. Loaded directives add constraints to ancestor directives rather than silently replacing them
8. A user-owned overwrite has final precedence only within its base file's scope

Loading a file makes it visible. A tag can classify it or affect loading. Neither operation creates authority by itself.

A clear instruction, correction, confirmation, or request to apply a settled choice is accepted within its stated scope. The agent should not ask for the same approval again. Tentative language remains contextual, and unresolved ambiguity must be clarified before dependent work treats it as accepted.

When accepted direction changes an authoritative source, that source is updated and useful superseded context is moved to the appropriate decision or archive. Unresolved conflicts are reported with their authoritative sources and scopes rather than silently resolved through file order.

## Core Primitives

### Directives

A directive is a binding instruction in a route-selected scope.

Direct files under the baseline root directive route apply workspace-wide. A child directive entrypoint establishes a narrower positive scope before its direct directive files load. Once selected and loaded, a directive does not ask the agent to decide applicability a second time.

Child directives add to broader directives. Narrower scope does not create a hidden authority override. A conflict must be surfaced.

Use a directive when behavior is mandatory and needs an independently authoritative route.

### Guidance

Guidance provides adaptable judgment for a recurring situation, decision, or tradeoff. It explains a preferred approach and why it helps while allowing the current context to justify another choice.

Guidance is advisory. It neither binds behavior nor replaces an executable capability or workflow.

### Patterns

A pattern defines a concrete reusable shape for an inspectable result, such as code organization, file placement, an API boundary, naming, or a document structure.

An applicable pattern is the established default shape in its scope. A deliberate alternative remains possible when the pattern does not fit. If a shape is mandatory, a directive must own that requirement in addition to any explanatory pattern.

### Skills

A skill is a specialized capability expressed through the standard `SKILL.md` contract supported by the active agent runtime.

Open Forge makes skills routable without redefining their activation, instructions, resources, or execution. A selected `SKILL.md` remains authoritative for its references, scripts, assets, and loading decisions.

### Templates

A template is a reusable source artifact intended to be instantiated into independently owned workspace content.

An agent selects a relevant template, copies it, removes irrelevant material, replaces placeholders and source metadata, and assigns the result to its correct destination. From that point, the destination is authoritative for the result. Later template changes do not propagate to it.

This ownership transfer distinguishes Templates from Patterns. A Pattern remains the established reusable shape for later creation and review. A Template contributes starting contents. A template may implement or link to a Pattern, and a Directive or Axiom may require continuing behavior, but the template itself creates neither continuing conformance nor authority.

Generic templates are fallbacks rather than universal schemas. A specialization earns a separate file only when it offers materially different copy-ready content. Templates may be scoped, edited, replaced, or removed through the same file-native customization model as other routes.

Each template makes its selection contract explicit before instantiation. Its route description states the need it satisfies and the primary question or result it answers, while removable source instructions repeat that context for a reader inspecting the template directly.

The installable Framework currently ships the [Templates category contract](../../../../templates/_templates.md) without concrete starter artifacts. This repository dogfoods candidate document and Memory templates locally before any of them are considered for the shared payload. The [maintenance contract](../maintenance/payload/agents/templates.md) is authoritative for that distribution boundary.

The [Templates as a Core primitive decision](../../decisions/template-primitive.md) preserves why this role was added instead of assigning copy-ready source content to Patterns or another existing primitive.

### Workflows

A workflow is a repeatable Markdown recipe for reaching a defined goal through multiple steps, capabilities, or handoffs.

Workflows are optional recipes, not provider-specific orchestration objects and not a mandatory project lifecycle. Work may start at any appropriate point, skip unnecessary preparation, repeat steps, move backward when evidence changes, or proceed directly when no installed workflow adds value.

The workflow route exposes enough description and tags to choose a useful recipe before opening it. The recipe is then authoritative for its goal, required context, constraints, steps, loops, outputs, and completion contract. Its authoritative route may evolve the exact authoring schema during migration without changing this architectural role.

### Workspace

A workspace route is a coarse map to an important project location or declared external authoritative system. It explains what the destination contains and when it matters without copying the destination's details.

Workspace routes connect the Framework to source code, documentation, issue systems, repositories, datasets, products, or other sources of truth. The destination retains its own authority.

### Why There Is No Rules Primitive

Core does not provide a separate `rules/` route.

Binding rules already have two clear authoritative sources:

- Loader and entrypoint Axioms are authoritative for universal or inherited Framework mechanics
- Directives are authoritative for independently routed mandatory behavior

A third rules category would duplicate those roles and make placement less obvious. A workspace may create a linked rule map for discovery, but such a map points to authoritative routes and does not reproduce their contents.

## Memory

[Memory](../../../_memory.md) is the Framework's self-growing Markdown state for live work, agent communication and coordination, continuity, accepted records, historical context, and candidate learning. It preserves useful information across work without turning every conversation or recorded statement into current truth or active behavior.

Memory has two independent dimensions:

- State describes how the material should currently be treated
- Scope describes the person, project, component, discipline, repository, or other subject to which it applies

The route path expresses both dimensions. Neither requires a centralized registry.

### Memory States

| State | Purpose | Normal authority |
|---|---|---|
| Working | Temporary context needed to continue or resume active work | Contextual |
| Emerging | Potentially reusable material that remains unsettled | Contextual |
| Crystallized | Accepted durable state within its declared scope | Current truth |
| Archived | Useful historical material that no longer governs current work | Historical context |

The states are not maturity scores and do not form a mandatory pipeline. Material moves directly to whichever state matches its current meaning and authority.

### Working

[Working Memory](../../../working/_working.md) includes plans, active priorities, sessions, handoffs, intermediate state, and coordination needed by current work.

Agents may freely maintain Working Memory within the task's authority. Its defining property is expected expiration. When the active need ends, useful material is:

- Extracted to an accepted authoritative source
- Moved to Emerging for further development
- Archived as useful history
- Consolidated into another active checkpoint
- Pruned when it has no future value

The shipped `handoffs/` route provides concise static transfers across agents, sessions, tasks, or people. The shipped `sessions/` route preserves fuller chronological work context and reconstruction material. Projects may add planning, backlog, or other Working scopes when those routes earn their cost.

### Emerging

[Emerging Memory](../../../emerging/_emerging.md) preserves useful candidates whose validity, acceptance, or final destination remains unsettled.

The shipped routes distinguish:

- Analysis for structured reasoning and comparison
- Ideas for possibilities, experiments, and open questions
- Observations for grounded findings that may become reusable learning

An explicit request to preserve or explore an idea is enough to record it. An agent records an observation after one occurrence when the finding is plausibly reusable, surprising, or costly enough that losing it could cause meaningful rediscovery.

Before creating a parallel observation, later agents search for an existing record and add the new evidence there when scope and meaning match. Repeated independent occurrences are primarily a signal for consolidation and a promotion proposal, not a prerequisite for initial capture.

This capture policy favors distributed learning while still requiring plausible future usefulness. Git makes extra information recoverable, but it does not remove attention, search, and review costs.

### Crystallized

[Crystallized Memory](../../../crystallized/_crystallized.md) contains accepted durable state within its declared scope.

Clear operator direction may authorize a Crystallized update directly. Acceptance does not require a ritual phrase or separate promotion command. An agent may update the appropriate authoritative source when the request clearly establishes the result, should report the durable change, and should not request redundant confirmation.

Tentative choices, alternatives under investigation, and ambiguous conclusions remain Working or Emerging until dependent work requires clarification or the direction becomes accepted.

The standard Crystallized routes are [decisions](../../decisions/_decisions.md) and [documents](../_documents.md). Other accepted records may live in their own scoped routes or in declared external systems.

### Archived

[Archived Memory](../../../archived/_archived.md) preserves useful context after it stops governing current work.

Before archival, current meaning is extracted to its new authoritative source. Archived material records its origin, why it became historical, and what replaced it when a replacement exists.

Restoration is a new transition rather than an authority reversal. The material is validated against current conditions and moved into an explicit Working, Emerging, Crystallized, or external destination before it can govern work again.

Archive child routes may mirror the origin or responsibility that makes history easier to find. They are organizational scopes, not additional Memory states.

### Transitions

Any state transition is valid when the destination accurately represents the material's meaning, scope, and authority.

Common examples include:

- Working to Emerging when an active discovery may benefit future work
- Working to Archived when a session ends but its history remains useful
- Emerging to Crystallized when direction is accepted
- Emerging to Archived when a candidate is rejected but its reasoning remains valuable
- Crystallized to Archived when accepted state is superseded
- Archived to Emerging when an old idea becomes relevant but must be reconsidered
- Direct creation in Crystallized when the operator clearly establishes accepted state

Movement should update links and authoritative sources so the old location does not continue to imply current authority.

## Current Knowledge Roles

Memory provides standard roles for different forms of durable knowledge:

| Role | Primary question | Contract |
|---|---|---|
| Current document | What is true now, and how does it work? | Explains a coherent accepted concept completely enough to use |
| Decision | What was chosen, and why? | Preserves a discrete accepted choice and useful rationale |
| Directive | What behavior is mandatory here? | Binds work in its loaded route-selected scope |
| Archive | What happened before? | Preserves useful history without governing current work |

A current document integrates accepted state. When a decision contains useful rationale, the current document states the accepted concept and links to the decision for why. The decision identifies the accepted choice, preserves relevant alternatives and tradeoffs, and links forward to the authoritative source that expresses the result.

This is intentional limited overlap, not duplicated authority. The current document remains usable by itself. The decision does not become a fragmented substitute for the document.

Vision, Principles, and Architecture are specialized current documents with distinct questions. `Principles` is the formal role for stable filters used to judge unfamiliar choices. `Foundation` describes how load-bearing a principle is, and `identity` describes what the complete foundational set preserves. `Essence` may summarize a concept in its description or opening, but it is not a separate knowledge role.

A scope earns its own Principles document only when several recurring unfamiliar choices depend on stable filters that are specific to that scope and not already answered by broader principles. Otherwise, the scope follows the broader principles and keeps its distinct structural meaning in Architecture. Scoped principles supplement broader principles; they do not silently override them.

`#CurrentTruth` and `#Evergreen` remain independent:

- `#CurrentTruth` marks accepted current state within scope
- `#Evergreen` marks material that must remain synchronized when the accepted state it represents changes

Evergreen creates no authority or loading behavior. An authoritative source may be CurrentTruth without requiring active synchronization, and a derived explanation may be Evergreen without being authoritative for the underlying truth.

## Relationships

Open Forge uses ordinary Markdown relationships before adding specialized retrieval machinery.

The primary relationship surface is:

- Frontmatter descriptions that become entry labels
- Containing-file-relative links that identify authoritative sources and related context
- Heading anchors that target the relevant concept
- Established descriptive tags that support classification, association, and search
- Short prose that explains why a relationship matters

Tags are signals rather than a second authority or inference system. They stay readable in Markdown and cheap for agents and tools to search.

When another authoritative source already contains the detailed meaning, a file links to it instead of restating it. Controlled mirrors are allowed only where an independently complete entry boundary needs a small synchronized contract.

This relationship model is intentionally sufficient for future graph, semantic, or vector retrieval. A derived system may traverse or rank these relationships, but its index remains rebuildable and advisory.

## Recursive Customization

Every standard route may contain local files, child scopes, or scoped Framework routes. The same entrypoint, inheritance, description, link, and tag contract works at every depth.

The preferred customization choices are:

1. Add a local routed file when the new meaning stands independently
2. Add a scope when authority or meaning needs a narrower route
3. Use a user-owned `{name}.overwrite.md` companion for a small local adjustment to a mostly suitable base
4. Edit or replace the base when the desired model is fundamentally different
5. Remove routes that provide no local value

An overwrite loads immediately after its base, inherits the base route and loading behavior, and has final precedence only within that file's scope. It is not independently indexed.

These are clarity preferences rather than limits on ownership. Users own the installed files and may choose the representation that remains easiest for their workspace to understand.

An installer or updater preserves existing user content by default. Missing standard routes are not assumed to be accidental. Completion, upgrade, replacement, and explicit restoration are distinct intents even if the future CLI exposes their final mechanics differently.

## Generated And Deterministic State

Generated `Entries` regions are derived navigation metadata. Their authored sources are route files, entrypoint frontmatter, and the filesystem structure. Rebuilding a generated region cannot change the intended meaning of the routed content.

Receipts, caches, indexes, vector databases, and other machine state may support installation, validation, retrieval, or safe removal. They remain:

- Inspectable or explainable through their public effect
- Replaceable or rebuildable
- Outside the semantic authority path
- Unnecessary for ordinary Markdown inspection

Deterministic tools may:

- Assemble ordered context
- Follow explicit route chains
- Validate links, entrypoints, metadata, and inheritance
- Rebuild generated entries
- Preview and apply bounded file changes
- Preserve user content
- Detect collisions and unsafe paths
- Report provenance and affected authoritative sources

They may not silently promote a candidate, infer accepted direction, or make a private database the only place where a rule or relationship exists.

## Extensions Boundary

Extensions add optional reusable content through the same Framework routes and primitive meanings. They do not create a second loader, root authority model, or runtime interpretation system.

After installation, an extension's files behave like ordinary directives, patterns, guidance, skills, templates, workflows, workspace routes, or Memory. Its packaging metadata does not become necessary to understand its runtime meaning.

The current extensions implementation is an MVP under planned architectural review. The [Extensions MVP Architecture](../extensions/architecture.md) is authoritative for its present manifests, dependency system, ownership lifecycle, and liabilities. The [Extensions overhaul candidate](../../../emerging/ideas/extensions-overhaul.md) preserves prospective replacement design. The [CLI MVP Architecture](../cli/architecture.md) is authoritative for the current deterministic implementation, while the [CLI overhaul candidate](../../../emerging/ideas/cli-overhaul.md) preserves its prospective replacement.

## Distribution And Dogfood

Users receive the installable Framework from [`src/open-forge/`](../../../../../src/open-forge/). Installed files must contain every contract required to navigate and use the Framework. Governance, repository history, and unpublished design context cannot be hidden runtime dependencies.

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

## Migration Contract

This architecture is the target current view for the Framework migration.

For each older source, governance file, or decision:

1. Compare its meaning with this architecture and current maintainer direction
2. Keep useful current behavior at its correct authoritative source
3. Improve unclear or unnecessarily expensive contracts
4. Extract distinct rationale, patterns, directives, or maintenance requirements
5. Remove duplication and obsolete mechanisms
6. Update every affected link and route
7. Preserve only historical material that retains future value

An existing file is not retained merely because another file links to it. Links are part of the migration surface and move with authority.

The [approved design baseline](../../../archived/sessions/2026-07-26_open-forge-design-baseline.md) preserves the broader reasoning used to establish this architecture.

## Related Authoritative Sources

- [Open Forge vision](../vision.md)
- [Open Forge principles](../principles.md)
- [Top Open Forge architecture](../architecture.md)
- [Open Forge Markdown scope](markdown/_markdown.md)
- [Open Forge Routing scope](routing/_routing.md)
- [Extensions MVP Architecture](../extensions/architecture.md)
- [CLI MVP Architecture](../cli/architecture.md)
- [Canonical Framework loader](../../../../loader.md)
- [Current Core directives](../../../../directives/_directives.md)
- [Current Templates route](../../../../templates/_templates.md)
- [Templates source maintenance contract](../maintenance/payload/agents/templates.md)
- [Current Memory contract](../../../_memory.md)
- [Current route to important repository authoritative sources](../../../../workspace/sources-of-truth.md)

## Migration Inputs

The following files contain earlier decisions or governance that may help migration. They are not architectural proof and may be rewritten, consolidated, moved, or archived as their subjects receive final authoritative sources:

- [Product direction rationale](../../decisions/product-direction.md)
- [Distinct Core primitive role rationale](../../decisions/core-primitives.md)
- [Templates as a Core primitive rationale](../../decisions/template-primitive.md)
- [Routing model rationale](../../decisions/routing-model.md)
- [Routing surface rationale](../../decisions/routing-surfaces.md)
- [Scope and slug rationale](../../decisions/scope-and-slugs.md)
- [Tag rationale](../../decisions/tags.md)
- [Loading reliability rationale](../../decisions/loading-reliability.md)
- [Memory model rationale](../../decisions/memory-model.md)
- [Workflow shape rationale](../../decisions/workflow-shape.md)
- [Source and packaging rationale](../../decisions/source-and-packaging.md)
- [Typed authoritative source terminology](../../decisions/authoritative-source-terminology.md)
- [Canonical Markdown authoring rationale](../../decisions/canonical-markdown.md)
- [Agent primitives migration descriptor](../../../../../docs/framework/concepts/agent-primitives.md)
- [Truth lifecycle migration descriptor](../../../../../docs/framework/concepts/truth-lifecycle.md)
- [Layer migration descriptor](../../../../../docs/framework/concepts/layers.md)
- [Overwrite migration descriptor](../../../../../docs/framework/concepts/overwrites.md)
- [Payload boundary migration descriptor](../../../../../docs/framework/concepts/payload-boundary.md)

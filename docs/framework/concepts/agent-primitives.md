# Agent Primitives

## Description

This descriptor governs the meaning, relationships, and scope of directives, patterns, guidance, skills, and workflows across Open Forge.

These primitives separate mandatory behavior, reusable form, contextual judgment, bounded capability, and goal-oriented orchestration.

## Represents

Agent primitives represent the kinds of routed material that shape or perform agent work.

Their type determines how an agent uses their contents. Their category placement, description, and tags communicate scope. Generated `entries` provide navigation and reserved load policy only.

## Directives

A directive is a binding instruction within a route-selected scope.

A directive loaded through the active directive route chain governs the work in that selected scope. It has no second applicability decision inside the file. Merely inspecting an example, archive, source payload, or inactive route does not activate it.

The root directives category is baseline-loaded, so every direct directive file beneath it is workspace-wide and binding. A narrower directive belongs below a positively described child directive `entrypoint`; path, description, tags, and ancestor meaning make the scope decision before the directive body is opened.

Every direct directive file defines exactly one substantive level-2 `## Axioms` section and no `Applies To` gate. Operational conditions may be stated by an Axiom, but the loaded directive itself remains binding. Use directives only for mandatory work, process, artifact, technical, or project behavior; put optional behavior in guidance, a skill, or a workflow.

Loaded child directives add to loaded ancestor directives. Narrower routing changes scope, not authority. If loaded directives conflict or cannot be followed, the agent must report the conflict and obtain an explicit decision or exception.

## Patterns

A pattern defines a concrete, reusable arrangement with stable relationships and variable contents.

A pattern must produce a recognizable shape in code, files, naming, placement, boundaries, APIs, documents, or another inspectable result. Adapter and interface relationships, feature folder trees, class organization, and document structures are patterns.

An applicable pattern is the established default shape. A different shape requires a deliberate reason. A mandatory shape must also be expressed as a directive.

Patterns may nest recursively by technology, domain, artifact, or any other useful positive scope.

## Guidance

Guidance provides contextual judgment for a recurring scenario.

It explains how to approach the scenario, why the approach is useful, relevant tradeoffs, and the patterns, skills, or workflows that may help. Suggested actions remain adaptable to the current work.

Guidance is advisory. It does not create a mandatory requirement or an executable workflow.

Guidance may nest recursively by domain, scenario, decision area, or any other useful positive scope.

## Skills

A skill is a bounded reusable agent capability package.

Skills preserve the established meaning used by AI tools. Open Forge prefers the native package shape `.agents/skills/{skill-name}/SKILL.md`, with optional runtime resources under that folder. Skills may be invoked directly or by workflows. Open Forge routes skill packages without redefining a runtime's activation or execution model.

## Workflows

A workflow is a repeatable markdown recipe for reaching a defined goal that takes more than one step or more than one skill, such as brainstorming, task creation, test-driven development, review, or implementation.

Open Forge workflows are not runtime orchestration objects from an agent SDK. They are routed recipes that describe how work should proceed.

A workflow defines an early `Mode` (`linear` or `iterative`), `Goal` (outcome, acceptance, stop), `Required Routes`, always-present `Constraints`, ordered `Steps`, `Loop` behavior, expected `Outputs`, and a `Completion` checklist, in that order. `Constraints` states `- none` when no workflow-specific invariant applies. Every workflow seeks its Goal; goal-seeking is not a separate mode. Generated `Entries` express what a workflow contains; `Required Routes` express cross-tree dependencies it needs from elsewhere.

Every complete workflow recipe declares exactly one primary phase tag: `PhaseDiscovery`, `PhaseDefinition`, `PhasePlanning`, `PhaseDelivery`, or `PhaseVerification`. Phases are non-waterfall wayfinding for inferring current state and choosing the Goal that covers the requested transition; work may start anywhere, skip, repeat, or move backward. An earlier prerequisite is recommended only when routed current truth exposes a concrete missing or contradictory input. If no installed Goal matches at all, the agent presents the closest installed option or options and direct execution once.

Agents read every `Required Routes` route before Step 1 and report a route that cannot be read as a blocker. "none" is a valid value.

A workflow step may consult guidance, apply patterns, invoke skills, delegate to a subagent, or hand off to another workflow by route while obeying directives loaded through the active route chain.

## Relationships

The primitives interact in this order of purpose:

```text
Workflow pursues a larger goal
  -> invokes skills
  -> obeys loaded directives
  -> uses guidance for contextual judgment
  -> produces work shaped by patterns
```

This order describes composition, not one authority ladder. Directives constrain work. Patterns shape results. Guidance informs judgment. Skills and workflows perform work.

## Core Payload Contract

The core payload must install these root categories:

```text
.agents/
  directives/
    _directives.md
  patterns/
    _patterns.md
  guidance/
    _guidance.md
  skills/
    _skills.md
  workflows/
    _workflows.md
```

Each category must begin with only the minimum category contract and generated index region required for routing. The core payload does not seed opinionated directive, pattern, guidance, skill, or workflow content.

These categories are populated by routed files and child categories.

Core primitive `entrypoints` must use #Core plus the singular route type tag that matches the category: #Directive, #Pattern, #Guidance, #Skill, or #Workflow.

## Scope Contract

Scope must be visible before a routed body is opened through category placement, concise descriptions, useful paths, and useful tags.

Tags must compress useful routing information such as primitive type, domain, work type, topic, technology, or artifact. Primitive type tags use singular PascalCase. For example, #Directive #Database #Migration lets an agent identify likely scope without opening the routed file.

Tags reinforce route selection but never establish authority by themselves. A direct file under the baseline-loaded root directive route is workspace-wide; a child directive route establishes narrower scope through its visible selection surface. Reserved load-policy tags affect loading only and are governed by the routing concept.

Every routed primitive file inherits the positive scope of its containing category. A child category must state whether it narrows that scope or preserves it for organization.

## Directive Loading Contract

The default root directives category is tagged #LoadNow, so its generated loader `entry` loads with the loader.

The root `entrypoint` must route agents to:

- every direct directive file, read fully
- every child directive route whose path, description, tags, or defined tag behavior match the current work

After a child directive route is selected, agents read every direct directive file exposed by that loaded child `entrypoint`. Directive bodies outside the active route chain remain routed but inactive.

Every direct directive file contains exactly one non-empty level-2 `## Axioms` section. It does not contain an `Applies To` section; the active route has already established scope.

Workspace-wide means mandatory across Open Forge work. Current user instructions, platform constraints, runtime safety, and declared external sources of truth remain higher authority.

## Workflow-Local Bundles

A workflow may contain local directive, pattern, guidance, and skill categories beneath its workflow folder.

While a workflow is active, a workflow-local directive route selected through that workflow's `Entries` establishes local scope. Its loaded directives bind within the workflow, and workspace directives remain active. Local routing narrows scope but does not silently override broader authority; unresolved conflicts must be reported.

Workflow-local patterns, guidance, and skills are preferred over broader workspace material within that workflow when safe and allowed. They apply only to that workflow unless another active route references them.

Mixed local primitive bundles are owned by workflows. Other categories extend their own primitive recursively or reference root primitive categories instead of embedding a separate mixed framework scope.

The workflow bundle uses the existing recursive category contract. It does not contain another `AGENTS.md`, root loader, workspace category, or independent Open Forge installation.

## Why

Visible path-based scope keeps agent behavior reviewable without adding activation fields, inherited metadata, or broad tag semantics.

Workflow-local bundles provide useful locality and reuse while one root framework preserves consistent authority and routing.

## Alignment Checks

Agent primitives are aligned when:

- active directive routes establish scope before their direct files are loaded
- every directive loaded through the active route chain is binding
- direct directive files contain one substantive level-2 Axioms section and no `Applies To` gate
- optional behavior uses guidance, skills, or workflows instead of directives
- patterns define concrete inspectable shapes
- guidance provides adaptable contextual judgment
- skills remain bounded reusable capability packages
- skills prefer native `SKILL.md` packages
- workflows remain repeatable markdown recipes for reaching defined goals
- workflows define Mode, Goal, Required Routes, Constraints, Steps, Loop, Outputs, and Completion in order
- workflow Mode is linear or iterative, and Constraints uses `- none` when no local invariant applies
- every complete workflow declares one primary development phase tag and treats phases as non-waterfall wayfinding
- workflow selection uses the request plus routed current truth and recommends at most one evidence-backed earlier prerequisite
- a no-match case presents the closest installed option or options and direct execution once, while explicit choice or opt-out wins
- agents read every Required Routes route before Step 1 and report unreadable routes as blockers
- every core primitive category is installed with its minimum `entrypoint`
- core primitive `entrypoints` use #Core and singular primitive tags
- opinionated primitive content remains local or optional
- tags provide compact scope and classification signals
- direct root directive files are workspace-wide because the root directive route is baseline-loaded
- nested directive categories expose their narrower selection scope through path, description, tags, and ancestor meaning
- workspace directives remain active inside workflows
- workflow-local directives bind within the active workflow without silently overriding ancestor authority
- mixed local primitive bundles exist only inside workflows
- workflow bundles reuse the root framework instead of duplicating it

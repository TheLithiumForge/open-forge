# Agent Primitives

## Description

This descriptor governs the meaning, relationships, and scope of directives, patterns, guidance, skills, and workflows across Open Forge.

These primitives separate mandatory behavior, reusable form, contextual judgment, bounded capability, and goal-oriented orchestration.

## Represents

Agent primitives represent the kinds of routed material that shape or perform agent work.

Their type determines how an agent uses their contents. Their category placement, description, and tags communicate scope. Generated `entries` provide navigation and reserved load policy only.

## Directives

A directive is a mandatory modifier within its declared scope.

An applicable directive governs every task and workflow in that scope. If applicable directives conflict or cannot be followed, the agent must report the conflict and obtain an explicit decision or exception.

The root directives category represents workspace-wide directive scope. Direct directive files in that category apply to all workspace work.

Nested directive categories must define a positive work type, topic, domain, project area, or organizational scope. A nested category may narrow its parent scope or explicitly preserve it while grouping related directives.

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

A workflow is a repeatable markdown recipe for reaching a defined goal, such as brainstorming, task creation, test-driven development, review, or implementation.

Open Forge workflows are not runtime orchestration objects from an agent SDK. They are routed recipes that describe how work should proceed.

A workflow defines required skill packages when it uses skills, ordered steps, loop behavior, expected outputs, and completion or handoff conditions.

`Required Skill Packages` in a selected workflow are load-bearing. Agents must load listed packages before running workflow steps and report missing routes.

A workflow may consult guidance, apply patterns, invoke skills, and produce artifacts while obeying applicable directives.

## Relationships

The primitives interact in this order of purpose:

```text
Workflow pursues a larger goal
  -> invokes skills
  -> obeys applicable directives
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

Scope must be visible through category placement, concise descriptions, and useful tags.

Tags must compress useful routing information such as primitive type, domain, work type, topic, technology, or artifact. Primitive type tags use singular PascalCase. For example, #Directive #Database #Migration lets an agent identify likely scope without opening the routed file.

Tags may reinforce and describe scope, but they must not be the only indication that a directive is workspace-wide or mandatory. Tags never establish authority by themselves. Reserved load-policy tags affect loading only and are governed by the routing concept.

Every routed primitive file inherits the positive scope of its containing category. A child category must state whether it narrows that scope or preserves it for organization.

## Directive Loading Contract

The default root directives category is tagged #LoadNow, so its generated loader `entry` loads with the loader.

The root `entrypoint` must route agents to:

- every direct workspace-wide directive file
- every child directive route whose path, description, tags, or defined tag behavior match the current work

Directive bodies outside the current scope remain routed but unloaded.

Workspace-wide means mandatory across Open Forge work. Current user instructions, platform constraints, and runtime safety remain higher authority.

## Workflow-Local Bundles

A workflow may contain local directive, pattern, guidance, and skill categories beneath its workflow folder.

Workflow-local directives apply while that workflow is active. Workspace directives remain active and are inherited automatically. Local directives are preferred within the workflow when safe and allowed. Unresolved conflicts must be reported.

Workflow-local patterns, guidance, and skills are preferred over broader workspace material within that workflow when safe and allowed. They apply only to that workflow unless another active route references them.

Mixed local primitive bundles are owned by workflows. Other categories extend their own primitive recursively or reference root primitive categories instead of embedding a separate mixed framework scope.

The workflow bundle uses the existing recursive category contract. It does not contain another `AGENTS.md`, root loader, workspace category, or independent Open Forge installation.

## Why

Visible path-based scope keeps agent behavior reviewable without adding activation fields, inherited metadata, or broad tag semantics.

Workflow-local bundles provide useful locality and reuse while one root framework preserves consistent authority and routing.

## Alignment Checks

Agent primitives are aligned when:

- directives are mandatory within visible positive scope
- patterns define concrete inspectable shapes
- guidance provides adaptable contextual judgment
- skills remain bounded reusable capability packages
- skills prefer native `SKILL.md` packages
- workflows remain repeatable markdown recipes for reaching defined goals
- workflows define required skill packages, steps, loop behavior, outputs, and completion
- selected workflows load their listed required skill packages before steps
- every core primitive category is installed with its minimum `entrypoint`
- core primitive `entrypoints` use #Core and singular primitive tags
- opinionated primitive content remains local or optional
- tags provide compact scope and classification signals
- paths or descriptions expose workspace-wide and mandatory scope without relying on tags alone
- root directive files are workspace-wide
- nested categories state whether they narrow or preserve scope
- workspace directives remain active inside workflows
- safe workflow-local material takes preference within its active workflow
- mixed local primitive bundles exist only inside workflows
- workflow bundles reuse the root framework instead of duplicating it

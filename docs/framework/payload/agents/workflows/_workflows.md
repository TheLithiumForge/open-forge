# Workflows Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/workflows/_workflows.md`.

The workflows category entrypoint defines how agents discover larger goal-oriented workflows and how an active workflow owns local supporting material.

## Represents

The workflows category represents repeatable agent workflows for larger goals.

A workflow organizes work toward an outcome, such as brainstorming, task creation, implementation, review, test-driven development, handoff, or learning.

## Contains

The installed workflows category entrypoint must contain:

- scoped `open-forge:` frontmatter with a description and useful tags, including `Core`, `Workflow`, and `Index`
- a title
- one short definition of workflows
- compact relevance, loading, workflow-local bundle, and completion axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 40 non-empty lines. Generated entries do not count toward this limit.

## Workflow Contract

Every routed workflow must identify its goal, starting context, ordered work shape, expected outputs, and completion or handoff condition.

A workflow may be a direct workflow file or a child workflow category. A child workflow category can contain its own entrypoint, workflow files, nested workflow categories, and local supporting categories.

## Local Bundle Contract

An active workflow may own local `directives/`, `patterns/`, `guidelines/`, and `skills/` categories beneath its workflow folder.

Workflow-local material applies only while that workflow is active. It is preferred over broader workspace material for that active workflow when safe and allowed. Unresolved conflicts must be reported.

Workflow-local categories reuse the same recursive category contract. They must not create another `AGENTS.md`, root loader, workspace category, or independent Open Forge installation.

## Loading Contract

The workflows category is relevant when current work matches a repeatable goal that may have an established workflow.

The entrypoint must route agents to direct workflow files and child workflow categories whose path, description, or tags match the current work. Each selected child entrypoint applies the same contract recursively.

When a workflow is selected, agents load its workflow entrypoint or file first, then load any relevant local supporting categories routed by that workflow.

## Scope Contract

Direct workflow files describe workflows available across the workspace within their stated applicability.

Nested workflow categories narrow or explicitly preserve their parent scope by goal, domain, work type, project area, or another positive context. Placement, descriptions, and tags must make that scope cheap to identify.

Workflows in a narrower selected scope are preferred over broader workflows when safe and allowed.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct workflow files and direct child workflow categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when work may benefit from an established goal-oriented workflow.

## Why

Workflows make repeatable agent work explicit while keeping reusable local support next to the workflow that needs it.

The empty core category gives each workspace room to add only workflows it actually uses.

## Alignment Checks

The implementation is aligned when it:

- is named `_workflows.md`
- lives in `.agents/workflows/`
- includes `Core`, `Workflow`, and `Index` in scoped `open-forge:` tags
- defines workflows as larger goal-oriented agent workflows
- selects workflow routes by visible relevance
- requires workflows to state goal, work shape, outputs, and completion
- permits local supporting primitive categories only under active workflows
- reuses the root recursive category contract for local workflow support
- prevents nested Open Forge roots under workflows
- supports recursive positive workflow scope
- prefers narrower selected workflow scopes when safe and allowed
- routes only through its final generated region
- remains empty until workflow files or child workflow categories are added

# Workflows Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/workflows/_workflows.md`.

The workflows category `entrypoint` defines how agents discover repeatable markdown workflow recipes for reaching defined goals and how an active workflow owns local #Core routes.

## Represents

The workflows category represents repeatable markdown workflow recipes for reaching defined goals.

A workflow recipe organizes work toward an outcome, such as brainstorming, task creation, implementation, review, test-driven development, handoff, or learning.

## Contains

The installed file follows the shared category `entrypoint` shape owned by the formatting concept: scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Workflow Contract

Every routed workflow defines `Goal` (outcome, acceptance, stop), `Required Routes`, ordered `Steps`, `Loop` behavior, expected `Outputs`, and a `Completion` checklist. It adds `Constraints` only when cross-step invariants exist; forcing the section everywhere creates filler.

Open Forge workflows are routed markdown recipes, not runtime orchestration objects from an agent SDK.

Generated `Entries` express containment: what lives under the workflow folder. `Required Routes` express dependency: cross-tree edges to routes the workflow needs but does not contain. The two never compete.

`Required Routes` are flat and unconditional; agents read every listed route before Step 1 and report a route that cannot be read as a blocker, not a step to skip. "none" is a valid value. Entrypoint-level targets are preferred; stable routed files are allowed. Lines use the generated entry format so tooling can parse and follow them. Directives never appear in the list because workspace directives are already loaded. Keep the list short; split the workflow or route through a package when it grows.

Loop behavior must state whether the workflow is linear or iterative, what causes another pass, and what stops the loop. The workflow mode emerges from section weight: deterministic work carries rich `Steps`, iterative work carries a `Loop` over a step range, and goal-seeking work carries a rich `Goal` with an assess-act-check loop until acceptance.

A step may invoke a skill, consult guidance, delegate to a subagent, or hand off to another workflow by route. A sub-workflow's `Required Routes` are read at that activation. Delegation handoffs name the workflow route and the active step. Composition stays in `Steps`; a workflow never silently absorbs another workflow's axioms.

A workflow may be a direct workflow file or a child workflow category. A child workflow category can contain its own `entrypoint`, workflow files, nested workflow categories, and workflow-local #Core routes.

## Local Core Contract

A workflow may own workflow-local #Core routes beneath its folder; they apply only while that workflow is active and are preferred over broader routes when safe and allowed, the natural consequence of the loader's narrower-scope preference. Unresolved conflicts must be reported.

Local directives are the typical use. Local skills are discouraged: native skill packages belong under `.agents/skills/` where runtimes discover them; workflows share them through `Required Routes` instead.

Workflow-local #Core routes reuse the same recursive category contract. They must not create another independent Open Forge installation.

## Loading Contract

The workflows category is relevant before non-trivial work and whenever current work may match an established repeatable goal.

The `entrypoint` must require agents to read `Entries` before non-trivial work and load matching direct workflow files or child workflow categories. Each selected child `entrypoint` applies the same contract recursively.

When a workflow is selected, agents load its workflow `entrypoint` or file first, then load any relevant workflow-local #Core routes routed by that workflow.

## Scope Contract

Direct workflow files describe workflows available across the workspace within their stated applicability.

Nested workflow categories narrow or explicitly preserve their parent scope by goal, domain, work type, project area, or another positive context. Placement, descriptions, and tags must make that scope cheap to identify.

Workflows in a narrower selected scope are preferred over broader workflows when safe and allowed.

## Generated Region

The final generated region uses the shared category `entrypoint` shape owned by the formatting concept.

Generated `entries` list direct workflow files and direct child workflow categories. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when work may benefit from an established goal-oriented workflow.

## Why

Workflows make repeatable agent work explicit while keeping reusable local #Core routes next to the workflow that needs them.

The empty default route gives each workspace room to add only workflows it actually uses.

## Alignment Checks

The implementation is aligned when it:

- is named `_workflows.md`
- lives in `.agents/workflows/`
- includes `Core` and `Workflow` in scoped `open-forge:` tags
- defines workflows as repeatable markdown workflow recipes for reaching a defined goal
- selects workflow routes by visible relevance
- requires agents to check workflows before non-trivial work
- requires workflows to define Goal, Required Routes, Steps, Loop, Outputs, and Completion, with Constraints only when invariants exist
- requires agents to read every Required Routes route before Step 1 and report unreadable routes as blockers
- keeps generated `Entries` as containment and `Required Routes` as cross-tree dependency
- lets steps invoke skills, consult guidance, delegate to subagents, or hand off to other workflows by route
- discourages workflow-local skills in favor of native packages shared through Required Routes
- permits workflow-local #Core routes only under active workflows
- reuses the recursive category contract for workflow-local #Core routes
- prevents independent installs under workflows
- supports recursive positive workflow scope
- prefers narrower selected workflow scopes when safe and allowed
- routes only through its final generated region
- remains empty until workflow files or child workflow categories are added
- keeps compact relevance, loading, skill package, step, loop, workflow-local #Core route, and completion axioms
- keeps the authored portion between 10 and 40 non-empty lines

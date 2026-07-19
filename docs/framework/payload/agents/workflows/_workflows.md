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

Every workflow recipe defines `Mode`, `Goal` (outcome, acceptance, stop), `Required Routes`, `Constraints`, ordered `Steps`, `Loop` behavior, expected `Outputs`, and a `Completion` checklist, in that order. Every contract section is a level-2 Markdown heading so schema validation and Required Routes parsing use the same structure. `Mode` is exactly `linear` or `iterative`. `Constraints` states `- none` when no workflow-specific invariant exists. Every complete recipe declares exactly one primary phase tag: `PhaseDiscovery`, `PhaseDefinition`, `PhasePlanning`, `PhaseDelivery`, or `PhaseVerification`; other tags remain topical.

Phases describe increasing commitment, not mandatory chronology. Work may begin at any phase when enough current truth exists, skip, repeat, move backward, or use verification evidence to reopen an earlier phase. The taxonomy remains metadata on existing routes rather than adding physical phase folders and routing layers.

A child workflow `entrypoint` may be organizational: it can narrow scope and route descendant workflows using category Axioms and Entries without pretending to be a recipe. If an `entrypoint` declares any workflow-recipe heading, it declares the complete ordered contract. Every non-entrypoint file owned as a workflow is a complete recipe. Workflow-local directive, pattern, guidance, skill, workspace, or memory categories keep their own explicit primitive type; the nearest primitive route owns validation.

Open Forge workflows are routed markdown recipes, not runtime orchestration objects from an agent SDK.

Generated `Entries` express containment: what lives under the workflow folder. `Required Routes` express dependency: cross-tree edges to routes the workflow needs but does not contain. The two never compete.

`Required Routes` are flat and unconditional; agents read every listed route before Step 1 and report a route that cannot be read as a blocker, not a step to skip. "none" is a valid value. Entrypoint-level targets are preferred; stable routed files are allowed. Lines use the generated entry format so tooling can parse and follow them. Individual directive files never appear in the list because binding instructions enter through active directive routes: workspace-wide root directives are already loaded, and workflow-specific directives belong under a workflow-local directive `entrypoint` selected with the workflow. Keep the list short; split the workflow or route through a package when it grows.

Mode is visible before Goal so an agent knows whether to expect one pass or repetition before entering the contract. Loop behavior supplies the detail: linear workflows execute Steps once; iterative workflows state what causes another pass, which steps repeat, what evidence each pass adds, and what Goal condition stops it. Every workflow seeks its Goal, so goal-seeking is not a separate mode.

A step may invoke a skill, consult guidance, delegate to a subagent, or hand off to another workflow by route. A sub-workflow's `Required Routes` are read at that activation. Delegation handoffs name the workflow route and the active step. Composition stays in `Steps`; a workflow never silently absorbs another workflow's axioms.

A workflow may be a direct workflow file or a child workflow category. A child workflow category can contain its own `entrypoint`, workflow files, nested workflow categories, and workflow-local #Core routes.

## Local Core Contract

A workflow may own workflow-local #Core routes beneath its folder; they become available only while that workflow is active. Narrower non-directive routes are preferred over broader routes when safe and allowed.

A selected workflow-local directive route follows the stricter directive contract: its route settles scope before bodies are opened, every direct directive file there is binding while that workflow scope is active, and it adds to rather than silently overriding loaded ancestor directives. Unresolved conflicts must be reported.

Local directives are the typical use. Local skills are discouraged: native skill packages belong under `.agents/skills/` where runtimes discover them; workflows share them through `Required Routes` instead.

Workflow-local #Core routes reuse the same recursive category contract. They must not create another independent Open Forge installation.

## Loading Contract

The workflows category is relevant before non-trivial work and whenever current work may match an established repeatable goal. The agent infers the established development state from routed current truth and the transition requested by the user, then selects the workflow whose Goal best covers that transition. It recommends at most one prerequisite only when a concrete missing or contradictory input would make the requested transition unreliable; otherwise it starts at the matching phase without ceremony. If no installed Goal matches, it presents the closest installed option or options and direct execution once. Several matches compose as one primary workflow with evidence-triggered handoffs. Explicit workflow choice or opt-out wins.

The `entrypoint` must require agents to read `Entries` before non-trivial work and load matching direct workflow files or child workflow categories. Each selected child `entrypoint` applies the same contract recursively.

When a workflow is selected, agents load its workflow `entrypoint` or file first, then load relevant workflow-local #Core routes routed by that workflow. Entering a workflow-local directive route loads every direct directive file in that selected scope; those files cannot add another applicability gate.

## Scope Contract

Direct workflow files describe workflows available across the workspace within the scope selected by their route.

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
- requires workflow recipes to define Mode, Goal, Required Routes, Constraints, Steps, Loop, Outputs, and Completion in order
- requires exactly one recognized primary phase tag on every complete workflow recipe and none on category-only entrypoints
- treats phases as non-waterfall wayfinding that may start anywhere, skip, repeat, or move backward
- requires every recipe contract section to use a level-2 Markdown heading
- permits category-only workflow `entrypoints` without recipe headings and requires the full contract as soon as any recipe heading appears
- preserves the nearest explicit primitive type for workflow-local directive, pattern, guidance, skill, workspace, or memory routes
- restricts Mode to linear or iterative and requires `- none` for empty Constraints
- requires agents to read every Required Routes route before Step 1 and report unreadable routes as blockers
- keeps generated `Entries` as containment and `Required Routes` as cross-tree dependency
- lets steps invoke skills, consult guidance, delegate to subagents, or hand off to other workflows by route
- discourages workflow-local skills in favor of native packages shared through Required Routes
- permits workflow-local #Core routes only under active workflows
- reuses the recursive category contract for workflow-local #Core routes
- makes every direct directive file in a selected workflow-local directive scope binding without silently overriding ancestor directives
- prevents independent installs under workflows
- supports recursive positive workflow scope
- prefers narrower selected workflow scopes when safe and allowed
- routes only through its final generated region
- remains empty until workflow files or child workflow categories are added
- infers state and requested transition, defaults to the Goal that covers it, recommends at most one evidence-backed prerequisite, presents closest installed option or options plus direct execution once when no Goal matches, composes multiple matches as evidence-triggered handoffs, and honors explicit workflow choice or opt-out
- keeps compact relevance, loading, skill package, step, loop, workflow-local #Core route, and completion axioms
- keeps the authored portion between 10 and 40 non-empty lines

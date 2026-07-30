---
open-forge:
  description: Raw handoff used to finish the Workflow catalogue rework and consolidate the first-party Extensions
  tags: [Memory, Archived, Handoff, Contextual, Historical, Workflow, Refactor, Extension, CLI, Migration]
---

# Workflow Rework Handoff

## Outcome

The handoff was completed on 2026-07-29 through the consolidated `development-toolkit` catalogue. Its contents remain raw design and migration input rather than current runtime truth.

## Purpose

This chat did contain a substantial Workflow redesign. It covered the Workflow primitive, selection, schema, CLI validation, first-party recipes, Skill dependencies, extension consolidation, lifecycle routing, handoffs, Evergreen separation, documentation, tests, and explicit non-goals.

The discussion and its earlier handover are design evidence, not automatically current truth. The repository has since accepted and implemented a different minimum recipe shape in the current uncommitted migration. Preserve that newer result unless the maintainer deliberately reopens it.

## Current Status

The worktree contains a completed but uncommitted Workflow primitive migration:

- Workflows are optional routed Markdown recipes; direct execution remains valid.
- The former eight-section, fixed-mode, and phase-aware contract was replaced by required `Goal`, `Steps`, and `Completion` sections in that order.
- `Required Routes` is optional and appears between `Goal` and `Steps` only for unconditional routed dependencies.
- Recipe-specific headings remain allowed.
- Organizational Workflow `entrypoints` may omit recipe sections.
- Fixed phase tags, mandatory active-Workflow naming, and Workflow-specific reporting ceremony were removed.
- CLI validation, first-party recipe bodies, documentation, tests, and historical records were migrated together.

The completed migration is recorded in the [Workflow migration session](../../archived/sessions/2026-07-29_workflow-migration.md). Its current owners are the [Workflow contract](../../crystallized/documents/framework/primitives/workflows.md), [Workflow shape decision](../../crystallized/decisions/workflow-shape.md), [installed entrypoint](../../../workflows/_workflows.md), and [installable source](../../../../src/open-forge/.agents/workflows/_workflows.md).

The session records 25 unit tests, 163 closure tests, both Doctor targets, build, package dry-run, index, retired-vocabulary scans, and `git diff --check` as passing. Re-run affected verification if the worktree changes before review or commit.

One wording issue still needs review: the current contract names `Goal` as part of Workflow selection, but a child body's `Goal` is not visible before top-down selection. The visible parent `description`, tags, route meaning, and explicit user direction should select the child; its `Goal` may confirm fit after loading. Resolve this explicitly at review rather than weakening top-down routing.

## Accepted Workflow Boundaries

- A Workflow is an optional recipe for a defined goal, not the engine beneath ordinary work.
- Native agent competence remains the default. A Workflow should add structure that materially changes or improves execution.
- Selection remains top-down. Visible parent descriptions, tags, route meaning, `Goal`, and explicit user direction select a Workflow; a child body does not contain the information needed to discover whether it should have been loaded.
- An explicit Workflow choice or opt-out governs optional use.
- Generated `Entries` express containment. `Required Routes` express unconditional cross-tree dependencies.
- Steps keep composition explicit when invoking Skills, Guidance, Patterns, Templates, Workspace routes, delegation, or another Workflow.
- Other Core roots remain separate. A Workflow is a recipe, not a miniature mixed-Core installation or provider-specific orchestration runtime.
- Handoffs name a Workflow or active step only when that information materially helps resumption.

Do not restore the retired `Mode`, mandatory `Constraints`, `Loop`, `Outputs`, fixed phase vocabulary, dependency sentinel, mandatory Workflow selection, active-Workflow commentary, or universal closeout ceremony without new evidence and explicit approval.

## Unfinished Catalogue Streamlining

The broader chat direction has not been implemented.

The current catalogue still contains separate generic capability Skills, individual Workflow packages, and convenience packs. It still includes packages such as `vision-capability`, `architecture-capability`, `planning-capability`, `implementation-capability`, `quality-capability`, `vision-workflow`, `architecture-workflow`, `implementation-workflow`, `testing-workflow`, `dev-workflow`, `brainstorming-workflow`, `workflow-essentials`, `planning-workflows`, and `quality-workflows`. Several manifests and READMEs still couple generic Workflows to generic capability Skills.

The discussed target was:

- One coherent optional `team-development` extension.
- Six lean recipes: Vision, Architecture, Planning, Development, Debugging, and Review.
- Development absorbs generic implementation, testing, and refactoring behavior.
- Debugging remains distinct because evidence and cause should precede mutation.
- Review remains distinct because independent scrutiny differs from creation.
- Generic capability Skill dependencies are removed where they only restate native competence.
- Skills remain available for user methodology, domain expertise, specialized capability, and deliberate customization.
- Generic brainstorming is retired as a development-phase Workflow. Deeper divergent, convergent, adversarial, premortem, or multi-perspective reasoning belongs in a separate optional deliberation capability.
- Specialized design material remains optional and should stay separate unless consolidation clearly preserves its domain value.

This was strong conversational direction, including explicit support for bundling the Workflows and removing their required Skills, but it is not represented as accepted current architecture. Reassess it through the [Extensions overhaul](../../emerging/ideas/extensions-overhaul.md) before changing package ownership or deleting packages.

## Related Boundaries

### Evergreen

Workflow execution and truth maintenance are independent. A Vision Workflow may deliberately produce accepted product direction, but accepted direction from any conversation or activity must update affected #Evergreen current views without requiring that Workflow. Do not put general document freshness inside Workflow recipes.

### Continuity

Do not force a new conversation after every Workflow. Persist durable accepted state, then use a fresh handoff only when a major goal or phase change and accumulated exploratory noise make clean context materially safer. Shared continuity belongs in the general Handoff contract or one clearly justified shared owner, not repeated in every recipe.

### Activation Policy

The chat explored `manual`, `recommend`, and `automatic` policies, with `recommend` suggested as a possible default. No such policy was accepted. The current contract uses visible selection surfaces plus explicit choice or opt-out. Do not invent automatic activation machinery during catalogue consolidation.

### Former Phase Vocabulary

The earlier discussion proposed retaining Discovery, Definition, Planning, Delivery, and Verification as extension hook identifiers. The newer accepted migration removed fixed phase vocabulary because descriptions and topical tags already provide selection and the phases implied an unwanted lifecycle. Treat the phase proposal as superseded unless deliberately reopened.

## Non-Goals

- Making Workflows mandatory for non-trivial work
- Replacing native competence with generic advice
- Embedding child activation rules
- Building a hidden Workflow runtime or background executor
- Adding provider-specific orchestration to Core
- Requiring a fresh conversation at every Workflow boundary
- Making Workflows responsible for Evergreen synchronization
- Preserving fragmented packages solely because they already exist
- Deleting or aliasing public packages without first deciding the compatibility obligation

## Recommended Next Gates

1. Review and commit the current primitive migration as one coherent gate before extending its scope.
2. Inventory every first-party Workflow, capability Skill, pack, manifest dependency, README claim, test, fixture, and installation path.
3. Decide the target catalogue and compatibility policy through the Extensions architecture: whether `team-development` is one mixed package, a convenience pack over narrower owners, or another clearly justified shape.
4. Present the exact keep, merge, move, replace, and remove mapping for maintainer approval.
5. Implement the approved catalogue migration with source, documentation, tests, indexes, ownership, and historical extraction updated together.
6. Review deliberation and continuity separately; neither should block a clean catalogue simplification.

Keep each gate small, explicit, and independently reviewable. Do not fold the unfinished extension redesign into the already completed Workflow primitive diff without maintainer approval.

## Historical Inputs

- [Archived Workflow overhaul inputs](../../archived/ideas/2026-07-29_workflow-overhaul-inputs.md)
- [Earlier Workflow redesign](../../archived/ideas/workflow-redesign.md)
- [Current extension catalogue](../../../../src/extensions/README.md)
- [Current Extensions architecture](../../crystallized/documents/extensions/architecture.md)

---
open-forge:
  description: Current Workflow role, recipe contract, selection, dependencies, iteration, phase wayfinding, composition, and workflow-local Core boundary
  responsibility: Define how routed Markdown Workflows pursue goals without becoming a mandatory lifecycle or provider-specific orchestration runtime
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Workflow, Goal, Composition]
---

# Workflows

## Role

A Workflow is a repeatable Markdown recipe for reaching a defined goal through multiple steps, capabilities, or handoffs.

Workflows are optional routed recipes. They are not provider-specific orchestration objects and do not impose one development lifecycle. Work may begin wherever enough current truth exists, skip unnecessary preparation, repeat, move backward when evidence changes, or proceed directly when no installed Workflow adds value.

## Selection

A Workflow is selected when its goal and approach materially help the current work. Direct execution remains valid when no installed recipe adds value, and explicit choice or opt-out governs optional use.

One Workflow remains primary for a goal. Additional Workflows compose as explicit ordered handoffs rather than becoming an implicit execution graph. Helpful prior work may be recommended without becoming a hidden prerequisite.

## Recipe Contract

Every complete Workflow recipe contains these level-2 sections in order:

1. `Mode`
2. `Goal`
3. `Required Routes`
4. `Constraints`
5. `Steps`
6. `Loop`
7. `Outputs`
8. `Completion`

`Goal` defines the outcome, acceptance, stop condition, and optional helpful prior work. `Constraints` always exists and uses `- none` when no Workflow-specific constraint applies. `Steps` define ordered execution, `Loop` defines repetition or its absence, `Outputs` define expected artifacts or results, and `Completion` provides the final evidence checklist.

An entrypoint may organize descendant Workflows without becoming a recipe. If an entrypoint declares any recipe section, it declares the complete ordered contract. Every non-entrypoint file classified as a Workflow is a complete recipe.

## Modes And Phases

`Mode` is exactly `linear` or `iterative`.

A linear Workflow executes its Steps once. An iterative Workflow states what causes another pass, which Steps repeat, what evidence each pass adds, and which Goal condition stops it. Every Workflow seeks its Goal, so goal-seeking is not a separate mode.

Every complete recipe declares exactly one primary phase tag:

- #PhaseDiscovery
- #PhaseDefinition
- #PhasePlanning
- #PhaseDelivery
- #PhaseVerification

Phases are wayfinding by increasing commitment, not a waterfall. Work may start at any phase, skip, repeat, or use verification evidence to reopen an earlier phase. Organizational entrypoints do not pretend to be complete recipes and carry no primary phase.

## Route Dependencies

Generated `Entries` express containment. `Required Routes` express unconditional cross-tree dependencies.

Before Step 1, the agent reads every Required Route and reports an unreadable route as a blocker. `- none` is valid. Each route uses the canonical linked entry shape, resolves relative to the Workflow file, and includes useful tags with at least the target primitive type.

Prefer entrypoint-level dependencies and keep the list short. Stable routed files are allowed when the Workflow requires one exact source. Direct Directive files do not appear because binding behavior enters through active Directive routes.

## Composition

A Step may invoke a Skill, consult Guidance, apply a Pattern, instantiate a Template, follow a Workspace route, delegate bounded work, or hand off to another Workflow. Activating a nested Workflow also activates its own Required Routes.

Composition remains explicit in Steps and handoffs. A Workflow does not silently absorb another Workflow's Axioms or turn optional supporting material into a hidden prerequisite.

## Workflow-Local Core

A Workflow may contain local #Core routes when their meaning is genuinely limited to that Workflow. These routes reuse the ordinary recursive category and routing contracts rather than creating another Open Forge installation.

Local Directives follow the stricter [Directive contract](directives.md): selecting their route establishes Workflow scope, every direct file becomes binding there, and active broader Directives remain in force. Conflicts are reported rather than silently overridden.

Reusable Skills normally remain under the standard Skills route and are shared through Required Routes. Other local Core categories are justified only when locality improves meaning enough to offset another nested route.

## Runtime And Validation

The installed [Workflows entrypoint](../../../../../workflows/_workflows.md) owns compact runtime selection behavior. The [Maintenance contract](../../maintenance/payload/agents/workflows.md) owns source alignment and deterministic verification. Validation can preserve the authored recipe contract without becoming authoritative for its meaning.

## Related Current Sources

- [Core primitive model](model.md)
- [Routing model](../routing/model.md)
- [Routed Markdown representation](../markdown/routes.md)
- [Workflows maintenance contract](../../maintenance/payload/agents/workflows.md)

## Decisions And Rationale

- [Workflow shape](../../../decisions/workflow-shape.md)
- [Distinct Core primitive roles](../../../decisions/core-primitives.md)
- [Routing surfaces](../../../decisions/routing-surfaces.md)

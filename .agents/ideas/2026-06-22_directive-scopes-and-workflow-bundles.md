---
open-forge:
  description: Directive scope through category placement and workflow-local bundles
  tags: [OpenForge, Directive, Scope, Routing, Workflow]
---

# Directive Scopes And Workflow Bundles

## Problem

Directives are mandatory modifiers within their scope. The framework needs a predictable way to distinguish workspace-wide directives from directives that apply only to a work type, topic, project area, or workflow.

The solution should use the recursive category tree before introducing activation fields, reserved tags, or CLI-specific condition logic.

## Example

A directive that requires token-saving caveman speech for all agent conversation would be workspace-wide. It belongs in the root directives scope.

The directive still needs to state its actual boundary precisely. It may govern agent conversation while preserving requested output style, source code, documentation, quoted text, and other artifacts that require normal language.

An optional version of the same behavior would be a skill rather than a workspace-wide directive.

## Recommended Scope Rule

Every directive inherits the positive scope defined by its containing category entrypoint.

- `.agents/directives/_directives.md` represents the workspace-wide directive scope.
- Direct directive files under `.agents/directives/` apply to all workspace work.
- A nested directive category defines a narrower work type, topic, domain, project area, or other positive scope.
- Direct files inside that nested category apply whenever that category scope applies.
- A nested category may preserve the parent scope when it exists only to organize many global directives, but its description must state that clearly.

This creates one recursive rule at every depth. Placement determines inherited scope; descriptions make that scope visible in generated entries.

## Example Structure

```text
.agents/
  directives/
    _directives.md
    communication.md
    change-control.md
    frontend/
      _frontend.md
      accessibility.md
    database/
      _database.md
      migrations.md
    global-communication/
      _global-communication.md
      concise-language.md
      reporting.md
```

In this example:

- `communication.md` and `change-control.md` are workspace-wide.
- `frontend/` and `database/` narrow scope by work type.
- `global-communication/` preserves workspace-wide scope while grouping several related directives.

The grouping folder does not need a reserved name. Its category description must say that its directives apply to all work.

## Loading Contract

The loader always exposes and loads the root directives category when it exists.

The root directive entrypoint must:

- load its direct directive files for every request
- expose nested directive scopes through generated entries
- require agents to load each nested scope that matches the current work
- treat loaded directives as mandatory

Descriptions provide positive scope. Tags reinforce it with compact primitive, domain, work-type, topic, technology, and artifact signals without controlling activation or authority.

## Workflow-Local Directives

A workflow bundle may contain its own directives category:

```text
.agents/workflows/task-creation/
  _task-creation.md
  directives/
    _directives.md
    acceptance-criteria.md
```

These directives apply whenever the task-creation workflow is active. They inherit workspace directives and add workflow-local requirements.

Workflow-local directives must not silently contradict workspace directives. A conflict requires an explicit decision or exception.

Work-type directives shared by several workflows belong in the root directives tree. Requirements owned by one workflow belong beside that workflow.

## Meaning Of Always

Workspace-wide does not outrank the current user, runtime safety, or platform constraints.

Within Open Forge material, an active directive is mandatory throughout its declared scope. It is not a recommendation and does not require repeated activation by each workflow.

## Optional Global And Scoped Organization

```text
directives/
  global/
    _global.md
  scoped/
    _scoped.md
    frontend/
      _frontend.md
```

This layout is permitted when it improves scanning. `global/` explicitly preserves workspace-wide scope. `scoped/` is an organizational routing category whose child categories define actual work scopes.

Root directive files, direct work-scope categories, explicit `global/` and `scoped/` folders, or a combination are valid. Scope must remain visible in category paths and descriptions. Tags should reinforce it with compact signals such as `#Directive #Database #Migration`, but tags alone must not make a directive workspace-wide or mandatory.

## Alternatives Considered

### Per-File Scope Or Activation Metadata

Fields such as `when`, `scope`, or `activation` can express more detail but require defaults, inheritance, CLI rendering, propagation, and conflict behavior. They should remain postponed until recursive placement and descriptions prove insufficient.

## Recommendation

Adopt inherited category scope as the initial directive-scoping mechanism.

Permit root files, direct work-scope categories, and optional `global/` plus `scoped/` organization together. Keep the axiom permissive while requiring every category path and description to communicate whether it preserves or narrows scope.

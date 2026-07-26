---
open-forge:
  description: Current route types, recursive scope placement, concrete slugs, loaded inheritance, and narrower-scope specialization
  responsibility: Define how routed placement narrows meaning and how loaded route scopes inherit or specialize broader context
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Routing, Scope, Slug, Inheritance, Authority]
---

# Route Scope And Inheritance

## Scope

This document is authoritative for how Open Forge route placement expresses scope and how loaded routes inherit broader meaning.

The [routing model](model.md) defines navigation and selection. The [loader](../../../../../loader.md) defines the universal runtime terms and authority Axioms. Component sources define semantics that belong only to their own route type.

## Route Types

A `root route` is exposed directly by the loader.

A `framework route` is a standard Core or Memory route shipped by Open Forge.

A `scope route` is a local routed subtree whose concrete folder name narrows authority or meaning for routes below it.

A `scoped framework route` initializes a standard Framework route inside a scope that needs it.

A `slug` is the concrete folder name used in an installed route path. `Child route` describes a relationship to a parent entrypoint, while `slug` describes the concrete path segment itself.

These terms describe different aspects of one recursive route mechanism. They do not require separate registries or hidden metadata.

## Concrete Scope

A scope route is a concrete slug folder with its own entrypoint.

Route patterns may use placeholders such as `[scope]`, `[route]`, or `[state]` in documentation, Templates, CLI plans, or Extension definitions. Installed workspaces contain concrete slugs only.

For example:

```text
memory/crystallized/mobile-app/documents/
```

This scopes documents for `mobile-app` inside the broader Crystallized state.

```text
memory/mobile-app/crystallized/documents/
```

This gives `mobile-app` its own Memory states.

Both are valid because slug placement changes meaning. Their entrypoints must make that meaning visible.

Open Forge does not reserve organizational grouping folders such as `projects/`, `domains/`, `teams/`, or `platforms/`. A workspace creates them as ordinary scope routes when useful.

## Recursive Scope

Scope routes may appear before, after, or between standard Framework route segments.

Each visible folder in the chain has one entrypoint. A scoped Framework route reuses the normal Framework contract inside its local scope unless an accepted local edit or overwrite changes it.

Users may add, reorganize, replace, or remove routes. The standard installed routes are the product defaults Open Forge ships, not an untouchable taxonomy.

## Loaded Inheritance

A loaded ancestor entrypoint establishes meaning and Axioms for its selected descendants.

A child entrypoint adds only what is specific to its route. It does not restate ancestor Axioms.

A missing or empty local `Axioms` section adds no local Axioms. An explicit `inherited` or `none` sentinel also adds nothing and cannot cancel loaded ancestor Axioms.

Inheritance follows the loaded route chain. Merely inspecting an inactive source payload, archived file, example, or unselected branch does not activate the scope that file would govern.

## Narrower Meaning

Material in a narrower selected non-directive scope may safely specialize broader material of the same kind.

Loaded directives are additive. A narrower directive route changes scope without silently replacing broader loaded directives, and unresolved conflicts are reported.

Component sources remain authoritative for any additional local composition rule. Scope placement alone does not transform Guidance into a Directive, create current truth, or grant authority to a tag.

## Related Current Sources

- [Routing model](model.md)
- [Loading and continuity](loading.md)
- [Path identity and containment](paths.md)
- [Framework Architecture](../architecture.md)
- [Canonical loader](../../../../../loader.md)

## Decisions And Rationale

- [Scope and slugs](../../../decisions/scope-and-slugs.md)
- [Routing model](../../../decisions/routing-model.md)
- [Typed authoritative source terminology](../../../decisions/authoritative-source-terminology.md)

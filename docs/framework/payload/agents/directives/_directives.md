# Directives Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/directives/_directives.md`.

The directives category entrypoint defines mandatory workspace modifiers, their recursive scope model, and generated navigation to directive files and child directive categories.

## Represents

The directives category represents requirements that govern how applicable agent work is performed.

The root category represents workspace-wide directive scope. Nested directive categories narrow that scope to a work type, topic, domain, project area, or other positively described context, or explicitly preserve a parent scope for organization.

## Contains

The installed directives category entrypoint must contain:

- scoped `open-forge:` frontmatter with a description and useful tags
- a title
- one short definition of directives
- compact loading, authority, and scope axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 40 non-empty lines. Generated entries do not count toward this limit.

## Loading Contract

The loader must load the root directives category for every request when it exists.

The root directives entrypoint must require agents to load:

- every direct directive file in the root category
- every child category that explicitly preserves workspace-wide scope
- every routing-only category needed to discover narrower scopes
- every child directive category whose positive scope matches the current work

Directive bodies outside the current scope remain routed but unloaded.

## Authority Contract

An applicable directive is mandatory throughout its scope.

Current user instructions, platform constraints, and runtime safety remain higher authority. Conflicting applicable directives require an explicit decision or exception. Agents must not invent implicit precedence between conflicting directives.

Directives in a narrower selected scope are preferred over broader directives when safe and allowed. Unresolved conflicts require an explicit decision or exception.

## Scope Contract

Direct directive files under `.agents/directives/` are workspace-wide.

Every child category entrypoint must state a positive scope in its description. It must state whether the category narrows its parent scope or preserves that scope for organization.

The directives tree may combine:

- direct workspace-wide directive files
- direct child categories for concrete work scopes
- an optional `global/` category that preserves workspace-wide scope
- an optional `scoped/` routing category whose children define concrete work scopes

Folder names, descriptions, and tags work together to make scope cheap to identify. Tags must provide compact signals such as `#Directive #Database #Migrations`. Paths or descriptions must keep workspace-wide and mandatory meaning readable without relying on tags alone.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct directive files and direct child directive categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

The loader exposes this category as mandatory root material. Every request uses its workspace-wide directives and follows its routes to additional applicable scopes.

## Why

The directives category makes mandatory behavior continuously discoverable without loading every scoped directive body.

Visible recursive scope supports small workspaces and deeply organized directive trees without activation metadata or framework-specific condition logic.

## Alignment Checks

The implementation is aligned when it:

- is named `_directives.md`
- lives in `.agents/directives/`
- is loaded for every request
- defines direct root files as workspace-wide
- supports direct scope categories and optional `global/` plus `scoped/` organization together
- uses descriptions and tags as compact scope signals
- treats applicable directives as mandatory
- prefers narrower selected directive scopes when safe and allowed
- routes only through its final generated region
- leaves generated entries empty until local files or optional modules add directive content

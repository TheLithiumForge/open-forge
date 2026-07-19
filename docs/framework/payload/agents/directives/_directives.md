# Directives Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/directives/_directives.md`.

The directives category `entrypoint` defines mandatory instructions agents must follow when they apply to the current work, their recursive scope model, and generated navigation to directive files and child directive categories.

## Represents

The directives category represents requirements that govern how applicable agent work is performed.

The root category routes directive scope but does not silently make every direct file applicable. Each directive states a positive `Applies To`; nested directive categories narrow that scope to a work type, topic, domain, project area, or other positively described context, or use the hybrid `inherited` declaration when they add no narrower scope.

## Contains

The installed file follows the shared category `entrypoint` shape owned by the formatting concept: scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Loading Contract

The root directives category is loaded through its generated loader `entry` because its installed metadata includes #LoadNow.

The root directives `entrypoint` must require agents to load:

- every direct directive file far enough to evaluate its explicit `Applies To`
- every child directive route whose path, description, tags, or defined tag behavior match the current work

Directive bodies outside the current scope remain routed but unloaded.

## Authority Contract

An applicable directive is mandatory throughout its scope.

Current user instructions, platform constraints, and runtime safety remain higher authority. Conflicting applicable directives require an explicit decision or exception. Agents must not invent implicit precedence between conflicting directives.

Directives in a narrower selected scope are preferred over broader directives when safe and allowed. Unresolved conflicts require an explicit decision or exception.

## Scope Contract

Every directive file declares one non-empty `## Applies To` before `## Axioms`. A direct file under `.agents/directives/` says `workspace-wide` explicitly when that is intended; root placement and tags are not substitutes.

Every child category `entrypoint` must state a positive scope in its description. It must state whether the category narrows its parent scope or preserves that scope for organization; `inherited` is allowed for this hybrid category role.

The directives tree may use direct child categories, organizational routing categories, or deeper nested categories when their `entrypoints` make scope and loading behavior explicit.

Folder names, descriptions, and tags work together to make scope cheap to identify. Tags must provide compact signals such as #Directive #Database #Migration. Paths or descriptions must keep workspace-wide and mandatory meaning readable without relying on tags alone.

## Generated Region

The final generated region uses the shared category `entrypoint` shape owned by the formatting concept.

Generated `entries` list direct directive files and direct child directive categories. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

The loader exposes this category as mandatory root material. Every request evaluates direct applicability and follows its routes to additional applicable scopes.

## Why

The directives category makes mandatory behavior continuously discoverable without loading every scoped directive body.

Visible recursive scope supports small workspaces and deeply organized directive trees without broad activation metadata or framework-specific condition logic.

## Alignment Checks

The implementation is aligned when it:

- is named `_directives.md`
- lives in `.agents/directives/`
- includes `Core`, `Directive`, and `LoadNow` in scoped `open-forge:` tags
- requires every directive file to declare explicit `Applies To` before Axioms
- requires direct root files to say workspace-wide when that is intended
- supports recursively scoped child directive categories
- uses descriptions and tags as compact scope signals
- treats applicable directives as mandatory
- prefers narrower selected directive scopes when safe and allowed
- routes only through its final generated region
- leaves generated `entries` empty until directive files or child directive categories are added
- keeps compact loading, authority, and scope axioms
- keeps the authored portion between 10 and 40 non-empty lines

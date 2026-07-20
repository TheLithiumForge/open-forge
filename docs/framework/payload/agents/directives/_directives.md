# Directives Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/directives/_directives.md`.

The directives category `entrypoint` defines binding instructions, their visible route-selected scope, and generated navigation to directive files and child directive categories.

## Represents

The directives category represents mandatory requirements for work reached through the active directive route chain.

The root category is baseline-loaded, so every direct directive file beneath it is workspace-wide and binding. Nested directive categories narrow scope to a positively described work type, topic, domain, project area, or other context before their direct files are opened.

## Contains

The installed file follows the shared category `entrypoint` shape owned by the formatting concept: scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Loading Contract

The root directives category is loaded through its generated loader `entry` because its installed metadata includes #LoadNow.

Every direct directive file carries #LoadNow in its metadata, so its generated entry is read whenever its parent directive `entrypoint` has already been loaded.

The root directives `entrypoint` must require agents to:

- read every direct directive entry through #LoadNow
- select every child directive route whose path, description, tags, or ancestor meaning match the current work

Selecting and loading a child directive `entrypoint` establishes its narrower scope before its direct entries are considered. Those direct entries also carry #LoadNow, so every direct directive file in the selected route is read without a directive-specific traversal rule. Directive bodies outside the active route chain remain routed but inactive.

## Authority Contract

A directive loaded through the active directive route chain is binding throughout that selected scope. Inspecting an example, archive, source payload, or inactive route does not activate it.

Current user instructions, platform constraints, runtime safety, and declared external sources of truth remain higher authority. Conflicting loaded directives require an explicit decision or exception. Agents must not invent implicit precedence between conflicting directives.

Loaded child directives add to loaded ancestor directives. Narrower routing changes scope, not authority; unresolved conflicts require an explicit decision or exception.

## Scope Contract

Every direct directive file carries #LoadNow and contains exactly one non-empty level-2 `## Axioms` section. Operational conditions belong inside the relevant Axiom, while optional behavior belongs in guidance, a skill, or a workflow.

Every child category `entrypoint` must expose a positive selection scope through its path, description, tags, and ancestor meaning. Its Axioms may add scope-specific rules or use `inherited` or `none` when it adds no local Axioms.

The directives tree may use direct child categories, organizational routing categories, or deeper nested categories when their `entrypoints` make scope and loading behavior explicit.

Folder names, descriptions, and tags work together to make scope cheap to identify before the body is opened. Tags provide compact signals such as #Directive #Database #Migration, but route selection creates scope and loaded Axioms create authority.

## Generated Region

The final generated region uses the shared category `entrypoint` shape owned by the formatting concept.

Generated `entries` list direct directive files and direct child directive categories. Direct file entries expose their authored #LoadNow tag. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

The loader exposes this category as mandatory root material. Every request loads its direct workspace-wide directives and follows selected child routes to narrower binding scopes.

## Why

The directives category makes mandatory behavior continuously discoverable while keeping one top-to-bottom decision: select the route, then obey every directive loaded through it.

Visible recursive scope supports small workspaces and deeply organized directive trees without broad activation metadata or framework-specific condition logic.

## Alignment Checks

The implementation is aligned when it:

- is named `_directives.md`
- lives in `.agents/directives/`
- includes `Core`, `Directive`, and `LoadNow` in scoped `open-forge:` tags
- requires #LoadNow on every direct directive file so ordinary loaded-parent traversal reads it
- requires every direct directive file to contain one non-empty level-2 Axioms section
- defines direct root files as workspace-wide and binding
- supports recursively scoped child directive categories
- uses paths, descriptions, tags, and ancestor meaning as the pre-load scope-selection surface
- treats every directive loaded through the active route chain as binding
- makes child routes additive without inventing hidden authority precedence
- routes only through its final generated region
- leaves generated `entries` empty until directive files or child directive categories are added
- keeps compact loading, authority, and scope axioms
- keeps the authored portion between 10 and 40 non-empty lines

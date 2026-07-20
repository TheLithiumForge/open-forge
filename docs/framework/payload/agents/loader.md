# Loader

## Description

This descriptor governs `src/open-forge/.agents/loader.md`.

The loader is the first Open Forge `entrypoint` after `AGENTS.md`. It owns only the universal top-down authority, inheritance, routing, and load-tag contract plus the generated registry of direct root routes.

## Represents

The loader represents how an agent enters an installed workspace and follows visible routes without needing a tool or category-specific bootstrap knowledge.

## Contains

The installed loader contains a short introduction, terms, one grouped `## Axioms` section, and a final marker-bounded `## Entries` registry.

Its Axioms use these groups in order:

1. `Authority And Inheritance`
2. `Routing`
3. `Tags And Loading`

Workflow execution, memory lifecycle, skill behavior, directive scope details, and other category behavior belong to their routed #LoadNow owners.

## Authority Contract

Loaded ancestor Axioms remain active below them, and child `entrypoints` add only scope-specific behavior. Loaded Axioms remain mandatory unless higher-priority user instructions, platform constraints, runtime safety, or a declared external source of truth conflicts.

Local active truth overrides Open Forge defaults. Narrower selected non-directive material is preferred over broader material of the same type when safe and allowed. Loaded directive scopes are additive and report conflicts rather than silently overriding ancestors.

## Routing Contract

The loader defines `entrypoint`, `entry`, `root route`, `framework route`, `scope route`, `scoped framework route`, `slug`, and `axiom` in installed language.

The loader exposes direct active root routes. Every routed folder has exactly one recognized `entrypoint`, and every folder in a nested route path has its own `entrypoint`. A scope route uses the same mechanism with a concrete slug.

Generated `Entries` remain navigation metadata. Entries without a load-policy tag are selected on demand from their path, description, tags, and current request. Routed owners contain detailed behavior.

When a user-owned `{name}.overwrite.md` exists, agents read it immediately after `{name}.md`. It inherits the base route and has final precedence within that file's scope.

## Loading Contract

#LoadNow is relative to an already-loaded parent. Agents read each tagged entry immediately in listed order; when the target is an `entrypoint`, its own visible Entries then apply the same rule.

#KeepInMind is the complete routed continuity set. Agents read it at task start or resume, after detected context restoration, and before a handoff or closeout. They recheck it during work only when its follow-ups may have changed. The owning content retains its normal authority.

`open-forge load --bodies` may batch exactly this traversal. Plain Markdown traversal is complete and authoritative when the command is absent.

## Category Registry

The loader ends with one generated entry for each direct child folder under `.agents/` that contains exactly one recognized category `entrypoint`.

Paths are concrete and workspace-relative. Descriptions and tags derive from each category `entrypoint`; the registry never flattens nested category contents.

## Tags Contract

The loader defines #LoadNow, #KeepInMind, #Core, #Memory, #Extension, #Contextual, and #CurrentTruth positively. Undefined tags remain routing and search signals. Load-policy tags change loading only; they do not create authority, scope, or precedence.

## Why

The loader gives an agent one small universal contract while routed owners provide all specialized behavior.

## Alignment Checks

The implementation is aligned when it:

- is immediately routable after `AGENTS.md`
- keeps the source and dogfood authored loader body aligned
- groups Axioms in the required order and keeps `## Entries` last
- defines routing terms consistently, including `scope route`
- applies ancestor Axioms before child additions
- leaves category behavior to routed #LoadNow owners
- defines #LoadNow relative to an already-loaded parent
- uses the narrowed complete #KeepInMind refresh triggers
- loads only user-owned `.overwrite.md` after its base
- treats `load --bodies` as optional batched traversal
- generates direct root route entries only
- defines reserved tags without making them authority
- keeps the authored portion within 35 to 70 non-empty lines

# Routing

## Description

This descriptor governs the shared routing model across Open Forge.

Routing lets agents discover relevant context without loading the entire workspace.

## Represents

A route identifies a destination and explains its relevance.

A category groups related routes under one category `entrypoint`. Open Forge-authored categories use `_{category}.md`; the CLI may recognize compatibility aliases for external tools.

A category `entrypoint` combines stable category meaning with generated navigation.

## Ownership

The loader contains the generated registry of active root routes and their one-line purposes.

Each category `entrypoint` owns its installed meaning, boundaries, authority, and generated `entries`.

Each framework descriptor governs its corresponding installed category file.

A routed destination owns its detailed truth.

The installed payload boundary is governed by `docs/framework/concepts/payload-boundary.md`.

Layer boundaries are governed by `docs/framework/concepts/layers.md`.

Directive, pattern, guidance, skill, and workflow semantics are governed by `docs/framework/concepts/agent-primitives.md`.

## Category Contract

Every active category `entrypoint` must define:

- what the category represents
- what its routed files represent
- when the category is relevant
- the authority of its contents
- how nested categories extend it
- its final generated index region

An installed category becomes active when its folder contains exactly one recognized category `entrypoint`. An Open Forge-authored category also requires an approved framework descriptor before it enters the payload.

The CLI must generate a loader `entry` for each direct active root route under `.agents/`.

The category `entrypoint` must expose a one-line description that provides enough meaning for an agent to decide whether to load the category without opening it first. Open Forge-authored categories use scoped frontmatter. Local categories may use supported metadata or their first body description.

Category placement and descriptions expose positive scope. Tags add compact scope signals such as domain, work type, topic, technology, and artifact.

Tags must not be the only indication of workspace-wide or mandatory behavior. Tags never create authority. Reserved load-policy tags create only the loading behavior defined in this concept and in the installed loader.

Reserved load-policy tags are #LoadNow and #KeepInMind. Layer tags such as #Core, #Memory, and #Extension are classification signals only. Built-in route type tags use singular PascalCase, such as #Directive, #Pattern, #Guidance, #Skill, #Workflow, and #Workspace.

A primitive category may extend its own type recursively at any depth. Workflows may also own mixed local bundles of directives, patterns, guidance, and skills. Other category types reference root primitives instead of embedding mixed local scopes.

Within a recursively selected category, material in a narrower positive scope is preferred over broader material of the same primitive when safe and allowed. The category `entrypoint` owns any additional precedence rules for its contents.

Axioms of loaded ancestor `entrypoints` apply to all routes below them. A child `entrypoint` adds only what is specific to its scope and does not restate ancestor axioms.

## Route Contract

A route must identify:

- where the destination is
- what the destination represents
- enough context to determine relevance

Routes are navigation. They never replace the destination's detailed truth.

Generated route metadata never defines instructions, behavior, or authority. Only reserved load-policy tags may affect loading.

Generated category `entries` may point to direct markdown files, direct child category `entrypoints`, and supported native skill package entrypoints inside skills routes.

## Load Tags

#LoadNow loads baseline route context.

When an `entrypoint` is loaded, agents read each generated `entry` tagged #LoadNow, in listed order. If the target is a category `entrypoint`, only that `entrypoint` is read first; that child `entrypoint`'s own `entries` then apply the same routing contract.

#KeepInMind loads standing follow-up context.

When an `entrypoint` is loaded, agents read each generated `entry` tagged #KeepInMind like #LoadNow. Its instructions stay active while working. Before ending meaningful work, agents recheck loaded #KeepInMind routes, in listed order, and perform the follow-ups they require, such as routing useful material produced during the work.

Load-policy tags do not create authority, scope, precedence, or a write requirement. They do not search unloaded trees; autoload exists only through a visible chain of loaded parent `entrypoints`.

The loaded target still gets its meaning from its category and authored content.

## Path Contract

Generated paths must be concrete and relative to the active workspace root.

The active workspace root is the directory whose `AGENTS.md` selected the loader. Agents and tooling must resolve `.agents/...` from that directory. They must not infer the root from Git boundaries or from the physical location of a symlink or submodule target.

This contract lets the same routed knowledge work in a repository, monorepo, shared submodule, or symlinked `.agents/` tree without runtime path constants.

## Scoped Routes

A `framework route` is an Open Forge core route with stable default meaning.

A `scope route` is a local route used to narrow meaning or ownership for routes below it. A `scope route` is created with a concrete `slug` folder and its own `entrypoint`.

A `scoped framework route` is a `framework route` initialized inside a `scope route`. It keeps the framework contract inside that scope unless a local edit or overwrite changes it.

A `slug` is the concrete folder segment used in a route path. Use `child route` when the relationship to a parent `entrypoint` matters. Use `slug` when the folder segment or route-template placeholder matters.

Every folder in a visible route chain needs an `entrypoint`. A deep file below a folder without an `entrypoint` is not reachable through generated routing.

A native skill package folder is different: the generated route points to its `SKILL.md`, and files below that folder are runtime skill resources loaded only when `SKILL.md` makes them relevant.

Route patterns may use placeholders such as `[scope]`, `[route]`, or `[state]` before install:

```text
memory/crystallized/documents/
memory/[scope]/crystallized/documents/
memory/crystallized/[scope]/documents/
memory/[scope]/crystallized/[scope]/documents/
```

The first pattern is an unscoped `framework route`. The others insert `scope routes` before, after, or between pinned `framework route` segments.

Installed workspaces contain concrete `slugs` only:

```text
memory/mobile-app/_mobile-app.md
memory/mobile-app/crystallized/_crystallized.md
memory/mobile-app/crystallized/documents/_documents.md
```

Open Forge does not pin typed grouping folders such as `projects/`, `domains/`, `teams/`, or `platforms/`. They are user-created `scope routes` when useful.

`Slug` placement changes meaning. A `slug` below a state route scopes material inside that state. A `slug` above a state route owns its own state routes. Both are valid when `entrypoints` make the scope clear.

Agents route through concrete paths and `entrypoint` content, not template syntax.

The CLI may identify `scoped framework route` `entrypoints` by known path shape and canonical `entrypoint` filename. It must not require hidden template or version metadata in installed framework files for this.

## Load Contract

Agents load routing layers in this order:

1. Load the loader.
2. Read generated `entries` tagged #LoadNow or #KeepInMind, in listed order.
3. Repeat load-policy loading inside each loaded `entrypoint`.
4. Apply every loaded `entrypoint`'s authored axioms.
5. Let the current request select other relevant `entries` by path, description, and tags.
6. Follow selected routes to the destinations that own detailed truth.
7. Before ending meaningful work, recheck loaded `entries` tagged #KeepInMind, in listed order, and perform the follow-ups they require.

Whenever a markdown file is loaded, its `.overwrite.md` companion must be loaded after it when present.

## Why

This model keeps the loader small, gives every category one authoritative meaning, supports recursive routing, and avoids a duplicated central registry.

## Alignment Checks

Routing is aligned when:

- the loader generates `entries` for direct active root routes only
- loader `entries` provide enough meaning to route by relevance
- every loader category has an installed `entrypoint`
- every Open Forge-authored category has a matching framework descriptor
- every category owns its detailed meaning in its `entrypoint`
- installed files provide enough meaning without governance descriptors
- generated `entries` remain navigation metadata plus reserved load policy
- reserved load-policy tags affect loading only
- #LoadNow reads visible baseline routes by default
- category placement and descriptions keep scope visible
- tags provide compact scope and classification signals
- layer tags and route type tags remain classification signals unless a loaded `entrypoint` defines more
- paths or descriptions expose mandatory and workspace-wide scope without relying on tags alone
- generated paths are concrete and workspace-root-relative
- physical repository and link boundaries do not change the active workspace root
- `framework routes`, `scope routes`, `scoped framework routes`, and `slugs` have distinct meanings
- `scope routes` are concrete `slug` folders with `entrypoints`
- every folder in a visible route chain has its own `entrypoint`
- `scope routes` can appear before, after, or between pinned route segments
- installed workspaces do not contain route-template placeholders
- `scoped framework route` `entrypoints` can be identified by path shape and canonical `entrypoint` filename
- routed destinations own detailed truth
- native skill packages route through `SKILL.md` and keep package internals runtime-owned
- nested categories use the same contract at every depth
- ancestor axioms apply within loaded route chains without restatement
- nested autoload exists only through loaded parent `entrypoints`
- standing follow-ups exist only through loaded parent `entrypoints`
- mixed local primitive bundles are limited to workflows
- overwrite companions load after their base files
- default Open Forge core `entrypoints` use #LoadNow
- default `memory/emerging/` and `memory/emerging/observations/` `entrypoints` use #KeepInMind
- narrower selected scopes take safe preference within the same primitive

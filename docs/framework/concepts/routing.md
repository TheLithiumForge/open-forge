# Routing

## Description

This descriptor governs the shared routing model across Open Forge.

Routing lets agents discover relevant context without loading the entire workspace.

## Represents

A route identifies a destination and explains its relevance.

A category groups related routes under one category entrypoint. Open Forge-authored categories use `_{category}.md`; the CLI may recognize compatibility aliases for external tools.

A category entrypoint combines stable category meaning with generated navigation.

## Ownership

The loader contains the generated registry of active categories and their one-line purposes.

Each category entrypoint owns its installed meaning, boundaries, authority, and generated entries.

Each framework descriptor governs its corresponding installed category file.

A routed destination owns its detailed truth.

## Category Contract

Every active category entrypoint must define:

- what the category represents
- what its routed files represent
- when the category is relevant
- the authority of its contents
- how nested categories extend it
- its final generated index region

An installed category becomes active when its folder contains exactly one recognized category entrypoint. An Open Forge-authored category also requires an approved framework descriptor before it enters the payload.

The CLI must generate a loader entry for each direct active category under `.agents/`.

The category entrypoint must expose a one-line description that provides enough meaning for an agent to decide whether to load the category without opening it first. Open Forge-authored categories use scoped frontmatter. Local categories may use supported metadata or their first body description.

## Route Contract

A route must identify:

- where the destination is
- what the destination represents
- enough context to determine relevance

Routes are navigation. They never replace the destination's detailed truth.

Generated route metadata never defines instructions, behavior, or authority.

## Path Contract

Generated paths must be concrete and relative to the active workspace root.

The active workspace root is the directory whose `AGENTS.md` selected the loader. Agents and tooling must resolve `.agents/...` from that directory. They must not infer the root from Git boundaries or from the physical location of a symlink or submodule target.

This contract lets the same routed knowledge work in a repository, monorepo, shared submodule, or symlinked `.agents/` tree without runtime path constants.

## Load Contract

Agents load routing layers in this order:

1. Load the loader.
2. Select a relevant active category from the loader entries.
3. Load that category's generated entrypoint path.
4. Select relevant routed files.
5. Follow those routes to the destinations that own detailed truth.

## Why

This model keeps the loader small, gives every category one authoritative meaning, supports recursive routing, and avoids a duplicated central registry.

## Alignment Checks

Routing is aligned when:

- the loader generates entries for direct active categories only
- loader entries provide enough meaning to route by relevance
- every loader category has an installed entrypoint
- every Open Forge-authored category has a matching framework descriptor
- every category owns its detailed meaning in its entrypoint
- generated entries remain navigation metadata
- generated paths are concrete and workspace-root-relative
- physical repository and link boundaries do not change the active workspace root
- routed destinations own detailed truth
- nested categories use the same contract at every depth

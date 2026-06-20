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

The CLI must generate a loader entry for each direct active category under `{forgePath}`.

The category entrypoint must expose a one-line description that provides enough meaning for an agent to decide whether to load the category without opening it first. Open Forge-authored categories use scoped frontmatter. Local categories may use supported metadata or their first body description.

## Route Contract

A route must identify:

- where the destination is
- what the destination represents
- enough context to determine relevance

Routes are navigation. They never replace the destination's detailed truth.

Generated route metadata never defines instructions, behavior, or authority.

## Load Contract

Agents load routing layers in this order:

1. Load the loader.
2. Load the constants required to resolve generated category paths.
3. Select a relevant active category from the loader entries.
4. Load that category's generated entrypoint path.
5. Select relevant routed files.
6. Follow those routes to the destinations that own detailed truth.

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
- routed destinations own detailed truth
- nested categories use the same contract at every depth

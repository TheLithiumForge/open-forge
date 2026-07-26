# Ideas Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/emerging/ideas/_ideas.md`.

The ideas memory category `entrypoint` defines how agents discover future possibilities, experiments, open questions, and options to explore later.

## Represents

Ideas represent future potential, unexplored paths, experiments, possible work, directions, designs, questions, improvements, and alternatives.

They are candidate memory, not commitments and not accepted truth.

## Contains

The installed file follows the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md): scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Idea Contract

Idea files hold possibilities before the workspace accepts, rejects, analyzes, or archives them.

They must keep the problem, opportunity, or motivation visible enough to revisit later.

Ideas do not create work obligations, current truth, or behavior rules.

## Loading Contract

The ideas category is relevant when current work explores possibilities, tries experimental paths, plans future work, revisits postponed options, or needs prior exploration output.

The `entrypoint` must route agents to direct idea files and child idea categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents load ideas selectively and keep unrelated possibilities out of current work.

## Promotion Contract

Accepted ideas must move to the route or system that owns their new state.

They may become another #Memory route, matching #Core material, external state, or archived history.

Repeated or stale ideas must be merged, refined, promoted, or archived.

## Scope Contract

Nested idea categories may group possibilities by any useful positive scope.

Subcategories are encouraged when they keep candidate material easier to compare and revisit.

## Generated Region

The final generated region uses the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md).

Generated `entries` list direct idea files and direct child idea categories. The [routed Markdown contract](../../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md) defines metadata, `entry` representation, entrypoint naming, and marker shape. The routing model governs recursive discovery and generation.

## Used By

Agents use this category when candidate possibilities may inform current exploration or planning.

Any process that writes ideas must keep them separate from commitments and accepted truth.

## Why

Ideas exist so possible future value has a place without becoming fake commitment.

They let Open Forge remember possibilities while keeping current truth clean.

## Alignment Checks

The implementation is aligned when it:

- is named `_ideas.md`
- lives in `.agents/memory/emerging/ideas/`
- includes `Exploration`, `OrganicGrowth`, `Contextual`, and `Candidate` in scoped `open-forge:` tags
- defines ideas as future potential and candidate possibilities
- keeps ideas separate from obligations and accepted truth
- promotes accepted ideas to the route that owns their new state
- allows accepted ideas to become another #Memory route, matching #Core material, external state, or archived history
- supports recursive positive scope
- routes only through its final generated region
- keeps compact candidate, loading, promotion, and scope axioms
- keeps the authored portion between 10 and 40 non-empty lines

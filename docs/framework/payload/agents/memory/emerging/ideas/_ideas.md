# Ideas Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/emerging/ideas/_ideas.md`.

The ideas memory category entrypoint defines how agents discover candidate possibilities and unaccepted future options.

## Represents

Ideas represent possible future work, directions, designs, questions, improvements, or alternatives.

They are candidate memory, not commitments and not accepted truth.

## Contains

The installed ideas entrypoint must contain:

- scoped `open-forge:` frontmatter with description and useful tags, including `Contextual` and `Candidate`
- a title
- one short definition of ideas
- compact candidate, loading, promotion, and scope axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 40 non-empty lines. Generated entries do not count toward this limit.

## Idea Contract

Idea files hold possibilities before the workspace accepts, rejects, analyzes, or archives them.

They must keep the problem, opportunity, or motivation visible enough to revisit later.

Ideas do not create work obligations, current truth, or behavior rules.

## Loading Contract

The ideas category is relevant when current work explores possibilities, plans future work, revisits postponed options, or needs prior brainstorm output.

The entrypoint must route agents to direct idea files and child idea categories whose path, description, or tags match the current request. Each selected child entrypoint applies the same contract recursively.

Agents load ideas selectively and keep unrelated possibilities out of current work.

## Promotion Contract

Accepted ideas must move to the route or system that owns their new state.

They may become another #Memory route, matching #Core material, external state, or archived history.

Repeated or stale ideas should be merged, refined, promoted, or archived.

## Scope Contract

Nested idea categories may group possibilities by any useful positive scope.

Subcategories are encouraged when they keep candidate material easier to compare and revisit.

## Generated Region

The final section must use the shared category entrypoint shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated entries list direct idea files and direct child idea categories. The shared formatting and routing governors own metadata extraction, entry formatting, naming, recursive discovery, marker validation, and regeneration.

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
- includes `Contextual` and `Candidate` in scoped `open-forge:` tags
- defines ideas as candidate possibilities
- keeps ideas separate from obligations and accepted truth
- promotes accepted ideas to the route that owns their new state
- allows accepted ideas to become another #Memory route, matching #Core material, external state, or archived history
- supports recursive positive scope
- routes only through its final generated region

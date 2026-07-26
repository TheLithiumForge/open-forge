# Analysis Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/emerging/analysis/_analysis.md`.

The analysis memory category `entrypoint` defines how agents discover structured reasoning, investigation, or comparison that is useful but not accepted truth.

## Represents

Analysis represents structured reasoning, investigation, comparison, critique, or synthesis before its conclusions become accepted memory.

It is emerging memory, not a decision and not a final document.

## Contains

The installed file follows the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md): scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Analysis Contract

Analysis files preserve reasoning in a route easier to find than raw sessions.

They must keep the question, evidence, assumptions, limits, and current conclusion visible.

Analysis does not become accepted truth until validated, accepted, or promoted.

## Loading Contract

The analysis category is relevant when current work needs prior reasoning, investigations, comparisons, critiques, or synthesis.

The `entrypoint` must route agents to direct analysis files and child analysis categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents load analysis selectively and check whether its assumptions still hold.

## Promotion Contract

Accepted analysis outcomes must move to the route or system that owns the resulting truth or behavior.

They may become another #Memory route, matching #Core material, external state, or archived history.

Superseded analysis must remain historical context, not current truth.

## Scope Contract

Nested analysis categories may group reasoning by any useful positive scope.

Subcategories are encouraged when they separate unrelated questions or evidence bases.

## Generated Region

The final generated region uses the category `entrypoint` shape defined by the [routed Markdown contract](../../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md).

Generated `entries` list direct analysis files and direct child analysis categories. The [routed Markdown contract](../../../../../../../.agents/memory/crystallized/documents/framework/markdown/routes.md) defines metadata, `entry` representation, entrypoint naming, and marker shape. The routing model governs recursive discovery and generation.

## Used By

Agents use this category when structured prior reasoning may affect current work.

Any process that writes analysis must keep assumptions, evidence, limits, and conclusions visible.

## Why

Analysis exists so reasoning can be preserved without pretending every conclusion is accepted.

It gives the system a reviewable bridge between raw sessions and crystallized memory.

## Alignment Checks

The implementation is aligned when it:

- is named `_analysis.md`
- lives in `.agents/memory/emerging/analysis/`
- includes `Reasoning`, `Contextual`, and `Candidate` in scoped `open-forge:` tags
- defines analysis as structured reasoning
- keeps assumptions, evidence, limits, and conclusion visible
- keeps analysis contextual until accepted or promoted
- allows accepted analysis to become another #Memory route, matching #Core material, external state, or archived history
- supports recursive positive scope
- routes only through its final generated region
- keeps compact reasoning, loading, promotion, and scope axioms
- keeps the authored portion between 10 and 40 non-empty lines

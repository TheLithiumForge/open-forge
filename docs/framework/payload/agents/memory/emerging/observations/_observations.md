# Observations Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/emerging/observations/_observations.md`.

The observations memory category `entrypoint` defines how agents discover agent-noticed findings for future learning and durable memory.

## Represents

Observations represent agent-noticed grounded findings that help future agents learn from work before they become accepted current memory.

They are stronger than ideas because they claim something was observed, but they remain contextual until validated or promoted.

Observations are the agent learning surface: they let future agents validate, promote, reject, or archive useful findings instead of relying on private agent memory.

## Contains

The installed observations `entrypoint` must contain:

- scoped `open-forge:` frontmatter with description and useful tags, including `AgentLearning`, `OrganicGrowth`, `Contextual`, `Candidate`, and `LoadForPostWorkReview`
- a title
- one short definition of observations
- compact finding, loading, promotion, and scope axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 40 non-empty lines. Generated `entries` do not count toward this limit.

## Observation Contract

Observation files record grounded facts, signals, constraints, recurring behavior, risks, or evidence that may matter later.

They must keep source, scope, and uncertainty visible.

Before ending meaningful work, agents write observations for grounded findings that may matter later but are not ready to become accepted truth, behavior, or external state.

An observation is not accepted current truth until validated, promoted, or explicitly accepted.

## Loading Contract

The observations category is relevant when current work may depend on prior noticed findings or when current work produced a grounded finding that may help future agents.

The `entrypoint` must route agents to direct observation files and child observation categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents load observations selectively and verify them before treating them as current.

## Promotion Contract

Repeated, validated, or important observations must trigger promotion to the route or system that owns the resulting material.

The destination may be another #Memory route, matching #Core material, external state, or archived history.

## Scope Contract

Nested observation categories may group findings by any useful positive scope.

Subcategories are encouraged when they make validation or promotion easier.

## Generated Region

The final section must use the shared category `entrypoint` shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated `entries` list direct observation files and direct child observation categories. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when prior findings may affect current work or when post-work review reveals a grounded finding worth preserving.

Any process that writes observations must keep source, scope, and uncertainty visible.

## Why

Observations exist so grounded findings do not vanish inside raw sessions.

They are the main bridge from incidental agent discovery to durable memory or #Core updates.

## Alignment Checks

The implementation is aligned when it:

- is named `_observations.md`
- lives in `.agents/memory/emerging/observations/`
- includes `AgentLearning`, `OrganicGrowth`, `Contextual`, `Candidate`, and `LoadForPostWorkReview` in scoped `open-forge:` tags
- defines observations as grounded noticed findings
- describes observations as agent learning material
- requires post-work observations for grounded findings that may matter later
- keeps source, scope, and uncertainty visible
- keeps observations contextual until validated or promoted
- allows validated observations to become another #Memory route, matching #Core material, external state, or archived history
- supports recursive positive scope
- routes only through its final generated region

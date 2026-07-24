# Observations Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/emerging/observations/_observations.md`.

The observations memory category `entrypoint` defines how agents discover agent-noticed findings for future learning and durable memory.

## Represents

Observations represent agent-noticed grounded findings that help future agents learn from work before they become accepted current memory.

They are stronger than ideas because they claim something was observed, but they remain contextual until validated and accepted.

Observations are the agent learning surface: they let future agents validate useful findings instead of relying on private agent memory.

## Contains

The installed file follows the shared category `entrypoint` shape owned by the formatting concept: scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Observation Contract

Observation files record grounded facts, signals, constraints, recurring behavior, risks, or evidence that may matter later.

They must keep source, scope, and uncertainty visible.

When a grounded finding may matter after the current context, agents record it before a handoff or closeout or report a blocked required write. "No observation warranted" remains valid.

An observation is not accepted current truth until it is validated and accepted.

## Loading Contract

The observations category is relevant when current work may depend on prior noticed findings or when current work produced a grounded finding that may help future agents.

Because this route is #KeepInMind, agents read it at task start or resume, after detected context restoration, and before a handoff or closeout. They recheck it during work only when its follow-ups may have changed.

The `entrypoint` must route agents to direct observation files and child observation categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents load observations selectively and verify them before treating them as current.

## Promotion Contract

Validated and accepted observations move to the route or system that owns the resulting material.

The destination may be another #Memory route, matching #Core material, external state, or archived history.

## Scope Contract

Nested observation categories may group findings by any useful positive scope.

Subcategories are encouraged when they make validation or routing easier.

## Generated Region

The final generated region uses the shared category `entrypoint` shape owned by the formatting concept.

Generated `entries` list direct observation files and direct child observation categories. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when prior findings may affect current work or when a continuity review reveals a grounded finding worth preserving.

Any process that writes observations must keep source, scope, and uncertainty visible.

## Why

Observations exist so grounded findings do not vanish inside raw sessions.

They are the main bridge from incidental agent discovery to durable memory or #Core updates.

## Alignment Checks

The implementation is aligned when it:

- is named `_observations.md`
- lives in `.agents/memory/emerging/observations/`
- includes `AgentLearning`, `OrganicGrowth`, `Contextual`, `Candidate`, and `KeepInMind` in scoped `open-forge:` tags
- defines observations as grounded noticed findings
- describes observations as agent learning material
- preserves grounded findings before handoff or closeout when they may outlive the current context
- follows the #KeepInMind refresh triggers
- requires preservation-worthy findings to be written or blocked before final response
- keeps source, scope, and uncertainty visible
- keeps observations contextual until validated and accepted
- allows validated accepted observations to become another #Memory route, matching #Core material, external state, or archived history
- supports recursive positive scope
- routes only through its final generated region
- keeps compact finding, loading, promotion, and scope axioms
- keeps the authored portion between 10 and 40 non-empty lines

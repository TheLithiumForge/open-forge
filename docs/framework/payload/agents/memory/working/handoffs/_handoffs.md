# Handoffs Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/working/handoffs/_handoffs.md`.

The handoffs memory category `entrypoint` defines how agents discover static, concise, accurate transfer notes for resuming work after context breaks.

## Represents

Handoffs represent static, concise, accurate, rereadable transfer notes for resuming work across agents, subagents, threads, workflows, or humans.

They are resumability memory, not complete history and not accepted truth.

## Contains

The installed file follows the shared category `entrypoint` shape owned by the formatting concept: scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## Handoff Contract

A handoff states the minimum useful context needed to continue work elsewhere.

It must stay concise and point to sessions, documents, code, or other routes when detail matters.

Handoffs are short static communication packets. They may include status, next action, blockers, relevant loaded context, and verification needs.

When work is transferred, delegated, interrupted, or handed to another agent or human, agents must create or update a handoff unless a more specific route already captures the complete resume context.

## Loading Contract

The handoffs category is relevant when work is resumed, transferred, delegated, interrupted, or reviewed after a context break.

The `entrypoint` must route agents to direct handoff files and child handoff categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents load the handoff closest to the current transfer scope.

## Freshness Contract

Handoffs are temporary unless explicitly preserved as history.

When work resumes, agents must extract useful material before replacing or archiving stale handoffs. Extracted material may become another #Memory route, matching #Core material, or external state.

## Scope Contract

Nested handoff categories may group transfers by any useful positive scope.

Handoff scope must make the intended receiver or resumed work easy to identify.

## Generated Region

The final generated region uses the shared category `entrypoint` shape owned by the formatting concept.

Generated `entries` list direct handoff files and direct child handoff categories. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when context must move across people, agents, subagents, threads, or workflows.

Any process that writes handoffs must keep them concise enough for resumption.

## Why

Handoffs exist because raw sessions are too long for quick resumption.

They keep transfer cheap without turning temporary context into durable truth.

## Alignment Checks

The implementation is aligned when it:

- is named `_handoffs.md`
- lives in `.agents/memory/working/handoffs/`
- includes `AgentCommunication`, `Contextual`, and `LoadNow` in scoped `open-forge:` tags
- defines handoffs as static, concise, accurate, rereadable transfer notes
- keeps handoffs contextual rather than authoritative
- requires handoffs for transfer, delegation, interruption, or agent/human handoff
- points to source routes when detail matters
- requires stale handoffs to be updated, replaced, extracted, or archived
- allows extracted handoff material to become another #Memory route, matching #Core material, or external state
- supports recursive positive scope
- routes only through its final generated region
- keeps compact transfer, loading, freshness, and scope axioms
- keeps the authored portion between 10 and 40 non-empty lines

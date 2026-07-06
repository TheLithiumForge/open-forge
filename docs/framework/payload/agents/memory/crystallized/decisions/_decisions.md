# Decisions Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/crystallized/decisions/_decisions.md`.

The decisions memory category `entrypoint` defines how agents discover accepted rationale for meaningful choices.

## Represents

Decisions represent accepted rationale within crystallized memory.

They explain why a meaningful choice was made, what alternatives or constraints mattered, and what the consequence is when that context helps future work.

## Contains

The installed decisions `entrypoint` must contain:

- scoped `open-forge:` frontmatter with description and useful tags, including `Rationale` and `CurrentTruth`
- a title
- one short definition of decisions
- compact decision, authority, loading, consolidation, and scope axioms
- a final marker-bounded generated index region

The authored portion must stay between 15 and 40 non-empty lines. Generated `entries` do not count toward this limit.

## Decision Contract

Decision files hold or route accepted rationale for meaningful choices.

A decision records why the choice was made. The current behavior, document, pattern, directive, workflow, workspace route, or external state belongs to the route or system that owns that material.

Decision records must keep only rationale that can help future work. They must not become transcripts, unresolved debates, or duplicated current truth.

## Authority Contract

Decisions are current memory within their stated scope unless superseded.

A decision does not create operational behavior by itself. If decision rationale creates behavior, reusable form, guidance, capability, workflow, workspace routing, or other #Core material, extract it into the matching #Core route, including user-created #Core categories and files.

## Loading Contract

The decisions category is relevant when current work needs accepted rationale for a meaningful choice.

The `entrypoint` must route agents to direct decision files and child decision categories whose path, description, or tags match the current request. Each selected child `entrypoint` applies the same contract recursively.

Agents load the smallest decision route that can answer the current question.

## Consolidation Contract

Decisions must avoid duplicate rationale.

When a decision overlaps an existing current decision, agents update, split, merge, or link the existing route instead of creating a competing record.

Superseded decisions must be archived or linked with enough context to understand what replaced them.

## Scope Contract

Nested decision categories may group rationale by any useful positive scope.

Decision child routes use the shared routing contract. Their `entrypoints` own scope.

## Generated Region

The final section must use the shared category `entrypoint` shape:

```md
## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
```

Generated `entries` list direct decision files and direct child decision categories. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

Agents use this category when they need accepted rationale for current work.

Any process that writes decisions must keep them as rationale, not as the behavior or record that the decision selected.

## Why

Decisions exist so meaningful choices remain understandable without mixing rationale into every active rule, pattern, workflow, or document.

They keep current rationale discoverable while letting the selected material live in the route that owns it.

## Alignment Checks

The implementation is aligned when it:

- is named `_decisions.md`
- lives in `.agents/memory/crystallized/decisions/`
- includes `Rationale` and `CurrentTruth` in scoped `open-forge:` tags
- defines decisions as accepted rationale
- keeps selected behavior in its owning route
- avoids duplicate rationale
- extracts operational decision material into matching #Core routes, including user-created #Core categories and files
- archives or links superseded decisions
- supports recursive positive scope
- routes only through its final generated region

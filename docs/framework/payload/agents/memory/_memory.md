# Memory Category Entrypoint

## Description

This descriptor governs `src/open-forge/.agents/memory/_memory.md`.

The memory category `entrypoint` defines installed Memory, its state containers, authority boundary, and generated navigation to memory state routes.

## Represents

The memory category represents self-growing markdown memory for human-AI work.

Memory records current truth, live work, AI communication, current records, historical records, and candidate learning.

It is a self-growing structure that starts from a small shared shape and becomes personal to each workspace over time.

## Contains

The installed file follows the shared category `entrypoint` shape owned by the formatting concept: scoped `open-forge:` frontmatter, a title, one short definition, compact scope-specific axioms, and a final marker-bounded generated index region.

## State Contract

Memory uses these root state containers:

```text
working/      memory alive in current work
emerging/     memory becoming useful but not accepted truth
crystallized/ accepted durable current memory
archived/     archived memory preserved for context
```

The normal lifecycle is:

```text
working -> emerging -> crystallized -> archived
```

The lifecycle describes promotion and decay, not a required move sequence. Workspaces may create, update, archive, or promote memory directly when the state is clear.

The base memory payload installs only universal child routes: sessions and handoffs under `working/`, observations, ideas, and analysis under `emerging/`, and documents and decisions under `crystallized/`.

Archive child routes, task routes, backlog routes, and other specialized containers are created only when an owning route needs them and their `entrypoint` defines their scope.

## Loading Contract

The root memory category is loaded through its generated loader `entry` because its installed metadata includes #LoadNow.

The memory `entrypoint` must rely on the loader-defined #LoadNow tag for baseline state routing.

The `working/`, `crystallized/`, and `archived/` state `entrypoints` must use #LoadNow. The `working/handoffs/` and `working/sessions/` child `entrypoints` must also use #LoadNow because they are bounded transfer and raw-context routes. The `emerging/` state `entrypoint` must use #KeepInMind. State `entrypoints` expose routes; state bodies stay relevance-routed.

The `working/`, `emerging/`, and `archived/` `entries` must include #Contextual. The `crystallized/` `entry` must include #CurrentTruth.

State `entrypoints` provide route awareness. They must let agents identify relevant memory files and child categories without loading every memory body.

Generated `entries` are navigation metadata plus reserved load policy. They never define memory authority, lifecycle state, or truth by themselves.

## Authority Contract

Memory records state; it is not a behavior primitive.

Memory material moves between #Memory routes when its state or owner changes.

Useful durable state must be written to the matching #Memory route when safe and allowed. Private or opaque agent memory is a hint, not the source of truth for workspace state.

If memory creates behavior, reusable form, guidance, capability, workflow, workspace routing, or other #Core material, extract it into the matching #Core route, including user-created #Core categories and files.

Memory uses the loader-defined #Contextual tag for supporting context that is not accepted current truth unless restored, validated, accepted, or promoted.

Memory uses the loader-defined #CurrentTruth tag for accepted current memory within stated scope. Current user instructions, runtime safety, platform constraints, declared external sources of truth, and applicable #Core routes still take precedence.

## Growth Contract

Memory must remain recursively customizable.

Subcategories are encouraged when they improve routing, ownership, or clarity. Child `entrypoints` and local files own the concrete taxonomy below each memory state.

Agents create child categories first when the installed states are sufficient. A new root memory state requires user agreement because it changes the state model.

When no existing route fits safely, agents must suggest a clearer child route and request user confirmation for important long-lived taxonomies.

## Generated Region

The final generated region uses the shared category `entrypoint` shape owned by the formatting concept.

Generated `entries` list direct memory state categories and direct memory files if a workspace adds any. The shared formatting and routing governors own metadata extraction, `entry` formatting, naming, recursive discovery, marker validation, and regeneration.

## Used By

The loader uses this `entrypoint` when Memory is installed. Agents use it to decide which memory state routes to load for the current request.

Any process that writes memory must choose the route whose state and scope match the material.

## Why

Memory exists so useful context can outlive a chat without making old chat history pretend to be current truth.

It separates living work, emerging material, durable current memory, and archived history so agents can preserve context without turning stale material into current truth.

## Alignment Checks

The implementation is aligned when it:

- is named `_memory.md`
- lives in `.agents/memory/`
- includes `OrganicGrowth` and `LoadNow` in scoped `open-forge:` tags
- defines `working/`, `emerging/`, `crystallized/`, and `archived/`
- describes Memory as self-growing markdown memory for workspace state, AI communication, current records, historical records, and learning
- installs only universal child routes under the relevant memory state
- marks `working/`, `crystallized/`, and `archived/` `entries` with #LoadNow
- marks `working/handoffs/` and `working/sessions/` `entries` with #LoadNow
- marks `emerging/` `entries` with #KeepInMind
- marks `working/`, `emerging/`, and `archived/` `entries` with #Contextual
- marks `crystallized/` `entries` with #CurrentTruth
- keeps Memory separate from primitive behavior routes
- prefers written #Memory routes over private agent memory for workspace state
- moves material between #Memory routes when its state or owner changes
- extracts operational memory material into matching #Core routes, including user-created #Core categories and files
- uses loader-defined #Contextual `entries` as contextual
- uses loader-defined #CurrentTruth `entries` as accepted current memory within stated scope
- encourages recursive child categories before new root memory states
- allows specialized child routes only under the route that owns their meaning
- keeps generated `entries` from defining instructions or authority
- routes only through its final generated region
- keeps compact lifecycle, authority, baseline loading, growth, child-route, and promotion axioms
- keeps the authored portion between 12 and 50 non-empty lines

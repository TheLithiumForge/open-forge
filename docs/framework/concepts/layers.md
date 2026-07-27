# Layers

## Description

This descriptor governs the conceptual layer model across Open Forge.

Layers separate the minimum framework, persisted memory, and optional capabilities.

## Represents

The layer model represents installable responsibility, not authority rank.

Open Forge defines three layers:

```text
Layer 1: Core
Layer 2: Memory
Layer 3: Extensions
```

Layer numbers describe dependency order and product shape. After this mapping is defined, prose should prefer #Core, #Memory, and #Extension when it refers to routed authority, installable material, promotion, or classification. Authority still comes from current user instructions, runtime safety, platform constraints, and the authoritative routed primitive or memory state.

Layer tags are classification tags. #Core marks Core material, #Memory marks Memory material, and #Extension marks Extensions material. These tags make routing, search, and reference graphs cheaper; they do not create authority by themselves.

## Layer 1: Core

#Core is the minimum Open Forge framework.

#Core contains the root `entrypoint`, loader, routing model, workspace route, formatting and overwrite contracts, and primitive categories for directives, patterns, guidance, skills, and workflows.

#Core must stay small. It installs empty primitive category `entrypoints` and the axioms needed to route and interpret them. It does not seed opinionated behavior, technology patterns, workflow packs, templates, or project memory.

#Core is the required base for every Open Forge install. Every other layer depends on #Core routing and #Core authority boundaries.

#Core payload routes must include the #Core tag plus their singular route type tag, such as #Directive, #Pattern, #Guidance, #Skill, #Workflow, or #Workspace.

## Layer 2: Memory

#Memory is the durable markdown persistence layer for human-AI work.

#Memory records live work, candidate learning, accepted current memory, and archived history through the installed `memory/` route.

#Memory uses these root state containers:

```text
working/      memory alive in current work
emerging/     memory becoming useful but not accepted truth
crystallized/ accepted durable current memory
archived/     archived memory preserved for context
```

#Memory may record any subject, including how work is performed, without making that behavior active. Accepted behavior, reusable form, guidance, capability, workflow, workspace routing, or other material that should guide future work belongs in the matching #Core route, including user-created #Core categories and files.

The base #Memory payload installs only universal child routes, including crystallized decisions for accepted rationale. More specialized containers, such as archive child routes, task routes, backlog routes, and workflow-specific outputs, are created under the route authoritative for their meaning. #Extension payloads may package those additions, but #Memory rules determine their state and scope.

#Memory may be installed with or after #Core. The default #Memory `entrypoint` is tagged #LoadNow, so its generated loader `entry` loads with the loader when #Memory is installed.

#Memory uses #KeepInMind on continuity-critical candidate-memory `entrypoints`. Agents recover the complete routed set of tagged routes at every continuity boundary so they can preserve and route useful material produced during work; unrelated descendant memory bodies remain selectively loaded through those `entrypoints`.

## Layer 3: Extensions

#Extension routes are optional installable packages.

An #Extension may add primitive files, nested primitive categories, workflow-local bundles, memory child routes, templates, skills, integrations, or other curated support.

#Extension payloads consume #Core routing and may read from or write to #Memory when #Memory is installed. They must not create another Open Forge root, another loader, or a competing framework authority model inside the same workspace.

An #Extension is not higher authority because it is installed later. Its content is interpreted by the route where it is installed: directives remain mandatory in scope, patterns shape inspectable results, guidance informs judgment, skills provide bounded capability, workflows orchestrate goals, and memory remains persisted context.

## Installation Contract

The install model must keep the layers visible even when multiple layers ship together:

- #Core is the required base.
- #Memory is the official persistence layer and may install its minimum state `entrypoints` with #Core.
- #Extension payloads are optional extensions added on top.

The CLI may expose commands using product-friendly names, but installed material must still map back to one of these layers.

#Extension payloads must add files into the existing routed structure instead of requiring agents to learn a separate root framework.

## Growth Contract

Workspaces grow by adding routed files and child categories first.

The [current overwrite contract](../../../.agents/memory/crystallized/documents/framework/routing/overwrites.md) defines the local adjustment mechanism between adding routed files and directly changing a base.

Workspaces expand #Memory through child categories. #Extension payloads expand existing routes by adding routed material. New root categories or new #Memory root states require stronger justification because they change the top-level routing model.

## Why

The layer model keeps Open Forge understandable as it grows.

#Core stays small enough to install everywhere. #Memory gives long-running work a durable place to evolve. #Extension payloads add useful capability without making every workspace start with a large, opinionated pack.

## Alignment Checks

Layers are aligned when:

- layer names map to #Core, #Memory, and #Extension
- layer classification tags are #Core, #Memory, and #Extension
- layer number describes dependency order, not authority rank
- #Core remains the minimum routing and primitive framework
- #Core does not seed opinionated local behavior
- #Memory is authoritative for the lifecycle of persisted context and extracted learning
- #Memory may describe behavior without activating it
- accepted behavior that should guide future work belongs in matching #Core routes, including user-created #Core categories and files
- #Memory installs universal child routes and leaves specialized containers to routes authoritative for their meaning
- #Memory loads for every request when installed
- continuity-critical #Memory routes participate in the complete routed #KeepInMind catalogue without preloading unrelated descendant bodies
- #Extension payloads install optional content into existing routes
- #Extension payloads do not create competing Open Forge roots
- #Extension payloads remain removable and understandable as layer additions
- new root routes or #Memory states require explicit design justification

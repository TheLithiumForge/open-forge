# Layers

## Description

This descriptor governs the conceptual layer model across Open Forge.

Layers separate the minimum framework, persisted memory, and optional capabilities.

## Represents

The layer model represents installable responsibility, not authority rank.

Open Forge uses three layers:

```text
Layer 1: Core
Layer 2: Memory
Layer 3: Extensions
```

Layer numbers describe dependency order and product shape. Authority still comes from current user instructions, runtime safety, platform constraints, and the routed primitive or memory state that owns the content.

Layer tags are classification tags. `#Core` marks Layer 1 material, `#Memory` marks Layer 2 material, and `#Extension` marks Layer 3 material. These tags make routing and search cheaper; they do not create authority by themselves.

## Layer 1: Core

Core is the minimum Open Forge framework.

Core contains the root entrypoint, loader, routing model, workspace route, formatting and overwrite contracts, and primitive categories for directives, patterns, guidelines, skills, and workflows.

Core must stay small. It installs empty primitive category entrypoints and the axioms needed to route and interpret them. It does not seed opinionated behavior, technology patterns, workflow packs, templates, or project memory.

Core is the required base for every Open Forge install. Every other layer depends on Core routing and Core authority boundaries.

Core payload routes must include the `#Core` tag plus their singular route type tag, such as `#Directive`, `#Pattern`, `#Guideline`, `#Skill`, `#Workflow`, or `#Workspace`.

## Layer 2: Memory

Memory is the persistence layer for human-AI work.

Memory records live work, candidate learning, accepted current memory, and archived history through the installed `memory/` route.

Memory uses these root state containers:

```text
working/      memory alive in current work
emerging/     memory becoming useful but not accepted truth
crystallized/ accepted durable current memory
archived/     archived memory preserved for context
```

Memory records state. If memory becomes operational behavior, reusable form, guidance, capability, workflow, workspace routing, or other `#Core` material, it must be promoted into the matching `#Core` route, including user-created `#Core` categories and files.

Memory may be installed with or after Core. The default Memory entrypoint is tagged `LoadWithParentEntrypoint`, so its generated loader entry loads with the loader when Memory is installed.

## Layer 3: Extensions

Extensions are optional installable packages.

An extension may add primitive files, nested primitive categories, workflow-local bundles, memory child routes, templates, skills, integrations, or other curated support.

Extensions consume Core routing and may read from or write to Memory when Memory is installed. They must not create another Open Forge root, another loader, or a competing framework authority model inside the same workspace.

An extension is not higher authority because it is installed later. Its content is interpreted by the route where it is installed: directives remain mandatory in scope, patterns shape inspectable results, guidelines inform judgment, skills provide bounded capability, workflows orchestrate goals, and memory remains persisted context.

## Installation Contract

The install model must keep the layers visible even when multiple layers ship together:

- Core is the required base.
- Memory is the official persistence layer and may install its minimum state entrypoints with Core.
- Extensions are optional modules or packs added on top.

The CLI may expose commands using product-friendly names, but installed material must still map back to one of these layers.

Optional modules must add files into the existing routed structure instead of requiring agents to learn a separate root framework.

## Growth Contract

Workspaces grow by adding ordinary routed files and child categories first.

Overwrites are the local adjustment mechanism for small changes to managed files. Direct edits to managed framework files are reserved for complete replacement or cases where base plus overwrite would confuse an agent.

Layer 2 child categories and Layer 3 modules may expand the framework recursively. New root categories or new Memory root states require stronger justification because they change the top-level routing model.

## Why

The layer model keeps Open Forge understandable as it grows.

Core stays small enough to install everywhere. Memory gives long-running work a durable place to evolve. Extensions add useful capability without making every workspace start with a large, opinionated pack.

## Alignment Checks

Layers are aligned when:

- official layer names are Core, Memory, and Extensions
- layer classification tags are `#Core`, `#Memory`, and `#Extension`
- layer number describes dependency order, not authority rank
- Core remains the minimum routing and primitive framework
- Core does not seed opinionated local behavior
- Memory owns persisted context and extracted learning
- Memory does not own operational behavior
- Memory promotes operational material into matching `#Core` routes, including user-created `#Core` categories and files
- Memory loads for every request when installed
- Extensions install optional content into existing routes
- Extensions do not create competing Open Forge roots
- optional modules remain removable and understandable as layer additions
- new root routes or Memory states require explicit design justification

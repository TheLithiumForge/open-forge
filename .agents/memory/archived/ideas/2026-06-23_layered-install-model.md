---
open-forge:
  description: Three-layer install model for core context, memory, and extensions
  tags: [Architecture, Layer, Core, Memory, Extension, CLI]
---

# Layered Install Model

## Idea

Open Forge should describe installable material in layers instead of treating every useful file set as equal.

Proposed layers:

1. Core context
2. Memory
3. Extensions

These names are working names. The CLI should use plain names that match the product model rather than exposing vague terms like goodies, addons, or enhancements.

## Layer 1: Core Context

Core context is the minimum Open Forge framework.

It contains the entrypoint, loader, routing rules, workspace routing, and primitive category entrypoints for directives, guidelines, patterns, skills, and workflows.

This layer is close to "durable memory", but that phrase is slightly too narrow. Core context also includes behavior, routing, and authority primitives, not only remembered facts.

Better names:

- user-facing: Core
- precise internal meaning: Core Context
- avoid as official name: Durable Memory

## Layer 2: Memory

Memory contains the files that record how work evolves over time and the durable project records that later work needs.

Candidates include:

- active memory
- sessions
- ideas
- observations
- analysis
- decisions
- documents
- handoffs
- tasks or backlog, if task ownership is explicitly designed
- archives as a nested convention, not necessarily a root category

This layer probably ships together, but its shape is not decided. Current analysis favors one umbrella `memory/` category over several root categories.

The existing meanings of these categories should be rewritten from scratch. Current descriptions are partly historical and should not constrain the final model.

Process Memory remains a useful subdomain, but it is too narrow as the official Layer 2 name because Layer 2 also needs project memory such as PRDs, architecture documents, design rationale, product context, and decisions.

## Layer 3: Extensions

Extensions are optional installable packages that add useful material on top of core and process memory.

Examples may include:

- technology-specific patterns
- workflow packs
- skill packs
- directive or guideline packs
- agent-tool adapters

Extension is a better layer name than addon or enhancement because it describes optional expansion without implying that the core is incomplete.

The CLI mechanism may still be called modules if that remains the best packaging word. In that case:

- extension = user-facing layer/category of optional additions
- module = concrete installable package unit

## CLI Direction

The CLI should make the layer explicit:

- `install` installs Core by default
- a future memory command or option installs Memory
- future module commands list, preview, install, update, and remove Extensions

Exact commands remain undecided. The important part is that users can understand what kind of thing they are adding before it enters agent context.

## Open Questions

- Whether Memory is installed by default, offered during install, or installed as a first official extension.
- Whether Memory should install the full proposed category set or a smaller starter set.
- Whether the official terms should be Core, Memory, and Extensions, or Core Context, Memory, and Modules.
- Whether `archive/` needs a framework descriptor or remains only a convention inside other categories.
- How to represent ownership, provenance, and update behavior for Memory and Extensions.

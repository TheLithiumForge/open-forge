---
open-forge:
  description: Optional CLI modules for skills, technology patterns, and other reusable additions
  tags: [OpenForge, CLI, Modules, Skills, Patterns]
---

# Optional CLI Modules

## Idea

Keep the default Open Forge installation limited to the small routing framework and its required primitives.

The core installs the minimum root entrypoints for directives, patterns, guidelines, skills, and workflows. These empty routing surfaces make the framework shape discoverable without seeding opinions or unused capabilities.

Let users explicitly discover and install optional modules through the CLI. A module is a selectable package of reusable files, not a new authority type and not part of the mandatory core.

Possible modules include:

- behavioral or workflow skills
- patterns for a specific technology, framework, or project shape
- focused guides or directive sets
- workflow and template packs
- adapters for particular agent tools

Any individual example is illustrative. The larger idea is a general mechanism for adding curated capabilities without seeding every workspace with files it may never use.

## Relationship to Skills

A module and a skill are different concepts.

- A module is installed through the CLI and may contain one or more files or categories.
- A skill is reusable behavior with a defined activation context and instructions.
- A skill does not require a slash command. Automatic selection, explicit naming, workflow activation, and tool-specific commands are possible activation mechanisms.
- A behavioral skill may contain directives that are mandatory only while that skill is active.

Modes may therefore be represented as behavioral skills instead of becoming a separate core primitive.

## Direction

Optional modules should:

- be installed only after explicit user selection
- expose their purpose and installed files before installation
- use the same category and routing contracts as local files
- remain removable without damaging unrelated local content
- avoid turning the default install into a large preset framework
- allow users to create local alternatives without adopting the catalog version

The CLI may eventually support listing, previewing, installing, updating, and removing modules independently from the core payload.

## Open Questions

- Where module manifests and source files live.
- Whether installed module files remain CLI-managed or become user-owned seeds.
- How updates preserve edits and local overwrite files.
- How module dependencies, conflicts, and compatibility are declared.
- How installed provenance and versions are recorded without adding runtime overhead.
- Whether modules install complete categories, routed files inside existing categories, or both.
- How external module catalogs can be trusted and reviewed before their instructions enter agent context.

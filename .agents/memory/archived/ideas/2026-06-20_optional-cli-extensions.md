---
open-forge:
  description: Optional CLI extensions for skills, technology patterns, and other reusable additions
  tags: [CLI, Extension, Skill, Pattern]
---

# Optional CLI Extensions

## Idea

Keep the default Open Forge installation limited to the small routing framework and its required primitives.

#Core installs the minimum root entrypoints for directives, patterns, guidelines, skills, and workflows. These empty routing surfaces make the framework shape discoverable without seeding opinions or unused capabilities.

Let users explicitly discover and install #Extension payloads through the CLI. An extension is a selectable package of reusable files, not a new authority type and not part of mandatory #Core.

Possible extensions include:

- behavioral or workflow skills
- patterns for a specific technology, framework, or project shape
- focused guides or directive sets
- workflow and template packs
- adapters for particular agent tools

Any individual example is illustrative. The larger idea is a general mechanism for adding curated capabilities without seeding every workspace with files it may never use.

## Relationship to Skills

An extension and a skill are different concepts.

- An extension is installed through the CLI and may contain one or more files or categories.
- A skill is reusable behavior with a defined activation context and instructions.
- A skill does not require a slash command. Automatic selection, explicit naming, workflow activation, and tool-specific commands are possible activation mechanisms.
- A behavioral skill may contain directives that are mandatory only while that skill is active.

Modes may therefore be represented as behavioral skills instead of becoming a separate #Core primitive.

## Direction

#Extension payloads should:

- be installed only after explicit user selection
- expose their purpose and installed files before installation
- use the same category and routing contracts as local files
- remain removable without damaging unrelated local content
- avoid turning the default install into a large preset framework
- allow users to create local alternatives without adopting the catalog version

The CLI may eventually support listing, previewing, installing, updating, and removing extensions independently from the #Core payload.

## Open Questions

- Where extension manifests and source files live.
- Whether installed extension files remain CLI-managed or become user-owned seeds.
- How updates preserve edits and local overwrite files.
- How extension dependencies, conflicts, and compatibility are declared.
- How installed provenance and versions are recorded without adding runtime overhead.
- Whether extensions install complete categories, routed files inside existing categories, or both.
- How external extension catalogs can be trusted and reviewed before their instructions enter agent context.

## Extension Payload Shape

Extensions should be standalone payloads that install into the normal routed tree.

Preferred source shape:

```text
src/extensions/{extension-id}/
  extension.json
  payload/
    .agents/...
```

An extension installs by copying `payload/` into the target workspace, preserving local blocks with the same rules as the base install, then rebuilding generated indexes.

If an extension targets a deep route whose ancestor category entrypoints do not exist, the extension manifest must provide scaffold metadata for every missing category. The CLI should fail clearly when that metadata is missing instead of generating meaningless category entrypoints.

The same scaffold logic should later support user-created categories and local extension authoring. A user can create a route template directly in a workspace, while a shareable extension writes the same kind of files under `src/extensions/{extension-id}/payload/`.

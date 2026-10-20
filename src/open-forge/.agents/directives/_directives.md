---
open-forge:
  description: Rules agents must follow in this workspace
  tags: [LoadNow, Core, Directive]
---

# Directives

Optional. Rule files here apply to the whole workspace, and subfolders narrow them to an area. Nothing else depends on this folder.

## Axioms

- Put each file's rules under `## Instructions`, with when they apply and their exceptions, so they make sense on their own.
- Tag each rule file #LoadNow. Limit it with `applyTo`, for example `**/*.cs`. A subfolder of rules needs a #LoadNow catalogue, limited the same way when it matches files.

## Entries

- none - No entries - #Empty

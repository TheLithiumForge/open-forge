---
open-forge:
  description: Rules agents must follow in this workspace
  tags: [LoadNow, Core, Directive]
---

# Directives

## What behavior is required in this scope?

Rule files here apply to the whole workspace.

## Axioms

- Put each file's rules under a non-empty `## Instructions`. State each obligation with its conditions and meaningful exceptions, so it stays understandable without its examples.
- Tag each rule file in this folder #LoadNow. Directives are mandatory within their scope. Limit a rule file with `applyTo` when it only matters for some files, for example `**/*.cs`.
- A subfolder narrows its rules to that area, and its catalogue may stay on demand. Its rules apply once the task selects it. Tag the subfolder's catalogue #LoadNow, limited with `applyTo` when it matches files, when its rules must load automatically for matching work.

## Entries

- none - No entries - #Empty

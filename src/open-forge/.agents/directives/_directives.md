---
open-forge:
  description: Required instructions loaded through selected routes
  tags: [LoadNow, Core, Directive]
---

# Directives

## What behavior is required in this scope?

Directives define required agent behavior within a scope.

## Axioms

- Each Directive file in the same folder as an entrypoint must be listed in its `Entries` with #LoadNow.
- Each of these files has one non-empty `## Instructions` section.
- Directive files in this root entrypoint's folder apply throughout the workspace.
- Select a child entrypoint before loading its Directive files. Their Instructions apply only within that narrower scope.
- Child Directives add to active parent Directives. A narrower scope does not create higher authority.
- Report any conflict or instruction that cannot be followed, and explain why.

## Entries

- none - No entries - #Empty

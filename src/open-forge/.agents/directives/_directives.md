---
open-forge:
  description: Required instructions loaded through selected routes
  tags: [LoadNow, Core, Directive]
---

# Directives

## What behavior is required in this scope?

Directives define required agent behavior within a scope.

## Axioms

- Each Directive file in the same folder as an `entrypoint` must carry #LoadNow. Directives remain mandatory within their scope. An optional `applyTo` condition may appear at the frontmatter root or under `open-forge:` and limits applicability to matching working files, not permission to edit files.
- Each Directive file keeps its instructions under a non-empty `## Instructions` heading.
- Directive files in this root `entrypoint`'s folder apply throughout the workspace, subject to any declared `applyTo` condition.
- A child entrypoint does not need #LoadNow. It loads only when selected.
- Select a child entrypoint before loading its Directive files. Their Instructions apply only within that narrower scope and any matching `applyTo` condition.
- Child Directives add to active parent Directives. A narrower scope does not create higher authority.
- Report any conflict or instruction that cannot be followed, and explain why.

## Entries

- none - No entries - #Empty

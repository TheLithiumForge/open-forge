---
open-forge:
  description: Required instructions loaded through selected routes
  tags: [LoadNow, Core, Directive]
---

# Directives

## What behavior is required in this scope?

Directives state the instructions that must be followed within their scope.

## Axioms

- Every sibling Directive listed under an entrypoint's `Entries` carries #LoadNow.
- Each sibling Directive has one non-empty `## Instructions` section.
- Sibling Directives listed by this root entrypoint apply throughout the workspace.
- A selected child entrypoint sets the narrower scope before its sibling Directives load.
- Those Instructions apply only within the child route's scope.
- Child Directives add to active parent Directives. A narrower scope does not create higher authority.
- Report any conflict or instruction that cannot be followed, and explain why.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

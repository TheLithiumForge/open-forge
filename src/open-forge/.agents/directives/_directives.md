---
open-forge:
  description: Mandatory instructions agents must follow when they apply to the current work
  tags: [LoadNow, Core, Directive]
---

# Directives

Directives are mandatory modifiers within their declared scope.

## Axioms

- Every directive file beside this `entrypoint` is workspace-wide; read all of them.
- Child directive `entrypoints` define positive scope through path, description, and tags.
- Load child directive routes when their path, description, tags, or defined tag behavior match the current work.
- Every loaded directive is mandatory within its scope.
- Report when a directive cannot be followed, and explain why.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

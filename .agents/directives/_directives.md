---
open-forge:
  description: Mandatory instructions agents must follow when they apply to the current work
  tags: [LoadNow, Core, Directive]
---

# Directives

Directives are mandatory modifiers within their declared scope.

## Axioms

- Read every direct directive file far enough to evaluate its explicit `Applies To`; root placement alone does not make a directive workspace-wide.
- Every directive file declares one positive `Applies To` scope before its Axioms. A workspace-wide directive says so explicitly.
- Child directive `entrypoints` define positive scope through authored meaning, path, description, and tags; a hybrid category entrypoint may state `inherited` when it adds no narrower scope.
- Load child directive routes when their path, description, tags, or defined tag behavior match the current work, then confirm applicability from `Applies To`.
- Every applicable loaded directive is mandatory within its declared scope.
- Report when a directive cannot be followed, and explain why.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

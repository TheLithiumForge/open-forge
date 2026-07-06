---
open-forge:
  description: Mandatory instructions that change how agents must work in this workspace
  tags: [OpenForge, Core, Directive, Index, LoadWithParentEntrypoint]
---

# Directives

Directives are mandatory modifiers within their declared scope.

## Axioms

- Every directive file beside this `entrypoint` is workspace-wide; load all of them.
- Child directive `entrypoints` define positive scope through path, description, and tags.
- Load child directive routes when their path, description, tags, or defined tag behavior match the current work.
- Follow each loaded child `entrypoint`'s scope and loading axioms recursively.
- Every loaded directive is mandatory within its scope.
- Prefer directives from a narrower selected scope over broader directives when safe and allowed; report unresolved conflicts.
- Current user instructions, platform constraints, and runtime safety take precedence.
- Generated `entries` are navigation and reserved load policy only.
- Report when a directive cannot be followed, and explain why.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

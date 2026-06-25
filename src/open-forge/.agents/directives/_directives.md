---
open-forge:
    description: Mandatory workspace modifiers; load for every request
    tags: [OpenForge, Directives, Global, Index]
---

# Directives

Directives are mandatory modifiers within their declared scope.

## Axioms

- Load this entrypoint for every request.
- Every directive file beside(alongside? or sibling file) this entrypoint is workspace-wide; load all of them.
- Child categories may define a work scope or organize directives under `global/` and `scoped/`.
- Read the entries: always load global routes and scoped routing indexes; load other child routes when their path, description, or tags match the current work.
- Follow each loaded child entrypoint's scope and loading axioms recursively.
- Every loaded directive is mandatory within its scope.
- Prefer directives from a narrower selected scope over broader directives when safe and allowed; report unresolved conflicts.
- Current user instructions, platform constraints, and runtime safety take precedence.
- Generated entries are navigation only.
- inform the user if a directive cant be followed and why

## Entries

<!-- open-forge:generated-index:start -->

- none - No entries - #Empty
    <!-- open-forge:generated-index:end -->

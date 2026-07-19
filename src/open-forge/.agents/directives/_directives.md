---
open-forge:
  description: Binding instructions whose route is selected before their contents are loaded
  tags: [LoadNow, Core, Directive]
---

# Directives

Directives are binding instructions selected through the route tree.

## Axioms

- Read every direct directive file exposed by a loaded directive `entrypoint`; direct files inherit the scope already selected by that route.
- A directive loaded through the active directive route chain is binding. It has no second applicability decision inside the file; merely inspecting an example, archive, source payload, or inactive route does not activate it.
- Direct files under this root are workspace-wide because this root route is always loaded. Put narrower directives under a positively described child route and select that route before opening its contents.
- Select child directive routes from their path, description, tags, and ancestor meaning. Do not open a directive speculatively and then decide whether to ignore it.
- Loaded child `entrypoint` Axioms and direct directive files add to loaded ancestor directives; narrower routing changes scope, not authority.
- Every direct directive file defines exactly one substantive level-2 `## Axioms` section and no `## Applies To` gate. Put optional behavior in guidance, a skill, or a workflow; put operational conditions inside the relevant Axiom.
- Report when a directive cannot be followed, and explain why.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

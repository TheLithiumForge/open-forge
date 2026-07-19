---
open-forge:
  description: Mandatory instructions
  tags: [LoadNow, Core, Directive]
---

# Directives

Directives are binding/mandatory instructions

## Axioms

- Read every direct directive file exposed by a loaded directive `entrypoint`; direct files inherit the scope already selected by that route. // Hn: directives can't really be scoped. Maybe we shouldn't scope them or we can control the loading via loadNow or the default load if the scope matches. Opinions? 
- A directive loaded through the active directive route chain is binding. It has no second applicability decision inside the file; merely inspecting an example, archive, source payload, or inactive route does not activate it.
- Direct files under this root are workspace-wide because this root route is always loaded. Put narrower directives under a positively described child route and select that route before opening its contents. //Hn: very good
- Select child directive routes from their path, description, tags, and ancestor meaning. Do not open a directive speculatively and then decide whether to ignore it.
- Loaded child `entrypoint` Axioms and direct directive files add to loaded ancestor directives; narrower routing changes scope, not authority.
- Every direct directive file defines exactly one substantive level-2 `## Axioms` section and no `## Applies To` gate. Put optional behavior in guidance, a skill, or a workflow; put operational conditions inside the relevant Axiom. //Hn: this is a thing that should be added to memory (although from all of my other notes, most should be added to memory, but this is very imp, absolutely necer write something like this where you explain what not to use, especially if thst was just a prototype on out end, always thing from an end user perspective how rhey would see this, how they would understand it, in this example they would think, why did they even mention appliesTo, what does that even mean, should i look for it, am I missing something and so on)
- Report when a directive cannot be followed, and explain why. // Hn: very good

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->

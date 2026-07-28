---
open-forge:
  description: Binding instructions whose `route` is selected before their contents are loaded
  tags: [LoadNow, Core, Directive]
---

# Directives

Directives are binding instructions selected through the route tree

## Axioms

- Every direct directive file carries #LoadNow, so an already-loaded directive `entrypoint` reads all of its direct files
- Direct files loaded from this root are binding throughout the workspace
- Select a child directive route only when its path, description, tags, and ancestor meaning match the work; loading that route establishes its narrower scope before its direct files are read
- Direct files loaded from a selected child route are binding within the selected route's scope
- Loaded child directives add to loaded ancestor directives; narrower routing changes scope, not authority
- Every direct directive file contains exactly one non-empty `## Axioms` section. Put optional behavior in guidance, a skill, or a workflow.
- Report any directive conflict or directive that cannot be followed, and explain why

## Entries

<!-- open-forge:generated-index:start -->
- [Keep Open Forge contract changes deliberate, current, dogfooded, reviewable, and evidence-backed](deliberate-framework-change.md) - #LoadNow #Directive #Framework #Change #Dogfood #Review #Evidence
- [Require independent semantic-loss, writing-quality, and optimization reviews during the current Open Forge migration](temporary-independent-review.md) - #LoadNow #Directive #Framework #Change #Migration #Review #Temporary
<!-- open-forge:generated-index:end -->

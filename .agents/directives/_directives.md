---
open-forge:
  description: Required instructions loaded through selected routes
  tags: [LoadNow, Core, Directive]
---

# Directives

Directives contain required instructions.

## Axioms

- Every sibling Directive listed under an entrypoint's `Entries` carries #LoadNow.
- Each sibling Directive has one non-empty `## Instructions` section.
- Sibling Directives listed by this root entrypoint apply throughout the workspace.
- Select a child Directive route only when its path, description, tags, and parent routes match the work.
- A selected child entrypoint sets the narrower scope before its sibling Directives load.
- Those Instructions apply only within the child route's scope.
- Child Directives add to active parent Directives. A narrower scope does not create higher authority.
- Report any conflict or instruction that cannot be followed, and explain why.

## Entries

<!-- open-forge:generated-index:start -->
- [Keep consequential decisions with the maintainer while agents and councils supply evidence, alternatives, challenges, and recommendations](decision-authority.md) - #LoadNow #Core #Directive #Decision #Collaboration #Authority
- [Binding Open Forge-maintenance instructions, narrowed by CLI, Framework, testing, and TypeScript scopes](open-forge/_open-forge.md) - #Directive #CLI #Framework #Testing #TypeScript
- [Keep behavior and its directly related source, contracts, tests, fixtures, and support together at the narrowest useful scope](source-locality.md) - #LoadNow #Core #Directive #Source #Locality #Contract #Testing #Structure
- [Write clear, consistent user communication and Open Forge source prose](writing.md) - #LoadNow #Core #Directive #Writing #Terminology
<!-- open-forge:generated-index:end -->

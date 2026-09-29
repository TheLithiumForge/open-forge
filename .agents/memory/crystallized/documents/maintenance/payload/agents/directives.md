---
open-forge:
  description: Current maintenance contract for the installable Directives Core category entrypoint
  responsibility: Preserve installed Directive activation, scope, authority, source alignment, and deterministic validation boundary
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Directive]
---

# Directives Category Maintenance Contract

## Source

[`src/open-forge/.agents/directives/_directives.md`](../../../../../../../src/open-forge/.agents/directives/_directives.md) is the canonical installed Directives entrypoint. The repository [Directives entrypoint](../../../../../../directives/_directives.md) dogfoods the same authored contract and may add local generated entries.

The [current Directive contract](../../../framework/primitives/directives.md) defines the coherent activation, authority, scope, and file model.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Directive so the `root route` enters baseline context with its primitive type visible
- The entrypoint keeps the one-pass model: select a Directive route, then load and follow each sibling Directive exposed through #LoadNow whose `applyTo` condition matches a working file or is absent
- Sibling Directives under the root apply across the workspace, subject to declared conditions. A selected child entrypoint sets the narrower scope before its applicable sibling Directives load
- Child Directives add to active parent Directives and report conflicts instead of creating hidden precedence
- Every sibling Directive carries #LoadNow and keeps its instructions under a non-empty level-2 `## Instructions` heading. An optional frontmatter `applyTo` condition may limit when those instructions apply to working files without replacing the #LoadNow requirement.
- The entrypoint defines the reusable category rules. Each sibling Directive defines its own required behavior

The [routed Markdown representation](../../../framework/markdown/routes.md) defines entrypoint and generated-region syntax. Directive-specific meaning remains in the source and current Directive contract rather than in shared Markdown rules.

## Verification

- Directive validation requires sibling-file #LoadNow, accepts `applyTo` as an optional pre-load file condition, and checks non-empty Instructions and active primitive classification
- Core installation tests verify that the root category installs, indexes, and remains baseline-loaded
